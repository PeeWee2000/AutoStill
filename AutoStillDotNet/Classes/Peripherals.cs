using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System.IO.Ports;
using System.Management;
using System.Configuration;
using System.Collections.Generic;
using System.Windows.Forms;
using System;

namespace AutoStillDotNet
{
    class Periphrials
    {
        //Try to initalize the arduino driver, if no arudino is found return null so the Main loop knows it needs to wait for an arduino to be plugged in
        private static readonly ArduinoDriver.ArduinoDriver driver = (ArduinoCOMPort() == null) ? null : new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, ArduinoCOMPort(), true);

        public static ArduinoDriver.ArduinoDriver InitializeArduinoDriver()
        {
            if (driver != null)
            {
                //Digial inputs
                driver.Send(new PinModeRequest(SystemProperties.FVEmptySwtich, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.FVCompleteSwitch, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.StillLowSwitch, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.StillHighSwitch, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.RVEmptySwitch, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.RVFullSwitch, PinMode.Input));

                //Digital outputs
                InitOutputPin(SystemProperties.StillFillValve);
                InitOutputPin(SystemProperties.StillFluidPump);
                InitOutputPin(SystemProperties.StillElement);
                InitOutputPin(SystemProperties.StillDrainValve);
                InitOutputPin(SystemProperties.RVFluidPump);
                InitOutputPin(SystemProperties.RVDrainValve);
                InitOutputPin(SystemProperties.VacuumPump);
                InitOutputPin(SystemProperties.FanSet1);
                InitOutputPin(SystemProperties.FanSet2);

                //Analog Inputs
                driver.Send(new PinModeRequest(SystemProperties.SensorColumnTemp, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.SensorPressure, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.SensorCoolantTemp1, PinMode.Input));
                driver.Send(new PinModeRequest(SystemProperties.SensorCoolantTemp2, PinMode.Input));

                //Analog Outputs
                InitOutputPin(SystemProperties.FanController1);
                InitOutputPin(SystemProperties.FanController2);
            }


            //Send the initialized driver object (or null) back to whatever called this sub
            return driver;
        }
        private static void InitOutputPin(byte Pin)
        {
            driver.Send(new PinModeRequest(Pin, PinMode.Output));
            if (ConfigurationManager.AppSettings.Get("InvertPinPolarity") == "true") //The V1.0 relay board is retarded and uses Low == On
            {                                                                        //so this is necessary in order to start up with everything off
                driver.Send(new DigitalWriteRequest(Pin, DigitalValue.High));
            }
        }


        private static string ArduinoCOMPort()
        {

            List<string> ArduinoPort = new List<string>();
            int ArduinosFound = 0;
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
                        try
                        {
                            SerialPort inuse = new SerialPort(deviceId);
                            inuse.Open();
                            inuse.Close();
                            ArduinosFound++;
                            ArduinoPort.Add(deviceId);
                        }
                        catch (UnauthorizedAccessException ex)
                        { } //If the above lines error out it means the arduino is already in use and should not be added to the list
                    }
                }
            }
            catch 
            {
            }
            if (ArduinosFound == 1)
            { return ArduinoPort[0]; }
            else if (ArduinosFound >= 1)
            {
                MessageBox.Show("Multiple Arduinos found, please choose the correct one");
                //Add code here to select the correct arduino
                return null;
            }
            else
            { MessageBox.Show("No arduino found please verify that it is plugged in");
              return null;
            }
        }

    }
}
