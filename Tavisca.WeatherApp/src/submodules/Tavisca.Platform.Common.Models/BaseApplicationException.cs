using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public class BaseApplicationException : Exception
    {
        #region Properties
        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public HttpStatusCode HttpStatusCode { get; }

        public List<Info> Info { get; } = new List<Info>();

        #endregion

        #region Constructors       
        

        public BaseApplicationException(string errorCode, string errorMessage, HttpStatusCode httpStatusCode, List<Info> info = null) : base(errorMessage)
        {
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
            this.HttpStatusCode = httpStatusCode;
            if (info != null)
                Info.AddRange(info);
        }
#if !NET_STANDARD
        public BaseApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ErrorCode = (string)info.GetValue(nameof(ErrorCode), typeof(string));
            this.ErrorMessage = (string)info.GetValue(nameof(ErrorMessage), typeof(string));
            this.HttpStatusCode = (HttpStatusCode)info.GetValue(nameof(HttpStatusCode), typeof(HttpStatusCode));
            this.Info = (List<Info>)info.GetValue(nameof(Info), typeof(List<Info>));
        }
#endif
        public BaseApplicationException(string message, string code) : base(message)
        {
            this.ErrorCode = code;
            this.ErrorMessage = message;
        }

        public BaseApplicationException(string message, string code, Exception inner) : base(message, inner)
        {
            this.ErrorCode = code;
            this.ErrorMessage = message;
        }

        #endregion

        #region Methods
#if !NET_STANDARD
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ErrorCode), this.ErrorCode);
            info.AddValue(nameof(ErrorMessage), this.ErrorMessage);
            info.AddValue(nameof(HttpStatusCode), this.HttpStatusCode, typeof(HttpStatusCode));
            info.AddValue(nameof(Info), this.Info, typeof(List<Info>));
            base.GetObjectData(info, context);
        }
#endif
    #endregion
    }
}
