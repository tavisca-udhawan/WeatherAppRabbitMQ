using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.WebApi
{
    public abstract class VersionedMediaTypeFormatter : MediaTypeFormatter
    {
        private readonly List<Tuple<VersionRange, MediaTypeFormatter>> _formatters = new List<Tuple<VersionRange, MediaTypeFormatter>>();

        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            var formatter = GetFormatter(GetVersion());
            return formatter.ReadFromStreamAsync(type, readStream, content, formatterLogger, cancellationToken);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return WriteToStreamAsync(type, value, writeStream, content, transportContext, CancellationToken.None);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            var formatter = GetFormatter(GetVersion());
            return formatter.WriteToStreamAsync(type, value, writeStream, content, transportContext, cancellationToken);
        }

        protected abstract Version GetVersion();

        protected abstract MediaTypeFormatter GetDefaultFormatter();

        private MediaTypeFormatter GetFormatter( Version version )
        {
            var formatter = _formatters.SingleOrDefault(x => x.Item1.Contains(version));
            return formatter?.Item2 ?? GetDefaultFormatter();
        }

        public void Register( Version fromInclusive, Version toExclusive, MediaTypeFormatter formatter )
        {
            var range = new VersionRange(fromInclusive, toExclusive);
            if (_formatters.Any(f => f.Item1.IsOverlapping(range)) == true)
                throw new ArgumentException("Version ranges must not overlap.");
            _formatters.Add(new Tuple<VersionRange, MediaTypeFormatter>(range, formatter));
        }
    }

    
}
