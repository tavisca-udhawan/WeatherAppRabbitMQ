namespace Tavisca.Common.Plugins.RabbitMq.Configuration
{
    public class Endpoint
    {
        public string VirtualHost { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public override int GetHashCode()
        {
            return string.Format("{0}-{1}-{2}-{3}", HostName, VirtualHost, UserName, Password).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var otherEndpoint = obj as Endpoint;
            if (otherEndpoint == null)
                return false;

            return HostName.Equals(otherEndpoint.HostName) && VirtualHost.Equals(otherEndpoint.VirtualHost)
                && UserName.Equals(otherEndpoint.UserName) && Password.Equals(otherEndpoint.Password);
        }
    }
}
