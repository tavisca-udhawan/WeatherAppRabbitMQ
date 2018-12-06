using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Serialization;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public static class HttpStub
    {
        public static ISerializer CreateMockSerializer(byte[] payload = null)
        {
            var serilizer = new Mock<ISerializer>();
            serilizer.Setup(x => x.Serialize(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<Stream, CancellationToken>((s, c) =>
            {

                if (payload != null)
                {
                    var memoryStream = new MemoryStream();
                    s.CopyTo(memoryStream);
                    Assert.Equal(memoryStream.ToArray(), payload);
                }
            });
            serilizer.Setup(x => x.Deserialize(It.IsAny<Stream>(), It.IsAny<Type>()));
            return serilizer.Object;
        }

        public static Mock<IHttpConnector> CreateHttpConnector(HttpResponse response, bool throwFault = false)
        {
            var httpConnector = new Mock<IHttpConnector>();
            httpConnector.Setup(x => x.SendAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    return Task.FromResult(ResponsdOrFault(response, throwFault));
                });
            return httpConnector;
        }

        private static HttpResponse ResponsdOrFault(HttpResponse response, bool throwFault)
        {
            if (throwFault)
                throw new Exception("Test Message");
            return response;
        }
    }
}
