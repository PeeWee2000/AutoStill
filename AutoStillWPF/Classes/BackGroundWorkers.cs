using DI2008Controller;
using RelayController;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace AutoStillWPF
{
    public static class BackGroundWorkers
    {
        private static BackgroundWorker SystemMonitor;  //Reads all the sensors and switches
        private static BackgroundWorker PressureController; //Determines when to turn the vaucuum pump on and off
        private static BackgroundWorker ElementController; //Determines when to turn the vaucuum pump on and off

        public static RelayBoard Relays = new RelayBoard();

        private static DI2008 DI2008 = new DI2008();
        private static ReadRecord DI2008Data = new ReadRecord();


        //These two relay functions exist to provide an easy way to switch to a debug mode
        public static void EnableRelay(int Device)
        {            
            lock (Relays)
            { 
                Relays.EnableRelay(Device);
            }
        }
        public static void DisableRelay(int Device)
        {
            lock (Relays)
            { 
                Relays.DisableRelay(Device);
            }
        }

        public static void InitializeDI2008()
        {

            DI2008.Channels.Analog0 = ChannelConfiguration.KTypeTC; // Column Head
            DI2008.Channels.Analog1 = ChannelConfiguration.KTypeTC; // Reflux Jacket
            DI2008.Channels.Analog2 = ChannelConfiguration.KTypeTC; // Condenser Jacket
            DI2008.Channels.Analog3 = ChannelConfiguration.KTypeTC; // Coolant Reservoir
            DI2008.Channels.Analog4 = ChannelConfiguration._5v; // System Pressure
            DI2008.Channels.Analog5 = ChannelConfiguration._100mv; // System Amperage
            DI2008.Channels.Analog6 = ChannelConfiguration._100mv;

            DI2008.Channels.Digital0 = ChannelConfiguration.DigitalInput; // Still Low Switch
            DI2008.Channels.Digital1 = ChannelConfiguration.DigitalInput; // Still High Switch
            DI2008.Channels.Digital2 = ChannelConfiguration.DigitalInput; // RV Low Switch
            DI2008.Channels.Digital3 = ChannelConfiguration.DigitalInput; // RV High Swtich

            ///////////////////////////////Dev Values//////////////////////////////////
            //DI2008.Channels.Analog0 = ChannelConfiguration._10v; // Column Head
            //DI2008.Channels.Analog1 = ChannelConfiguration._10v; // Reflux Jacket
            //DI2008.Channels.Analog2 = ChannelConfiguration.KTypeTC; // Condenser Jacket
            //DI2008.Channels.Analog3 = ChannelConfiguration.KTypeTC; // Coolant Reservoir
            //DI2008.Channels.Analog4 = ChannelConfiguration._10v; // System Pressure
            //DI2008.Channels.Analog5 = ChannelConfiguration._100mv; // System Amperage
            //DI2008.Channels.Analog6 = ChannelConfiguration._100mv;

            //DI2008.Channels.Digital0 = ChannelConfiguration.DigitalInput; // Still Low Switch
            //DI2008.Channels.Digital1 = ChannelConfiguration.DigitalInput; // Still High Switch
            //DI2008.Channels.Digital2 = ChannelConfiguration.DigitalInput; // RV Low Switch
            //DI2008.Channels.Digital3 = ChannelConfiguration.DigitalInput; // RV High Swtich


            DI2008.ConfigureChannels();
            DI2008.Functions.StartAcquiringData();
        }

        public enum SwitchableDevices
        {
            StillFillValve = 0,
            StillFill  = 1,
            StillDrainValve = 2,
            StillDrainPump = 3,
            RVDrainValve = 4,
            RVDrainPump = 5,
            VacuumPump = 6,
            Fans = 7,
            StillElement = 8,
            CoolantPump = 8
        }



        //Background worker to monitor all sensor valuess and switch states on the still and keep global variables and the UI updated
        //The idea here is to imtermittently check all variables and write to a local variable in memory to minimize commands sent to the arduino
        //This is also convienent as it minimizes the amount of long of code required to message the arduino in the control loop
        public static BackgroundWorker InitializeSystemMonitor(Dispatcher MainDispatcher)
        {
            SystemMonitor = new BackgroundWorker();
            SystemMonitor.WorkerSupportsCancellation = true;
            SystemMonitor.DoWork += new DoWorkEventHandler((state, args) =>

            {
                do
                {
                    if (StillController.CurrentState.Run != true)
                    { break; }
                    Thread.Sleep(1000);
                    bool success = false;
                    while (success == false)
                    {
                        try
                        {
                            DI2008Data = DI2008.Functions.ReadData();

                            while (!DI2008Data.Analog0.HasValue)
                            { Thread.Sleep(100); }


                            decimal PressureVoltage = DI2008Data.Analog4.Value.Value;
                            decimal CalibrationCorrection = (-55.3M * PressureVoltage) + 118M; //Determined via getting voltage at max vacuum and atmospheric pressure then plugging the Voltage / Actual values into this link https://www.symbolab.com/solver/slope-intercept-form-calculator/slope%20intercept%20%28-1%2C1%29%2C%28-2%2C-3%29?or=ex
                            decimal PresureInKPa = PressureVoltage / (CalibrationCorrection / 1000);

                            MainDispatcher.Invoke(new Action(() =>
                            {
                                StillController.CurrentState.ColumnTemp = Math.Round(DI2008Data.Analog0.Value.Value, 2);
                                
                                //StillController.CurrentState.ColumnTemp = Math.Round((DI2008Data.Analog0.Value.Value / (10 / 1000M)), 2);
                                
                                StillController.CurrentState.StillFluidTemp = Math.Round(DI2008Data.Analog1.Value.Value, 2);
                                StillController.CurrentState.RefluxTemp = Math.Round(DI2008Data.Analog2.Value.Value, 2);                                                                                                   
                                StillController.CurrentState.Pressure = Math.Round(PresureInKPa, 2);
                                
                                //StillController.CurrentState.Pressure = Math.Round(((DI2008Data.Analog1.Value.Value / (10 / 306816.7M)) / 1000), 2); //Converts to Pascals and Divides by 1000 for Kilo Pascals -- 306816.7 is 44.5PSI converted Pascals which is the range measurable by the transducer
                                
                                StillController.CurrentState.SystemAmperage = DI2008Data.Analog5.Value.Value;
                                StillController.CurrentState.StillFull = DI2008Data.Digital0.Value == DigtitalState.High ? true : false;
                                StillController.CurrentState.StillEmpty = DI2008Data.Digital1.Value == DigtitalState.Low ? true : false;                                
                                StillController.CurrentState.RVEmpty = DI2008Data.Digital2.Value == DigtitalState.Low ? true : false;
                                StillController.CurrentState.RVFull = DI2008Data.Digital3.Value == DigtitalState.Low ? true : false;
                            }));
                            success = true;
                        }
                        catch { }
                    }
                    if (StillController.CurrentState.Phase == -1) { StillController.CurrentState.Phase = 0; }
                } while (true);
            });
            return SystemMonitor;
        }

        public static BackgroundWorker InitializePressureWorker()
        {
            PressureController = new BackgroundWorker();
            PressureController.WorkerSupportsCancellation = true;
            PressureController.DoWork += new DoWorkEventHandler((state, args) =>
            {                
                DisableRelay(SystemProperties.VacuumPump);
                StillController.CurrentState.VacuumPumpOn = false;
                decimal LastPressure = -1M;
                while (true)
                {
                    if (Math.Abs(((StillController.CurrentState.Pressure - LastPressure) / LastPressure)) > .1M) //Run if the pressure has risen 10% above the "bottom out" pressure
                    {
                        EnableRelay(SystemProperties.VacuumPump);
                        StillController.CurrentState.VacuumPumpOn = true;

                        while (Math.Abs(((StillController.CurrentState.Pressure - LastPressure) / LastPressure)) > 0.01M ) //Run until the pressure stops going down
                        {
                            LastPressure = StillController.CurrentState.Pressure == 0 ? -1 : StillController.CurrentState.Pressure;
                            Thread.Sleep(20000);
                        }

                        DisableRelay(SystemProperties.VacuumPump);
                        StillController.CurrentState.VacuumPumpOn = false;
                    }
                    Thread.Sleep(StillController.RefreshRate);
                }
            });
            return PressureController;
        }

        public static BackgroundWorker InitializeElementWorker()
        {
            ElementController = new BackgroundWorker();
            ElementController.WorkerSupportsCancellation = true;
            ElementController.DoWork += new DoWorkEventHandler((state, args) =>
            {
                DisableRelay(SystemProperties.StillElement);
                StillController.CurrentState.ElementOn = false;
                while (true)
                {
                    if (StillController.CurrentState.Run != true || ElementController.CancellationPending == true)
                    { break; }

                    if (StillController.CurrentState.StillFluidTemp <= StillController.CurrentState.TheoreticalBoilingPoint && (StillController.CurrentState.Phase == 1 || StillController.CurrentState.Phase == 2))
                    {
                        EnableRelay(SystemProperties.StillElement);
                        StillController.CurrentState.ElementOn = true;

                        while ((StillController.CurrentState.TheoreticalBoilingPoint * 1.05M > StillController.CurrentState.StillFluidTemp || StillController.CurrentState.ColumnTemp < StillController.CurrentState.TheoreticalBoilingPoint * 0.95M) && ElementController.CancellationPending == false)
                        {
                            Thread.Sleep(5000); //Refresh the temperature every 5 seconds
                        }
                        
                        DisableRelay(SystemProperties.StillElement);
                        StillController.CurrentState.ElementOn = false;
                        Thread.Sleep(5000);
                    }
                } 
            });
            return ElementController;
        }      
    }
}
