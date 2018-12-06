using System;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.TransitCodeGenerator;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Core.TransitCode
{
    public class TransitCodeGeneratorTests
    {
        [Fact]
        public async Task TestTransitCodeGenrator_Success()
        {
            var request = new TransitCodeRequest
            {
                ClientInformation = new ClientInformation
                {
                    ClientId = "58",
                    ClientUniqueID = "58",
                    ProgramCode = "7844",
                    ProgramId = "399"
                },
                ClientEnvironmentToken = "qa",
                TransitCode = Guid.NewGuid().ToString(),
                Mode = "Simulator"
            };

            TransitCodeGenerator transitCodeGenerator = new TransitCodeGenerator();
            var response = await transitCodeGenerator.GenerateTransitCode(request, "");

            Assert.Equal("success", response.Status);
            Assert.NotNull(response.TransitCode);
        }
    }
}
