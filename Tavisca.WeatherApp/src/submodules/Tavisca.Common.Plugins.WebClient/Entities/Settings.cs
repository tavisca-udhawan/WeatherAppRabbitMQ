namespace Tavisca.Common.Plugins.WebClient
{
    public struct Settings
    {
        /// <summary>
        /// This is a optional field to specify the timeout for the client to wait for the response. If not set default value is 100s
        /// </summary>
        public int ? TimeOut { get; set; }

        public override int GetHashCode()
        {
            if (!TimeOut.HasValue)
                return int.MinValue;
            return TimeOut.Value; // Update the GetHashCode logic if new settings are added
        }

        public override bool Equals(object obj)
        {
            var settings = (Settings)obj;
            return settings.TimeOut.Equals(TimeOut); // Update the Equals logic if new settings are added
        }
    }
}
