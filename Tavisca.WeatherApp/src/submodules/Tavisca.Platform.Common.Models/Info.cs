using System;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public sealed class Info
    {
        public string Code { get; }
        public string Message { get; set; }

        public Info(string code, string message)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            Code = code;
            Message = message;
        }
    }
}
