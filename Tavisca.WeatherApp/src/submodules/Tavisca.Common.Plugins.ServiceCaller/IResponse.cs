using System;
using System.Collections.Generic;
using System.Net;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public interface IResponse<T>
    {
        T ReturnObject { get; set; }
        bool IsSuccess { get; set; }
        object ErrorPayLoad { get; set; }
        HttpStatusCode HttpStatusCode { get; set; }
    }

    public class ErrorPayLoad
    {
        public int HttpStatusCode { get; set; }

        public ErrorInfo ErrorInfo { get; set; }
    }
    [Serializable]
    public class ErrorInfo
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public List<Info> Info { get; set; }
    }

    public class Info
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}
