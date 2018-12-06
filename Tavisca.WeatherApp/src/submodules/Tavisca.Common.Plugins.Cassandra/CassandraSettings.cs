using System.Collections.Generic;

namespace Tavisca.Common.Plugins.Cassandra
{
    public class CassandraSettings
    {

        public string Type { get; set; }

        public List<string> Hosts { get; set; }

        public string KeySpace { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Compression { get; set; }

        public RetrySetting RetrySetting { get; set; }

        public string Signature
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}", Type, KeySpace, Compression, string.Join("|", Hosts));
            }
        }
    }

    public class RetrySetting
    {
        public int NumberOfRetry { get; set; } = 2;
        public int DelayInMs { get; set; } = 200;
    }
}
