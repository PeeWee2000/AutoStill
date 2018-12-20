
using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System;
using System.Diagnostics;

namespace StillController
{

    class Program
    {

        static void Main(string[] args)
        {
            var waef = "";
            var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, "COM7", true);
            driver.Send(new PinModeRequest(52, PinMode.Output));
            driver.Send(new PinModeRequest(35, PinMode.Output));
            driver.Send(new PinModeRequest(26, PinMode.Input));
            driver.Send(new PinModeRequest(54, PinMode.Input));
            while ( 1 == 1)
            { driver.Send(new DigitalWriteRequest(52, DigitalValue.High));
                driver.Send(new DigitalWriteRequest(35, DigitalValue.High));
                driver.Send(new DigitalWriteRequest(52, DigitalValue.Low));
                waef = driver.Send(new DigitalReadRequest(26)).PinValue.ToString();
                //Console.WriteLine("Digital Value " + waef);
                Console.WriteLine(driver.Send(new AnalogReadRequest(54)).PinValue);
                
            }



        }
    }
}
