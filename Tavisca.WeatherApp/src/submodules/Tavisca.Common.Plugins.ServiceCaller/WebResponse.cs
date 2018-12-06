using System;
using System.Net;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    [Serializable]
    public class WebResponse<T> : IResponse<T>
    {
        public T ReturnObject { get; set; }
        public bool IsSuccess { get; set; }
        public object ErrorPayLoad { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
