using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Common.Plugins.Aws.S3;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Aws
{
    public class S3StoreTests
    {
        private string _key = "key";
        private string _bucket = "test_bucket";

        public S3StoreTests()
        {
            ExceptionPolicy.Configure(new TestErrorHandler());
        }

        [Fact]
        public async Task AddAsync_S3ReturnsError_ThrowException()
        {
            var bytes = Guid.NewGuid().ToByteArray();

            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError });

            var s3Store = new S3Store(amazonClient.Object);

            var value = await Assert.ThrowsAsync<CommunicationException>(async () => await s3Store.AddAsync(_key, _bucket, bytes));
            Assert.Equal(FaultCodes.S3CommunicationError, value.ErrorCode);
        }

        [Fact]
        public void Add_S3ReturnsError_ThrowException()
        {
            var bytes = Guid.NewGuid().ToByteArray();

            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
               .Returns(() => { return Task.FromResult(new PutObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError });});

            var s3Store = new S3Store(amazonClient.Object);

            var value = Assert.Throws<CommunicationException>(() => s3Store.Add(_key, _bucket, bytes));
            Assert.Equal(FaultCodes.S3CommunicationError, value.ErrorCode);
        }

        [Fact]
        public async Task AddAsync_S3Returns_Ok_ShouldSucess()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            Stream returnValue = null;

            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PutObjectRequest, CancellationToken>((r, c) => returnValue = r.InputStream)
                .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = returnValue });

            var s3Store = new S3Store(amazonClient.Object);

            await s3Store.AddAsync(_key, _bucket, bytes);

            var value = s3Store.GetAsync(_key, _bucket);

            Assert.NotNull(value);
        }

        [Fact]
        public void Add_S3Returns_Ok_ShouldSuccess()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            Stream returnValue = null;

            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .Callback<PutObjectRequest, CancellationToken>((r, c) => returnValue = r.InputStream)
                .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .Returns( () => { return Task.FromResult(new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = returnValue }); });

            var s3Store = new S3Store(amazonClient.Object);

            s3Store.Add(_key, _bucket, bytes);

            var value = s3Store.Get(_key, _bucket);

            Assert.NotNull(value);
        }

        [Fact]
        public async Task GetAsync_S3ReturnsError_ThrowException()
        {
            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError });

            var s3Store = new S3Store(amazonClient.Object);

            var value = await Assert.ThrowsAsync<CommunicationException>(async () => await s3Store.GetAsync(_key, _bucket));
            Assert.Equal(FaultCodes.S3CommunicationError, value.ErrorCode);
        }

        [Fact]
        public void Get_S3ReturnsError_ThrowException()
        {
            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .Returns(() => {return Task.FromResult(new GetObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError }); });

            var s3Store = new S3Store(amazonClient.Object);

            var value = Assert.Throws<CommunicationException>(() => s3Store.Get(_key, _bucket));
            Assert.Equal(FaultCodes.S3CommunicationError, value.ErrorCode);
        }


        [Fact]
        public async Task GetAsync_S3Returns_NotFound_ShouldReturn_Null()
        {
            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("NotFound", ErrorType.Sender, "0", "", HttpStatusCode.NotFound));

            var s3Store = new S3Store(amazonClient.Object);

            var value = await s3Store.GetAsync(_key, _bucket);
            Assert.Null(value);
        }

        [Fact]
        public void Get_S3Returns_NotFound_ShouldReturn_Null()
        {
            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new AmazonS3Exception("NotFound", ErrorType.Sender, "0", "", HttpStatusCode.NotFound));

            var s3Store = new S3Store(amazonClient.Object);

            var value = s3Store.Get(_key, _bucket);
            Assert.Null(value);
        }

        [Fact]
        public async Task GetAsync_S3ReturnsValue_ShouldReturnValue()
        {
            var bytes = Guid.NewGuid().ToByteArray();

            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = new MemoryStream(bytes) });

            var s3Store = new S3Store(amazonClient.Object);

            var value = await s3Store.GetAsync(_key, _bucket);
            Assert.NotNull(value);
        }

        [Fact]
        public void Get_S3ReturnsValue_ShouldReturnValue()
        {
            var bytes = Guid.NewGuid().ToByteArray();

            var amazonClient = new Mock<IAmazonS3>();
            amazonClient.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse { HttpStatusCode = HttpStatusCode.OK, ResponseStream = new MemoryStream(bytes) });

            var s3Store = new S3Store(amazonClient.Object);

            var value = s3Store.Get(_key, _bucket);
            Assert.NotNull(value);
        }
    }
}
