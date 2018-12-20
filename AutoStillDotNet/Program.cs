using System;
using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using ArduinoUploader.Hardware;
using System.Diagnostics;
using System.Windows.Forms;
using System.Data;


namespace AutoStillDotNet
{

    static class Program
    {


        [STAThread]
        static void Main()
        {





            //Datatable for statistics and calculating when to turn the element off
            DataTable StillStats = new DataTable("StillStats");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "ID";
            column.AutoIncrement = true;
            column.ReadOnly = true;
            column.Unique = true;

            StillStats.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Time";
            column.DataType = System.Type.GetType("System.DateTime");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Temperature";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);
            
            column = new DataColumn();
            column.ColumnName = "TemperatureDelta";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);
            
            column = new DataColumn();
            column.ColumnName = "Pressure";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
            PrimaryKeyColumns[0] = StillStats.Columns["id"];
            StillStats.PrimaryKey = PrimaryKeyColumns;

            for( int i = 0; i<=2; i++)
            { 
            row = StillStats.NewRow();
                row["ID"] = i;
                row["Time"] = DateTime.Now;
                row["Temperature"] = 666;
                row["TemperatureDelta"] = 9001;
                row["Pressure"] = 69;
                StillStats.Rows.Add(row);
            }

            //Digital Pins
            byte Stop = 22;          //Red Button (Emergency Stop)
            byte Start = 23;         // Green Button (GO!)
            byte FVSwitch = 24;      // Fermentation Vessel empty switch       
            byte FVPump = 25;        //Pump leading from FV to Still
            byte StillLowSwitch = 26;      //Still Safety Switch
            byte StillFullSwitch = 27;      //Still Safety Switch
            byte StillDrain = 28;    // Solenoid and or pump from still to waste
            byte RVPump = 29;        //Solenoid and pump from receiving vessel to storage
            byte Element = 30;      //Heating element
            byte VacuumPump = 31;   //Dildos

            //Analog Pins
            byte RFTempSensor = 54; //Temperature sensor on top of reflux column
            byte VacuumSensor = 55; // Vacuum sensor on FV
                
            

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


            //Sensor Inputs
            driver.Send(new PinModeRequest(RFTempSensor, PinMode.Input));
            driver.Send(new PinModeRequest(VacuumSensor, PinMode.Input));


            bool Run = false;

            while (Run == false)
            {
                if (driver.Send(new DigitalReadRequest(Start)).PinValue.ToString() == "High")
                { Run = true; }
            }


            bool FVEmpty = true;
            bool StillEmpty = false;

            //Keep running unless the red button is pressed
            while (Run == true)
            {
                    //Check to see if there is any liquid in the still, if so drain it (the circuit is closed when the still is empty)
                    if (driver.Send(new DigitalReadRequest(StillLowSwitch)).PinValue.ToString() == "Low")
                    {
                        while (driver.Send(new DigitalReadRequest(StillLowSwitch)).PinValue.ToString() == "Low")
                        { driver.Send(new DigitalWriteRequest(StillDrain, DigitalValue.High));
                            System.Threading.Thread.Sleep(60000);
                        }
                        driver.Send(new DigitalWriteRequest(StillDrain, DigitalValue.Low));
                    }

                    //Check to see if there is any liquid in the fermentation vessel, if so pump it into the still
                    //The switches are closed (high) when the black parts are touching -- so the switch should fall and be open (low) when the vessel is empty
                    if (driver.Send(new DigitalReadRequest(FVSwitch)).PinValue.ToString() == "High")
                {
                    //Start pumping and check to see if the still is full ever 15 seconds
                    driver.Send(new DigitalWriteRequest(FVPump, DigitalValue.High));
                    while (driver.Send(new DigitalReadRequest(StillFullSwitch)).PinValue.ToString() == "High")
                    {
                        System.Threading.Thread.Sleep(15000);
                    }
                    driver.Send(new DigitalWriteRequest(FVPump, DigitalValue.Low));
                }

                driver.Send(new DigitalWriteRequest(VacuumPump, DigitalValue.High));
                driver.Send(new DigitalWriteRequest(Element, DigitalValue.High));
                int InitialRefluxTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(RFTempSensor)).PinValue.ToString());
                int ETHPlateau = 0;
                    //Start distilling and wait for first plateau
                    while (driver.Send(new DigitalReadRequest(StillLowSwitch)).PinValue.ToString() == "Low") 
                            
                { }

                    //Wait until end of first plateau

                    
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
