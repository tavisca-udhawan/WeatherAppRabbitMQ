using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.StatsD;
using Xunit;
using Moq;
using Tavisca.Platform.Common.Configurations;
using System.Collections.Specialized;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Platform.Common.Tests.Metrics
{
    public class MetricsFixture
    {
        public MetricsFixture()
        {
            var nvc = new NameValueCollection()
            {
                { "host", "192.168.10.182"},
                { "port", "8125"},
                { "mtu", "1432"},
                { "batchingInterval", "1"},
                { "applyPrefix", "Y"},
                { "prefix", "testTenant"},
            };
            var config = new Mock<IConfigurationProvider>();
            config
                .Setup(c => c.GetTenantConfigurationAsNameValueCollectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(nvc);
            config
                .Setup(c => c.GetGlobalConfigurationAsNameValueCollectionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(nvc);
            _configuration = config.Object;
            Metering.ConfigureMetrics(new MetricsFactory(_configuration, _appName));

        }

        private IConfigurationProvider _configuration;
        private string _appName = "UnitTest";
        private string _tenant = "Demo";

        [Fact]
        public void TenantSpecificMeteringTest()
        {

            Metering.GetMeterForTenant(_tenant).Meter("meter-stat");

        }


        [Fact]
        public void BookingEngineMetricTest()
        {
            Metering.GetGlobalMeter().Meter("booking-meter");
            Metering.GetGlobalMeter().Counter("booking-counter", 1);
            Metering.GetGlobalMeter().Gauge("booking-gauge", 10);
        }

        [Fact]
        public void TenantSpecificCounterTest()
        {
            var meter = Metering.GetMeterForTenant(_tenant);
            meter.Counter("counter-stat", 1);
            meter.Counter("counter-stat", 2);
            meter.Counter("counter-stat", 3);
        }

        [Fact]
        public void TenantSpecificGaugeTest()
        {
            var meter = Metering.GetMeterForTenant(_tenant);
            meter.Gauge("tenant-gauge-stat", 10);
        }


        [Fact]
        public void GlobalMeteringTest()
        {
            var meter = Metering.GetGlobalMeter();
            meter.Meter("meter-stat");
        }

        [Fact]
        public void GlobalCounterTest()
        {
            var meter = Metering.GetGlobalMeter();
            meter.Counter("counter-stat", 1);
            meter.Counter("counter-stat", 2);
            meter.Counter("counter-stat", 3);

        }

        [Fact]
        public void GlobalGaugeTest()
        {
            var meter = Metering.GetGlobalMeter();
            meter.Gauge("gauge-stat", 10);
        }

    }
}
