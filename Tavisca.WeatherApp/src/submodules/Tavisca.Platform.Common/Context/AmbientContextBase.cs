using System;
using System.Threading;

namespace Tavisca.Platform.Common.Context
{
    [Serializable]
    public abstract class AmbientContextBase
    {
        private static readonly AsyncLocal<AmbientContextBase> _current = new AsyncLocal<AmbientContextBase>();

        public static AmbientContextBase Current
        {
            get
            {
                return _current.Value;
            }
            internal set
            {
                _current.Value = value;
            }
        }
    }
}
