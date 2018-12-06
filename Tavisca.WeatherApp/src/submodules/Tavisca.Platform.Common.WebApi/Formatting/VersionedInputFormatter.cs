using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.WebApi
{
    public abstract class VersionedInputFormatter : InputFormatter
    {
        private readonly List<Tuple<VersionRange, InputFormatter>> _formatters = new List<Tuple<VersionRange, InputFormatter>>();

        public VersionedInputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.ModelType == null)
                throw new ArgumentNullException(nameof(context.ModelType));
            return true;
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var formatter = GetFormatter(GetVersion());
            return await formatter.ReadRequestBodyAsync(context);
        }

        protected abstract Version GetVersion();

        protected abstract InputFormatter GetDefaultFormatter();

        private InputFormatter GetFormatter(Version version)
        {
            var formatter = _formatters.SingleOrDefault(x => x.Item1.Contains(version));
            return formatter?.Item2 ?? GetDefaultFormatter();
        }

        public void Register(Version fromInclusive, Version toExclusive, InputFormatter formatter)
        {
            var range = new VersionRange(fromInclusive, toExclusive);
            if (_formatters.Any(f => f.Item1.IsOverlapping(range)) == true)
                throw new ArgumentException("Version ranges must not overlap.");
            _formatters.Add(new Tuple<VersionRange, InputFormatter>(range, formatter));
        }
    }
}
