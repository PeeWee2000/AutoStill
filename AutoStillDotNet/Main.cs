using System;
using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using System.Windows.Forms;
using System.Data;
using System.Windows.Threading;
using System.ComponentModel;

namespace AutoStillDotNet
{
    public partial class Main : Form
    {      
        private BackgroundWorker PressureRegulator; //Determines when to turn the vaucuum pump on and off
        private BackgroundWorker StillMonitor;  //Reads all the sensors and switches
        private BackgroundWorker StillController; //Turns the element, pumps and valves on and off

        public volatile bool Run = true; //Used to shut down or start the whole process
        public volatile int Phase = 0; //Used to control the main still control background worker and report progress

        //Varaiables written to and read by all the various loops -- Assume the still is empty and all periphrials are off when starting up
        public volatile string ColumnTemp; 
        public volatile string Pressure;
        public volatile bool StillEmpty = true;
        public volatile bool StillFull = false;
        public volatile bool ElementOn = false;
        public volatile bool VacuumPumpOn = false;
        public volatile bool StillPumpOn = false;
        public volatile bool StillValveOpen = false;
        public volatile bool RVPumpOn = false;
        public volatile bool RVValveOpen = false;



        public Main()
        {
            InitializeComponent();
            lblStatus.Text = "Initializing";

            //Datatable for statistics and calculating when to turn the element off
            DataTable StillStats = Statistics.InitializeTable();

            Statistics.CreateHeader(DateTime.Now, DateTime.Now, true);


            //Hardware Addresses and other settings
            var properties = new SystemProperties();

            //Instanciate the periphrial class and start up the arduino
            var Periphrials = new Periphrials();
            var driver = Periphrials.InitializeArduinoDriver();
            
            //Dispatcher to accept commands from the various background workers
            Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

            //Background worker to monitor all sensors and switches on the still and keep global variables and the UI updated
            //The idea here is to imtermittently check all variables and write to a local variable in memory to minimize commands sent to the arduino
            //This is also convienent as it minimizes the amount of long of code required to message the arduino in the control loop
            StillMonitor = new BackgroundWorker();
            StillMonitor.WorkerSupportsCancellation = true;
            StillMonitor.DoWork += new DoWorkEventHandler((state, args) =>

            {
                do
                {
                    if (Run != true)
                    { break; }
                    //Check Temperature
                    MainDispatcher.Invoke(new Action(() => { ColumnTemp = Convert.ToInt64((((Convert.ToDouble(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString()) * (5.0 / 1023.0)) - 1.25) / 0.005)).ToString(); }));
                    MainDispatcher.Invoke(new Action(() => { lblTemp1.Text = ColumnTemp; }));
                    System.Threading.Thread.Sleep(250);


                    //Check the low level switch -- a value of low means the lower still switch is open
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low")
                        { StillEmpty = true; }
                        else
                        { StillEmpty = false; }
                    }));
                    System.Threading.Thread.Sleep(250);

                    //Check the high level switch -- a value of high means the upper still switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(properties.StillHighSwitch)).PinValue.ToString() == "High")
                        { StillFull = true; }
                        else
                        { StillFull = false; }
                    }));
                    System.Threading.Thread.Sleep(250);

                    //Check the pressure
                    MainDispatcher.Invoke(new Action(() => { Pressure = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString(); }));
                    MainDispatcher.Invoke(new Action(() => { lblPressure.Text = Pressure; }));

                } while (true);
            });

       


            StillController = new BackgroundWorker();
            StillController.WorkerSupportsCancellation = true;
            StillController.DoWork += new DoWorkEventHandler((state, args) =>

            {
                do
                {
                    DateTime RunStart = DateTime.Now;

                    //Run unless a stop condition is hit
                    if (Run != true)
                    { break; }

                    //Check to see if the still is full, if not fill it. This ensures there is no product wasted if the previous batch was stopped half way
                    if (StillFull == false  && Phase == 0)
                    {
                        {
                            //Open the inlet valve and turn the inlet pump on
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFillValve, DigitalValue.Low)); }));
                            StillValveOpen = true;
                            //Wait 5 seconds for the valve to open
                            System.Threading.Thread.Sleep(5000);
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFluidPump, DigitalValue.Low)); }));
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Still"; }));
                            StillPumpOn = true;
                                                        
                            //Check once a second to see if the still is full now -- note that StillFull is updated by the monitor worker
                            while (StillFull == false)
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                            //Close the valve and turn off the pump
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFillValve, DigitalValue.High)); }));
                            StillValveOpen = false;
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFluidPump, DigitalValue.High)); }));
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Complete"; }));

                            StillPumpOn = false;

                            //If this line is reached that means the still has liquid in it and is ready to start distilling
                            Phase = 1;
                        }
                    }

                    //Make sure the first loop was passed and the still didnt magically empty itself
                    if (Phase == 1 && StillFull == true)
                    {
                        //Turn on the element and vacuum pump
                        driver.Send(new DigitalWriteRequest(properties.StillElement, DigitalValue.Low));
                        ElementOn = true;
                        PressureRegulator.RunWorkerAsync();
                        MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Distilling"; }));



                        //Set up variables for calculating when to turn off the element
                        int CurrentTemp = Convert.ToInt16(ColumnTemp);
                        int CurrentDelta = 0;
                        int Counter = 0;
                        int Temp1 = 0;
                        int Temp2 = 0;
                        double AverageDelta = 1.0;
                       
                        DataRow row;

                        row = StillStats.NewRow();
                        row["Time"] = DateTime.Now;
                        row["Temperature"] = CurrentTemp;
                        row["TemperatureDelta"] = 0;
                        row["Pressure"] = Convert.ToInt16(Pressure);
                        StillStats.Rows.Add(row);

                        //Get the last written row for collecting temperature rise statistics
                        DataRow LastRow = StillStats.Rows[0];

                        //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained from the given time span
                        DataRow Delta1;
                        DataRow Delta2;

                        //Keep the element on and keep collecting data every 10 seconds until the first plateau is reached then go to the next loop
                        while (StillEmpty == false && AverageDelta >= 0.02)

                        {
                            //Once the element has been on for 10 minutes start checking for the plateau
                            if (Counter < 60)
                            {
                                Counter = Counter + 1;
                            }
                            else
                            {
                                Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                                Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                                Temp1 = Delta1.Field<Int32>("Temperature");
                                Temp2 = Delta2.Field<Int32>("Temperature");
                                AverageDelta = ((Temp2 - Temp1) / Temp2);
                            }


                            System.Threading.Thread.Sleep(10000);
                            CurrentTemp = Convert.ToInt32(ColumnTemp);
                            CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = CurrentDelta;
                            row["Pressure"] = Convert.ToInt16(Pressure);
                            StillStats.Rows.Add(row);
                            LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                        }

                        //Once the first plateau is reached allowing for 2% variance in temperature wait until it ends 
                        //Or end the batch if the saftey limit switch is triggered
                        while (StillEmpty == false && AverageDelta <= 0.02)

                        {
                            Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                            Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                            Temp1 = Delta1.Field<Int32>("Temperature");
                            Temp2 = Delta2.Field<Int32>("Temperature");
                            AverageDelta = ((Temp2 - Temp1) / Temp2);

                            System.Threading.Thread.Sleep(10000);
                            CurrentTemp = Convert.ToInt32(ColumnTemp);
                            CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = CurrentDelta;
                            row["Pressure"] = Convert.ToInt16(Pressure);
                            StillStats.Rows.Add(row);
                            LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                        }
                        
                        //Batch complete!
                        driver.Send(new DigitalWriteRequest(properties.StillElement, DigitalValue.High));
                        ElementOn = false;
                        Phase = 2;
                    }
                    //If the run completed without issue then calculate the header info and write the data to a local sqldb
                    //Note that this must be done sequentially as the records must relate to a header record
                    //Once the table is succesfully written set the phase back to 0 and start another run
                    if (Phase == 2)
                    {
                        Statistics.CreateHeader(RunStart, DateTime.Now, true);
                        Statistics.SaveRun(StillStats);
                        Phase = 0;
                    }
                } while (true);
            });

            //Turns the pump on and off to maintain a pressure range
            PressureRegulator = new BackgroundWorker();
            PressureRegulator.WorkerSupportsCancellation = true;
            PressureRegulator.DoWork += new DoWorkEventHandler((state, args) =>
            {
                do
                {
                    //Make sure the pump is off
                    MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.High)); }));
                    VacuumPumpOn = false;

                    if (Run != true)
                    { break; }

                    System.Threading.Thread.Sleep(1000);
                    if (Convert.ToDouble(Pressure) > properties.TargetPressure)
                    {
                        //Turn the vacuum pump on
                        MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.Low)); }));
                        VacuumPumpOn = true;

                        //Refresh the pressure has changed every second -- Note that the pressure is set in the still monitor background worker
                        do
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        while (Convert.ToDouble(Pressure) > (properties.TargetPressure - properties.TgtPresHysteresisBuffer));

                        //Once the pressure has reached its target turn the pump off
                        MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.High)); }));
                        VacuumPumpOn = false;

                    }
                } while (true);
            });

            //Start the workers and pause for 2 seconds to allow for initial values to be collected
            StillMonitor.RunWorkerAsync();
            System.Threading.Thread.Sleep(2000);
            StillController.RunWorkerAsync();
            

       
        }
        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
