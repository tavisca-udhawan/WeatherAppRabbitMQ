using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using Tavisca.WeatherApp.Service;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;

namespace Worker
{
    class WeatherAppWorker
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            CityNameRequest request = new CityNameRequest();
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "weatherApp",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");
              
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                    

                    Console.WriteLine(" [x] Done");
                    string outputjson = message.Replace("\\", "");
                    WeatherAppJsonRequest obj = JsonConvert.DeserializeObject<WeatherAppJsonRequest>(outputjson);
                     request.cityName = obj.Init_request;
                    GetReport(request);
                    //  channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: "weatherApp",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
            }
        public static void GetReport(CityNameRequest request)
        {
            WeatherAppService weatherAppService = new WeatherAppService();
            var response = weatherAppService.GetReportByCityName(request);
        }
    }
}
