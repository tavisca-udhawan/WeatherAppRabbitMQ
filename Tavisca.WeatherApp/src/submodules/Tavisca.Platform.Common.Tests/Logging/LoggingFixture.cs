using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Logging;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{

    /*
        Things to test:
        1.  All fields are being returned. - done
        2. Custom fields are returned - done.
        2.  Maps are returned properly - done
        3.  Bytes are returned properly - done
        4.  Card masking - done
        5.  Payment filter - done
        6.  Api key filter
        7.  Sink is called properly - done
        8.  Formatter is called properly - done
        9.  JsonFormatter is working properly
        10. Map formatting test
        11. Payload is working properly test.
        12. Payload tests.
    */

    public class LoggingFixture
    {

        private static readonly string CorrelationId = "{1D7872F8-7753-4BFF-BD5D-A6BAF3EB89CE}";
        private static readonly string StackId = "{55EC6376-8559-4193-9953-01F34C4876E0}";
        private static readonly string TenantId = "unit_test";
        private static readonly string Category = "test_category";
        private static readonly string Message = "test_message";
        private static readonly string App = "unit_test_app";
        private static readonly string TxId = "trx_id";
        private static readonly string AppTxId = "application_transaction_id";

        [Fact]
        public void Default_fields_should_be_populated_test()
        {
            var time = DateTime.UtcNow;
            TraceLog log = new TraceLog() { LogTime = time, ApplicationName = App, Message = Message };
            var fields = log.GetFields().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Assert.NotEmpty(fields["id"].ToString());
            Assert.NotEmpty(fields["msg"].ToString());
            Assert.NotEmpty(fields["type"].ToString());
            Assert.NotEmpty(fields["app_domain"].ToString());
            Assert.NotEmpty(fields["app_name"].ToString());
            Assert.NotEmpty(fields["machine"].ToString());
            Assert.True((int)(fields["pid"]) > 0);
            Assert.Equal(time, (DateTime)(fields["log_time"]));
        }

        [Fact]
        public void Trace_log_fields_should_be_populated_test()
        {
            TraceLog log = new TraceLog();
            log.Category = Category;
            log.CorrelationId = CorrelationId;
            log.StackId = StackId;
            log.Message = Message;
            log.TenantId = TenantId;
            log.ApplicationName = App;
            log.ApplicationTransactionId = AppTxId;
            var fields = log.GetFields().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(13, fields.Count);
            Assert.Equal(Category, fields["category"].ToString());
            Assert.Equal(StackId, fields["sid"].ToString());
            Assert.Equal(TenantId, fields["tid"].ToString());
            Assert.Equal(CorrelationId, fields["cid"].ToString());
            Assert.Equal(Message, fields["msg"].ToString());
            Assert.Equal("trace", fields["type"].ToString());
            Assert.Equal(AppTxId, fields["app_txid"].ToString());
        }

        [Fact]
        public void Exception_log_fields_should_be_populated_test()
        {
            var log = new ExceptionLog();
            log.CorrelationId = CorrelationId;
            log.StackId = StackId;
            log.Message = Message;
            log.TenantId = TenantId;
            log.ApplicationName = App;
            log.ExceptionType = "ex_type";
            log.Source = "ex_source";
            log.InnerException = "ex_innerException";
            log.StackTrace = "ex_stack";
            log.TargetSite = "ex_target_site";
            log.ApplicationTransactionId = AppTxId;

            var fields = log.GetFields().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(17, fields.Count);
            Assert.Equal("ex_type", fields["ex_type"].ToString());
            Assert.Equal("ex_source", fields["source"].ToString());
            Assert.Equal("ex_innerException", fields["inner_exception"].ToString());
            Assert.Equal("ex_stack", fields["stack_trace"].ToString());
            Assert.Equal("ex_target_site", fields["target_site"].ToString());
            Assert.Equal("exception", fields["type"].ToString());
            Assert.Equal(AppTxId, fields["app_txid"].ToString());
        }

        [Fact]
        public void Exception_log_exceptiondata_fields_should_be_populated_test()
        {
            var ex = new Exception(Message);
            ex.Data.Add("data_field1", "value1");
            ex.Data.Add(123, "value2");
            ex.Data.Add("data_field2", new object());
            var log = new ExceptionLog(ex);
            log.CorrelationId = CorrelationId;
            log.StackId = StackId;
            log.Message = Message;
            log.TenantId = TenantId;
            log.ApplicationName = App;
            log.ExceptionType = "ex_type";
            log.Source = "ex_source";
            log.InnerException = "ex_innerException";
            log.StackTrace = "ex_stack";
            log.TargetSite = "ex_target_site";
            log.ApplicationTransactionId = AppTxId;

            var fields = log.GetFields().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(18, fields.Count);
            Assert.Equal("ex_type", fields["ex_type"].ToString());
            Assert.Equal("ex_source", fields["source"].ToString());
            Assert.Equal("ex_innerException", fields["inner_exception"].ToString());
            Assert.Equal("ex_stack", fields["stack_trace"].ToString());
            Assert.Equal("ex_target_site", fields["target_site"].ToString());
            Assert.Equal("exception", fields["type"].ToString());
            Assert.Equal("value1", fields["data_field1"]);
            Assert.False(fields.ContainsKey("data_field2"));
            Assert.Equal(AppTxId, fields["app_txid"].ToString());
        }

        [Fact]
        public void Api_log_fields_should_be_populated_test()
        {
            var log = new ApiLog();
            log.CorrelationId = CorrelationId;
            log.StackId = StackId;
            log.Message = Message;
            log.TenantId = TenantId;
            log.ApplicationName = App;

            log.Api = "api";
            log.Url = "url";
            log.ClientIp = System.Net.IPAddress.Loopback;
            log.Verb = "verb";
            log.Request = new Payload("request");
            log.Response = new Payload("response");
            log.TransactionId = TxId;
            log.ApplicationTransactionId = AppTxId;
            log.IsSuccessful = true;
            log.TimeTakenInMs = 10.0d;
            log.RequestHeaders["header"] = "rqheaderValue";
            log.ResponseHeaders["header"] = "rsheaderValue";

            var fields = log.GetFields().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(23, fields.Count);
            Assert.Equal("api", fields["api"].ToString());
            Assert.Equal("url", fields["url"].ToString());
            Assert.Equal("verb", fields["verb"].ToString());
            Assert.Equal("request", ((Payload)(fields["request"])).GetString());
            Assert.Equal("response", ((Payload)(fields["response"])).GetString());
            Assert.Equal(TxId, fields["txid"].ToString());
            Assert.Equal(AppTxId, fields["app_txid"].ToString());
            Assert.Equal(IPAddress.Loopback, (IPAddress)(fields["client_ip"]));
            Assert.Equal("success", fields["status"].ToString());
            Assert.Equal(10.0d, ((double)fields["time_taken_ms"]));
            Assert.True(fields["rq_headers"] is Map);
            Assert.True(((Map)fields["rq_headers"]).Value["header"] == "rqheaderValue");
            Assert.True(fields["rs_headers"] is Map);
            Assert.True(((Map)fields["rs_headers"]).Value["header"] == "rsheaderValue");
            Assert.Equal("api", fields["type"].ToString());
        }

        [Fact]
        public void Custom_fields_should_be_populated_test()
        {
            var log = new TraceLog { Category = Category };
            var dt = DateTime.UtcNow;
            var bytes = new UTF8Encoding(false).GetBytes("payload");
            log.SetValue("int_field", 1);
            log.SetValue("string_field", "str");
            log.SetValue("dt_field", dt);
            log.SetValue("long_field", 1L);
            log.SetValue("float_field", 1.0f);
            log.SetValue("double_field", 2.0d);
            log.SetValue("decimal_field", 3.0m);
            log.SetValue("bool_field", true);
            log.SetValue("map_field", new Dictionary<string, string>() { { "k", "v" } });
            log.SetValue("payload_field", bytes);
            log.SetValue("geo_field", new GeoPoint(10, 10));
            log.SetValue("ip_field", IPAddress.Loopback);
            var fields = log.GetFields().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(1, fields["int_field"].As<int>());
            Assert.Equal("str", fields["string_field"].As<string>());
            Assert.Equal(dt, fields["dt_field"].As<DateTime>());
            Assert.Equal(1L, fields["long_field"].As<long>());
            Assert.Equal(1.0f, fields["float_field"].As<float>());
            Assert.Equal(2.0d, fields["double_field"].As<double>());
            Assert.Equal(3.0m, fields["decimal_field"].As<decimal>());
            Assert.Equal(true, fields["bool_field"].As<bool>());
            Assert.Equal(IPAddress.Loopback, fields["ip_field"].As<IPAddress>());
            Assert.Equal("payload", fields["payload_field"].As<Payload>().GetString());
            Assert.Equal(new GeoPoint(10, 10), fields["geo_field"].As<GeoPoint>());
            Assert.Equal("v", fields["map_field"].As<Map>().Value["k"]);
        }

        [Theory]
        [InlineData("4485941735183973")]    // Visa
        [InlineData("6011936969058832")]    // Discover
        //[InlineData("6706058628148316")]    // Diners Club - Carte Blanche
        [InlineData("30288083427463")]      // LASER
        [InlineData("5386108708297761")]    // 4556758427475309MasterCard
        [InlineData("3541958962953708")]    // JCB
        [InlineData("36809380260929")]      // Diners Club - International
        [InlineData("4913683901768268")]    // Visa Electron
        [InlineData("370112543558048")]     // American Express (AMEX)
        [InlineData("5592698835827688")]    // Diners Club - North America
        //[InlineData("0604487412584913")]    // Maestro
        //[InlineData("6378409543839827")]    // InstaPayment
        public void Card_masking_should_return_first4_and_last4_test(string card)
        {
            var expected = card.Substring(0, 6) + string.Empty.PadRight(card.Length - 10, '*') + card.Substring(card.Length - 4);
            Assert.Equal(expected, CreditcardMask.Mask(card));
        }

        [Fact]
        public async Task Enable_sanitization_should_enable_payment_filter_test()
        {
            var waitHandle = new ManualResetEvent(false);
            var nullSink = new Mock<ILogSink>();
            nullSink
                .Setup(s => s.WriteAsync(It.IsAny<ILog>(), It.IsAny<byte[]>()))
                .Returns(() =>
                {
                    waitHandle.Set();
                    return Task.CompletedTask;
                });
            ILog updatedLog = null;
            var mockFormatter = new Mock<ILogFormatter>();
            mockFormatter
                .Setup(f => f.Format(It.IsAny<ILog>()))
                .Returns<ILog>(l =>
                {
                    updatedLog = l;
                    return new byte[] { };
                });

            var mockConfigurationProvider = new Mock<IConfigurationProvider>();
            mockConfigurationProvider.Setup(x => x.GetGlobalConfigurationAsync<bool>(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var log = new TraceLog();
            PopulateDefaults(log);
            log.SetValue("txt_with_card", "<card>4111111111111111</card>");
            log.EnableSanitization();
            
            var writer = new LogWriter(mockFormatter.Object, nullSink.Object);
            await writer.WriteAsync(log);
            waitHandle.WaitOne();
            Assert.NotNull(updatedLog);
            var cardText = updatedLog.GetFields().SingleOrDefault(x => x.Key.Equals("txt_with_card")).Value;
            Assert.NotNull(cardText);
            Assert.Equal("<card>411111******1111</card>", cardText);
        }

        [Fact]
        public void Enable_sanitization_should_add_payment_filter_test()
        {
            var log = new TraceLog();
            PopulateDefaults(log);
            log.EnableSanitization();
            Assert.True(log.Filters.Exists(f => f is PaymentDataFilter));
        }

        private void PopulateDefaults(LogBase log)
        {
            log.ApplicationName = App;
            log.AppDomain = AppDomain.CurrentDomain.FriendlyName;
            log.CorrelationId = CorrelationId;
            log.StackId = StackId;
            log.Message = Message;
            log.TenantId = TenantId;
        }
    }

    internal static class ObjExtensions
    {
        public static T As<T>(this object o)
        {
            return (T)o;
        }
    }
}
