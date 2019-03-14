using System;
using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using System.Windows.Forms;
using System.Data;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing;

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
        public volatile bool RVFull = true;
        public volatile bool RVEmpty = false;
        public volatile float RVWeight = 0;


        public Main()
        {
            InitializeComponent();
            StillLoop();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            //This section sets up the Chart

            var properties = new SystemProperties();
            ChartArea chartArea = new ChartArea();
            chartRun.ChartAreas[0].Axes[0].MajorGrid.Enabled = false;//x axis
            chartRun.ChartAreas[0].AxisY.LabelStyle.Format = "####0°" + ((properties.Units == "Metric") ? "C" : "F"); //Set the Y axis to use up to 4 digits and if there is no digit set a 0 then tack a degree and a "C" on the end -- documentation available here https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings 
            chartRun.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number; //Dont know why it has to be a "DateTime" interval type but it works

            //chart1.ChartAreas[0].AxisX.ScaleView.Zoom(0, 13);
            chartRun.ChartAreas[0].AxisX.LabelStyle.Format = "mm:ss";
            chartRun.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartRun.ChartAreas[0].Axes[1].MajorGrid.Enabled = true;//y axis
            chartRun.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartRun.ChartAreas[0].AxisY2.LabelStyle.Format = "###0.0##" + ((properties.Units == "Metric") ? "kPa" : "PSI");
            chartRun.ChartAreas[0].AxisY2.IntervalAutoMode = IntervalAutoMode.FixedCount;
            //chartRun.ChartAreas[0].AxisY2.IntervalType = System.Windows.Forms.DataVisualization.Charting.IntervalType.Number;



            Series temperatureseries = new Series("Temperature");
            temperatureseries.BorderWidth = 2;
            temperatureseries.Color = Color.Red;
            temperatureseries.XValueMember = "Time";
            temperatureseries.YValueMembers = "Temperature";
            chartRun.Series[0] = temperatureseries;
            chartRun.Series[0].ChartType = SeriesChartType.Line;
            chartRun.Series[0].XValueType = ChartValueType.DateTime;


            Series pressureseries = new Series("Pressure");
            pressureseries.BorderWidth = 2;
            pressureseries.Color = Color.Blue;
            pressureseries.XValueMember = "Time";
            pressureseries.YValueMembers = "Pressure";
            chartRun.Series.Add(pressureseries);
            chartRun.Series[1].ChartType = SeriesChartType.Line;
            chartRun.Series[1].YValueType = ChartValueType.Single;
            chartRun.Series[1].XValueType = ChartValueType.DateTime;
            chartRun.Series[1].YAxisType = AxisType.Secondary;

        }

        public void StillLoop()
        {
            //Hardware Addresses and other settings
            var properties = new SystemProperties();

            //Datatable for statistics and calculating when to turn the element off
            DataTable StillStats = Statistics.InitializeTable();

            chartRun.DataSource = StillStats;

       


            //Instanciate the periphrial class and start up the arduino
            var Periphrials = new Periphrials();
            ArduinoDriver.ArduinoDriver driver = Periphrials.InitializeArduinoDriver();

            //int Value = 1;
            //while (true)
            //{
            //    while (Value < 250)
            //    { 
            //    if (Value < 255)
            //    { Value = Value + 5; }
            //    else
            //    { Value = 1; }
            //     driver.Send(new AnalogWriteRequest(properties.FanController2, Convert.ToByte(Value)));
            //    }
            //    while (Value > 6)
            //    {
            //        if (Value > 1)
            //        { Value = Value - 5; }
            //        else
            //        { Value = 1; }
            //        driver.Send(new AnalogWriteRequest(properties.FanController2, Convert.ToByte(Value)));
            //    }


            //}

            if (driver == null)
            {
                lblStatus.Text = "No controller found";
                btnRescan.Visible = true;
            }
            else
            {
                btnRescan.Visible = false;
                lblStatus.Text = "Starting";
            }

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
                    if (Run != true || driver == null) //The periphrials class returns null if no arduin is found on any of the com ports
                    { break; }
                    System.Threading.Thread.Sleep(1000);
                    //Check Temperature
                    bool success = false;
                    while (success == false)
                    {
                        try
                        {
                            MainDispatcher.Invoke(new Action(() => { ColumnTemp = Convert.ToInt64((((Convert.ToDouble(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString()) * (5.0 / 1023.0)) - 1.25) / 0.005)).ToString(); }));
                            success = true;
                        }
                        catch { MainDispatcher.Invoke(new Action(() => { driver = Periphrials.InitializeArduinoDriver(); })); }
                    }
                    MainDispatcher.Invoke(new Action(() => { lblTemp1.Text = ColumnTemp + ((properties.Units == "Metric") ? "°C" : "°F"); }));

                    //Check the low level switch -- a value of low means the lower still switch is open
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue == DigitalValue.High)
                        { StillEmpty = true; }
                        else
                        { StillEmpty = false; }
                    }));

                    //Check the high level switch -- a value of high means the upper still switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(properties.StillHighSwitch)).PinValue == DigitalValue.Low)
                        { StillFull = true; }
                        else
                        { StillFull = false; }
                    }));

                    //Check the recieving vessel low level switch -- a value of high means the upper rv switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(properties.RVEmptySwitch)).PinValue == DigitalValue.High)
                        { RVEmpty = true; }
                        else
                        { RVEmpty = false; }

                    }));

                    //Check the recieving vessel high level switch -- a value of high means the upper rv switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(properties.RVFullSwitch)).PinValue == DigitalValue.High)
                        { RVFull = true; }
                        else
                        { RVFull = false; }

                    }));

                    //Check the pressure (1024 is the resolution of the ADC on the arduino, 45.1 is the approximate pressure range in PSI that the sensor is capable of reading, the -15 makes sure that STP = 0 PSI/kPa)
                    MainDispatcher.Invoke(new Action(() => { Pressure = Math.Round((((Convert.ToDouble(driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString()) / 1024) * 45.1) - 15) * ((properties.Units == "Metric") ? 6.895 : 1), 2).ToString(); }));
                    MainDispatcher.Invoke(new Action(() => { lblPressure.Text = Pressure + ((properties.Units == "Metric") ? "kPa" : "PSI"); }));

                } while (true);
            });

            StillController = new BackgroundWorker();
            StillController.WorkerSupportsCancellation = true;
            StillController.DoWork += new DoWorkEventHandler((state, args) =>

            {
                do
                {
                    try {                     
                    DateTime RunStart = DateTime.Now;
                    StillStats.Clear();
                    //Run unless a stop condition is hit
                    if (Run != true || driver == null)
                    { break; }

                    //Check to see if the still is full, if not fill it. This ensures there is no product wasted if the previous batch was stopped half way
                    if (StillFull == false && Phase < 2)
                    {
                        {
                            //Open the inlet valve and turn the inlet pump on
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFillValve, DigitalValue.High)); }));
                            StillValveOpen = true;
                            //Wait 5 seconds for the valve to open
                            System.Threading.Thread.Sleep(3000);
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFluidPump, DigitalValue.High)); }));
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Still"; }));
                            StillPumpOn = true;

                            
                                //Check once a second to see if the still is full now -- note that StillFull is updated by the monitor worker
                                while (StillFull == false)
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                            //Close the valve and turn off the pump
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFillValve, DigitalValue.Low)); }));
                            StillValveOpen = false;
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillFluidPump, DigitalValue.Low)); }));
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Complete"; }));

                            StillPumpOn = false;

                            //If this line is reached that means the still has liquid in it and is ready to start distilling
                            Phase = 1;
                        }
                    }

                    //Make sure the first loop was passed and the still didnt magically empty itself
                    if (Phase < 2 && StillFull == true)
                    {
                        //Turn on the element and vacuum pump
                        driver.Send(new DigitalWriteRequest(properties.StillElement, DigitalValue.High));
                        ElementOn = true;
                        PressureRegulator.RunWorkerAsync();
                        MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Heating"; }));



                        //Set up variables for calculating when to turn off the element
                        int CurrentTemp = Convert.ToInt16(ColumnTemp);
                        int CurrentDelta = 0;
                        int Counter = 0;
                        double PlateauTemp = 0;
                        string StartTempRaw = null;
                        while (StartTempRaw == null)
                        {
                            try { StartTempRaw = driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString(); } catch { }
                        }
                        double StartTemp = Convert.ToInt64((((Convert.ToDouble(StartTempRaw) * (5.0 / 1023.0)) - 1.25) / 0.005));
                        double Temp1 = 0.0;
                        double Temp2 = 0.0;
                        double AverageDelta = 1.0;
                        double TotalDelta = 0.0;

                        DataRow row;

                        row = StillStats.NewRow();
                        row["Time"] = DateTime.Now;
                        row["Temperature"] = CurrentTemp;
                        row["TemperatureDelta"] = 0;
                        row["Pressure"] = Convert.ToDecimal(Pressure);
                        row["Phase"] = Phase;
                        StillStats.Rows.Add(row);

                        //Get the last written row for collecting temperature rise statistics
                        DataRow LastRow = StillStats.Rows[0];

                        //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained from the given time span
                        DataRow Delta1;
                        DataRow Delta2;

                        //Keep the element on and keep collecting data every 10 seconds until the first plateau is reached then go to the next loop 
                        //note thate the total delta is there incase it takes longer than 10 minutes to start seeing a temperature rise at the sensor
                        while ((StillEmpty == false && AverageDelta >= 0.02) || TotalDelta < 0.6)

                        {
                            //Change this back to 10 seconds
                            System.Threading.Thread.Sleep(250);
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
                                TotalDelta = TotalDelta + AverageDelta;
                            }

                            CurrentTemp = Convert.ToInt32(ColumnTemp);
                            CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = CurrentDelta;
                            row["Pressure"] = Convert.ToDecimal(Pressure);
                            row["Phase"] = Phase;
                            StillStats.Rows.Add(row);
                            LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                            MainDispatcher.Invoke(new Action(() => { chartRun.DataBind(); }));
                        }

                        //Once the first plateau is reached allowing for a 4 degree change at the most
                        //or end the batch if the saftey limit switch is triggered also reset the Delta counters so the next step is not skipped
                        Phase = 2;
                        AverageDelta = 0;
                        TotalDelta = 0;
                        PlateauTemp = LastRow.Field<Int32>("Temperature");

                        MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Distilling"; }));
                        while (StillEmpty == false && (Temp2 - PlateauTemp) < 5 && RVFull == false)

                        {
                            Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                            Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                            Temp1 = Delta1.Field<Int32>("Temperature");
                            Temp2 = Delta2.Field<Int32>("Temperature");
                            AverageDelta = Math.Abs(((Temp2 - Temp1) / Temp2));
                            //Change this back to 10 seconds
                            System.Threading.Thread.Sleep(250);
                            CurrentTemp = Convert.ToInt32(ColumnTemp);
                            CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = CurrentDelta;
                            row["Pressure"] = Convert.ToDecimal(Pressure);
                            row["Phase"] = Phase;
                            StillStats.Rows.Add(row);
                            LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                            MainDispatcher.Invoke(new Action(() => { chartRun.DataBind(); }));
                        }

                        //Batch complete!
                        MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Batch Complete, Saving Run Data"; }));
                        MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillElement, DigitalValue.Low)); }));
                        ElementOn = false;
                        Phase = 3;
                    }
                        //If the run completed without issue then calculate the header info and write the data to a local sqldb
                        //Note that this must be done sequentially as the records must relate to a header record
                        //Once the table is succesfully written set the phase back to 0 and start another run
                        if (Phase == 3)
                        {
                            Statistics.CreateHeader(RunStart, DateTime.Now, true, properties.Units);
                            Statistics.SaveRun(StillStats, RunStart);

                            PressureRegulator.CancelAsync(); //Turn off the vacuum pump synchronously with the main thread
                            while (PressureRegulator.IsBusy == true || PressureRegulator.CancellationPending == true)
                            { System.Threading.Thread.Sleep(100); }


                            //Fill the system with air so it is at a neutral pressure before pumping any fluids -- note that the system will pull air from the drain valve  
                            //since it eventually vents somewhere that is at atmospheric pressure
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.High)); }));
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Draining Still"; }));
                            while (StillEmpty == false || Convert.ToDouble(Pressure) <= -0.2)
                            { System.Threading.Thread.Sleep(1500); }
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.Low)); }));
                            System.Threading.Thread.Sleep(3000);


                            //Make sure that the switches are working then pump the Recieving vessels contents into a storage tank so the next run can begin
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Draining Distillate"; }));
                            MainDispatcher.Invoke(new Action(() => { RVEmpty = (driver.Send(new DigitalReadRequest(properties.RVEmptySwitch)).PinValue == DigitalValue.Low) ? true : false; }));
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.RVDrainValve, DigitalValue.High)); }));
                            System.Threading.Thread.Sleep(3000); //3 second delay so the valve has time to open
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.RVFluidPump, DigitalValue.High)); }));
                            while (RVEmpty == false)
                            {
                                //MainDispatcher.Invoke(new Action(() => { RVEmpty = (driver.Send(new DigitalReadRequest(properties.RVEmptySwitch)).PinValue == DigitalValue.Low) ? true : false; }));
                                System.Threading.Thread.Sleep(1500);
                            }
                            //Turn off the pump and shut the valves and give them 3 seconds to close
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.RVFluidPump, DigitalValue.Low)); }));
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.RVDrainValve, DigitalValue.Low)); }));




                            Phase = 0;

                        }
                    }
                    //The arduino driver reference has a tendency to randomly throw null reference exceptions, for now I will handle it by just restarting the arduino
                    //the code is designed to pick up where it left off if it errors since the phase is still in memory
                    catch (NullReferenceException)
                    {
                        MainDispatcher.Invoke(new Action(() => { driver = Periphrials.InitializeArduinoDriver(); }));
                    }
                } while (true);
            });

            //Turns the pump on and off to maintain a pressure range
            PressureRegulator = new BackgroundWorker();
            PressureRegulator.WorkerSupportsCancellation = true;
            PressureRegulator.DoWork += new DoWorkEventHandler((state, args) =>
            {
                //Make sure the pump is off
                MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.Low)); }));
                VacuumPumpOn = false;
                do
                {
                    try
                    {
                        if (Run != true || PressureRegulator.CancellationPending == true)
                        { break; }

                        System.Threading.Thread.Sleep(1000);
                        if (Convert.ToDouble(Pressure) > properties.TargetPressure)
                        {
                            //Turn the vacuum pump on
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.High)); }));
                            VacuumPumpOn = true;

                            //Refresh the pressure has changed every second -- Note that the pressure is set in the still monitor background worker
                            do
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                            while (Convert.ToDouble(Pressure) > (properties.TargetPressure - properties.TgtPresHysteresisBuffer) && PressureRegulator.CancellationPending == false);

                            //Once the pressure has reached its target turn the pump off
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.Low)); }));
                            VacuumPumpOn = false;

                        }
                    }
                    catch { MainDispatcher.Invoke(new Action(() => { driver = Periphrials.InitializeArduinoDriver(); })); }
                } while (true);
            });

            //Start the workers and pause for 2 seconds to allow for initial values to be collected
            StillMonitor.RunWorkerAsync();
            System.Threading.Thread.Sleep(2000);
            StillController.RunWorkerAsync();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {

        }
    }
}
