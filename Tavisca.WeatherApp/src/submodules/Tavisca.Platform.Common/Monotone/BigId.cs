using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public struct BigId : IEquatable<BigId>
    {
        private readonly long _value;

        public BigId(long value) : this()
        {
            _value = value;
            _stringValue = null;
        }

        public long Value { get { return _value; } }


        public static BigId None { get { return new BigId(0); } }

        private string _stringValue;
        public override string ToString()
        {
            _stringValue = _stringValue ?? ToBase36(_value);
            return _stringValue;
        }

        private static readonly char[] Digits = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
            'u', 'v', 'w', 'x', 'y', 'z'
        };


        private string ToBase36(long value)
        {
            // 13 is the worst cast buffer size for base 36 and ulong.MaxValue
            uint maxSize = 13;
            uint targetBase = 36;
            char[] buffer = new char[] { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
            uint i = maxSize;
            do
            {
                buffer[--i] = Digits[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);
            return new string(buffer);
        }

        public static bool TryParse(string value, out BigId id)
        {
            try
            {
                id = default(BigId);
                if (string.IsNullOrWhiteSpace(value) == true)
                    return false;
                value = value.ToLower();
                if (value.Length > 13)
                    return false;
                if (value.Length < 13)
                    value = new string('0', 13 - value.Length) + value;


                long idValue = 0;
                var digits = value.ToCharArray();
                for (int i = digits.Length - 1; i >= 0; i--)
                {
                    var digit = digits[i];
                    // Ensure that digit is valid
                    if (char.IsDigit(digit) == true)
                        idValue += Convert.ToInt64(Math.Pow(36, 12 - i) * ((digit - '0')));
                    else if (IsAlpha(digit) == true)
                        idValue += Convert.ToInt64(Math.Pow(36, 12 - i) * ((digit - 'a') + 10));
                    else return false;
                }
                id = new BigId(idValue);

                return true;
            }
            catch
            {
                id = None;
                return false;
            }

        }

        public string ToNumericString()
        {
            return this.Value.ToString();
        }

        private static bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        public override bool Equals(object obj)
        {
            if (obj is BigId)
                return Equals((BigId)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(BigId other)
        {
            return Value.Equals(other.Value);
        }
    }
}