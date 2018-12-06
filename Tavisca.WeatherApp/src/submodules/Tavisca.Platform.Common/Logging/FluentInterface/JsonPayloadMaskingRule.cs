using Microsoft.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class JsonPayloadMaskingRule : PayloadMaskingRule
    {
        public JsonPayloadMaskingRule(string field, params PayloadFieldMaskingRule[] rules)
        {
            Field = field;
            FieldMaskingRules.AddRange(rules);
        }

        public override Payload Apply(Payload target)
        {
            Payload masked;
            var targetBytes = target.GetBytes();
            var streamManager = new RecyclableMemoryStreamManager();

            using (var writerBuffer = streamManager.GetStream())
            {
                using (var writer = new StreamWriter(writerBuffer))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        using (var readerBuffer = streamManager.GetStream("json_masking", targetBytes, 0, targetBytes.Length))
                        {
                            using (var reader = new StreamReader(readerBuffer))
                            {
                                using (var jsonReader = new JsonTextReader(reader))
                                {
                                    while (jsonReader.Read())
                                    {
                                        var rule = FindMatchingRule(jsonReader.Path);
                                        var isMatch = new HashSet<JsonToken>
                                        {
                                            JsonToken.String,
                                            JsonToken.Integer,
                                            JsonToken.Date,
                                            JsonToken.Float
                                        }.Contains(jsonReader.TokenType) && rule != null;

                                        if (isMatch == false)
                                        {
                                            jsonWriter.WriteToken(jsonReader, false);
                                        }
                                        else
                                        {
                                            var value = jsonReader.Value.ToString().Trim();
                                            jsonWriter.WriteValue(rule.Mask != null ? rule.Mask.Mask(value) : Masks.DefaultMask.Mask(value));
                                        }
                                    }

                                    //Flush contents to underlying memory stream
                                    jsonWriter.Flush();
                                    masked = new Payload(writerBuffer.ToArray());
                                }
                            }
                        }
                    }
                }
            }

            return masked;
        }

        private PayloadFieldMaskingRule FindMatchingRule(string path)
        {
            return FieldMaskingRules.Find(x =>
            {
                return new Regex("^" + x.Path.Replace("[", @"\[").Replace("]", @"\]") + "$", RegexOptions.IgnoreCase).IsMatch(path);
            });
        }
    }

}
