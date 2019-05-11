using System.Data.SqlClient;
using System.Configuration;
using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace AutoStillDotNet
{
    public static class SystemProperties
    {
        private static byte GetPin(String Periphrial)
        {
            try
            { return Convert.ToByte(Regex.Match(ConfigurationManager.AppSettings.Get(Periphrial), @"(?<=Pin=)\d+").Value); }
            catch
            { MessageBox.Show("Invalid pin number for " + Periphrial + ", check app config pin settings");  return 1; }
        }

        private static void WriteSetting()
        { }

        public static byte FVEmptySwtich { get; set; } = GetPin("fvemptyswitch");
        public static byte FVCompleteSwitch { get; set; } = GetPin("fvcompleteswitch");
        public static byte StillFluidPump { get; set; } = GetPin("stillfluidpump");
        public static byte StillFillValve { get; set; } = GetPin("stillfillvalve");
        public static byte StillLowSwitch { get; set; } = GetPin("stilllowswitch");
        public static byte StillHighSwitch { get; set; } = GetPin("stillhighswitch");
        public static byte StillElement { get; set; } = GetPin("stillelement");
        public static byte StillDrainValve { get; set; } = GetPin("stilldrainvalve");
        public static byte RVFluidPump { get; set; } = GetPin("rvfluidpump");
        public static byte RVDrainValve { get; set; } = GetPin("rvdrainvalve");
        public static byte RVFullSwitch { get; set; } = GetPin("rvfullswitch");
        public static byte RVEmptySwitch { get; set; } = GetPin("rvemptyswitch");
        public static byte VacuumPump { get; set; } = GetPin("vacuumpump");
        public static byte FanController1 { get; set; } = GetPin("fancontroller1");
        public static byte FanController2 { get; set; } = GetPin("fancontroller2");
        public static byte SensorPressure { get; set; } = 55;
        public static byte SensorColumnTemp { get; set; } = 54;
        public static byte SensorCoolantTemp1 { get; set; } = GetPin("sensorcoolanttemp1");
        public static byte SensorCoolantTemp2 { get; set; } = GetPin("sensorcoolanttemp2");
        public static byte SensorElementAmperage { get; set; } = GetPin("sensorelementamperage");
        public static double TargetPressure { get; set; } = -5;
        public static double TgtPresHysteresisBuffer { get; set; } = 0.5;
        public static string Units { get; set; } = "Imperial";
        public static SqlConnection sqlconnection { get; set; } = new SqlConnection(ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString);

    }
}
