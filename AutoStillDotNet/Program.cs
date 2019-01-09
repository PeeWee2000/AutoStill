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
            DataTable StillStats = Statistics.InitializeTable();
            var driver = ArduinoPeripherals.InitializeDriver();



            
            

            //string Dev = "0";
            //while (1 == 1)
            //{
            //    driver.Send(new DigitalWriteRequest(StillFullSwitch, DigitalValue.High));
            //    driver.Send(new DigitalWriteRequest(StillFullSwitch, DigitalValue.Low));
            //    driver.Send(new DigitalWriteRequest(Element, DigitalValue.High));
            //    driver.Send(new DigitalWriteRequest(Element, DigitalValue.Low));
            //    Dev = driver.Send(new AnalogReadRequest(RFTempSensor)).PinValue.ToString();
            //}

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

                //Turn on the element and vacuum sensor
                driver.Send(new DigitalWriteRequest(VacuumPump, DigitalValue.High));
                driver.Send(new DigitalWriteRequest(Element, DigitalValue.High));

                int CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(RFTempSensor)).PinValue.ToString());
                int CurrentDelta = 0;
                int Counter = 0;
                int Temp1 = 0;
                int Temp2 = 0;
                double AverageDelta = 1.0;

                row = StillStats.NewRow();
                row["Time"] = DateTime.Now;
                row["Temperature"] = CurrentTemp;
                row["TemperatureDelta"] = 0;
                row["Pressure"] = driver.Send(new AnalogReadRequest(VacuumSensor)).PinValue.ToString();
                StillStats.Rows.Add(row);

                //Get the last written row for collecting temperature rise statistics
                DataRow LastRow = StillStats.Rows[0];

                //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained the given time span
                DataRow Delta1; 
                DataRow Delta2;

                //Keep the element on and keep collecting data every 10 seconds until the first plateau is reached then go to the next loop

                while (driver.Send(new DigitalReadRequest(StillLowSwitch)).PinValue.ToString() == "Low" && AverageDelta >= 0.02) 
                            
                {
                    //Once the element has been on for 5 minutes start checking for the plateau
                    if (Counter < 36)
                    { Counter = Counter + 1; }
                    else
                    { Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                        Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                        Temp1 = Delta1.Field<Int32>("Temperature");
                        Temp2 = Delta2.Field<Int32>("Temperature");
                        AverageDelta = ((Temp2 - Temp1) / Temp2); }


                    System.Threading.Thread.Sleep(10000);
                    CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(RFTempSensor)).PinValue.ToString());
                    CurrentDelta = CurrentTemp -  LastRow.Field<Int32>("Temperature");
                    row = StillStats.NewRow();
                    row["Time"] = DateTime.Now;
                    row["Temperature"] = CurrentTemp;
                    row["TemperatureDelta"] = CurrentDelta;
                    row["Pressure"] = driver.Send(new AnalogReadRequest(VacuumSensor)).PinValue.ToString();
                    StillStats.Rows.Add(row);
                    LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                }

                //Once the first plateau is reached allowing for 2% variance in temperature wait until it ends 
                //Or end the batch if the saftey limit switch is triggered
                while (driver.Send(new DigitalReadRequest(StillLowSwitch)).PinValue.ToString() == "Low"  && AverageDelta <= 0.02)

                {
                    Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                    Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                    Temp1 = Delta1.Field<Int32>("Temperature");
                    Temp2 = Delta2.Field<Int32>("Temperature");
                    AverageDelta = ((Temp2 - Temp1) / Temp2);

                    System.Threading.Thread.Sleep(10000);
                    CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(RFTempSensor)).PinValue.ToString());
                    CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                    row = StillStats.NewRow();
                    row["Time"] = DateTime.Now;
                    row["Temperature"] = CurrentTemp;
                    row["TemperatureDelta"] = CurrentDelta;
                    row["Pressure"] = driver.Send(new AnalogReadRequest(VacuumSensor)).PinValue.ToString();
                    StillStats.Rows.Add(row);
                    LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
