using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System;
using System.Data;

namespace AutoStillDotNet
{
    class ArduinoPeripherals
    {
        private byte Stop = 22;          //Red Button (Emergency Stop)
        private byte Start = 23;         // Green Button (GO!)
        private byte FVSwitch = 24;      // Fermentation Vessel empty switch       
        private byte FVPump = 25;        //Pump leading from FV to Still
        private byte StillLowSwitch = 26;      //Still Safety Switch
        private byte StillFullSwitch = 27;      //Still Safety Switch
        private byte StillDrain = 28;    // Solenoid and or pump from still to waste
        private byte RVPump = 29;        //Solenoid and pump from receiving vessel to storage
        private byte Element = 53;      //Heating element
        private byte VacuumPump = 31;   //Dildos

        //Analog Pins
        private byte RFTempSensor = 54; //Temperature sensor on top of reflux column
        private byte VacuumSensor = 55; // Vacuum sensor on FV





        private byte month = 7;  // Backing store

        public byte Month
        {
            get
            {
                return month;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    month = value;
                }
            }
        }

        public byte FVSwitch
        {
            get
            {
                return FVSwitch;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    FVSwitch = value;
                }
            }
        }

        public  ArduinoDriver.ArduinoDriver InitializeDriver()
        { 
        ////Digital Pins
        //byte Stop = 22;          //Red Button (Emergency Stop)
        //byte Start = 23;         // Green Button (GO!)
        //byte FVSwitch = 24;      // Fermentation Vessel empty switch       
        //byte FVPump = 25;        //Pump leading from FV to Still
        //byte StillLowSwitch = 26;      //Still Safety Switch
        //byte StillFullSwitch = 27;      //Still Safety Switch
        //byte StillDrain = 28;    // Solenoid and or pump from still to waste
        //byte RVPump = 29;        //Solenoid and pump from receiving vessel to storage
        //byte Element = 53;      //Heating element
        //byte VacuumPump = 31;   //Dildos

        ////Analog Pins
        //byte RFTempSensor = 54; //Temperature sensor on top of reflux column
        //byte VacuumSensor = 55; // Vacuum sensor on FV



        //Declare the arduino itself
        var driver = new ArduinoDriver.ArduinoDriver(ArduinoModel.Mega2560, "COM7", true);

        //Digial inputs
        driver.Send(new PinModeRequest(Stop, PinMode.Input));
            driver.Send(new PinModeRequest(Start, PinMode.Input));
            driver.Send(new PinModeRequest(FVSwitch, PinMode.Input));
            driver.Send(new PinModeRequest(StillLowSwitch, PinMode.Input));


            //Digital outputs
            driver.Send(new PinModeRequest(FVPump, PinMode.Output));
            driver.Send(new PinModeRequest(StillDrain, PinMode.Output));
            driver.Send(new PinModeRequest(RVPump, PinMode.Output));
            driver.Send(new PinModeRequest(Element, PinMode.Output));
            driver.Send(new PinModeRequest(StillFullSwitch, PinMode.Output));


            //Sensor Inputs
            driver.Send(new PinModeRequest(RFTempSensor, PinMode.Input));
            driver.Send(new PinModeRequest(VacuumSensor, PinMode.Input));

            return driver;
        }
        public static void TestPeriphrials(DataTable table)
        {
            for (int i = 0; i <= 10; i++)
            {
                DataRow row;

                row = table.NewRow();
                //row["ID"] = i;
                row["Time"] = DateTime.Now;
                row["Temperature"] = 666;
                row["TemperatureDelta"] = 9001;
                row["Pressure"] = 69;
                table.Rows.Add(row);
            }
        }

      
    }
}
