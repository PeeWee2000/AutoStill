using FTD2XX_NET;
using System;
using System.Collections.Generic;

namespace RelayController
{
    public class RelayBoard
    {
        public static FTDI myFtdiDevice = new FTDI();
        public static FTDI.FT_STATUS ftStatus;
        public static byte[] sentBytes = new byte[2];
        public static uint receivedBytes;
        public static byte[] Command = new byte[2];
        public static List<Tuple<int, bool>> RelayStatuses = new List<Tuple<int, bool>>();
        public static int Relays = 8;
        public static int TotalBits = (int)Math.Pow(2, Relays) - 1;


        public RelayBoard()
        {

            //Get serial number of device with index 0
            ftStatus = myFtdiDevice.OpenByIndex(0);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                return;
            }
            //Reset device
            ftStatus = myFtdiDevice.ResetDevice();
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                return;
            }
            //Set Baud Rate
            ftStatus = myFtdiDevice.SetBaudRate(921600);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                return;
            }
            //Set Bit Bang
            ftStatus = myFtdiDevice.SetBitMode(255, FTDI.FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                return;
            }


            for (int i = 0; i <= 8; i++)
            { DisableRelay(i); }


            //int x = 1;
            //while (true)
            //{

            //    while(x <= 8)
            //    { 
            //        EnableRelay(x);
            //        x++;
            //    }
            //    x = 1;
            //    Thread.Sleep(250);

            //    while (x <= 8)
            //    { 
            //    DisableRelay(x);
            //        x++;
            //    }

            //    x = 1;
            //    Thread.Sleep(250);
            //}




        }

        public void EnableRelay(int RelayID)
        {
            Command[0] = (byte)(Command[0]  | GetBitPosition(RelayID));
            myFtdiDevice.Write(Command, 1, ref receivedBytes);


        }
        public void DisableRelay(int RelayID)
        {
            Command[0] = (byte)(Command[0] & (TotalBits - GetBitPosition(RelayID)));
            myFtdiDevice.Write(Command, 1, ref receivedBytes);
        }

        public static int GetBitPosition(int RelayID)
        {
            int BitPosition = 1;
            for (int i = 1; i < RelayID; i++)
            { BitPosition *= 2; }
            return BitPosition;
        }

        public static void GetRelayStates()
        {
            RelayStatuses.Clear();

            int BitMultiplier = 1;
            for (int i = 0; i < Relays; i++)
            {
                bool Status = ((sentBytes[0] & BitMultiplier) == 0) ? false : true;
                var RelayStatus = new Tuple<int, bool>(i, Status);
                RelayStatuses.Add(RelayStatus);
                BitMultiplier *= 2;
            }
        }

    }
}
