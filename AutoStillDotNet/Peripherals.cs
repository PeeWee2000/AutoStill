using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System;
using System.Data;

namespace AutoStillDotNet
{
    class Periphrials
    {
        public ArduinoDriver.ArduinoDriver InitializeArduinoDriver()
        {

            //Declare the arduino itself
            var properties = new SystemProperties();
            var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, "COM7", true);

            //Digial inputs
            driver.Send(new PinModeRequest(properties.FVEmptySwtich, PinMode.Input));
            driver.Send(new PinModeRequest(properties.FVCompleteSwitch, PinMode.Input));
            driver.Send(new PinModeRequest(properties.FVFluidPump, PinMode.Input));
            driver.Send(new PinModeRequest(properties.StillLowSwitch, PinMode.Input));
            driver.Send(new PinModeRequest(properties.StillHighSwitch, PinMode.Input));

            //Digital outputs
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

            //Send the initialized driver object back to whatever called this sub
            return driver;
        }
    }
}
