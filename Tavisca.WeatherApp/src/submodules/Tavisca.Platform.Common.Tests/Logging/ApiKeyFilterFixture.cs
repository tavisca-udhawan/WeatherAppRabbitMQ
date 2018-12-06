using System.Linq;
using Tavisca.Platform.Common.Logging;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{
    public class ApiKeyFilterFixture
    {
        [Theory]
        [InlineData("12345", "12345")]
        [InlineData("123456", "12******56")]
        [InlineData("1234567", "12******67")]
        [InlineData("12345678", "123******678")]
        public void WhenApiKeyLengthGreaterThanFive_MaskingShouldHappen(string apiKey, string expected)
        {
            var filter = new ApiKeyFilter();
            var filteredLog = filter.Apply(new ApiLog { RequestHeaders = { { "header1", "value1" }, { "oski-apikey", apiKey } } });
            Assert.NotNull(filteredLog);
            var headers = filteredLog.GetFields().FirstOrDefault(x => x.Key == "rq_headers").Value as Map;
            Assert.NotNull(headers);
            Assert.Equal(expected, headers.Value["oski-apikey"]);
        }
    }
}