using AutoStillDotNet.Classes;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;

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

        public static bool Run = true; //Used to shut down or start the whole process
        public static int Phase = -1; //Used to control the main still control background worker and report progress -1 = initializing, 0 = filling still and checking values, 1 = heating / vacuuming, 2 = distilling, 3 = draining
        public static int RefreshRate = 250; //Refresh rate of data collection in milliseconds
        //Varaiables written to and read by all the various loops -- Assume the still is empty and all periphrials are off when starting up
       
        public static Variables CurrentState = new Variables();
        public static double ColumnTemp; 
        public static decimal Pressure;
        public static decimal? RefluxTemp;
        public static decimal? CondensorTemp;
        public static decimal? SystemAmperage;
        public static bool StillEmpty = true;
        public static bool StillFull = false;
        public static bool ElementOn = false;
        public static bool VacuumPumpOn = false;
        public static bool StillPumpOn = false;
        public static bool StillValveOpen = false;
        public static bool RVPumpOn = false;
        public static bool RVValveOpen = false;
        public static bool RVFull = true;
        public static bool RVEmpty = false;
        public static double RVWeight = 0;
        public static double PlateauTemp;
        


        public Main()
        {
            InitializeComponent();
            StillLoop();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            //Documentation for how this chart works available here https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings 
            ChartArea chartArea = new ChartArea();
            chartRun.ChartAreas[0].Axes[0].MajorGrid.Enabled = false;//X axis
            chartRun.ChartAreas[0].AxisY.LabelStyle.Format = "####0°" + (SystemProperties.Units == "Metric" ? "C" : "F"); //Set the Y axis to use up to 4 digits and if there is no digit set a 0 then tack a degree and a "C" on the end
            chartRun.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number;
            chartRun.ChartAreas[0].AxisX.LabelStyle.Format = "mm:ss";
            chartRun.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartRun.ChartAreas[0].Axes[1].MajorGrid.Enabled = true;//Y axis
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

            BackGroundWorkers.InitializeDI2008();
            //BackGroundWorkers.InitializeRelayBoard();

            SystemMonitor = BackGroundWorkers.InitializeSystemMonitor(MainDispatcher);
            PressureRegulator = BackGroundWorkers.InitializePressureWorker(MainDispatcher);
            //FanController1 = BackGroundWorkers.InitializeFanController1(driver, MainDispatcher);
            //FanController2 = BackGroundWorkers.InitializeFanController2(driver, MainDispatcher);
            
            DataTable StillStats = Statistics.InitializeTable();
            chartRun.DataSource = StillStats;

            StillController = new BackgroundWorker();
            StillController.WorkerSupportsCancellation = true;
            StillController.DoWork += new DoWorkEventHandler((state, args) =>

            {
                while (true)
                {
                    try {                     
                    DateTime RunStart = DateTime.Now;
                    StillStats.Clear();
                    if (Run != true)
                    { break; }

                        while (Phase == -1 || ColumnTemp == 0)
                        { Thread.Sleep(250); }

                        //Check to see if the still is full, if not fill it. This ensures there is no product wasted if the previous batch was stopped half way
                        if (StillFull == false && Phase < 2)
                        {
                            {
                                BackGroundWorkers.EnableRelay(SystemProperties.StillFillValve);
                                StillValveOpen = true;
                                Thread.Sleep(3000); //Wait 3 seconds for the valve to open
                                BackGroundWorkers.EnableRelay(SystemProperties.StillFluidPump);
                                MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Still"; }));
                                StillPumpOn = true;
                            
                                while (StillFull == false)
                                { Thread.Sleep(1000); }

                                BackGroundWorkers.DisableRelay(SystemProperties.StillFillValve);
                                StillValveOpen = false;

                                BackGroundWorkers.DisableRelay(SystemProperties.StillFluidPump);
                                StillPumpOn = false;

                                MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Filling Complete"; }));
                                Phase = 1;
                            }
                        }

                        if (Phase < 2 && StillFull == true && StillEmpty == false)
                        {

                            BackGroundWorkers.EnableRelay(SystemProperties.StillElement);
                            ElementOn = true;

                            PressureRegulator.RunWorkerAsync();
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Heating"; }));


                            //Variables for calculating when to turn off the element
                            int CurrentTemp = Convert.ToInt16(ColumnTemp);
                            int CurrentDelta = 0;
                            PlateauTemp = 0;
                            double StartTemp = ColumnTemp;
                            double Temp1 = 0.0;
                            double Temp2 = 0.0;
                            double LastDelta = 0.0;
                            double TotalDelta = 0.0;


                            var Data = new RunRecord();
                            Data.rrTime = DateTime.Now;
                            Data.rrTemp = CurrentTemp;
                            Data.rrTempDelta = 0;
                            Data.rrPressure = CurrentState.Pressure;
                            Data.rrPhase = Phase;
                            Data.rrAmperage = SystemAmperage;
                            Data.rrRefluxTemperature = RefluxTemp;
                            Data.rrCondensorTemperature = CondensorTemp;

                            var CurrentRun = new List<RunRecord>();
                            CurrentRun.Add(Data);

                            DataRow row;

                            row = StillStats.NewRow();
                            row["Time"] = DateTime.Now;
                            row["Temperature"] = CurrentTemp;
                            row["TemperatureDelta"] = 0;
                            row["Pressure"] = Convert.ToDecimal(Pressure);
                            row["Phase"] = Phase;
                            row["Amperage"] = SystemAmperage;
                            row["RefluxTemperature"] = RefluxTemp;
                            row["CondensorTemperature"] = CondensorTemp;
                            StillStats.Rows.Add(row);

                            //Get the last written row for collecting temperature rise statistics
                            DataRow LastRow = StillStats.Rows[0];

                            //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained from the given time span
                            DataRow Delta1;
                            DataRow Delta2;

                            //FanController1.RunWorkerAsync();
                            //FanController2.RunWorkerAsync();

                            DateTime PhaseStart = DateTime.Now;
                            Thread.Sleep(RefreshRate);

                            while (TotalDelta < .05)
                            {

                            }

                            while ((StillEmpty == false && LastDelta >= 0.02) || TotalDelta < 0.25)
                            {
                                Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                                Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                                Temp1 = Delta1.Field<Int32>("Temperature");
                                Temp2 = Delta2.Field<Int32>("Temperature");
                                LastDelta = Temp2 != 0 ? ((Temp2 - Temp1) / Temp2) : 0;
                                if (Temp2 > Temp1)
                                { TotalDelta = Temp2 != 0 ? ((Temp2 - StartTemp) / Temp2) : 0; }

                                
                                CurrentTemp = Convert.ToInt32(ColumnTemp);
                                CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                                row = StillStats.NewRow();
                                row["Time"] = DateTime.Now;
                                row["Temperature"] = CurrentTemp;
                                row["TemperatureDelta"] = CurrentDelta;
                                row["Pressure"] = Convert.ToDecimal(Pressure);
                                row["Phase"] = Phase;
                                row["Amperage"] = SystemAmperage;
                                row["RefluxTemperature"] = RefluxTemp;
                                row["CondensorTemperature"] = CondensorTemp;
                                StillStats.Rows.Add(row);
                                LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                                MainDispatcher.Invoke(new Action(() => { chartRun.DataBind(); }));
                            }

                            //Prep variables related to the distillation phase and start the fan controller for the condensor
                            Phase = 2;
                            LastDelta = 0;
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
                                LastDelta = Math.Abs(((Temp2 - Temp1) / Temp2));
                                Thread.Sleep(250); //Change this back to 10 seconds
                                CurrentTemp = Convert.ToInt32(ColumnTemp);
                                CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                                row = StillStats.NewRow();
                                row["Time"] = DateTime.Now;
                                row["Temperature"] = CurrentTemp;
                                row["TemperatureDelta"] = CurrentDelta;
                                row["Pressure"] = Convert.ToDecimal(Pressure);
                                row["Phase"] = Phase;
                                row["Amperage"] = SystemAmperage;
                                row["RefluxTemperature"] = RefluxTemp;
                                row["CondensorTemperature"] = CondensorTemp;
                                StillStats.Rows.Add(row);
                                LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                                MainDispatcher.Invoke(new Action(() => { chartRun.DataBind(); }));
                            }

                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Batch Complete, Saving Run Data"; }));
                            BackGroundWorkers.DisableRelay(SystemProperties.StillElement);

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

                            PressureRegulator.CancelAsync();
                            //FanController1.CancelAsync();
                            //FanController2.CancelAsync();
                            //while (PressureRegulator.CancellationPending == true || FanController1.CancellationPending == true || FanController2.CancellationPending == true)
                            while (PressureRegulator.CancellationPending == true )
                            { Thread.Sleep(100); }


                            //Fill the system with air so it is at a neutral pressure before pumping any fluids -- note that the system will pull air from the drain valve  
                            //since it eventually vents somewhere that is at atmospheric pressure
                            BackGroundWorkers.EnableRelay(SystemProperties.StillDrainValve);

                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Draining Still"; }));
                            while (StillEmpty == false || Convert.ToDouble(Pressure) <= -0.2)
                            { Thread.Sleep(1500); }
                            BackGroundWorkers.DisableRelay(SystemProperties.StillDrainValve);

                            Thread.Sleep(3000);


                            //Make sure that the switches are working then pump the Recieving vessels contents into a storage tank so the next run can begin
                            MainDispatcher.Invoke(new Action(() => { lblStatus.Text = "Draining Distillate"; }));
                            BackGroundWorkers.EnableRelay(SystemProperties.RVDrainValve);

                            Thread.Sleep(3000); //3 second delay so the valve has time to open
                            BackGroundWorkers.EnableRelay(SystemProperties.RVFluidPump);

                            while (RVEmpty == false)
                            {
                                Thread.Sleep(1500);
                            }
                            BackGroundWorkers.DisableRelay(SystemProperties.RVFluidPump);
                            BackGroundWorkers.DisableRelay(SystemProperties.RVDrainValve);


                            Phase = 0;
                        }
                    }
                    catch (NullReferenceException)
                    { }
                }
            });

            UIUpdater = new BackgroundWorker();
            UIUpdater.WorkerSupportsCancellation = true;
            UIUpdater.DoWork += new DoWorkEventHandler((state, args) =>
            {
                do
                {
                    Thread.Sleep(1000);
                    MainDispatcher.Invoke(new Action(() => { 
                        lblPressure.Text = Main.Pressure + ((SystemProperties.Units == "Metric") ? "kPa" : "PSI");
                        lblTemp1.Text = Main.ColumnTemp + ((SystemProperties.Units == "Metric") ? "°C" : "°F");
                        lblStillLowSwitch.Text = Main.StillEmpty.ToString();
                        lblStillHighSwitch.Text = Main.StillFull.ToString();
                        lblRVLowSwitch.Text = Main.RVEmpty.ToString();
                        lblRVHighSwtich.Text = Main.RVFull.ToString();

                    }));
                }
                while (true);
            });

            SystemMonitor.RunWorkerAsync();          
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
