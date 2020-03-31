using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace AutoStillWPF
{
    public static class StillController
    {
        private static BackgroundWorker SystemMonitor;  //Reads all the sensors and switches
        private static BackgroundWorker PressureRegulator; //Determines when to turn the vaucuum pump on and off
        private static BackgroundWorker ElementRegulator; //Determines when to turn the vaucuum pump on and off
        private static BackgroundWorker StillRegulator; //Turns the element, pumps and valves on and off

        private static StillStatsEntities Context = new StillStatsEntities();

        public static StateVariables CurrentState = new StateVariables(); 
        public static List<RunRecord> CurrentRun = new List<RunRecord>();

        public static int RefreshRate = 1000;


        public static void Start()
        {
            Dispatcher StillControllerDispatcher = Dispatcher.CurrentDispatcher;

            BackGroundWorkers.InitializeDI2008();

            SystemMonitor = BackGroundWorkers.InitializeSystemMonitor(StillControllerDispatcher);
            PressureRegulator = BackGroundWorkers.InitializePressureWorker();
            ElementRegulator = BackGroundWorkers.InitializeElementWorker();

            StillRegulator = new BackgroundWorker();
            StillRegulator.WorkerSupportsCancellation = true;
            StillRegulator.DoWork += new DoWorkEventHandler((state, args) =>

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

                    Distill();
                    CurrentState.Phase = 2;

                    DrainVessels();

                    Header.rhComplete = true;
                    Header.rhEnd = DateTime.Now;
                    Header.rhAvgPressure = CurrentRun.Select(i => i.rrPressure).Average();
                    Context.RunRecords.AddRange(CurrentRun);

                    Context.SaveChanges();
                    CurrentRun.Clear();

                    CurrentState.Phase = 0;
                }
            });

            SystemMonitor.RunWorkerAsync();
            StillRegulator.RunWorkerAsync();
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



        public static void FillStill()
        {
            if (CurrentState.StillFull == false) 
            { 
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
        }

        //public static void HeatUntilPlateau()
        //{
        //    RecordCurrentState();

        //    PressureRegulator.RunWorkerAsync();
        //    ElementRegulator.RunWorkerAsync();

        //    CurrentState.PlateauTemp = 0;
        //    decimal StartTemp = CurrentRun.First().rrColumnHeadTemp;
        //    decimal LastDelta = 0M;
        //    decimal TotalDelta = 0M;


        //    while (!CurrentState.StillEmpty &&  !CurrentState.RVFull && (CurrentState.ColumnTemp <= StartTemp * 1.1M || CurrentRun.Count < 20))
        //    {
        //        if (CurrentState.ColumnTemp > StartTemp)
        //        {
        //            RecordCurrentState();
        //        }
        //        Thread.Sleep(RefreshRate);
        //    }

        //    while ((!CurrentState.StillEmpty && !CurrentState.RVFull && (LastDelta >= 0.02M) || TotalDelta < 0.25M))
        //    {
        //       decimal Temp1 = CurrentRun[CurrentRun.Count - 1].rrColumnHeadTemp;
        //       decimal Temp2 = CurrentRun[CurrentRun.Count - 20].rrColumnHeadTemp;

        //        LastDelta = Temp2 != 0 ? ((Temp2 - Temp1) / Temp2) : 0;
        //        if (Temp2 > Temp1)
        //        { TotalDelta = Temp2 != 0 ? ((Temp2 - StartTemp) / Temp2) : 0; }

        //        RecordCurrentState();
        //        Thread.Sleep(RefreshRate);
        //    }
        //}

        public static void Distill()
        {

            PressureRegulator.RunWorkerAsync();
            ElementRegulator.RunWorkerAsync();
            //CurrentState.PlateauTemp = CurrentRun.Last().rrColumnHeadTemp;
            BackGroundWorkers.EnableRelay(SystemProperties.CoolantPump);

            //Keep distilling as long as the theoretical boiling point is not overran allowing for a 2% margin of error in calculation
            while (!CurrentState.StillEmpty && !CurrentState.RVFull && CurrentState.ColumnTemp <= CurrentState.TheoreticalBoilingPoint * 1.02M)
            {
                RecordCurrentState();
                Thread.Sleep(RefreshRate);
            }

            BackGroundWorkers.DisableRelay(SystemProperties.StillElement);
            CurrentState.ElementOn = false;
        }

        public static void DrainVessels()
        {
            PressureRegulator.CancelAsync();
            ElementRegulator.CancelAsync();

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
    }
}
