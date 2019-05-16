using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System.IO.Ports;
using System.Management;
using System.Configuration;
using System.Collections.Generic;
using System.Windows.Forms;
using System;
using System.Text.RegularExpressions;

namespace AutoStillDotNet
{
    class Periphrials
    {
        //Try to initalize the arduino driver, if no arudino is found return null so the Main loop knows it needs to wait for an arduino to be plugged in
        private static  ArduinoDriver.ArduinoDriver driver = (ArduinoCOMPort() == null) ? null : new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, ArduinoCOMPort(), true);

        public static ArduinoDriver.ArduinoDriver InitializeArduinoDriver()
        {
            if (driver == null)
            { driver = (ArduinoCOMPort() == null) ? null : new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, ArduinoCOMPort(), true); }
            if (driver != null)
            {
                foreach (string Key in ConfigurationManager.AppSettings.AllKeys)
                {
                    string Value = ConfigurationManager.AppSettings.Get(Key);

                    //Use regex to search teh appconfig values for the pin numbers and pin types
                    if (Regex.IsMatch(Value, @"Type=(\D+)InputPin") == true) 
                    {  DriverFunctions.SetInput(driver, Convert.ToByte(Regex.Match(Value, @"(?<=Pin=)\d+").Value)); }
                    if (Regex.IsMatch(Value, @"Type=(\D+)OutputPin") == true)
                    {
                        DriverFunctions.SetOutput(driver, Convert.ToByte(Regex.Match(Value, @"(?<=Pin=)\d+").Value));
                        if (ConfigurationManager.AppSettings.Get("InvertPinPolarity") == "True" && Regex.IsMatch(Value, @"Type=RelayOutputPin") == true)
                        {
                            driver.Send(new DigitalWriteRequest(Convert.ToByte(Regex.Match(Value, @"(?<=Pin=)\d+").Value), DigitalValue.High));
                        }
                    }
                }
            }

           //Send the initialized driver object (or null) back to whatever called this sub
            return driver;
        }
        private static string ArduinoCOMPort()
        {
            int ArduinosFound = 0;
            List<string> ArduinoPort = new List<string>();
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
                        catch { } //If the above lines error out it means the arduino is already in use and should not be added to the list
                    }
                }
            }
            catch { }
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
