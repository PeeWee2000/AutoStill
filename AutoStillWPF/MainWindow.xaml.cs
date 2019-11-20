using LiveCharts;
using LiveCharts.Wpf;
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

            Main.TemperatureChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Column Temperature",
                    Values = new ChartValues<decimal>(),
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Still Fluid Temperature",
                    Values = new ChartValues<decimal>(),
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Theoretical Boiling Point",
                    Values = new ChartValues<decimal>(),
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Pressure",
                    Values = new ChartValues<decimal>(),
                    PointGeometry = null
                }
            };


            var UIUpdater = new BackgroundWorker();
            UIUpdater.WorkerSupportsCancellation = true;
            UIUpdater.DoWork += new DoWorkEventHandler((state, args) =>
            {
                string Status;

                while (true)
                {
                    var CurrentState = StillController.CurrentState;
                    switch (CurrentState.Phase)
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

                    CurrentState.TheoreticalBoilingPoint = BoilingPointCalculator.Functions.GetWaterBoilingPoint(CurrentState.Pressure * 1000);
                    decimal MeOHBoilingPoint = BoilingPointCalculator.Functions.GetMeOHBoilingPoint(CurrentState.Pressure * 1000);
                    decimal EtOHBoilingPoint = BoilingPointCalculator.Functions.GetEtOHBoilingPoint(CurrentState.Pressure * 1000);
                    MainDispatcher.Invoke(new Action(() => {
                        //Main.TemperatureChart.Series[0].Values.Add(CurrentState.ColumnTemp);
                        //Main.TemperatureChart.Series[1].Values.Add(CurrentState.StillFluidTemp);
                        //Main.TemperatureChart.Series[2].Values.Add(CurrentState.TheoreticalBoilingPoint);
                        //Main.TemperatureChart.Series[3].Values.Add(CurrentState.Pressure);

                        Main.PressureGauge.Value = Convert.ToDouble(CurrentState.Pressure);

                        Main.lblPressure.Content = CurrentState.Pressure + "kPa";
                        Main.lblTheoretical1.Content = "Water : " + CurrentState.TheoreticalBoilingPoint + "°C";
                        Main.lblTheoretical2.Content = "Ethanol : " + EtOHBoilingPoint + "°C";
                        Main.lblTheoretical3.Content = "Methanol : " + MeOHBoilingPoint + "°C";
                        Main.lblColumnTemp.Content = CurrentState.ColumnTemp + "°C";
                        Main.lblStillTemp.Content = CurrentState.StillFluidTemp + "°C";
                        Main.lblRefluxTemp.Content = CurrentState.RefluxTemp + "°C";
                        Main.lblStillEmpty.Content = CurrentState.StillEmpty.ToString();
                        Main.lblStillFull.Content = CurrentState.StillFull.ToString();
                        Main.lblRVEmpty.Content = CurrentState.RVEmpty.ToString();
                        Main.lblRVFull.Content = CurrentState.RVFull.ToString();
                        Main.lblStatus.Content = Status;
                    }));
                    Thread.Sleep(250);
                }
            });
            UIUpdater.RunWorkerAsync();
        }
    }
}
