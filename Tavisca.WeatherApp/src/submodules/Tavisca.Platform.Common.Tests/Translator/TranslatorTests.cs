using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tavisca.Platform.Common.Models;
using Tavisca.Platform.Common.WebApi;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Translator
{
    public class TranslatorTests
    {
        [Fact]
        public void TestErrorInfoTranslator()
        {
            ErrorInfo errorInfo = new ErrorInfo("123", "TestMessage", HttpStatusCode.BadRequest)
            {
                Info = {
                                               new Info("I123", "info1"),
                                               new Info("I456", "info2")
                                           }
            };

            var serializedErrorInfo1 = JsonConvert.SerializeObject(errorInfo, Formatting.Indented, new ErrorInfoTranslator(), new InfoTranslator());
            var deserializedObj = JsonConvert.DeserializeObject<ErrorInfo>(serializedErrorInfo1, new ErrorInfoTranslator(),
                new InfoTranslator());
            var serializedErrorInfo2 = JsonConvert.SerializeObject(deserializedObj, Formatting.Indented, new ErrorInfoTranslator(), new InfoTranslator());
            Assert.Equal(serializedErrorInfo1, serializedErrorInfo2);
        }
    }
}
