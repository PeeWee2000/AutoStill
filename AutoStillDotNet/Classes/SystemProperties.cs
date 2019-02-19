using System.Data.SqlClient;
using System.Configuration;

namespace AutoStillDotNet
{
    class SystemProperties
    {
        //Digital IO Pins
        private byte fvemptyswitch = 1;
        private byte fvcompleteswitch = 1;
        private byte stillfluidpump = 30;
        private byte stillfillvalve = 28;
        private byte stilllowswitch = 49;
        private byte stillhighswitch = 51;
        private byte stillelement = 1;
        private byte stilldrainvalve = 1;
        private byte rvfluidpump = 32;
        private byte rvdrainvalve = 31;
        private byte vacuumpump = 27;
        private byte fanset1 = 1;
        private byte fanset2 = 1;
        private byte fancontroller1 = 1;
        private byte fancontroller2 = 1;

        //Analog IO Pins
        private byte sensorpressure = 55;
        private byte sensorcolumntemp = 54;

        //System Targets (I.E. Target Pressure to Maintain)
        private byte targetpressure = 200; //Target value in raw ADC units
        private byte tgtpreshysteresisbuffer = 15; //How far under the target to actually pump until to prevent the pump from turning on and off rapidly
        

        public byte FVEmptySwtich
        {
            get
            {
                return fvemptyswitch;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    fvemptyswitch = value;
                }
            }
        }
        public byte FVCompleteSwitch
        {
            get
            {
                return fvcompleteswitch;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    fvcompleteswitch = value;
                }
            }
        }
        public byte StillFluidPump
        {
            get
            {
                return stillfluidpump;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    stillfluidpump = value;
                }
            }
        }
        public byte StillFillValve
        {
            get
            {
                return stillfillvalve;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    stillfillvalve = value;
                }
            }
        }
        public byte StillLowSwitch
        {
            get
            {
                return stilllowswitch;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    stilllowswitch = value;
                }
            }
        }
        public byte StillHighSwitch
        {
            get
            {
                return stillhighswitch;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    stillhighswitch = value;
                }
            }
        }
        public byte StillElement
        {
            get
            {
                return stillelement;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    stillelement = value;
                }
            }
        }
        public byte StillDrainValve
        {
            get
            {
                return stilldrainvalve;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    stilldrainvalve = value;
                }
            }
        }
        public byte RVFluidPump
        {
            get
            {
                return rvfluidpump;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    rvfluidpump = value;
                }
            }
        }
        public byte RVDrainValve
        {
            get
            {
                return rvdrainvalve;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    rvdrainvalve = value;
                }
            }
        }
        public byte VacuumPump
        {
            get
            {
                return vacuumpump;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    vacuumpump = value;
                }
            }
        }
        public byte FanSet1
        {
            get
            {
                return fanset1;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    fanset1 = value;
                }
            }
        }
        public byte FanSet2
        {
            get
            {
                return fanset2;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    fanset2 = value;
                }
            }
        }
        public byte FanController1
        {
            get
            {
                return fancontroller1;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    fancontroller1 = value;
                }
            }
        }
        public byte FanController2
        {
            get
            {
                return fancontroller2;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    fancontroller2 = value;
                }
            }
        }
        public byte SensorPressure
        {
            get
            {
                return sensorpressure;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    sensorpressure = value;
                }
            }
        }
        public byte SensorColumnTemp
        {
            get
            {
                return sensorcolumntemp;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    sensorcolumntemp = value;
                }
            }
        }


        public byte TargetPressure
        {
            get
            {
                return targetpressure;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    targetpressure = value;
                }
            }
        }
        public byte TgtPresHysteresisBuffer
        {
            get
            {
                return tgtpreshysteresisbuffer;
            }
            set
            {
                if ((value > 0) && (value < 13))
                {
                    tgtpreshysteresisbuffer = value;
                }
            }
        }

        SqlConnection sqlconnection = new SqlConnection(ConfigurationManager.ConnectionStrings["LocalDB"].ConnectionString);
        public SqlConnection sqlConnection
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
