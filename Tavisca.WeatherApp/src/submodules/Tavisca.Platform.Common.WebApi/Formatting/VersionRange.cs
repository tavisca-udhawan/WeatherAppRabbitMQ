using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.WebApi
{
    internal class VersionRange
    {
        public VersionRange(Version fromInclusive, Version toExclusive)
        {
            if (fromInclusive >= toExclusive)
                throw new ArgumentException("From version must be less than to version.");
            From = fromInclusive;
            To = toExclusive;
        }

        public bool Contains(Version version)
        {
            /* We want to keep From as an inclusive check and To as an exclusive check.
               This is to handle sitations like 
               v1.0 -> v2.0 use formatterA
               v2.0 -> v3.0 use formatterB
               Here v2.0 should match the second range.
            */
            return From <= version && To > version;
        }

        public bool IsOverlapping(VersionRange other)
        {
            if (other == null)
                return false;
            bool isOnRight = other.From >= To;
            bool isOnLeft = other.To < From;
            return isOnRight == false && isOnLeft == false;
        }

        public Version From { get; }

        public Version To { get; }
    }
}
