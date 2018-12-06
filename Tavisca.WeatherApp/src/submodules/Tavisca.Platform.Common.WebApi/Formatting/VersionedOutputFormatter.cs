using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.WebApi
{
    public abstract class VersionedOutputFormatter : OutputFormatter
    {
        private readonly List<Tuple<VersionRange, OutputFormatter>> _formatters = new List<Tuple<VersionRange, OutputFormatter>>();

        public VersionedOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            /* ObjectType is set to null when controller returns 'null'
             * if (context.ObjectType == null)
                throw new ArgumentNullException(nameof(context.ObjectType));
                */
            return true;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var formatter = GetFormatter(GetVersion());
            return formatter.WriteAsync(context);
        }

        protected abstract Version GetVersion();

        protected abstract OutputFormatter GetDefaultFormatter();

        private OutputFormatter GetFormatter(Version version)
        {
            var formatter = _formatters.SingleOrDefault(x => x.Item1.Contains(version));
            return formatter?.Item2 ?? GetDefaultFormatter();
        }

        public void Register(Version fromInclusive, Version toExclusive, OutputFormatter formatter)
        {
            var range = new VersionRange(fromInclusive, toExclusive);
            if (_formatters.Any(f => f.Item1.IsOverlapping(range)) == true)
                throw new ArgumentException("Version ranges must not overlap.");
            _formatters.Add(new Tuple<VersionRange, OutputFormatter>(range, formatter));
        }
    }
}
