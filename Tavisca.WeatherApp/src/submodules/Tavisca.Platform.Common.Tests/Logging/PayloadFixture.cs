using System;
using System.IO;
using System.Linq;
using System.Text;
using Tavisca.Platform.Common.Logging;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{
    public class PayloadFixture
    {
        [Fact]
        public void WhenEncodingIsSentAsNull_DefaultEncodingUTF8WithoutBom_ShouldBeUsed()
        {
            const string expectedString = "payload";
            var payload = new Payload(expectedString, null);
            Assert.NotNull(payload.Encoding);
            Assert.IsType(typeof(UTF8Encoding), payload.Encoding);
            Assert.Equal(expectedString, Encoding.UTF8.GetString(payload.GetBytes()));
            Assert.Equal(expectedString, payload.GetString());
        }

        [Fact]
        public void WhenNullOrEmptyValueIsGivenInByteArray_ShouldReturnEmptyPayload()
        {
            var payload = new Payload(default(byte[]));
            Assert.Equal(string.Empty, payload.GetString());
            Assert.Empty(payload.GetBytes());
            Assert.Equal(0, payload.Length);

            payload = new Payload(new byte[0]);
            Assert.Equal(string.Empty, payload.GetString());
            Assert.Empty(payload.GetBytes());
            Assert.Equal(0, payload.Length);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
        }

        [Fact]
        public void WhenNullOrEmptyValueIsGivenInString_ShouldReturnEmptyPayload()
        {
            var payload = new Payload(default(string));
            Assert.Equal(string.Empty, payload.GetString());
            Assert.Empty(payload.GetBytes());
            Assert.Equal(0, payload.Length);

            payload = new Payload(string.Empty);
            Assert.Equal(string.Empty, payload.GetString());
            Assert.Empty(payload.GetBytes());
            Assert.Equal(0, payload.Length);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
        }

        [Fact]
        public void WhenNullValueIsGivenInFunc_ShouldReturnEmptyPayload()
        {
            var payload = new Payload(default(Func<byte[]>));
            Assert.Equal(string.Empty, payload.GetString());
            Assert.Empty(payload.GetBytes());
            Assert.Equal(0, payload.Length);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
        }

        [Fact]
        public void WhenFuncThrowsException_ShouldReturnEmptyPayload()
        {
            var payload = new Payload(() => { throw new InvalidDataException(); });
            Assert.Equal(string.Empty, payload.GetString());
            Assert.Empty(payload.GetBytes());
            Assert.Equal(0, payload.Length);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
        }

        [Fact]
        public void WhenStringIsGivenInConstructor_CorrectValuesShouldBeReturned()
        {
            const string expectedString = "payload";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedString);
            var payload = new Payload(expectedString);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
            Assert.Equal(expectedString, payload.GetString());
            Assert.Equal(expectedBytes.Length, payload.Length);
            Assert.True(expectedBytes.SequenceEqual(payload.GetBytes()));
        }

        [Fact]
        public void WhenByteArrayIsGivenInConstructor_CorrectValuesShouldBeReturned()
        {
            const string expectedString = "payload";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedString);
            var payload = new Payload(expectedBytes);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
            Assert.Equal(expectedString, payload.GetString());
            Assert.Equal(expectedBytes.Length, payload.Length);
            Assert.True(expectedBytes.SequenceEqual(payload.GetBytes()));
        }

        [Fact]
        public void WhenValidFuncIsGivenInConstructor_CorrectValuesShouldBeReturned()
        {
            const string expectedString = "payload";
            var expectedBytes = Encoding.UTF8.GetBytes(expectedString);
            var payload = new Payload(() => expectedBytes);
            Assert.IsType<UTF8Encoding>(payload.Encoding);
            Assert.Equal(expectedString, payload.GetString());
            Assert.Equal(expectedBytes.Length, payload.Length);
            Assert.True(expectedBytes.SequenceEqual(payload.GetBytes()));
        }

        [Fact]
        public void WhenNonDefaultEncodingIsUsed_CorrectValuesShouldBeReturned()
        {
            const string expectedString = "payload";
            var expectedBytes = Encoding.UTF32.GetBytes(expectedString);
            var payload = new Payload(() => expectedBytes, Encoding.UTF32);
            Assert.IsType<UTF32Encoding>(payload.Encoding);
            Assert.Equal(expectedString, payload.GetString());
            Assert.Equal(expectedBytes.Length, payload.Length);
            Assert.True(expectedBytes.SequenceEqual(payload.GetBytes()));
        }
    }
}