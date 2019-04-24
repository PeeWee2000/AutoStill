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
        private BackgroundWorker SystemMonitor;  //Reads all the sensors and switches
        private BackgroundWorker UIUpdater;  //Updates the UI to reflect current sensor values
        private BackgroundWorker ElementController; //Turns the element on and off to prevent overloading of the reflux column and condensor
        private BackgroundWorker PressureRegulator; //Determines when to turn the vaucuum pump on and off
        private BackgroundWorker StillController; //Turns the element, pumps and valves on and off
        private BackgroundWorker FanController1; //Turns the fan set for the reflux column on, off, up and down depending on target temperature and distillation speed
        private BackgroundWorker FanController2; //Turns the fan set for the condensor on, off, up and down depending on target temperature and distillation speed

        //Note that after implementing these variables this way I found an article statin that using volatiles is not best practice but for the way this application is set up it isn't a show stopper
        //since it isnt critical that I get the latest values and in practice the threads will only be updating values every couple of seconds rather than instantaneously
        //https://stackoverflow.com/questions/72275/when-should-the-volatile-keyword-be-used-in-c
        public static volatile bool Run = true; //Used to shut down or start the whole process
        public static volatile int Phase = -1; //Used to control the main still control background worker and report progress -1 = initializing, 0 = filling still and checking values, 1 = heating / vacuuming, 2 = distilling, 3 = draining

        //Varaiables written to and read by all the various loops -- Assume the still is empty and all periphrials are off when starting up
       
        public static volatile string ColumnTemp; 
        public static volatile string Pressure;
        public static volatile int RefluxTemp;
        public static volatile int CondensorTemp;
        public static volatile int ElementAmperage;
        public static volatile bool StillEmpty = true;
        public static volatile bool StillFull = false;
        public static volatile bool ElementOn = false;
        public static volatile bool VacuumPumpOn = false;
        public static volatile bool StillPumpOn = false;
        public static volatile bool StillValveOpen = false;
        public static volatile bool RVPumpOn = false;
        public static volatile bool RVValveOpen = false;
        public static volatile bool RVFull = true;
        public static volatile bool RVEmpty = false;
        public static volatile float RVWeight = 0;
        public static volatile int PlateauTemp;
        
        public Main()
        {

            //var waef = new Forms.Settings();
            //waef.Show();
            InitializeComponent();
            StillLoop();
        }
        private void Main_Load(object sender, EventArgs e)
        {

            //This section sets up the Chart
            //Documentation for how this chart works available here https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings 
            ChartArea chartArea = new ChartArea();
            chartRun.ChartAreas[0].Axes[0].MajorGrid.Enabled = false;//x axis
            chartRun.ChartAreas[0].AxisY.LabelStyle.Format = "####0°" + ((SystemProperties.Units == "Metric") ? "C" : "F"); //Set the Y axis to use up to 4 digits and if there is no digit set a 0 then tack a degree and a "C" on the end
            chartRun.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number; //Dont know why it has to be a "DateTime" interval type but it works
            chartRun.ChartAreas[0].AxisX.LabelStyle.Format = "mm:ss";
            chartRun.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartRun.ChartAreas[0].Axes[1].MajorGrid.Enabled = true;//y axis
            chartRun.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartRun.ChartAreas[0].AxisY2.LabelStyle.Format = "###0.0##" + ((SystemProperties.Units == "Metric") ? "kPa" : "PSI");
            chartRun.ChartAreas[0].AxisY2.IntervalAutoMode = IntervalAutoMode.FixedCount;


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

            //Dispatcher to accept commands from the various background workers
            Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

            //Instanciate the periphrial class and start up the arduino
            ArduinoDriver.ArduinoDriver driver = Periphrials.InitializeArduinoDriver();

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

            //Declare the background workers
            SystemMonitor = BackGroundWorkers.InitializeSystemMonitor(driver, MainDispatcher);
            PressureRegulator = BackGroundWorkers.InitializePressureWorker(driver, MainDispatcher);
            //StillController;
            FanController1 = BackGroundWorkers.InitializeFanController1(driver, MainDispatcher);
            FanController2 = BackGroundWorkers.InitializeFanController2(driver, MainDispatcher);
            
            //Datatable for statistics and calculating when to turn the element off
            DataTable StillStats = Statistics.InitializeTable();
            chartRun.DataSource = StillStats;


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

                        while (Phase == -1)//Wait for initial values to be collected before starting
                        { System.Threading.Thread.Sleep(250); }

                        //Check to see if the still is full, if not fill it. This ensures there is no product wasted if the previous batch was stopped half way
                        if (StillFull == false && Phase < 2)
                        {
                            {
                                //Open the inlet valve and turn the inlet pump on
                                MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOn(driver, SystemProperties.StillFillValve); }));
                                StillValveOpen = true;
                                //Wait 5 seconds for the valve to open
                                System.Threading.Thread.Sleep(3000);
                                MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOn(driver, SystemProperties.StillFluidPump); }));
                                MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Still"; }));
                                StillPumpOn = true;

                            
                                    //Check once a second to see if the still is full now -- note that StillFull is updated by the monitor worker
                                    while (StillFull == false)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                }
                                //Close the valve and turn off the pump
                                MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOff(driver, SystemProperties.StillFillValve); }));
                                StillValveOpen = false;
                                MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOff(driver, SystemProperties.StillFluidPump); }));
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
                            DriverFunctions.TurnOn(driver, SystemProperties.StillElement);
                            ElementOn = true;
                            PressureRegulator.RunWorkerAsync();
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Heating"; }));



                            //Set up variables for calculating when to turn off the element
                            int CurrentTemp = Convert.ToInt16(ColumnTemp);
                            int CurrentDelta = 0;
                            int Counter = 0;
                            PlateauTemp = 0;
                            string StartTempRaw = null;
                            while (StartTempRaw == null)
                            {
                                try { StartTempRaw = driver.Send(new AnalogReadRequest(SystemProperties.SensorColumnTemp)).PinValue.ToString(); } catch { }
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
                            row["Amperage"] = ElementAmperage;
                            row["RefluxTemperature"] = RefluxTemp;
                            row["CondensorTemperature"] = CondensorTemp;
                            StillStats.Rows.Add(row);

                            //Get the last written row for collecting temperature rise statistics
                            DataRow LastRow = StillStats.Rows[0];

                            //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained from the given time span
                            DataRow Delta1;
                            DataRow Delta2;

                            //Start both fan controllers to maintain the temperature of the coolant in Reflux column and the Condensor
                            //Note that these are started here because they are auto-regulating and will shut off by themselves when not necessary
                            //FanController1.RunWorkerAsync();
                            //FanController2.RunWorkerAsync();

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
                                row["Amperage"] = ElementAmperage;
                                row["RefluxTemperature"] = RefluxTemp;
                                row["CondensorTemperature"] = CondensorTemp;
                                StillStats.Rows.Add(row);
                                LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                                MainDispatcher.Invoke(new Action(() => { chartRun.DataBind(); }));
                            }

                            //Prep variables related to the distillation phase and start the fan controller for the condensor
                            Phase = 2;
                            AverageDelta = 0;
                            TotalDelta = 0;
                            PlateauTemp = LastRow.Field<Int32>("Temperature");
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Distilling"; }));


                                //Once the first plateau is reached allowing for a 4 degree change at the most
                                //or end the batch if the saftey limit switch is triggered also reset the Delta counters so the next step is not skipped
                            while (StillEmpty == false && (Temp2 - PlateauTemp) < 5 && RVFull == false)
                            {
                                Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                                Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                                Temp1 = Delta1.Field<Int32>("Temperature");
                                Temp2 = Delta2.Field<Int32>("Temperature");
                                AverageDelta = Math.Abs(((Temp2 - Temp1) / Temp2));
                                System.Threading.Thread.Sleep(250); //Change this back to 10 seconds
                                CurrentTemp = Convert.ToInt32(ColumnTemp);
                                CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                                row = StillStats.NewRow();
                                row["Time"] = DateTime.Now;
                                row["Temperature"] = CurrentTemp;
                                row["TemperatureDelta"] = CurrentDelta;
                                row["Pressure"] = Convert.ToDecimal(Pressure);
                                row["Phase"] = Phase;
                                row["Amperage"] = ElementAmperage;
                                row["RefluxTemperature"] = RefluxTemp;
                                row["CondensorTemperature"] = CondensorTemp;
                                StillStats.Rows.Add(row);
                                LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                                MainDispatcher.Invoke(new Action(() => { chartRun.DataBind(); }));
                            }

                            //Batch complete!
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Batch Complete, Saving Run Data"; }));
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOff(driver, SystemProperties.StillElement); }));
                            ElementOn = false;
                            Phase = 3;
                        }
                        //If the run completed without issue then calculate the header info and write the data to a local sqldb
                        //Note that this must be done sequentially as the records must relate to a header record
                        //Once the table is succesfully written set the phase back to 0 and start another run
                        if (Phase == 3)
                        {
                            Statistics.CreateHeader(RunStart, DateTime.Now, true, SystemProperties.Units);
                            Statistics.SaveRun(StillStats, RunStart);

                            //Turn off the vacuum pump and fan controllers synchronously with the main thread
                            PressureRegulator.CancelAsync();
                            //FanController1.CancelAsync();
                            //FanController2.CancelAsync();
                            while (PressureRegulator.CancellationPending == true || FanController1.CancellationPending == true || FanController2.CancellationPending == true)
                            { System.Threading.Thread.Sleep(100); }


                            //Fill the system with air so it is at a neutral pressure before pumping any fluids -- note that the system will pull air from the drain valve  
                            //since it eventually vents somewhere that is at atmospheric pressure
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOn(driver, SystemProperties.StillDrainValve); }));
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Draining Still"; }));
                            while (StillEmpty == false || Convert.ToDouble(Pressure) <= -0.2)
                            { System.Threading.Thread.Sleep(1500); }
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOff(driver, SystemProperties.StillDrainValve); }));
                            System.Threading.Thread.Sleep(3000);


                            //Make sure that the switches are working then pump the Recieving vessels contents into a storage tank so the next run can begin
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Draining Distillate"; }));
                            MainDispatcher.Invoke(new Action(() => { RVEmpty = (driver.Send(new DigitalReadRequest(SystemProperties.RVEmptySwitch)).PinValue == DigitalValue.Low) ? true : false; }));
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOn(driver, SystemProperties.RVDrainValve); }));
                            System.Threading.Thread.Sleep(3000); //3 second delay so the valve has time to open
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOn(driver, SystemProperties.RVFluidPump); }));
                            while (RVEmpty == false)
                            {
                                //MainDispatcher.Invoke(new Action(() => { RVEmpty = (driver.Send(new DigitalReadRequest(SystemProperties.RVEmptySwitch)).PinValue == DigitalValue.Low) ? true : false; }));
                                System.Threading.Thread.Sleep(1500);
                            }
                            //Turn off the pump and shut the valves and give them 3 seconds to close
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOff(driver, SystemProperties.RVFluidPump); }));
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.TurnOn(driver, SystemProperties.RVDrainValve); }));

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

            UIUpdater = new BackgroundWorker();
            UIUpdater.WorkerSupportsCancellation = true;
            UIUpdater.DoWork += new DoWorkEventHandler((state, args) =>
            {
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    MainDispatcher.Invoke(new Action(() => { lblPressure.Text = Main.Pressure + ((SystemProperties.Units == "Metric") ? "kPa" : "PSI"); }));
                    MainDispatcher.Invoke(new Action(() => { lblTemp1.Text = Main.ColumnTemp + ((SystemProperties.Units == "Metric") ? "°C" : "°F"); }));
                }
                while (true);
            });

            //Start the workers and pause for 2 seconds to allow for initial values to be collected
            SystemMonitor.RunWorkerAsync();
            System.Threading.Thread.Sleep(2000);
            StillController.RunWorkerAsync();
            UIUpdater.RunWorkerAsync();
        }

        //TODO Put this into a background worker that only focuses on the UI

        private void btnScan_Click(object sender, EventArgs e)
        {
            StillLoop();
        }

        private void PinSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var waef = new Forms.Settings();
            waef.Show();
        }
    }
}
