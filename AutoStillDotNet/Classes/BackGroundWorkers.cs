using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace AutoStillDotNet
{

    public static class BackGroundWorkers
    {
        private static BackgroundWorker SystemMonitor;  //Reads all the sensors and switches
        private static BackgroundWorker PressureWorker; //Determines when to turn the vaucuum pump on and off
        private static BackgroundWorker StillController; //Turns the element, pumps and valves on and off
        private static BackgroundWorker FanController1; //Turns the fan set for the reflux column on, off, up and down depending on target temperature and distillation speed
        private static BackgroundWorker FanController2; //Turns the fan set for the condensor on, off, up and down depending on target temperature and distillation speed

        //Background worker to monitor all sensor valuess and switch states on the still and keep global variables and the UI updated
        //The idea here is to imtermittently check all variables and write to a local variable in memory to minimize commands sent to the arduino
        //This is also convienent as it minimizes the amount of long of code required to message the arduino in the control loop
        public static BackgroundWorker InitializeSystemMonitor(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        {
            SystemMonitor = new BackgroundWorker();
            SystemMonitor.WorkerSupportsCancellation = true;
            SystemMonitor.DoWork += new DoWorkEventHandler((state, args) =>

            {
                do
                {
                    if (Main.Run != true || driver == null) //The periphrials class returns null if no arduino is found on any of the com ports
                    { break; }
                    System.Threading.Thread.Sleep(1000);
                    //Check Temperature
                    bool success = false;
                    while (success == false)
                    {
                        try
                        {
                            MainDispatcher.Invoke(new Action(() => 
                            {
                                Main.ColumnTemp = DriverFunctions.GetTemperature(driver, SystemProperties.SensorColumnTemp).ToString(); 
                                Main.RefluxTemp = DriverFunctions.GetTemperature(driver, SystemProperties.SensorCoolantTemp1);
                                Main.CondensorTemp = DriverFunctions.GetTemperature(driver, SystemProperties.SensorCoolantTemp2); 
                                Main.ElementAmperage = DriverFunctions.GetAmperage(driver, SystemProperties.SensorColumnTemp);
                             }));
                            success = true;
                        }
                        catch
                        {
                            MainDispatcher.Invoke(new Action(() => {
                                driver.Dispose();
                                driver = Periphrials.InitializeArduinoDriver();
                            }));
                        }
                    }

                    //Check the low level switch -- a value of low means the lower still switch is open
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(SystemProperties.StillLowSwitch)).PinValue == DigitalValue.Low)
                        { Main.StillEmpty = true; }
                        else
                        { Main.StillEmpty = false; }
                    }));

                    //Check the high level switch -- a value of high means the upper still switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(SystemProperties.StillHighSwitch)).PinValue == DigitalValue.High)
                        { Main.StillFull = true; }
                        else
                        { Main.StillFull = false; }
                    }));

                    //Check the recieving vessel low level switch -- a value of high means the upper rv switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(SystemProperties.RVEmptySwitch)).PinValue == DigitalValue.High)
                        { Main.RVEmpty = true; }
                        else
                        { Main.RVEmpty = false; }

                    }));

                    //Check the recieving vessel high level switch -- a value of high means the upper rv switch is closed
                    MainDispatcher.Invoke(new Action(() => {
                        if (driver.Send(new DigitalReadRequest(SystemProperties.RVFullSwitch)).PinValue == DigitalValue.Low)
                        { Main.RVFull = true; }
                        else
                        { Main.RVFull = false; }

                    }));

                    //Check the pressure (1024 is the resolution of the ADC on the arduino, 45.1 is the approximate pressure range in PSI that the sensor is capable of reading, the -15 makes sure that STP = 0 PSI/kPa)
                    MainDispatcher.Invoke(new Action(() => { Main.Pressure = Math.Round((((Convert.ToDouble(driver.Send(new AnalogReadRequest(SystemProperties.SensorPressure)).PinValue.ToString()) / 1024) * 45.1) - 16.5) * ((SystemProperties.Units == "Metric") ? 6.895 : 1), 2).ToString(); }));

                    if (Main.Phase == -1) { Main.Phase = 0; }

                } while (true);
            });
            return SystemMonitor;
        }

        public static BackgroundWorker InitializeStillController(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        {
            return StillController;
        }
            public static BackgroundWorker InitializePressureWorker(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        {
            PressureWorker = new BackgroundWorker();
            PressureWorker.WorkerSupportsCancellation = true;
            PressureWorker.DoWork += new DoWorkEventHandler((state, args) =>
            {
                //Make sure the pump is off
                MainDispatcher.Invoke(new Action(() => { DriverFunctions.RelayOff(driver,SystemProperties.VacuumPump); }));
                Main.VacuumPumpOn = false;
                do
                {
                    try
                    {
                        if (Main.Run != true || PressureWorker.CancellationPending == true)
                        { break; }

                        System.Threading.Thread.Sleep(1000);
                        if (Convert.ToDouble(Main.Pressure) > SystemProperties.TargetPressure && Main.VacuumPumpOn == false)
                        {
                            //Turn the vacuum pump on
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.RelayOn(driver, SystemProperties.VacuumPump); }));
                            Main.VacuumPumpOn = true;

                            //Refresh the pressure has changed every second -- Note that the pressure is set in the still monitor background worker
                            do
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                            while (Convert.ToDouble(Main.Pressure) > (SystemProperties.TargetPressure - SystemProperties.TgtPresHysteresisBuffer) && PressureWorker.CancellationPending == false);

                            //Once the pressure has reached its target turn the pump off
                            MainDispatcher.Invoke(new Action(() => { DriverFunctions.RelayOff(driver, SystemProperties.VacuumPump); }));
                            Main.VacuumPumpOn = false;
                        }
                    }
                    catch { MainDispatcher.Invoke(new Action(() => { driver = Periphrials.InitializeArduinoDriver(); })); }
                } while (true);
            });
            return PressureWorker;
        }
        public static BackgroundWorker InitializeFanController1(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        {
            //Turns fan set 1 on, off, up and down 
            //The target temperature should be just under the distillation plateau temp so it doesnt slow down the distillation process but keeps the column cool enough that only the target substance can come over
            FanController1 = new BackgroundWorker();
            FanController1.WorkerSupportsCancellation = true;
            FanController1.DoWork += new DoWorkEventHandler((state, args) =>
            {
                int TargetTemp = Main.PlateauTemp - 1;
                byte FanSpeed = 50;
                do
                {
                    if (Main.Run != true || FanController1.CancellationPending == true)
                    { break; }
                    try
                    {
                        System.Threading.Thread.Sleep(10000);
                        if (Main.RefluxTemp > TargetTemp)
                        {
                            FanSpeed = Convert.ToByte((int)FanSpeed + 5);
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
                        }
                        else if (Main.RefluxTemp < TargetTemp)
                        {
                            FanSpeed = Convert.ToByte((int)FanSpeed - 5);
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
                        }
                    }
                    catch { MainDispatcher.Invoke(new Action(() => { driver = Periphrials.InitializeArduinoDriver(); })); }
                } while (true);
            });
            return FanController1;
        }

            public static BackgroundWorker InitializeFanController2(ArduinoDriver.ArduinoDriver driver, Dispatcher MainDispatcher)
        {
            //Turns fan set 2 on, off, up and down 
            //The idea here is to start the fan at half speed, see if and see it has any noticable effect on temperature -- if the temperature drops maintain the speed, if it rises increase the speed, if it drops lower the speed
            FanController2 = new BackgroundWorker();
            FanController2.WorkerSupportsCancellation = true;
            FanController2.DoWork += new DoWorkEventHandler((state, args) =>
            {
                int LastTemp = Main.CondensorTemp;
                byte FanSpeed = 125;
                do
                {
                    if (Main.Run != true || FanController2.CancellationPending == true)
                    { break; }
                    try
                    {
                        System.Threading.Thread.Sleep(10000);
                        if (Main.CondensorTemp > LastTemp)
                        {
                            FanSpeed = Convert.ToByte((int)FanSpeed + 5);
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
                        }
                        else if (Main.CondensorTemp < LastTemp)
                        {
                            FanSpeed = Convert.ToByte((int)FanSpeed - 5);
                            MainDispatcher.Invoke(new Action(() => { driver.Send(new AnalogWriteRequest(SystemProperties.FanController1, FanSpeed)); }));
                        }
                    }
                    catch { MainDispatcher.Invoke(new Action(() => { driver = Periphrials.InitializeArduinoDriver(); })); }
                } while (true);
            });
            return FanController2;
        }
    }
}
