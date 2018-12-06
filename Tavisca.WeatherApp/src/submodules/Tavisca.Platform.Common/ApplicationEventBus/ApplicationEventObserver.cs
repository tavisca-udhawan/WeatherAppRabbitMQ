using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ApplicationEventBus
{
    public class ApplicationEventObserver : IObserver<ApplicationEvent>
    {
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(ApplicationEvent value)
        {
            try
            {
                this.Process(value);
            }
            catch (Exception ex)
            {
               // DO logging
            }
        }

        public virtual void Process(ApplicationEvent eventData) { }


    }
}
