using Xunit;
using Tavisca.Platform.Common.ConfigurationHandler;

namespace Tavisca.Platform.Common.Core.Tests
{
    public class CommonConfiguation
    {
        [Fact]
        public void Test_SettingsRead_Should_Returns_AllData_Success()
        {
            var value = ConfigurationManager.GetAppSetting("test");
            Assert.NotNull(value);
            Assert.Equal("test-data", value);
        }
    }
}
