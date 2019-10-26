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
        public static List<RunRecord> CurrentRun = new List<RunRecord>();

        public static decimal ColumnTemp;
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
        public static decimal RVWeight = 0;
        public static decimal PlateauTemp;



        public Main()
        {
            InitializeComponent();
            StillLoop();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            //Documentation for how this chart works available here https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings 
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
                    DateTime RunStart = DateTime.Now;

                    if (Run != true)
                    { break; }

                    while (ColumnTemp == 0)
                    { Thread.Sleep(250); }


                    FillStill();
                    Phase = 1;

                    HeatUntilPlateau();
                    Phase = 2;

                    Distill();
                    Phase = 3;

                    DrainVessels();


                    Statistics.CreateHeader(RunStart, DateTime.Now, true, SystemProperties.Units);
                    Statistics.SaveRun(StillStats, RunStart);
                    CurrentRun.Clear();
                    StillStats.Clear();

                    Phase = 0;
                }
            });

            UIUpdater = UIWorker(MainDispatcher);

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

        /// <summary>
        /// Write the current values of all variables to the CurrentRun List
        /// </summary>
        public static RunRecord RecordCurrentState()
        {
            var Data = new RunRecord
            {
                rrTime = DateTime.Now,
                rrTemp = ColumnTemp,
                rrPressure = CurrentState.Pressure,
                rrPhase = Phase,
                rrAmperage = SystemAmperage,
                rrRefluxTemperature = RefluxTemp,
                rrCondensorTemperature = CondensorTemp
            };

            if (CurrentRun.Count < 2)
            { Data.rrTempDelta = 0; }
            else
            { Data.rrTempDelta = ColumnTemp - CurrentRun.LastOrDefault().rrTemp; }

            CurrentRun.Add(Data);
            return Data;
        }


        public BackgroundWorker UIWorker(Dispatcher MainDispatcher)
        {
            var UIUpdater = new BackgroundWorker();
            UIUpdater.WorkerSupportsCancellation = true;
            UIUpdater.DoWork += new DoWorkEventHandler((state, args) =>
            {
                string Status;
                
                while (true)
                {
                    switch (Phase)
                    {
                        case 0:
                            Status = "Filling Still";
                            break;
                        case 1:
                            Status = "Heating";
                            break;
                        case 2:
                            Status = "Distilling";
                            break;
                        case 3:
                            Status = "Draining Vessels";
                            break;
                        default:
                            Status = "Idle";
                            break;
                    }



                    MainDispatcher.Invoke(new Action(() => {
                        lblPressure.Text = Main.Pressure + ((SystemProperties.Units == "Metric") ? "kPa" : "PSI");
                        lblTemp1.Text = Main.ColumnTemp + ((SystemProperties.Units == "Metric") ? "°C" : "°F");
                        lblStillLowSwitch.Text = Main.StillEmpty.ToString();
                        lblStillHighSwitch.Text = Main.StillFull.ToString();
                        lblRVLowSwitch.Text = Main.RVEmpty.ToString();
                        lblRVHighSwtich.Text = Main.RVFull.ToString();
                        lblStatus.Text = Status;
                        chartRun.DataBind();
                    }));
                    Thread.Sleep(RefreshRate);
                }
                
            });
            return UIUpdater;
        }


        public void FillStill()
        {
            BackGroundWorkers.EnableRelay(SystemProperties.StillFillValve);
            StillValveOpen = true;
            Thread.Sleep(3000); //Wait 3 seconds for the valve to open

            BackGroundWorkers.EnableRelay(SystemProperties.StillFluidPump);
            StillPumpOn = true;

            while (StillFull == false)
            { Thread.Sleep(RefreshRate); }

            BackGroundWorkers.DisableRelay(SystemProperties.StillFillValve);
            StillValveOpen = false;

            BackGroundWorkers.DisableRelay(SystemProperties.StillFluidPump);
            StillPumpOn = false;
        }

        public void HeatUntilPlateau()
        {
            RecordCurrentState();

            BackGroundWorkers.EnableRelay(SystemProperties.StillElement);
            ElementOn = true;

            PressureRegulator.RunWorkerAsync();

            PlateauTemp = 0;
            decimal StartTemp = CurrentRun.First().rrTemp;
            decimal LastDelta = 0M;
            decimal TotalDelta = 0M;


            //FanController1.RunWorkerAsync();
            //FanController2.RunWorkerAsync();


            while (ColumnTemp <= StartTemp * 1.1M && CurrentRun.Count < 20)
            {
                if (ColumnTemp > StartTemp)
                {
                    RecordCurrentState();
                }
                Thread.Sleep(RefreshRate);
            }

            while ((StillEmpty == false && LastDelta >= 0.02M) || TotalDelta < 0.25M)
            {
               decimal Temp1 = CurrentRun[CurrentRun.Count].rrTemp;
               decimal Temp2 = CurrentRun[CurrentRun.Count - 20].rrTemp;

                LastDelta = Temp2 != 0 ? ((Temp2 - Temp1) / Temp2) : 0;
                if (Temp2 > Temp1)
                { TotalDelta = Temp2 != 0 ? ((Temp2 - StartTemp) / Temp2) : 0; }

                RecordCurrentState();
            }
        }

        public void Distill()
        {
            PlateauTemp = CurrentRun.Last().rrTemp;

            //Once the first plateau is reached allowing for a 4 degree change at the most
            //or end the batch if the saftey limit switch is triggered also reset the Delta counters so the next step is not skipped
            while (StillEmpty == false && (CurrentRun.Last().rrTemp - PlateauTemp) < 5 && RVFull == false)
            {
                decimal Temp1 = CurrentRun.Last().rrTemp;
                decimal Temp2 = CurrentRun[CurrentRun.Count - 20].rrTemp;
                decimal LastDelta = Math.Abs(((Temp2 - Temp1) / Temp2));


                RecordCurrentState();

                Thread.Sleep(RefreshRate);
            }

            BackGroundWorkers.DisableRelay(SystemProperties.StillElement);

            ElementOn = false;
        }

        public void DrainVessels()
        {
            PressureRegulator.CancelAsync();
            //FanController1.CancelAsync();
            //FanController2.CancelAsync();
            //while (PressureRegulator.CancellationPending == true || FanController1.CancellationPending == true || FanController2.CancellationPending == true)
            while (PressureRegulator.CancellationPending == true)
            { Thread.Sleep(100); }


            //Fill the system with air so it is at a neutral pressure before pumping any fluids -- note that the system will pull air from the drain valve  
            //since it eventually vents somewhere that is at atmospheric pressure
            BackGroundWorkers.EnableRelay(SystemProperties.StillDrainValve);

            while (StillEmpty == false || Convert.ToDouble(Pressure) <= -0.2)
            { Thread.Sleep(RefreshRate); }

            BackGroundWorkers.DisableRelay(SystemProperties.StillDrainValve);
            Thread.Sleep(3000);

            //Make sure that the switches are working then pump the Recieving vessels contents into a storage tank so the next run can begin
            BackGroundWorkers.EnableRelay(SystemProperties.RVDrainValve);

            Thread.Sleep(3000); //3 second delay so the valve has time to open
            BackGroundWorkers.EnableRelay(SystemProperties.RVFluidPump);

            while (RVEmpty == false)
            {
                Thread.Sleep(RefreshRate);
            }
            BackGroundWorkers.DisableRelay(SystemProperties.RVFluidPump);
            BackGroundWorkers.DisableRelay(SystemProperties.RVDrainValve);
        }
    }
}
