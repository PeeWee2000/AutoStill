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
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
        }
    }

