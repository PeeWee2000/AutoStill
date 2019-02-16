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




        public Main()
        {
            InitializeComponent();

            //Datatable for statistics and calculating when to turn the element off
            DataTable StillStats = Statistics.InitializeTable();

            //Hardware Addresses and other settings
            var properties = new SystemProperties();

            //Instanciate the periphrial class and start up the arduino
            var Periphrials = new Periphrials();
            var driver = Periphrials.InitializeArduinoDriver();
            
            //Dispatcher to accept commands from the various background workers
            Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

            //Background worker to monitor all sensors and switches on the still and keep global variables and the UI updated
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

                    //Check the low level switch
                    if (driver.Send(new DigitalReadRequest(properties.StillHighSwitch)).PinValue.ToString() == "Low")
                    { StillFull = true; }
                    else
                    { StillFull = false; }
                    System.Threading.Thread.Sleep(250);

                    //Check the low level switch
                    if (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low")
                    { StillEmpty = true; }
                    else
                    { StillEmpty = false; }
                    System.Threading.Thread.Sleep(250);

                    //Check the pressure
                    MainDispatcher.Invoke(new Action(() => { Pressure = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString(); }));
                    MainDispatcher.Invoke(new Action(() => { lblPressure.Text = Pressure; }));

                } while (true);
            });

            //Turns the pump on and off to maintain a pressure range
            PressureRegulator = new BackgroundWorker();
            PressureRegulator.WorkerSupportsCancellation = true;
            PressureRegulator.DoWork += new DoWorkEventHandler((state, args) =>
            {
                do
                {
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


            StillController = new BackgroundWorker();
            StillController.WorkerSupportsCancellation = true;
            StillController.DoWork += new DoWorkEventHandler((state, args) =>

            {
                do
                {
                    //Run unless a stop condition is hit
                    if (Run != true)
                    { break; }

                        //Check to see if there is any liquid in the still, if so check the full switch to see if the liquid is tails or a fresh batch
                        //(the circuit is closed when the still is empty)
                        if (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low")
                        {
                            if (driver.Send(new DigitalReadRequest(properties.StillHighSwitch)).PinValue.ToString() == "High")
                            {
                                while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low")
                                {
                                    driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.High));
                                    System.Threading.Thread.Sleep(60000);
                                }
                                driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.Low));
                            //The phase integer is used to let the controller background worker know what if loop to enter while looping
                            Phase = 1;
                            }
                        }

                        //Check to see if there is any liquid in the fermentation vessel, if so then check to see that fermentation is complete, if so pump it into the still
                        //The switches are closed (high) when the black parts are touching -- so the switch should fall and be open (low) when the vessel is empty
                        if (Phase == 1 && driver.Send(new DigitalReadRequest(properties.FVEmptySwtich)).PinValue.ToString() == "High" && driver.Send(new DigitalReadRequest(properties.FVCompleteSwitch)).PinValue.ToString() == "Low")
                        {
                            //Start pumping and check to see if the still is full every 15 seconds
                            driver.Send(new DigitalWriteRequest(properties.FVFluidPump, DigitalValue.High));
                            while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "High")
                            {
                                System.Threading.Thread.Sleep(15000);
                            }
                            driver.Send(new DigitalWriteRequest(properties.FVFluidPump, DigitalValue.Low));
                        Phase = 2;
                        }

                    if (Phase == 2)
                    {
                        //    //Turn on the element and vacuum sensor
                        driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.High));
                        driver.Send(new DigitalWriteRequest(properties.StillElement, DigitalValue.High));

                        int CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString());
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
                        row["Pressure"] = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString();
                        StillStats.Rows.Add(row);

                        //Get the last written row for collecting temperature rise statistics
                        DataRow LastRow = StillStats.Rows[0];

                        //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained from the given time span
                        DataRow Delta1;
                        DataRow Delta2;

                        //Keep the element on and keep collecting data every 10 seconds until the first plateau is reached then go to the next loop

                        while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low" && AverageDelta >= 0.02)

                        {
                            //Once the element has been on for 5 minutes start checking for the plateau
                            if (Counter < 30)
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
                            CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString());
                            CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = CurrentDelta;
                            row["Pressure"] = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString();
                            StillStats.Rows.Add(row);
                            LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                        }

                        //Once the first plateau is reached allowing for 2% variance in temperature wait until it ends 
                        //Or end the batch if the saftey limit switch is triggered
                        while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low" && AverageDelta <= 0.02)

                        {
                            Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                            Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                            Temp1 = Delta1.Field<Int32>("Temperature");
                            Temp2 = Delta2.Field<Int32>("Temperature");
                            AverageDelta = ((Temp2 - Temp1) / Temp2);

                            System.Threading.Thread.Sleep(10000);
                            CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString());
                            CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = CurrentDelta;
                            row["Pressure"] = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString();
                            StillStats.Rows.Add(row);
                            LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                        }
                    }
                } while (true);
            });


            //StillController.RunWorkerAsync();
            StillMonitor.RunWorkerAsync();
            PressureRegulator.RunWorkerAsync();

       
        }
        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
