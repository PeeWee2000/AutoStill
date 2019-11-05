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
        //private BackgroundWorker ElementController; //Turns the element on and off to prevent overloading of the reflux column and condensor
        private BackgroundWorker PressureRegulator; //Determines when to turn the vaucuum pump on and off
        private BackgroundWorker ElementRegulator; //Determines when to turn the vaucuum pump on and off
        private BackgroundWorker StillController; //Turns the element, pumps and valves on and off
        //private BackgroundWorker FanController1; //Turns the fan set for the reflux column on, off, up and down depending on target temperature and distillation speed
        //private BackgroundWorker FanController2; //Turns the fan set for the condensor on, off, up and down depending on target temperature and distillation speed
        StillStatsEntities Context = new StillStatsEntities();

        public static Variables CurrentState = new Variables(); 
        public static List<RunRecord> CurrentRun = new List<RunRecord>();

        public static int RefreshRate = 1000;

        public Main()
        {
            InitializeComponent();
            StillLoop();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            //Documentation for how this chart works available here https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-numeric-format-strings 
            chartRun.ChartAreas[0].Axes[0].MajorGrid.Enabled = false;//X axis
            chartRun.ChartAreas[0].AxisY.LabelStyle.Format = "####0°" + "C"; //Set the Y axis to use up to 4 digits and if there is no digit set a 0 then tack a degree and a "C" on the end
            chartRun.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number;
            chartRun.ChartAreas[0].AxisX.LabelStyle.Format = "mm:ss";
            chartRun.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
            chartRun.ChartAreas[0].Axes[1].MajorGrid.Enabled = true;//Y axis
            chartRun.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chartRun.ChartAreas[0].AxisY2.LabelStyle.Format = "###0.0##" + "kPa";
            chartRun.ChartAreas[0].AxisY2.IntervalAutoMode = IntervalAutoMode.FixedCount;

            
            Series columntempseries = new Series("Column Temperature");
            columntempseries.BorderWidth = 2;
            columntempseries.Color = Color.Red;
            columntempseries.XValueMember = nameof(RunRecord.rrTime);
            columntempseries.YValueMembers = nameof(RunRecord.rrColumnHeadTemp);
            chartRun.Series[0] = columntempseries;
            chartRun.Series[0].ChartType = SeriesChartType.Line;
            chartRun.Series[0].XValueType = ChartValueType.DateTime;


            Series pressureseries = new Series("Pressure");
            pressureseries.BorderWidth = 2;
            pressureseries.Color = Color.Blue;
            pressureseries.XValueMember = nameof(RunRecord.rrTime);
            pressureseries.YValueMembers = nameof(RunRecord.rrPressure);
            chartRun.Series.Add(pressureseries);
            chartRun.Series[1].ChartType = SeriesChartType.Line;
            chartRun.Series[1].YValueType = ChartValueType.Single;
            chartRun.Series[1].XValueType = ChartValueType.DateTime;
            chartRun.Series[1].YAxisType = AxisType.Secondary;

            //Series stilltempseries = new Series("Still Fluid Temperature");
            //stilltempseries.BorderWidth = 2;
            //stilltempseries.Color = Color.PaleVioletRed;
            //stilltempseries.XValueMember = nameof(RunRecord.rrTime);
            //stilltempseries.YValueMembers = nameof(RunRecord.rrStillTemp);
            //chartRun.Series[2] = stilltempseries;
            //chartRun.Series[2].ChartType = SeriesChartType.Line;
            //chartRun.Series[2].XValueType = ChartValueType.DateTime;
        }



        public void StillLoop()
        {



            //Dispatcher to accept commands from the various background workers
            Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;

            BackGroundWorkers.InitializeDI2008();
            //BackGroundWorkers.InitializeRelayBoard();

            SystemMonitor = BackGroundWorkers.InitializeSystemMonitor(MainDispatcher);
            PressureRegulator = BackGroundWorkers.InitializePressureWorker();
            ElementRegulator = BackGroundWorkers.InitializeElementWorker();
            //FanController1 = BackGroundWorkers.InitializeFanController1(driver, MainDispatcher);
            //FanController2 = BackGroundWorkers.InitializeFanController2(driver, MainDispatcher);

            //chartRun.DataSource = StillStats;


            StillController = new BackgroundWorker();
            StillController.WorkerSupportsCancellation = true;
            StillController.DoWork += new DoWorkEventHandler((state, args) =>

            {
                while (true)
                {
                    var Header = new RunHeader();
                    Header.rhStart = DateTime.Now;
                    Header.rhEnd = DateTime.Now;
                    Header.rhComplete = false;
                    Header.rhAvgPressure = 0;

                    Context.RunHeaders.Add(Header);
                    Context.SaveChanges();

                    CurrentState.RunID = Header.rhID;

                    if (CurrentState.Run != true)
                    { break; }

                    while (CurrentState.ColumnTemp == 0)
                    { Thread.Sleep(250); }

                    FillStill();
                    CurrentState.Phase = 1;

                    HeatUntilPlateau();
                    CurrentState.Phase = 2;

                    Distill();
                    CurrentState.Phase = 3;

                    DrainVessels();

                    Header.rhComplete = true;
                    Header.rhEnd = DateTime.Now;
                    Header.rhAvgPressure = CurrentRun.Select(i => i.rrPressure).Average();
                    Context.RunRecords.AddRange(CurrentRun);

                    Context.SaveChanges();
                    CurrentRun.Clear();
                    //StillStats.Clear();

                    CurrentState.Phase = 0;
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
                rrRHID = CurrentState.RunID,
                rrTime = DateTime.Now,
                rrColumnHeadTemp = CurrentState.ColumnTemp,
                rrPressure = CurrentState.Pressure,
                rrPhase = CurrentState.Phase,
                rrAmperage = CurrentState.SystemAmperage,
                rrRefluxTemp = CurrentState.RefluxTemp,
                rrCondensorTemp = CurrentState.CondensorTemp,
                rrStillTemp = CurrentState.StillFluidTemp
                
            };

            if (CurrentRun.Count < 2)
            { Data.rrTempDelta = 0; }
            else
            { Data.rrTempDelta = CurrentState.ColumnTemp - CurrentRun.LastOrDefault().rrColumnHeadTemp; }

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
                    switch (CurrentState.Phase)
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

                    CurrentState.TheoreticalBoilingPoint = BoilingPointCalculator.Functions.GetWaterBoilingPoint(CurrentState.Pressure * 1000);

                    MainDispatcher.Invoke(new Action(() => {
                        lblPressure.Text = CurrentState.Pressure + "kPa";
                        lblTheoretical.Text =  CurrentState.TheoreticalBoilingPoint + "°C";
                        lblTemp1.Text = CurrentState.ColumnTemp + "°C";
                        lblTemp2.Text = CurrentState.StillFluidTemp + "°C";
                        lblTemp3.Text = CurrentState.RefluxTemp + "°C";
                        lblStillLowSwitch.Text = CurrentState.StillEmpty.ToString();
                        lblStillHighSwitch.Text = CurrentState.StillFull.ToString();
                        lblRVLowSwitch.Text = CurrentState.RVEmpty.ToString();
                        lblRVHighSwtich.Text = CurrentState.RVFull.ToString();
                        lblStatus.Text = Status;


                        chartRun.DataSource = CurrentRun.ToList();
                        chartRun.DataBind();
                        
                    }));
                    Thread.Sleep(RefreshRate);
                }
                
            });
            return UIUpdater;
        }


        public void FillStill()
        {
            PressureRegulator.RunWorkerAsync();

            BackGroundWorkers.EnableRelay(SystemProperties.StillFillValve);
            CurrentState.StillValveOpen = true;
            Thread.Sleep(3000); //Wait 3 seconds for the valve to open

            BackGroundWorkers.EnableRelay(SystemProperties.StillFluidPump);
            CurrentState.StillPumpOn = true;

            while (CurrentState.StillFull == false)
            { Thread.Sleep(RefreshRate); }

            BackGroundWorkers.DisableRelay(SystemProperties.StillFillValve);
            CurrentState.StillValveOpen = false;

            BackGroundWorkers.DisableRelay(SystemProperties.StillFluidPump);
            CurrentState.StillPumpOn = false;
        }

        public void HeatUntilPlateau()
        {
            RecordCurrentState();

            ElementRegulator.RunWorkerAsync();                    

            CurrentState.PlateauTemp = 0;
            decimal StartTemp = CurrentRun.First().rrColumnHeadTemp;
            decimal LastDelta = 0M;
            decimal TotalDelta = 0M;


            //FanController1.RunWorkerAsync();
            //FanController2.RunWorkerAsync();


            while (CurrentState.StillEmpty == false &&  CurrentState.RVFull == false && CurrentState.ColumnTemp <= StartTemp * 1.1M || CurrentRun.Count < 20)
            {
                if (CurrentState.ColumnTemp > StartTemp)
                {
                    RecordCurrentState();
                }
                Thread.Sleep(RefreshRate);
            }

            while ((CurrentState.StillEmpty == false && CurrentState.RVFull == false && LastDelta >= 0.02M) || TotalDelta < 0.25M)
            {
               decimal Temp1 = CurrentRun[CurrentRun.Count - 1].rrColumnHeadTemp;
               decimal Temp2 = CurrentRun[CurrentRun.Count - 20].rrColumnHeadTemp;

                LastDelta = Temp2 != 0 ? ((Temp2 - Temp1) / Temp2) : 0;
                if (Temp2 > Temp1)
                { TotalDelta = Temp2 != 0 ? ((Temp2 - StartTemp) / Temp2) : 0; }

                RecordCurrentState();
                Thread.Sleep(RefreshRate);
            }
        }

        public void Distill()
        {
            CurrentState.PlateauTemp = CurrentRun.Last().rrColumnHeadTemp;
            BackGroundWorkers.EnableRelay(SystemProperties.CoolantPump);


            //Once the first plateau is reached allowing for a 4 degree change at the most
            //or end the batch if the saftey limit switch is triggered also reset the Delta counters so the next step is not skipped
            while (CurrentState.StillEmpty == false && CurrentState.RVFull == false && (CurrentRun.Last().rrColumnHeadTemp - CurrentState.PlateauTemp) < 5 )
            {
                decimal Temp1 = CurrentRun.Last().rrColumnHeadTemp;
                decimal Temp2 = CurrentRun[CurrentRun.Count - 20].rrColumnHeadTemp;

                RecordCurrentState();
                Thread.Sleep(RefreshRate);
            }

            BackGroundWorkers.DisableRelay(SystemProperties.StillElement);
            CurrentState.ElementOn = false;
        }

        public void DrainVessels()
        {
            PressureRegulator.CancelAsync();
            ElementRegulator.CancelAsync();
            //FanController1.CancelAsync();
            //FanController2.CancelAsync();
            //while (PressureRegulator.CancellationPending == true || FanController1.CancellationPending == true || FanController2.CancellationPending == true)
            while (PressureRegulator.CancellationPending == true)
            { Thread.Sleep(100); }


            //Fill the system with air so it is at a neutral pressure before pumping any fluids -- note that the system will pull air from the drain valve  
            //since it eventually vents somewhere that is at atmospheric pressure
            BackGroundWorkers.EnableRelay(SystemProperties.StillDrainValve);

            while (CurrentState.StillEmpty == false ||CurrentState.Pressure <= -0.2M)
            { Thread.Sleep(RefreshRate); }

            BackGroundWorkers.DisableRelay(SystemProperties.StillDrainValve);
            Thread.Sleep(3000);

            //Make sure that the switches are working then pump the Recieving vessels contents into a storage tank so the next run can begin
            BackGroundWorkers.EnableRelay(SystemProperties.RVDrainValve);

            Thread.Sleep(3000); //3 second delay so the valve has time to open
            BackGroundWorkers.EnableRelay(SystemProperties.RVFluidPump);

            while (CurrentState.RVEmpty == false)
            {
                Thread.Sleep(RefreshRate);
            }
            BackGroundWorkers.DisableRelay(SystemProperties.RVFluidPump);
            BackGroundWorkers.DisableRelay(SystemProperties.RVDrainValve);
        }

            //public static DataRow ConvertEntityToRow(RunRecord RunRecord)
        //{
        //    DataRow row = StillStats.NewRow();

        //    row["Time"] = RunRecord.rrTime;
        //    row["Temperature"] = RunRecord.rrColumnHeadTemp;
        //    row["Pressure"] = RunRecord.rrPressure;
        //    row["Amperage"] = RunRecord.rrAmperage;
        //    row["RefluxTemperature"] = RunRecord.rrRefluxTemperature;
        //    row["CondensorTemperature"] = RunRecord.rrCondensorTemperature;

        //    return row;
        //}
    }
}
