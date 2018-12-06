using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Spooling;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class SignatureGeneratorFixture
    {
        [Fact]
        public void GetSignature_sameKeyAndData_GivesSameSignature()
        {
            var signature1 = SignatureGenerator.GetSignature("testKey", "testdata");
            var signature2 = SignatureGenerator.GetSignature("testKey", "testdata");
            Assert.Equal(signature1, signature2);
        }
    }
}
