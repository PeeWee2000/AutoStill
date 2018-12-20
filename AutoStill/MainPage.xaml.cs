using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Gpio;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AutoStill
{

    public sealed partial class MainPage : Page
    {
        GpioPin _pin;
        GpioPin _pin2;

        public MainPage()
        {
            this.InitializeComponent();
            this.Dat_Boi.Text = "Auto Still";
            Loaded += MainPage_Loaded;

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)

        {

            var controller = GpioController.GetDefault();

            _pin = controller.OpenPin(26);

            _pin.SetDriveMode(GpioPinDriveMode.Output);

            _pin.Write(GpioPinValue.High);

            _pin2 = controller.OpenPin(21);

            _pin2.SetDriveMode(GpioPinDriveMode.Output);

            _pin2.Write(GpioPinValue.High);

        }
        private BackgroundWorker _worker = null;
        private BackgroundWorker _worker2 = null;


        private void LED_ON_Click(object sender, RoutedEventArgs e)
        {
                _worker = new BackgroundWorker();
                _worker.WorkerSupportsCancellation = true;

                _worker.DoWork += new DoWorkEventHandler((state, args) =>
                {
                    do
                    {
                        if (_worker.CancellationPending)
                            break;
                        _pin.Write(GpioPinValue.High);
                        Task.Delay(1000).Wait();
                        _pin.Write(GpioPinValue.Low);
                        Task.Delay(1000).Wait();


                    } while (true);
                });

                _worker.RunWorkerAsync();
                LED_ON.IsEnabled = false;
                LED_OFF.IsEnabled = true;


        }

        private void LED_OFF_Click(object sender, RoutedEventArgs e)
        {
            LED_OFF.IsEnabled = false;
            LED_ON.IsEnabled = true;
            _worker.CancelAsync();

        }

        private void LED2_ON_Click(object sender, RoutedEventArgs e)
        {
            _worker2 = new BackgroundWorker();
            _worker2.WorkerSupportsCancellation = true;

            _worker2.DoWork += new DoWorkEventHandler((state, args) =>
            {
                do
                {
                    if (_worker2.CancellationPending)
                        break;
                    _pin2.Write(GpioPinValue.High);
                    Task.Delay(1000).Wait();
                    _pin2.Write(GpioPinValue.Low);
                    Task.Delay(1000).Wait();


                } while (true);
            });

            _worker2.RunWorkerAsync();
            LED2_ON.IsEnabled = false;
            LED2_OFF.IsEnabled = true;
        }

        private void LED2_OFF_Click(object sender, RoutedEventArgs e)
        {
            LED2_OFF.IsEnabled = false;
            LED2_ON.IsEnabled = true;
            _worker2.CancelAsync();
        }

        private void temp()
        {

        }
    }
}

