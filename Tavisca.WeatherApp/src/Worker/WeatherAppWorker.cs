using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using Tavisca.WeatherApp.Core;
using Tavisca.WeatherApp.Service;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;
using Tavisca.WeatherApp.Service.FileSystem;
using Tavisca.WeatherApp.Service.Store;
using Microsoft.Extensions.DependencyInjection;
using Tavisca.WeatherApp.Model.Interfaces;
using System.Threading.Tasks;
using Tavisca.WeatherApp.Models.Interfaces;
using Tavisca.WeatherApp.Model;

namespace Worker
{
    public class WeatherAppWorker
    {
        public static void Main()
        {
            var services = new ServiceCollection()
                .AddTransient<IWeatherApp, WeatherApp>()
                .AddTransient<ISessionStore, SessionStore>()
                .AddTransient<IFileOperations, ReadFile>()
                .AddTransient<IFileInit, FileSystem>()
                .AddTransient<IWeatherReportResponseService, WeatherReportResponseService>();
            var serviceProvider = services.BuildServiceProvider();

            // resolve the dependency graph
            var appService = serviceProvider.GetService<IWeatherReportResponseService>();

            // run the application
            appService.RunAsync().Wait();

            }
    }

    public interface IWeatherReportResponseService
    {
        Task RunAsync();
        WeatherReportResponse GetReport(CityNameRequest request);
    }


    
    public class WeatherReportResponseService: IWeatherReportResponseService
    {
        private readonly IWeatherApp _weatherApp;
        private readonly ISessionStore _sessionStore;
        private readonly ISessionStore _busStore;
        private readonly IFileOperations _readFile;

        public WeatherReportResponseService(IWeatherApp weatherApp, ISessionStore sessionStore,ISessionStore busStore,IFileOperations operations)
        {
            _weatherApp = weatherApp;
            _sessionStore = sessionStore;
            _busStore = busStore;
            _readFile = operations;
        }
        public WeatherReportResponse GetReport(CityNameRequest request)
        {
            WeatherAppService weatherAppService = new WeatherAppService(_weatherApp, _sessionStore, _busStore, _readFile);
            WeatherReportResponse response = weatherAppService.GetReportByCityName(request);
            return response;
        }

        public Task RunAsync()
        {
            return Task.Run(() => {
            var factory = new ConnectionFactory() { HostName = "localhost" };
                CityNameRequest request = new CityNameRequest();
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "weatherApp",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                   

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

                    WeatherReportResponse WeatherResponse = GetReport(request);
                    ReadFile file = new ReadFile();
                    file.WriteToFile(WeatherResponse, obj.SessionId);
                };
                channel.BasicConsume(queue: "weatherApp",
                                     autoAck: false,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            
            }
            });
        }
    }
}
