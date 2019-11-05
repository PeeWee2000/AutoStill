using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace AutoStillWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {           
            InitializeComponent();
            Run();
        }
        public static void Run()
        {
            StillController.Start();
            var Main = ((MainWindow)Application.Current.MainWindow);
            Dispatcher MainDispatcher = Dispatcher.CurrentDispatcher;


            var UIUpdater = new BackgroundWorker();
            UIUpdater.WorkerSupportsCancellation = true;
            UIUpdater.DoWork += new DoWorkEventHandler((state, args) =>
            {
                


                string Status;

                while (true)
                {
                    switch (StillController.CurrentState.Phase)
                    {
                        case 0:
                            Status = "Filling Still";
                            break;
                        case 1:
                            Status = "Heating";
                            break;
                        case 2:
                            Status = "Distilling";
                            break;
                        case 3:
                            Status = "Draining Vessels";
                            break;
                        default:
                            Status = "Idle";
                            break;
                    }

                    StillController.CurrentState.TheoreticalBoilingPoint = BoilingPointCalculator.Functions.GetWaterBoilingPoint(StillController.CurrentState.Pressure * 1000);


                    while (true)
                    {
                        MainDispatcher.Invoke(new Action(() => {
                            Main.lblPressure.Content = StillController.CurrentState.Pressure + "kPa";
                            Main.lblTheoretical.Content = StillController.CurrentState.TheoreticalBoilingPoint + "°C";
                            Main.lblColumnTemp.Content = StillController.CurrentState.ColumnTemp + "°C";
                            Main.lblStillTemp.Content = StillController.CurrentState.StillFluidTemp + "°C";
                            Main.lblRefluxTemp.Content = StillController.CurrentState.RefluxTemp + "°C";
                            Main.lblStillEmpty.Content = StillController.CurrentState.StillEmpty.ToString();
                            Main.lblStillFull.Content = StillController.CurrentState.StillFull.ToString();
                            Main.lblRVEmpty.Content = StillController.CurrentState.RVEmpty.ToString();
                            Main.lblRVFull.Content = StillController.CurrentState.RVFull.ToString();
                            Main.lblStatus.Content = Status;
                        }));
                        Thread.Sleep(250);
                    }

                }
            });

            UIUpdater.RunWorkerAsync();
        }

    }
}
