namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class SerializerSettings
    {
        public DateFormatHandling DateFormat { get; private set; }

        public Formatting Formatting { get; private set; }

        public SerializerSettings(DateFormatHandling dateFormat = DateFormatHandling.MicrosoftDateFormat, Formatting formatting = Formatting.None)
        {
            Formatting = formatting;
            DateFormat = dateFormat;
        }
    }
}
