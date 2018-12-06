using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Logging;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{
    public class LoggerFixture
    {
        [Fact]
        public async Task WhenWriteAsyncIsCalledOnLogger_WriterFactoryAndLogWriterShouldBeCalledProperly()
        {
            var waitHandle = new ManualResetEventSlim(false);
            
            var mockLogWriter = new Mock<IApplicationLogWriter>();
            mockLogWriter.Setup(x => x.WriteAsync(It.IsAny<ILog>())).Returns(() =>
            {
                waitHandle.Set();
                return Task.CompletedTask;
            });

            var mockFactory = new Mock<ILogWriterFactory>();
            mockFactory.Setup(x => x.CreateWriter()).Returns(mockLogWriter.Object);

            Logger.Initialize(mockFactory.Object);
            var log = new ApiLog();
            await Logger.WriteLogAsync(log);

            waitHandle.Wait();
            mockFactory.Verify(x => x.CreateWriter(), Times.Once);
            mockLogWriter.Verify(x => x.WriteAsync(log), Times.Once);
        }
    }
}