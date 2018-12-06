using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Tavisca.Platform.Common.WebApi
{
    public class DependencyInjectedControllerActivator : IHttpControllerActivator
    {
        public DependencyInjectedControllerActivator(HttpConfiguration configuration) { }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return ObjectFactory.GetInstance(controllerType) as IHttpController;
        }
    }
}