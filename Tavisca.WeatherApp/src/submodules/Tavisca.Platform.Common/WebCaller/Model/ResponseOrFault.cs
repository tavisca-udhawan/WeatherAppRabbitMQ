namespace Tavisca.Platform.Common
{
    public class ResponseOrFault<TResponse, TFault>
    {
        public static ResponseOrFault<TResponse, TFault> Successful(TResponse response)
        {
            return new ResponseOrFault<TResponse, TFault>()
            {
                Response = response,
                Fault = default(TFault),
                IsFaulted = false
            };
        }

        public static ResponseOrFault<TResponse, TFault> Faulted(TFault fault)
        {
            return new ResponseOrFault<TResponse, TFault>()
            {
                Response = default(TResponse),
                Fault = fault,
                IsFaulted = true
            };
        }

        public TResponse Response { get; private set; }

        public TFault Fault { get; private set; }

        public bool IsFaulted { get; private set; }
    }
}
