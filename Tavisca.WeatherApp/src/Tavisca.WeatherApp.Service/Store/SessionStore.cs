using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.WeatherApp.Models.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Service.Store
{
    public class SessionStore : ISessionStore
    {
        private readonly IFileInit _fileInit; 
        public SessionStore(IFileInit fileInit)
        {
            this._fileInit = fileInit;
        }
        public WeatherReportByCityNameInitResponse CreateFile(string sessionId)
        {
            WeatherReportByCityNameInitResponse data = _fileInit.CreateInitFile(sessionId);
            return new WeatherReportByCityNameInitResponse
            {
                SessionId = data.SessionId
            };
        }

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