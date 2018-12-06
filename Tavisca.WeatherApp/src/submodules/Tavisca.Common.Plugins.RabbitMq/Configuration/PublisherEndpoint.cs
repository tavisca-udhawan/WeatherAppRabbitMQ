namespace Tavisca.Common.Plugins.RabbitMq.Configuration
{
    public class PublisherEndpoint : Endpoint
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
    }
}
