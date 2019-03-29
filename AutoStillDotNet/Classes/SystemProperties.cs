using System.Data.SqlClient;
using System.Configuration;
using System;
using System.Windows.Forms;

namespace AutoStillDotNet
{
    public static class SystemProperties
    {

        ////ACTUAL VALUES
        ////Digital IO Pins
        //private static byte fvemptyswitch = 1;
        //private static byte fvcompleteswitch = 1;
        //private static byte stillfluidpump = 30;
        //private static byte stillfillvalve = 7 + 21;
        //private static byte stilllowswitch = 49;
        //private static byte stillhighswitch = 51;
        //private static byte stillelement = 1;
        //private static byte stilldrainvalve = 1;
        //private static byte rvfluidpump = 1;
        //private static byte rvdrainvalve = 1;
        //private static byte rvfullswitch = 1;
        //private static byte vacuumpump = 6 + 21;
        //private static byte fanset1 = 1;
        //private static byte fanset2 = 1;
        //private static byte fancontroller1 = 1;
        //private static byte fancontroller2 = 1;

        ////Analog IO Pins
        //private static byte sensorpressure = 55;
        //private static byte sensorcolumntemp = 54;

        //DEV VALUES
        //Digital IO Pins
        //private string test = ConfigurationManager.AppSettings.Get("fvemptyswitch");
        private static byte fvemptyswitch = GetPin("fvemptyswitch");
        private static byte fvcompleteswitch = GetPin("fvcompleteswitch");
        private static byte stillfluidpump = GetPin("stillfluidpump");
        private static byte stillfillvalve = GetPin("stillfillvalve");
        private static byte stilllowswitch = GetPin("stilllowswitch");
        private static byte stillhighswitch = GetPin("stillhighswitch");
        private static byte stillelement = GetPin("stillelement");
        private static byte stilldrainvalve = GetPin("stilldrainvalve");
        private static byte rvfluidpump = GetPin("rvfluidpump");
        private static byte rvdrainvalve = GetPin("rvdrainvalve");
        private static byte rvfullswitch = GetPin("rvfullswitch");
        private static byte rvemptyswitch = GetPin("rvemptyswitch");
        private static byte vacuumpump = GetPin("vacuumpump");
        private static byte fanset1 = GetPin("fanset1");
        private static byte fanset2 = GetPin("fanset2");
        private static byte fancontroller1 = GetPin("fancontroller1");
        private static byte fancontroller2 = GetPin("fancontroller2");
        private static byte sensorcoolanttemp1 = GetPin("sensorcoolanttemp1");
        private static byte sensorcoolanttemp2 = GetPin("sensorcoolanttemp2");
        private static byte sensorelementamperage = GetPin("sensorelementamperage");

        

        //Analog IO Pins
        private static byte sensorpressure = 55;
        private static byte sensorcolumntemp = 54;

        //System Targets (I.E. Target Pressure to Maintain)
        private static double targetpressure = -10; //Target value in metric or imperial
        private static double tgtpreshysteresisbuffer = 0.5; //How far under the target to actually pump until to prevent the pump from turning on and off rapidly


        //System Settings
        private static string units = "Imperial";

        private static byte GetPin(String Periphrial)
        {
            try
            { return Convert.ToByte(ConfigurationManager.AppSettings.Get(Periphrial)); }
            catch
            { MessageBox.Show("Invalid pin number for " + Periphrial + ", check app config pin settings");  return 1; }
        }

        public static byte FVEmptySwtich
        {
            get
            {
                return fvemptyswitch;
            }
            set
            {
                
                    fvemptyswitch = value;
            }
        }
        public static byte FVCompleteSwitch
        {
            get
            {
                return fvcompleteswitch;
            }
            set
            {
                
                    fvcompleteswitch = value;
            }
        }
        public static byte StillFluidPump
        {
            get
            {
                return stillfluidpump;
            }
            set
            {
                
                    stillfluidpump = value;
            }
        }
        public static byte StillFillValve
        {
            get
            {
                return stillfillvalve;
            }
            set
            {
                
                    stillfillvalve = value;
            }
        }
        public static byte StillLowSwitch
        {
            get
            {
                return stilllowswitch;
            }
            set
            {
                
                    stilllowswitch = value;
            }
        }
        public static byte StillHighSwitch
        {
            get
            {
                return stillhighswitch;
            }
            set
            {
                
                    stillhighswitch = value;
            }
        }
        public static byte StillElement
        {
            get
            {
                return stillelement;
            }
            set
            {
                
                    stillelement = value;
            }
        }
        public static byte StillDrainValve
        {
            get
            {
                return stilldrainvalve;
            }
            set
            {
                
                    stilldrainvalve = value;
            }
        }
        public static byte RVFluidPump
        {
            get
            {
                return rvfluidpump;
            }
            set
            {
                
                    rvfluidpump = value;
            }
        }
        public static byte RVDrainValve
        {
            get
            {
                return rvdrainvalve;
            }
            set
            {
                
                    rvdrainvalve = value;
            }
        }
        public static byte RVFullSwitch
        {
            get
            {
                return rvfullswitch;
            }
            set
            {
                
                    rvfullswitch = value;
            }
        }
        public static byte RVEmptySwitch
        {
            get
            {
                return rvemptyswitch;
            }
            set
            {
                
                    rvemptyswitch = value;
            }
        }
        public static byte VacuumPump
        {
            get
            {
                return vacuumpump;
            }
            set
            {
                
                    vacuumpump = value;
            }
        }
        public static byte FanSet1
        {
            get
            {
                return fanset1;
            }
            set
            {
                
                    fanset1 = value;
            }
        }
        public static byte FanSet2
        {
            get
            {
                return fanset2;
            }
            set
            {
                
                    fanset2 = value;
            }
        }
        public static byte FanController1
        {
            get
            {
                return fancontroller1;
            }
            set
            {
                
                    fancontroller1 = value;
            }
        }
        public static byte FanController2
        {
            get
            {
                return fancontroller2;
            }
            set
            {
                
                    fancontroller2 = value;
            }
        }
        public static byte SensorPressure
        {
            get
            {
                return sensorpressure;
            }
            set
            {
                
                    sensorpressure = value;
            }
        }
        public static byte SensorColumnTemp
        {
            get
            {
                return sensorcolumntemp;
            }
            set
            {
                
                    sensorcolumntemp = value;
            }
        }
        public static byte SensorCoolantTemp1
        {
            get
            {
                return sensorcoolanttemp1;
            }
            set
            {

                sensorcoolanttemp1 = value;
            }
        }
        public static byte SensorCoolantTemp2
        {
            get
            {
                return sensorcoolanttemp2;
            }
            set
            {

                sensorcoolanttemp2 = value;
            }
        }
        public static byte SensorElementAmperage
        {
            get
            {
                return sensorelementamperage;
            }
            set
            {

                sensorelementamperage = value;
            }
        }
        public static double TargetPressure
        {
            get
            {
                return targetpressure;
            }
            set
            {
                
                    targetpressure = value;
            }
        }
        public static double TgtPresHysteresisBuffer
        {
            get
            {
                return tgtpreshysteresisbuffer;
            }
            set
            {
                
                    tgtpreshysteresisbuffer = value;
            }
        }

        public static string Units
        {
            get
            {
                return units;
            }
            set
            {
                    units = value;
            }
        }

        public static SqlConnection sqlconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString);
        public static SqlConnection sqlConnection
        {

            get
            {
                return sqlconnection;
            }
            set
            {
            }
        }

    }
}
