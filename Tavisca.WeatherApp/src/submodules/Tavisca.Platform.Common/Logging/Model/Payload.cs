using System;
using System.Text;

namespace Tavisca.Platform.Common.Logging
{
    [Serializable]
    public class Payload
    {
        public Payload(byte[] bytes, Encoding encoding = null) : this( () => bytes, bytes, encoding)
        {
        }

        public Payload(Func<byte[]> getBytes, Encoding encoding = null) : this(getBytes, null, encoding)
        {

        }

        public Payload(string value, Encoding encoding = null) : this(GetBytesFromString(value, encoding), encoding)
        {
        }

        private static byte[] GetBytesFromString(string value, Encoding encoding)
        {
            return value == null ? new byte[0] : (encoding ?? UTF8WithoutBom).GetBytes(value);
        }

        private Payload(Func<byte[]> getBytes, byte[] payload, Encoding encoding)
        {
            _value = payload;
            _getValue = getBytes;
            Encoding = encoding ?? UTF8WithoutBom;
        }

        

        private byte[] _value;
        private readonly Func<byte[]> _getValue;
        public Encoding Encoding { get; }
        private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);
        private static readonly byte[] Faulted = new byte[] { };

        public byte[] GetBytes()
        {
            _value = _value ?? GetBytesSafely() ?? new byte[] {};
            return _value;
        }

        private byte[] GetBytesSafely()
        {
            try
            {
                return _getValue == null ? new byte[] {} : _getValue();
            }
            catch
            {
                return Faulted;
            }
        }

        public string GetString()
        {
            var bytes = GetBytes();
            if (bytes?.Length > 0)
                return Encoding.GetString(GetBytes());
            else
                return string.Empty;
        }

        public int Length => GetBytes().Length;
    }
}
