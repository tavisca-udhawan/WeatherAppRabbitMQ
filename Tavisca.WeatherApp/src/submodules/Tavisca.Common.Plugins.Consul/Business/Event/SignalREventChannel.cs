using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Configuration.Service;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.ExceptionManagement;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Common.Plugins.Configuration
{
    public class SignalREventChannel : IEventChannel
    {
        private readonly static HubConnection _connection = new HubConnection(_hubAddress);

        private static System.Threading.Timer _connectionPingTimer;

        private static string _serviceAddress
        {
            get
            {

                var url = ConfigurationManager.AppSettings["ConsulServiceURL"];

                if (string.IsNullOrWhiteSpace(url))
                    url = "net.pipe://localhost/EventService/EventService";
                return url;
            }
        }

        private static string _hubAddress
        {
            get
            {

                var url = ConfigurationManager.AppSettings["EventServerURL"];

                if (string.IsNullOrWhiteSpace(url))
                    url = "http://localhost:8767/";
                return url;
            }
        }

        private IHubProxy _applicationEventHub;

        private static int _connectionErrorCount;

        IEventChannel _eventBus { get; set; }

        IJsonSerializer _serializer { get; set; }

        readonly object _padLock = new object();

        public SignalREventChannel() : this(new RXEventChannel(), new JsonSerializer())
        {

        }

        public SignalREventChannel(IEventChannel eventBus, IJsonSerializer serializer)
        {
            _eventBus = eventBus;
            _serializer = serializer;
            SetServerPing();
        }

        private void SetServerPing()
        {
            if (_connectionPingTimer == null)
                _connectionPingTimer = new System.Threading.Timer(o => TryCreateConnection(), null, 10, 1000);
        }

        private void TryCreateConnection()
        {

            if (_connection.State.Equals(ConnectionState.Disconnected))

                lock (_padLock)
                {
                    if (_connection.State.Equals(ConnectionState.Disconnected))
                    {
                        _applicationEventHub = _connection.CreateHubProxy("ApplicationEventHub");

                        _connection.Start().ContinueWith(task =>
                        {
                            if (task.IsFaulted)
                            {
                                _connectionErrorCount++;

                                if (_connectionErrorCount > 5)
                                {
                                    var exception = new SignalRConnectionException("999", "Error occurred while connecting to SignalR Hub", System.Net.HttpStatusCode.InternalServerError);
                                    Platform.Common.ExceptionPolicy.HandleException(exception, Policies.LogOnlyPolicy);

                                }
                            }
                            else
                            {
                                _connectionErrorCount = 0;
                            }


                        }).Wait();

                        _applicationEventHub.On<string>("postEvent", payload =>
                        {
                            Invoke(payload);
                        });
                    }
                }
        }

        private void Invoke(string payload)
        {
            try
            {
                LogIncomingMessage(payload);
                var eventData = (DecodeData(payload));
                _eventBus.Notify(eventData);
            }
            catch (Exception exception)
            {
                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
            }
        }

        private void LogIncomingMessage(string payload)
        {
            try
            {
                var log = new ApiLog
                {
                    ApplicationName = "signalr-hub",                    
                    Verb = "signalr-inbound-message",
                    Request = new Payload(payload)
                };
                log.SetValue("title", "received-user-event");

                var task = Task.Run(async () => {
                     await Logger.WriteLogAsync(log);
                });

                task.Wait(); // 

            }
            catch (Exception exception)
            {

                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
            }
        }

        private void LogOutgoingMessage(string payload)
        {
            try
            {
                // Log the payload
                var log = new ApiLog
                {
                    ApplicationName = "signalr-hub",
                    Verb = "signalr-outbound-message",
                    Request = new Payload(payload)

                };
                log.SetValue("title", "sent-user-event");

                var task = Task.Run(async () => {
                    await Logger.WriteLogAsync(log);
                });

                task.Wait(); // 

            }
            catch (Exception exception)
            {

                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
            }
        }

        public void Notify(ApplicationEvent eventData)
        {
            try
            {
                using (var client = GetChannel())
                {
                    string payload = EncodeData(eventData);
                    LogOutgoingMessage(payload);                    
                    client.Publish(payload);
                  
                }
            }
            catch (Exception exception)
            {
                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
            }
        }

        public IObservable<ApplicationEvent> GetChannel(string eventName)
        {
            return null;
            // DO Nothing
        }

        private string EncodeData(ApplicationEvent eventData)
        {

            if (eventData == null)
                return string.Empty;


            try
            {
                var data = _serializer.Serialize(eventData);
                var utf8WithoutBom = new UTF8Encoding(false);
                var plainTextBytes = utf8WithoutBom.GetBytes(data);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch (Exception e)
            {

                throw e;
            }

        }

        private ApplicationEvent DecodeData(string eventData)
        {

            if (string.IsNullOrWhiteSpace(eventData))
                return null;


            try
            {
                string decodedString = string.Empty;
                string jsonString = TryDecodeBase64(eventData, out decodedString) ? decodedString : eventData;
                return _serializer.Deserialize<ApplicationEvent>(jsonString);
            }
            catch (Exception ex)
            {
                throw Errors.ServerSide.FailedToDecodePayloadInConfigurationClient();
            }
        }

        private bool TryDecodeBase64(string eventData, out string decodedString)
        {
            try
            {
                var base64EncodedBytes = System.Convert.FromBase64String(eventData);
                var utf8WithoutBom = new UTF8Encoding(false);
                decodedString = utf8WithoutBom.GetString(base64EncodedBytes);
                return true;
            }
            catch (Exception)
            {
                decodedString = string.Empty;
                return false;
            }
        }

        private static EventServiceClient GetChannel()
        {
            return new EventServiceClient(new NetNamedPipeBinding() { SendTimeout = new TimeSpan(0, 10, 0) }
                                                , new EndpointAddress(_serviceAddress));



        }

    }
}
