using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Service.Queues
{
    public class RabbitMQDataStore : IQueue
    {
        public WeatherReportByCityNameInitResponse Enqueue(WeatherReportByCityNameInitResponse data, CityNameRequest request)
        {
            WeatherAppJsonRequest jsonRequest = new WeatherAppJsonRequest();
            jsonRequest.SessionId = data.SessionId;
            jsonRequest.Init_request = request.cityName;
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            using (var connection = factory.CreateConnection())
            using (var senderRequest = connection.CreateModel())
            {
                senderRequest.QueueDeclare(
                               queue: "weatherApp",
                               durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
                string message = JsonConvert.SerializeObject(jsonRequest);
                var body = Encoding.UTF8.GetBytes(message);
                senderRequest.BasicPublish(exchange: "", routingKey: "weatherApp", basicProperties: null, body: body);
                Console.WriteLine("Message has been sent successfully");
            }
            return new WeatherReportByCityNameInitResponse
            {
                SessionId = data.SessionId
            };
        }
    }
}
