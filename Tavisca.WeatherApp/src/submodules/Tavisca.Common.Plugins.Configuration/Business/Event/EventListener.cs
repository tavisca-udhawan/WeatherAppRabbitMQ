using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.ExceptionManagement;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Common.Plugins.Configuration
{
    public class EventListener : IEventChannel
    {
        private readonly static TcpListenerEx _conenction = new TcpListenerEx(IPAddress.Parse(_ipAddress),_port);

        private static string _ipAddress
        {
            get
            {
                return "127.0.0.1";
            }
        }

        private static int _maxCharRead
        {
            get
            {
                return 2046;
            }
        }

        private static int _port
        {
            get
            {
                return 8991;
            }
        }
        IEventChannel _eventBus { get; set; }

        IJsonSerializer _serializer { get; set; }

        private readonly object _padLock = new object();

        public EventListener() : this(new RXEventChannel(), new JsonSerializer())
        {

        }

        public EventListener(IEventChannel eventBus, IJsonSerializer serializer)
        {         
            _eventBus = eventBus;
            _serializer = serializer;         
            Task.Run(() => CreateConnection());
        }

        private void CreateConnection()
        {
            if (!_conenction.IsActive)
            {
                lock (_padLock)
                {
                    if (!_conenction.IsActive)
                    {
                        _conenction.Start();
                        var payload = string.Empty;
                        var childTask = new Task(async () =>
                        {
                            while (true)
                            {
                                using (var client = await _conenction.AcceptTcpClientAsync())
                                {
                                    using (var stream = client.GetStream())
                                    using (var streamReader = new StreamReader(stream))
                                    {
                                        var data = new char[_maxCharRead];
                                        var dataLength = await streamReader.ReadAsync(data, 0, _maxCharRead);
                                        payload = new string(data, 0, dataLength);
                                        Invoke(payload);
                                    }
                                }
                            }
                        });

                        childTask.ContinueWith(task => {
                            if (task.IsFaulted || task.IsCanceled)
                            {
                                var innerException = task.Exception.GetBaseException();
                                //remove this and convert to tcp connection exception. create your own.
                                var exception = innerException;
                                exception.Data.Add("message", "failed to aquire port " + _port);
                                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
                                
                            }
                        });

                        childTask.Start();
                    }
                }
            }
        }        

        private void Invoke(string payload)
        {
            try
            {
                LogIncomingMessage(payload);
                var eventData = DecodeData(payload);
                _eventBus.Notify(eventData);
            }
            catch (Exception exception)
            {
                exception.Data.Add("message", "subscribres invocation failed");
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
                               
                Logger.WriteLogAsync(log);
               
            }
            catch (Exception exception)
            {
                exception.Data.Add("message", "subscribres invocation failed");
                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
            }
        }

        public void Notify(ApplicationEvent eventData)
        {
            try
            {
                var userEvent = new Consul.UserEvent()
                {
                    ID = eventData.Id,
                    Name = "application-event",
                    Payload = EncodeData(eventData)
                };
                ConsulClientFactory.Instance.Event.Fire(userEvent);
            }
            catch (Exception exception)
            {
                exception.Data.Add("message", "event notification failed");
                Platform.Common.ExceptionPolicy.HandleException(exception, Policies.DefaultPolicy);
            }
        }

        public IObservable<ApplicationEvent> GetChannel(string eventName)
        {
            return null;
            // DO Nothing
        }

        private byte[] EncodeData(ApplicationEvent eventData)
        {

            if (eventData == null)
                return new byte[0];

            try
            {
                var data = _serializer.Serialize(eventData);
                var utf8WithoutBom = new UTF8Encoding(false);
                return utf8WithoutBom.GetBytes(data);
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
                var base64EncodedBytes = System.Convert.FromBase64String(eventData);
                var utf8WithoutBom = new UTF8Encoding(false);
                var jsonString = utf8WithoutBom.GetString(base64EncodedBytes);
                return _serializer.Deserialize<ApplicationEvent>(jsonString);
            }
            catch (Exception ex)
            {
                ex.Data.Add("message", "eventData decoding failed");
                Platform.Common.ExceptionPolicy.HandleException(ex, Policies.DefaultPolicy);
                return null;
            }
        }
    }
}
