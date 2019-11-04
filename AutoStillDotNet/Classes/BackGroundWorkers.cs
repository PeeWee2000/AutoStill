using DI2008Controller;
using RelayController;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace AutoStillDotNet
{

    public static class BackGroundWorkers
    {
        private static BackgroundWorker SystemMonitor;  //Reads all the sensors and switches
        private static BackgroundWorker PressureWorker; //Determines when to turn the vaucuum pump on and off
        //private static BackgroundWorker FanController1; //Turns the fan set for the reflux column on, off, up and down depending on target temperature and distillation speed
        //private static BackgroundWorker FanController2; //Turns the fan set for the condensor on, off, up and down depending on target temperature and distillation speed

        //public static RelayBoard Relays = new RelayBoard();
        public static RelayBoard Relays = new RelayBoard();

        private static DI2008 DI2008 = new DI2008();
        private static ReadRecord DI2008Data = new ReadRecord();


        //These two relay functions exist to provide an easy way to switch to a debug mode
        public static void EnableRelay(int Device)
        {
            Relays.EnableRelay(Device);
        }
        public static void DisableRelay(int Device)
        {
            Relays.DisableRelay(Device);
        }

        public static void InitializeDI2008()
        {

            //DI2008.Channels.Analog0 = ChannelConfiguration.KTypeTC; // Column Head
            //DI2008.Channels.Analog1 = ChannelConfiguration.KTypeTC; // Reflux Jacket
            //DI2008.Channels.Analog2 = ChannelConfiguration.KTypeTC; // Condenser Jacket
            //DI2008.Channels.Analog3 = ChannelConfiguration.KTypeTC; // Coolant Reservoir
            //DI2008.Channels.Analog4 = ChannelConfiguration._5v; // System Pressure
            //DI2008.Channels.Analog5 = ChannelConfiguration._100mv; // System Amperage
            //DI2008.Channels.Analog6 = ChannelConfiguration._100mv;

            //DI2008.Channels.Digital0 = ChannelConfiguration.DigitalInput; // Still Low Switch
            //DI2008.Channels.Digital1 = ChannelConfiguration.DigitalInput; // Still High Switch
            //DI2008.Channels.Digital2 = ChannelConfiguration.DigitalInput; // RV Low Switch
            //DI2008.Channels.Digital3 = ChannelConfiguration.DigitalInput; // RV High Swtich

            ///////////////////////////////Dev Values//////////////////////////////////
            DI2008.Channels.Analog0 = ChannelConfiguration._10v; // Column Head
            DI2008.Channels.Analog1 = ChannelConfiguration._10v; // Reflux Jacket
            DI2008.Channels.Analog2 = ChannelConfiguration.KTypeTC; // Condenser Jacket
            DI2008.Channels.Analog3 = ChannelConfiguration.KTypeTC; // Coolant Reservoir
            DI2008.Channels.Analog4 = ChannelConfiguration._10v; // System Pressure
            DI2008.Channels.Analog5 = ChannelConfiguration._100mv; // System Amperage
            DI2008.Channels.Analog6 = ChannelConfiguration._100mv;

            DI2008.Channels.Digital0 = ChannelConfiguration.DigitalInput; // Still Low Switch
            DI2008.Channels.Digital1 = ChannelConfiguration.DigitalInput; // Still High Switch
            DI2008.Channels.Digital2 = ChannelConfiguration.DigitalInput; // RV Low Switch
            DI2008.Channels.Digital3 = ChannelConfiguration.DigitalInput; // RV High Swtich


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
                    if (Main.CurrentState.Run != true)
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

                            MainDispatcher.Invoke(new Action(() =>
                            {
                                Main.CurrentState.ColumnTemp = Math.Round(DI2008Data.Analog0.Value.Value, 2);
                                //Main.CurrentState.ColumnTemp = Math.Round((DI2008Data.Analog0.Value.Value / (10M / 1000M)), 2);
                                Main.CurrentState.StillFluidTemp = Math.Round(DI2008Data.Analog1.Value.Value, 2);
                                Main.CurrentState.RefluxTemp = Math.Round(DI2008Data.Analog2.Value.Value, 2);
                                //Main.CurrentState.Pressure = Math.Round((DI2008Data.Analog4.Value.Value / (5M / 45M)) - 16.5M, 2);
                                //Main.CurrentState.Pressure = Math.Round((DI2008Data.Analog4.Value.Value / ((5M / 306816.7M) / 1000)), 2); //Converts to Pascals and Divides by 1000 for Kilo Pascals -- 306816.7 is 44.5PSI converted Pascals which is the range measurable by the transducer
                                Main.CurrentState.Pressure = Math.Round((DI2008Data.Analog1.Value.Value / (10M / 306816.7M)) / 1000, 2);
                                Main.CurrentState.SystemAmperage = DI2008Data.Analog5.Value.Value;
                                Main.CurrentState.StillFull = DI2008Data.Digital0.Value == DigtitalState.High ? true : false;
                                Main.CurrentState.StillEmpty = DI2008Data.Digital1.Value == DigtitalState.Low ? true : false;                                
                                Main.CurrentState.RVEmpty = DI2008Data.Digital2.Value == DigtitalState.Low ? true : false;
                                Main.CurrentState.RVFull = DI2008Data.Digital3.Value == DigtitalState.Low ? true : false;



                            }));
                            success = true;
                        }
                        catch { }

                    }
                    if (Main.CurrentState.Phase == -1) { Main.CurrentState.Phase = 0; }
                } while (true);
            });
            return SystemMonitor;
        }

        public static BackgroundWorker InitializePressureWorker(Dispatcher MainDispatcher)
        {
            PressureWorker = new BackgroundWorker();
            PressureWorker.WorkerSupportsCancellation = true;
            PressureWorker.DoWork += new DoWorkEventHandler((state, args) =>
            {                
                DisableRelay(SystemProperties.VacuumPump);
                Main.CurrentState.VacuumPumpOn = false;
                do
                {
                    try
                    {
                        if (Main.CurrentState.Run != true || PressureWorker.CancellationPending == true)
                        { break; }

                        Thread.Sleep(1000);
                        if (Convert.ToDouble(Main.CurrentState.Pressure) > SystemProperties.TargetPressure && Main.CurrentState.VacuumPumpOn == false)
                        {
                            EnableRelay(SystemProperties.VacuumPump);
                            Main.CurrentState.VacuumPumpOn = true;

                            //Refresh the pressure has changed every second -- Note that the pressure is set in the still monitor background worker
                            do
                            {
                                Thread.Sleep(1000);
                            }
                            while (Convert.ToDouble(Main.CurrentState.Pressure) > (SystemProperties.TargetPressure - SystemProperties.TgtPresHysteresisBuffer) && PressureWorker.CancellationPending == false);

                            //Once the pressure has reached its target turn the pump off
                            DisableRelay(SystemProperties.VacuumPump);
                            Main.CurrentState.VacuumPumpOn = false;
                        }
                    }
                    catch {  }
                } while (true);
            });
            return PressureWorker;
        }
        //public static BackgroundWorker InitializeFanController1(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        //{
        //    //Turns fan set 1 on, off, up and down 
        //    //The target temperature should be just under the distillation plateau temp so it doesnt slow down the distillation process but keeps the column cool enough that only the target substance can come over
        //    FanController1 = new BackgroundWorker();
        //    FanController1.WorkerSupportsCancellation = true;
        //    FanController1.DoWork += new DoWorkEventHandler((state, args) =>
        //    {
        //        double TargetTemp = Main.PlateauTemp - 1;
        //        byte FanSpeed = 50;
        //        do
        //        {
        //            if (Main.Run != true || FanController1.CancellationPending == true)
        //            { break; }
        //            try
        //            {
        //                System.Threading.Thread.Sleep(10000);
        //                if (Main.RefluxTemp > TargetTemp)
        //                {
        //                    FanSpeed = Convert.ToByte((int)FanSpeed + 5);
        //                    MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
        //                }
        //                else if (Main.RefluxTemp < TargetTemp)
        //                {
        //                    FanSpeed = Convert.ToByte((int)FanSpeed - 5);
        //                    MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
        //                }
        //            }
        //            catch {  }
        //        } while (true);
        //    });
        //    return FanController1;
        //}

        //    public static BackgroundWorker InitializeFanController2(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        //{
        //    //Turns fan set 2 on, off, up and down 
        //    //The idea here is to start the fan at half speed, see if and see it has any noticable effect on temperature -- if the temperature drops maintain the speed, if it rises increase the speed, if it drops lower the speed
        //    FanController2 = new BackgroundWorker();
        //    FanController2.WorkerSupportsCancellation = true;
        //    FanController2.DoWork += new DoWorkEventHandler((state, args) =>
        //    {
        //        double LastTemp = Main.CondensorTemp;
        //        byte FanSpeed = 125;
        //        do
        //        {
        //            if (Main.Run != true || FanController2.CancellationPending == true)
        //            { break; }
        //            try
        //            {
        //                System.Threading.Thread.Sleep(10000);
        //                if (Main.CondensorTemp > LastTemp)
        //                {
        //                    FanSpeed = Convert.ToByte((int)FanSpeed + 5);
        //                    MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
        //                }
        //                else if (Main.CondensorTemp < LastTemp)
        //                {
        //                    FanSpeed = Convert.ToByte((int)FanSpeed - 5);
        //                    MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
        //                }
        //            }
        //            catch {  }
        //        } while (true);
        //    });
        //    return FanController2;
        //}
    }
}
