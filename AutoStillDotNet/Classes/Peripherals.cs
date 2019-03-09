using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System.IO.Ports;
using System.Management;

namespace AutoStillDotNet
{
    class Periphrials
    {
        ArduinoDriver.ArduinoDriver driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, ArduinoCOMPort(), true);

        public ArduinoDriver.ArduinoDriver InitializeArduinoDriver()
        {
            //Declare the arduino itself
            var properties = new SystemProperties();

            //Digial inputs
            driver.Send(new PinModeRequest(properties.FVEmptySwtich, PinMode.Input));
            driver.Send(new PinModeRequest(properties.FVCompleteSwitch, PinMode.Input));
            driver.Send(new PinModeRequest(properties.StillLowSwitch, PinMode.Input));
            driver.Send(new PinModeRequest(properties.StillHighSwitch, PinMode.Input));

            //Digital outputs
            InitializePin(properties.StillFillValve, PinMode.Output);
            InitializePin(properties.StillFluidPump, PinMode.Output);
            InitializePin(properties.StillElement, PinMode.Output);
            InitializePin(properties.StillFillValve, PinMode.Output);
            


            //System.Threading.Thread.Sleep(3000);
            driver.Send(new PinModeRequest(properties.StillFluidPump, PinMode.Output));

            driver.Send(new DigitalWriteRequest(properties.StillFluidPump, DigitalValue.High));
            driver.Send(new DigitalWriteRequest(properties.StillFillValve, DigitalValue.High));




            driver.Send(new PinModeRequest(properties.StillElement, PinMode.Output));
            driver.Send(new PinModeRequest(properties.StillDrainValve, PinMode.Output));
            driver.Send(new PinModeRequest(properties.RVFluidPump, PinMode.Output));
            driver.Send(new PinModeRequest(properties.RVDrainValve, PinMode.Output));
            driver.Send(new PinModeRequest(properties.VacuumPump, PinMode.Output));
            driver.Send(new PinModeRequest(properties.FanSet1, PinMode.Output));
            driver.Send(new PinModeRequest(properties.FanSet2, PinMode.Output));

            //Analog Inputs
            driver.Send(new PinModeRequest(properties.SensorColumnTemp, PinMode.Input));
            driver.Send(new PinModeRequest(properties.SensorPressure, PinMode.Input));

            //Analog Outputs
            driver.Send(new PinModeRequest(properties.FanController1, PinMode.Output));
            driver.Send(new PinModeRequest(properties.FanController2, PinMode.Output));



            //Set all the starting values to 0 since the stupid relay board is jank

            driver.Send(new DigitalWriteRequest(properties.StillFillValve, DigitalValue.High));
            driver.Send(new DigitalWriteRequest(properties.RVFluidPump, DigitalValue.High));
            driver.Send(new DigitalWriteRequest(properties.RVDrainValve, DigitalValue.Low));
            driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.High));
            driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.High));




            //Send the initialized driver object back to whatever called this sub
            return driver;
        }
        private void InitializePin(byte Pin, PinMode pinMode)
        {
            driver.Send(new PinModeRequest(Pin, pinMode));
            if (pinMode == PinMode.Output)
            {
                driver.Send(new DigitalWriteRequest(Pin, DigitalValue.High));
            }
        }
        private void InitializePin(ArduinoDriver.ArduinoDriver Driver, byte Pin, PinMode pinMode)
        {
            Driver.Send(new PinModeRequest(Pin, pinMode));
            if (pinMode == PinMode.Output)
            { 
                Driver.Send(new DigitalWriteRequest(Pin, DigitalValue.High));
            }
        }
        private void InitializePin(ArduinoDriver.ArduinoDriver Driver, byte Pin, PinMode pinMode, DigitalValue digitalValue)
        {
            Driver.Send(new PinModeRequest(Pin, pinMode));
            Driver.Send(new DigitalWriteRequest(Pin, digitalValue));
        }

        private static string ArduinoCOMPort()
        {

            string ArduinoPort = "";


            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery); //These 3 lines query device manager for all active serial connections

            try
            {
                foreach (ManagementObject item in searcher.Get()) //This loop searches the results obtained above and attempts to identify the arduino
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (desc.Contains("Arduino"))
                    {
                        ArduinoPort = deviceId;
                        break;
                    }
                }
            }
            catch (ManagementException e)
            {
                //Put code here to select the arduino manually
            }
            return ArduinoPort;
        }

    }
}
