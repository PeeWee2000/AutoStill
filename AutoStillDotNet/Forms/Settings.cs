using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoStillDotNet.Forms
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            FillPeriphrials();
        }

        private void FillPeriphrials()
        {
            foreach (string Key in ConfigurationManager.AppSettings.AllKeys)
            {
                string Value = ConfigurationManager.AppSettings.Get(Key);
                if (Regex.IsMatch(Value, @"Type=(\D+)(\w+)Pin;") == true)
                {
                    cboPeriphrials.Items.Add(Key);
                } 
            }
        }

        //private void rbImperial_Checked(object sender, EventArgs e)
        //{
        //    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //    configuration.AppSettings.Settings["Units"].Value = "Imperial";
        //    configuration.Save(ConfigurationSaveMode.Full, true);
        //    ConfigurationManager.RefreshSection("appSettings");
        //}

        //private void RbMetric_CheckedChanged(object sender, EventArgs e)
        //{
        //    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //    configuration.AppSettings.Settings["Units"].Value = "Metric";
        //    configuration.Save(ConfigurationSaveMode.Full, true);
        //    ConfigurationManager.RefreshSection("appSettings");
        //}

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
