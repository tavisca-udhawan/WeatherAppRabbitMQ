using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Logging;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{
    public class LogWriterFixture
    {
        [Fact]
        public async Task WhenWriteAsyncIsCalledOnLogWriter_FilterFormatterAndSinkShouldBeCalledProperly()
        {
            var waitHandle = new ManualResetEventSlim(false);
            var log = new ApiLog();
            var logData = new byte[] { };
            var filteredLog1 = new ApiLog();
            var filteredLog2 = new ApiLog();

            var mockFilter1 = new Mock<ILogFilter>();
            mockFilter1.Setup(x => x.Apply(log)).Returns(filteredLog1);

            var mockFilter2 = new Mock<ILogFilter>();
            mockFilter2.Setup(x => x.Apply(filteredLog1)).Returns(filteredLog2);

            var mockLogFormatter = new Mock<ILogFormatter>();
            mockLogFormatter.Setup(x => x.Format(filteredLog2)).Returns(logData);

            var mockLogSink = new Mock<ILogSink>();
            mockLogSink.Setup(x => x.WriteAsync(filteredLog2, logData)).Returns(() =>
            {
                waitHandle.Set();
                return Task.CompletedTask;
            });

            var mockConfigurationProvider = new Mock<IConfigurationProvider>();
            mockConfigurationProvider.Setup(x => x.GetGlobalConfigurationAsync<bool>(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var logWriter = new LogWriter(mockLogFormatter.Object, mockLogSink.Object,
                new List<ILogFilter> {mockFilter1.Object});
            log.Filters.Add(mockFilter2.Object);

            await logWriter.WriteAsync(log);

            waitHandle.Wait();

            mockFilter1.Verify(x => x.Apply(log), Times.Once);
            mockFilter2.Verify(x => x.Apply(filteredLog1), Times.Once);
            mockLogFormatter.Verify(x => x.Format(filteredLog2), Times.Once);
            mockLogSink.Verify(x => x.WriteAsync(filteredLog2, logData), Times.Once);
        }
    }
}