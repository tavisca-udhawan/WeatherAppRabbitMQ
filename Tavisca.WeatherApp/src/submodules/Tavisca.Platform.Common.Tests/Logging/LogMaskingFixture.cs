using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Logging.Fluent;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{
    public class LogMaskingFixture
    {
        [Fact]
        public void VerifyTextFieldsAreMaskedCorrectly()
        {
            var apiLog = new ApiLog();
            var cc = "4111111111111111";
            var maskedCC = "411111******1111";
            var filter = new TextLogMaskingFilter(new List<TextMaskingRule> {
                new TextMaskingRule { Field = "textField", Mask = Masks.DefaultMask },
                new TextMaskingRule { Field = "textFieldCC", Mask = Masks.CreditCardMask }
            });

            apiLog.TrySetValue("textField", "textFieldValue");
            apiLog.TrySetValue("textFieldCC", cc);
            apiLog.TrySetValue("textFieldNotToBeMasked", "textFieldNotToBeMaskedValue");

            var masked = filter.Apply(apiLog);
            var fields = masked.GetFields();
            var maskedTextField = fields.First(x => x.Key == "textField").Value;
            var maskedTextFieldCC = fields.First(x => x.Key == "textFieldCC").Value;
            var maskedTextFieldNoToBeMasked = fields.First(x => x.Key == "textFieldNotToBeMasked").Value;

            Assert.Equal("t************e", maskedTextField);
            Assert.Equal(maskedCC, maskedTextFieldCC);
            Assert.Equal("textFieldNotToBeMaskedValue", maskedTextFieldNoToBeMasked);
        }

        [Fact]
        public void VerifyWhenMaskingFieldAsTextFails_ValueIsSetToDefaultMaskingFailureValue()
        {
            var apiLog = new ApiLog();
            var filter = new TextLogMaskingFilter(new List<TextMaskingRule> {
                new TextMaskingRule { Field = "textField", Mask = new FuncMask(new Func<string, string>((string value)=>{ throw new Exception("MaskingFailed"); })) }
            });

            apiLog.TrySetValue("textField", "textFieldValue");

            var masked = filter.Apply(apiLog);
            var fields = masked.GetFields();
            var maskedTextField = fields.First(x => x.Key == "textField").Value;

            Assert.Equal(maskedTextField, KeyStore.Masking.MaskingFailed);
            Assert.True(Boolean.Parse(fields.First(x => x.Key == KeyStore.Masking.MaskingFailedKey).Value.ToString()));
        }

        [Fact]
        public void VerifyQueryStringFieldsAreMaskedCorrectly()
        {
            var apiLog = new ApiLog();
            var cc = "4111111111111111";
            var maskedCC = "411111******1111";
            var query = "PARAM1= aa &paramCC= " + cc + " &param2= aaa &param3= aaaaa ";
            var filter = new TextLogMaskingFilter(new QueryStringMaskingRule("queryField", new TextMaskingRule[]
            {
                new TextMaskingRule{ Field = "param1", Mask = Masks.DefaultMask },
                new TextMaskingRule{ Field = "paramcc", Mask = Masks.CreditCardMask }
            }));

            apiLog.TrySetValue("queryField", query);
            apiLog.TrySetValue("queryFieldNotToBeMasked", query);


            var masked = filter.Apply(apiLog);
            var fields = masked.GetFields();
            var maskedQueryField = fields.First(x => x.Key == "queryField").Value;
            var maskedQueryFieldNoToBeMasked = fields.First(x => x.Key == "queryFieldNotToBeMasked").Value;

            Assert.Equal("PARAM1=**&paramCC=" + maskedCC + "&param2= aaa &param3= aaaaa ", maskedQueryField);
            Assert.Equal(query, maskedQueryFieldNoToBeMasked);
        }

        [Fact]
        public void VerifyJSONFieldsAreMaskedCorrectly()
        {
            var apiLog = new ApiLog();
            var cc = "4111111111111111";
            var maskedCC = "411111******1111";
            var filter = new StreamLogMaskingFilter(new List<JsonPayloadMaskingRule>
            {
                new JsonPayloadMaskingRule("jsonfield", new PayloadFieldMaskingRule []
                {
                    new PayloadFieldMaskingRule { Path = "param1" },
                    new PayloadFieldMaskingRule { Path = "param2" },
                    new PayloadFieldMaskingRule { Path = "param3" },
                    new PayloadFieldMaskingRule { Path = "param4.childParam" },
                    new PayloadFieldMaskingRule { Path = "param4.childParamCC", Mask = Masks.CreditCardMask }
                })
            });

            var json = string.Format(@"{{
                            ""PARAM1"": "" aa "",
                            ""param2"": "" aaaa "",
                            ""param3"": "" aaaaa "",
                            ""param4"": {{
                                ""childParam"": ""ccccc"",
                                ""childParamCC"": ""{0}""
                            }}
                       }}", cc);

            apiLog.TrySetValue("jsonField", new Payload(json));
            apiLog.TrySetValue("jsonFieldNotToBeMasked", new Payload(json));

            var masked = filter.Apply(apiLog);
            var fields = masked.GetFields();
            var maskedJsonField = (fields.First(x => x.Key == "jsonField").Value as Payload).GetString();
            var maskedJsonFieldNoToBeMasked = (fields.First(x => x.Key == "jsonFieldNotToBeMasked").Value as Payload).GetString();

            Assert.Equal(string.Format(Regex.Replace(@"{{
                            ""PARAM1"": ""**"",
                            ""param2"": ""a***"",
                            ""param3"": ""a***a"",
                            ""param4"": {{
                                ""childParam"": ""c***c"",
                                ""childParamCC"": ""{0}""
                            }}
                       }}", @"\s|\t|\n|\r", ""), maskedCC), maskedJsonField);
            Assert.Equal(json, maskedJsonFieldNoToBeMasked);
        }

        [Fact]
        public void VerifyXmlFieldsAreMaskedCorrectly()
        {
            var apiLog = new ApiLog();
            var cc = "4111111111111111";
            var maskedCC = "411111******1111";
            var filter = new StreamLogMaskingFilter(new List<PayloadMaskingRule>
            {
                new XmlPayloadMaskingRule("xmlField", new Dictionary<string, string>{ { "oski", "http://oski.io/my_custom_ns" } }, new PayloadFieldMaskingRule[]
                {
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child1/@node1Child1Attr" },
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child1/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child2/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/node1/node1Child3/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/oski:node2/text()" },
                    new PayloadFieldMaskingRule { Path = "/root/nodeCC/text()", Mask = Masks.CreditCardMask }
                })
            });

            var xml = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
                            <node1>
                                <node1Child1 node1Child1Attr="" attr_value ""> ab </node1Child1>
                                <node1Child2> abcd </node1Child2>
                                <node1Child3> abcde </node1Child3>
                            </node1>
                            <node2 xmlns=""http://oski.io/my_custom_ns"">uvwxyz</node2>
                            <nodeCC>{0}</nodeCC>
                            <node3>pqrst</node3>
                        </root>", cc);

            apiLog.TrySetValue("xmlField", new Payload(xml));
            apiLog.TrySetValue("xmlFieldNotToBeMasked", new Payload(xml));

            var masked = filter.Apply(apiLog);
            var fields = masked.GetFields();
            var maskedXmlField = (fields.First(x => x.Key == "xmlField").Value as Payload).GetString();
            var maskedXmlFieldNoToBeMasked = (fields.First(x => x.Key == "xmlFieldNotToBeMasked").Value as Payload).GetString();

            Assert.Equal(string.Format(Regex.Replace(Regex.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <root>
                            <node1>
                                <node1Child1 node1Child1Attr=""a********e"">**</node1Child1>
                                <node1Child2>a***</node1Child2>
                                <node1Child3>a***e</node1Child3>
                            </node1>
                            <node2 xmlns=""http://oski.io/my_custom_ns"">u****z</node2>
                            <nodeCC>{0}</nodeCC>
                            <node3>pqrst</node3>
                        </root>", @"\t|\n|\r", ""), @">\s*<", "><"), maskedCC), maskedXmlField);
            Assert.Equal(maskedXmlFieldNoToBeMasked, xml);
        }

        [Fact]
        public void VerifyWhenCustomMaskSpecified_FieldsAreMaskedCorrectly()
        {
            var apiLog = new ApiLog();
            var filter = new DelegateFilter((ILog log) =>
            {
                var copy = new List<KeyValuePair<string, object>>();
                var fields = log.GetFields();
                foreach (var field in fields)
                {
                    if (field.Key == "fieldToMask")
                    {
                        copy.Add(new KeyValuePair<string, object>(field.Key, Masks.DefaultMask.Mask(field.Value.ToString())));
                    }
                    else if (field.Key == "fieldNotToMask")
                    {
                        copy.Add(field);
                    }
                }
                return new SimpleLog(log.Id, log.LogTime, copy);
            });

            apiLog.TrySetValue("fieldToMask", "ValueToMask");
            apiLog.TrySetValue("fieldNotToMask", "ValueNotToMask");

            var masked = filter.Apply(apiLog);
            var maskedFields = masked.GetFields();
            var maskedField = maskedFields.First(x => x.Key == "fieldToMask").Value;
            var notMaskedField = maskedFields.First(x => x.Key == "fieldNotToMask").Value;

            Assert.Equal(maskedField, "V*********k");
            Assert.Equal(notMaskedField, "ValueNotToMask");
        }

        [Fact]
        public void VerifyWhenMaskingFieldAsPayloadFails_ValueIsSetToDefaultMaskingFailureValue()
        {
            var apiLog = new ApiLog();
            var filter = new StreamLogMaskingFilter(new List<JsonPayloadMaskingRule>
            {
                new JsonPayloadMaskingRule("jsonfield", new PayloadFieldMaskingRule []
                {
                    new PayloadFieldMaskingRule { Path = "param1", Mask = new FuncMask(new Func<string, string>((string value)=>{ throw new Exception("MaskingFailed"); })) }
                })
            });

            var json = string.Format(@"{{
                            ""PARAM1"": "" aa ""
                       }}");

            apiLog.TrySetValue("jsonField", new Payload(json));

            var masked = filter.Apply(apiLog);
            var fields = masked.GetFields();
            var maskedJsonField = fields.First(x => x.Key == "jsonField").Value.ToString();

            Assert.Equal(KeyStore.Masking.MaskingFailed, maskedJsonField);
            Assert.True(Boolean.Parse(fields.First(x => x.Key == KeyStore.Masking.MaskingFailedKey).Value.ToString()));
        }

        [Fact]
        public void VerifyJSONFluentInterfaceGeneratesCorrectFilters()
        {
            var loggingFilter = new LoggingHttpFilter()
                .ConfigureMasking()
                .MaskPayloadAsJson("content")
                    .WithField("path")
                    .Builder
                .Apply();

            var filters = loggingFilter.Filters;

            filters.Should().HaveCount(1);
            filters.Single().Should().BeOfType<StreamLogMaskingFilter>();
            (filters.Single() as StreamLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.Single() as StreamLogMaskingFilter).Rules.Single().Should().BeOfType<JsonPayloadMaskingRule>();
        }

        [Fact]
        public void VerifyXMLFluentInterfaceGeneratesCorrectFilters()
        {
            var loggingFilter = new LoggingHttpFilter()
                .ConfigureMasking()
                .MaskPayloadAsXml("content")
                    .WithField("path")
                    .Builder
                .Apply();

            var filters = loggingFilter.Filters;

            filters.Should().HaveCount(1);
            filters.Single().Should().BeOfType<StreamLogMaskingFilter>();
            (filters.Single() as StreamLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.Single() as StreamLogMaskingFilter).Rules.Single().Should().BeOfType<XmlPayloadMaskingRule>();
        }

        [Fact]
        public void VerifyQueryStringFluentInterfaceGeneratesCorrectFilters()
        {
            var loggingFilter = new LoggingHttpFilter()
                .ConfigureMasking()
                .MaskPayloadAsQueryString("content")
                    .WithField("path")
                    .Builder
                .Apply();

            var filters = loggingFilter.Filters;

            filters.Should().HaveCount(1);
            filters.Single().Should().BeOfType<TextLogMaskingFilter>();
            (filters.Single() as TextLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.Single() as TextLogMaskingFilter).Rules.Single().Should().BeOfType<QueryStringMaskingRule>();
        }

        [Fact]
        public void VerifyTextFluentInterfaceGeneratesCorrectFilters()
        {
            var loggingFilter = new LoggingHttpFilter()
                .ConfigureMasking()
                .MaskField("content")
                .Apply();

            var filters = loggingFilter.Filters;

            filters.Should().HaveCount(1);
            filters.Single().Should().BeOfType<TextLogMaskingFilter>();
            (filters.Single() as TextLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.Single() as TextLogMaskingFilter).Rules.Single().Should().BeOfType<TextMaskingRule>();
        }

        [Fact]
        public void VerifyFluentInterfaceGeneratesCombinationOfFiltersCorectly()
        {
            var loggingFilter = new LoggingHttpFilter()
                .ConfigureMasking()
                .MaskField("textContent")
                .MaskPayloadAsJson("jsonContent")
                    .WithField("jsonField")
                    .Builder
                .MaskPayloadAsQueryString("queryStringContent")
                    .WithField("queryStringField")
                    .Builder
                .MaskPayloadAsXml("xmlContent")
                    .WithField("xmlField")
                    .Builder
                .Apply();

            var filters = loggingFilter.Filters;

            filters.Should().HaveCount(4);
            filters.ElementAt(0).Should().BeOfType<TextLogMaskingFilter>();
            (filters.ElementAt(0) as TextLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.ElementAt(0) as TextLogMaskingFilter).Rules.Single().Should().BeOfType<TextMaskingRule>();

            filters.ElementAt(1).Should().BeOfType<StreamLogMaskingFilter>();
            (filters.ElementAt(1) as StreamLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.ElementAt(1) as StreamLogMaskingFilter).Rules.Single().Should().BeOfType<JsonPayloadMaskingRule>();

            filters.ElementAt(2).Should().BeOfType<TextLogMaskingFilter>();
            (filters.ElementAt(2) as TextLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.ElementAt(2) as TextLogMaskingFilter).Rules.Single().Should().BeOfType<QueryStringMaskingRule>();

            filters.ElementAt(3).Should().BeOfType<StreamLogMaskingFilter>();
            (filters.ElementAt(3) as StreamLogMaskingFilter).Rules.Should().HaveCount(1);
            (filters.ElementAt(3) as StreamLogMaskingFilter).Rules.Single().Should().BeOfType<XmlPayloadMaskingRule>();
        }

        [Fact]
        public void VerifyCustomFluentInterfaceGeneratesCorrectFilters()
        {
            var loggingFilter = new LoggingHttpFilter()
                .ConfigureMaskingDelegate((ILog log) => { return log; })
                .Apply();

            var filters = loggingFilter.Filters;

            filters.Should().HaveCount(1);
            filters.Single().Should().BeOfType<DelegateFilter>();
        }
    }
}
