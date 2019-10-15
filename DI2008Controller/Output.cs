namespace DI2008Controller
{
    public struct AnalogData
    {
        public ChannelConfiguration ChannelConfiguration { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }

    public enum DigitalState
    {
        Low = 0,
        High = 1
    }


    /// <summary>
    /// Contains a propety for each channel available for use on the Dataq, channels not enabled will return a null
    /// </summary>
    public class ReadRecord 
    {
        public ReadRecord() { }

        public  AnalogData? Analog0 { get; set; }
        public  AnalogData? Analog1 { get; set; }
        public  AnalogData? Analog2 { get; set; }
        public  AnalogData? Analog3 { get; set; }
        public  AnalogData? Analog4 { get; set; }
        public  AnalogData? Analog5 { get; set; }
        public  AnalogData? Analog6 { get; set; }
        public  AnalogData? Analog7 { get; set; }
        public DigitalState? Digital0 { get; set; }
        public DigitalState? Digital1 { get; set; }
        public DigitalState? Digital2 { get; set; }
        public DigitalState? Digital3 { get; set; }
        public DigitalState? Digital4 { get; set; }
        public DigitalState? Digital5 { get; set; }
        public DigitalState? Digital6 { get; set; }
    }
}
