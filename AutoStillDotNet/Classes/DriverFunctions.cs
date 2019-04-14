using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using System;
using System.Configuration;
using System.Windows.Threading;

namespace AutoStillDotNet
{
    public static class DriverFunctions
    {
        public static void SetInput(ArduinoDriver.ArduinoDriver driver, byte PinNumber, Dispatcher MainDispatcher) //Used for setting an input asynchronously
        {
            MainDispatcher.Invoke(new Action(() => { driver.Send(new PinModeRequest(PinNumber, PinMode.Input)); }));

        }
        public static void SetInput(ArduinoDriver.ArduinoDriver driver, byte PinNumber) //Used for setting an input synchronously
        {
            driver.Send(new PinModeRequest(PinNumber, PinMode.Input));
        }
        public static void SetOutput(ArduinoDriver.ArduinoDriver driver, byte PinNumber, Dispatcher MainDispatcher)
        {
            MainDispatcher.Invoke(new Action(() => { driver.Send(new PinModeRequest(PinNumber, PinMode.Output)); }));
            if (ConfigurationManager.AppSettings.Get("InvertPinPolarity") == "true")       //The V1.0 relay board is retarded and uses Low == On
            { driver.Send(new DigitalWriteRequest(PinNumber, DigitalValue.High)); }        //so this is necessary in order to start up with everything off

        }
        public static void SetOutput(ArduinoDriver.ArduinoDriver driver, byte PinNumber)
        {
            driver.Send(new PinModeRequest(PinNumber, PinMode.Output));
        }
        public static void TurnOn(ArduinoDriver.ArduinoDriver driver, byte PinNumber) //This simplifies turning periphrials on when switching from dev to production and shortens the syntax for calling "On"
        {
            if (ConfigurationManager.AppSettings.Get("InvertPinPolarity") == "False")
            { driver.Send(new DigitalWriteRequest(PinNumber, DigitalValue.Low)); }
            else
            { driver.Send(new DigitalWriteRequest(PinNumber, DigitalValue.High)); }
        }
        public static void TurnOff(ArduinoDriver.ArduinoDriver driver, byte PinNumber)  //This simplifies turning periphrials on when switching from dev to production and shortens the syntax for calling "Off"
        {
            if (ConfigurationManager.AppSettings.Get("InvertPinPolarity") == "True")
            { driver.Send(new DigitalWriteRequest(PinNumber, DigitalValue.Low)); }
            else
            { driver.Send(new DigitalWriteRequest(PinNumber, DigitalValue.High)); }
        }
        public static Int32 GetTemperature(ArduinoDriver.ArduinoDriver driver, byte PinNumber) //This is used just for simplyifying the syntax to read a temperature and assumes all thermocouples are K-Types, will add settings
        {                                                                                      //to the app config to allow this to be adjusted and incorporate metric imperial calculations as well.
            return Convert.ToInt32((((Convert.ToDouble(driver.Send(new AnalogReadRequest(PinNumber)).PinValue.ToString()) * (5.0 / 1023.0)) - 1.25) / 0.005));
        }
        public static Int32 GetAmperage(ArduinoDriver.ArduinoDriver driver, byte PinNumber) //This is used just for simplyifying the syntax, will need to incorporate voltage as well
        {
            return Convert.ToInt32((((Convert.ToDouble(driver.Send(new AnalogReadRequest(PinNumber)).PinValue.ToString()) * (5.0 / 1023.0)) - 1.25) / 0.005));
        }
    }
}
