using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class CompositeSinkFixture
    {
        private static readonly string CorrelationId = "{1D7872F8-7753-4BFF-BD5D-A6BAF3EB89CE}";
        private static readonly string StackId = "{55EC6376-8559-4193-9953-01F34C4876E0}";
        private static readonly string TenantId = "unit_test";
        private static readonly string Category = "test_category";
        private static readonly string Message = "test_message";
        private static readonly string App = "unit_test_app";
        private static readonly string TxId = "trx_id";

        [Fact]
        public async Task CompositeSinkWriter_WithNullSink_ShouldReturnWithoutWritingLogs()
        {
            var compositeLogWriter = new CompositeSink(JsonLogFormatter.Instance, null);
            var log = GetApiLog();
            await compositeLogWriter.WriteAsync(log, Format(log));
        }

        [Fact]
        public async Task CompositeSinkWriter_WithSuccessfulWriteInPrimarySink_ShouldNotCallSecondarySink()
        {
            var waitHandle = new ManualResetEvent(false);
            var redisClient = new Mock<ILogSink>();
            redisClient.Setup(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>())).Returns(() =>
            {
                waitHandle.Set();
                return Task.CompletedTask;
            });

            var firehoseClient = new Mock<ILogSink>();
            firehoseClient.Setup(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>())).Returns(() =>
            {
                waitHandle.Set();
                return Task.CompletedTask;
            });

            var compositeLogWriter = new CompositeSink(JsonLogFormatter.Instance, new List<ILogSink> { firehoseClient.Object, redisClient.Object});

            var log = GetApiLog();
            await compositeLogWriter.WriteAsync(log, Format(log));
            waitHandle.WaitOne();

            firehoseClient.Verify(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()), Times.Once);
            redisClient.Verify(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public async Task CompositeSinkWriter_WithUnSuccessfulWriteInPrimarySink_ShouldWriteToSecondarySink()
        {
            var waitHandle = new ManualResetEvent(false);
            var redisClient = new Mock<ILogSink>();
            var redisWriteCount = 0;
            redisClient.Setup(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()))
            .Callback(() => 
            {
                if(redisWriteCount == 1)
                    waitHandle.Set();
                redisWriteCount++;
            })
            .Returns(() =>
            {
                return Task.CompletedTask;
            });

            var firehoseClient = new Mock<ILogSink>();
            firehoseClient.Setup(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>())).Throws(new Exception());

            var compositeLogWriter = new CompositeSink(JsonLogFormatter.Instance, new List<ILogSink> { firehoseClient.Object, redisClient.Object });

            var log = GetApiLog();
            await compositeLogWriter.WriteAsync(log, Format(log));
            waitHandle.WaitOne();

            firehoseClient.Verify(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()), Times.Once);
            redisClient.Verify(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CompositeSinkWriter_WithUnSuccessfullWriteInBothSink_ShouldThrowException()
        {
            var waitHandle = new ManualResetEvent(false);
            var redisClient = new Mock<ILogSink>();
            redisClient.Setup(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>())).Callback(() => waitHandle.Set()).Throws(new Exception());

            var firehoseClient = new Mock<ILogSink>();
            firehoseClient.Setup(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>())).Throws(new Exception());
            
            var compositeLogWriter = new CompositeSink(JsonLogFormatter.Instance, new List<ILogSink> { firehoseClient.Object, redisClient.Object });

            var log = GetApiLog();
            
            await Assert.ThrowsAsync<AggregateException>( async () => await compositeLogWriter.WriteAsync(log, Format(log)));

            waitHandle.WaitOne();

            firehoseClient.Verify(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()), Times.Once);
            redisClient.Verify(x => x.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()), Times.Exactly(2));
        }

        private ApiLog GetApiLog()
        {
            var log = new ApiLog();
            var dt = DateTime.UtcNow;
            var bytes = new UTF8Encoding(false).GetBytes("payload");

            log.ApplicationName = App;
            log.AppDomain = AppDomain.CurrentDomain.FriendlyName;
            log.CorrelationId = CorrelationId;
            log.StackId = StackId;
            log.Message = Message;
            log.TenantId = TenantId;
            log.Request = new Payload("test request");
            log.Response = new Payload("test response");
            log.IsSuccessful = true;
            log.Verb = "POST";
            log.Url = "testurl";
            log.Api = "compositeSink";
            log.TimeTakenInMs = 10.0d;
            log.RequestHeaders["rqheader"] = "RqheaderValue";
            log.ResponseHeaders["rsheader"] = "RsheaderValue";
            log.SetValue("int_field", 1);
            log.SetValue("string_field", "str");
            log.SetValue("dt_field", dt);
            log.SetValue("long_field", 1L);
            log.SetValue("float_field", 1.0f);
            log.SetValue("double_field", 2.0d);
            log.SetValue("decimal_field", 3.0m);
            log.SetValue("bool_field", true);
            log.SetValue("map_field", new Dictionary<string, string>() { { "jsonMapField1", "jsonMapValue1" } });
            log.SetValue("payload_field", bytes);
            log.SetValue("geo_field", new GeoPoint(10, 10));
            log.SetValue("ip_field", IPAddress.Loopback);

            return log;
        }

        private byte[] Format(ILog log)
        {
            return JsonLogFormatter.Instance.Format(log);
        }
    }
}
