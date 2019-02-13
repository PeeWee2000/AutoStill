using System;
using ArduinoDriver;
using ArduinoDriver.SerialProtocol;
using System.Windows.Forms;
using System.Data;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AutoStillDotNet
{
    public partial class Main : Form
    {
        private BackgroundWorker StillMonitor = null;

        public Main()
        {
            InitializeComponent();


            //Datatable for statistics and calculating when to turn the element off
            DataTable StillStats = Statistics.InitializeTable();
            //Hardware Addresses
            var properties = new SystemProperties();
            //Instanciate the periphrial class and start up the arduino
            var Periphrials = new Periphrials();
            var driver = Periphrials.InitializeArduinoDriver();

            //Background worker to control the arduino and update the gui, datatable etc
            StillMonitor = new BackgroundWorker();
            StillMonitor.WorkerSupportsCancellation = true;
            StillMonitor.DoWork += new DoWorkEventHandler((state, args) =>

            {
                bool FVEmpty = true;
                bool StillEmpty = true;
                bool StillFull = false;
                bool Run = false;

                do
                {
                    if (StillMonitor.CancellationPending)
                    { break; }

                    //        //Check to see if there is any liquid in the still, if so check the full switch to see if the liquid is tails or a fresh batch
                    //        //(the circuit is closed when the still is empty)
                    if (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low")
                    {
                        if (driver.Send(new DigitalReadRequest(properties.StillHighSwitch)).PinValue.ToString() == "High")
                        {
                            while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low")
                            {
                                driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.High));
                                System.Threading.Thread.Sleep(60000);
                            }
                            driver.Send(new DigitalWriteRequest(properties.StillDrainValve, DigitalValue.Low));
                        }
                    }
                    else
                    { break; }
                    //Check to see if there is any liquid in the fermentation vessel, if so then check to see that fermentation is complete, if so pump it into the still
                    //The switches are closed (high) when the black parts are touching -- so the switch should fall and be open (low) when the vessel is empty
                    if (driver.Send(new DigitalReadRequest(properties.FVEmptySwtich)).PinValue.ToString() == "High" && driver.Send(new DigitalReadRequest(properties.FVCompleteSwitch)).PinValue.ToString() == "Low")
                    {
                        //Start pumping and check to see if the still is full every 15 seconds
                        driver.Send(new DigitalWriteRequest(properties.FVFluidPump, DigitalValue.High));
                        while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "High")
                        {
                            System.Threading.Thread.Sleep(15000);
                        }
                        driver.Send(new DigitalWriteRequest(properties.FVFluidPump, DigitalValue.Low));
                    }
                    else
                    { break; }

                    //    //Turn on the element and vacuum sensor
                    driver.Send(new DigitalWriteRequest(properties.VacuumPump, DigitalValue.High));
                    driver.Send(new DigitalWriteRequest(properties.StillElement, DigitalValue.High));

                    int CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString());
                    int CurrentDelta = 0;
                    int Counter = 0;
                    int Temp1 = 0;
                    int Temp2 = 0;
                    double AverageDelta = 1.0;

                    DataRow row;

                    row = StillStats.NewRow();
                    row["Time"] = DateTime.Now;
                    row["Temperature"] = CurrentTemp;
                    row["TemperatureDelta"] = 0;
                    row["Pressure"] = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString();
                    StillStats.Rows.Add(row);

                    //Get the last written row for collecting temperature rise statistics
                    DataRow LastRow = StillStats.Rows[0];

                    //Get two rows from two points in time 2.5 minutes apart so an average temperature change can be obtained the given time span
                    DataRow Delta1;
                    DataRow Delta2;

                    //    //Keep the element on and keep collecting data every 10 seconds until the first plateau is reached then go to the next loop

                    while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low" && AverageDelta >= 0.02)

                    {
                        //Once the element has been on for 5 minutes start checking for the plateau
                        if (Counter < 36)
                        {
                            Counter = Counter + 1;
                        }
                        else
                        {
                            Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                            Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                            Temp1 = Delta1.Field<Int32>("Temperature");
                            Temp2 = Delta2.Field<Int32>("Temperature");
                            AverageDelta = ((Temp2 - Temp1) / Temp2);
                        }


                        System.Threading.Thread.Sleep(10000);
                        CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString());
                        CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                        row = StillStats.NewRow();
                        row["Time"] = DateTime.Now;
                        row["Temperature"] = CurrentTemp;
                        row["TemperatureDelta"] = CurrentDelta;
                        row["Pressure"] = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString();
                        StillStats.Rows.Add(row);
                        LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                    }

                    //Once the first plateau is reached allowing for 2% variance in temperature wait until it ends 
                    //Or end the batch if the saftey limit switch is triggered
                    while (driver.Send(new DigitalReadRequest(properties.StillLowSwitch)).PinValue.ToString() == "Low" && AverageDelta <= 0.02)

                    {
                        Delta1 = StillStats.Rows[StillStats.Rows.Count - 19];
                        Delta2 = StillStats.Rows[StillStats.Rows.Count - 1];
                        Temp1 = Delta1.Field<Int32>("Temperature");
                        Temp2 = Delta2.Field<Int32>("Temperature");
                        AverageDelta = ((Temp2 - Temp1) / Temp2);

                        System.Threading.Thread.Sleep(10000);
                        CurrentTemp = Convert.ToInt32(driver.Send(new AnalogReadRequest(properties.SensorColumnTemp)).PinValue.ToString());
                        CurrentDelta = CurrentTemp - LastRow.Field<Int32>("Temperature");
                        row = StillStats.NewRow();
                        row["Time"] = DateTime.Now;
                        row["Temperature"] = CurrentTemp;
                        row["TemperatureDelta"] = CurrentDelta;
                        row["Pressure"] = driver.Send(new AnalogReadRequest(properties.SensorPressure)).PinValue.ToString();
                        StillStats.Rows.Add(row);
                        LastRow = StillStats.Rows[StillStats.Rows.Count - 1];
                    }
                } while (true);
            });


           



            ////LEFT OFF HERE
            ////Keep running unless a stop condition is hit
            while (1 == 1)
            {
              
            }
        }
        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
