using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class XmlPayloadMaskingRule : PayloadMaskingRule
    {
        private XmlDocument _xmlDoc;
        private XmlNamespaceManager _mgr;
        private Dictionary<string, string> _namespaces = new Dictionary<string, string>();

        public XmlPayloadMaskingRule(string field, Dictionary<string, string> namespaces = null, params PayloadFieldMaskingRule[] rules)
        {
            Field = field;
            FieldMaskingRules.AddRange(rules);

            if (namespaces != null && namespaces.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in namespaces)
                {
                    _namespaces[kvp.Key] = kvp.Value;
                }
            }
        }

        public override Payload Apply(Payload target)
        {
            var xml = target.GetString();
            _xmlDoc = new XmlDocument();
            _mgr = new XmlNamespaceManager(_xmlDoc.NameTable);

            foreach (var kv in _namespaces)
            {
                _mgr.AddNamespace(kv.Key, kv.Value);
            }

            _xmlDoc.LoadXml(xml);

            FieldMaskingRules.ForEach(rule =>
            {
                if (rule != null)
                {
                    var nodes = GetNodes(rule.Path);
                    if (nodes != null && nodes.Count>0)
                    {
                        foreach (XmlNode node in nodes)
                        {
                            var value = node.Value.Trim();
                            node.Value = rule.Mask != null ? rule.Mask.Mask(value) : Masks.DefaultMask.Mask(value);
                        }
                    }
                }
            });

            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    _xmlDoc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return new Payload(stringWriter.GetStringBuilder().ToString());
                }
            }
        }

        private XmlNodeList GetNodes(string path)
        {
            return _xmlDoc.SelectNodes(path, _mgr);
        }
    }
}
