using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Tavisca.Common.Plugins.Configuration;
using System.Linq;

namespace Tavisca.Platform.Common.Tests
{
    [Ignore]
    [TestClass]
    public class ConsulConfigurationStoreTest
    {

        [TestMethod]
        public void GetConfugurationFromConsul()
        {

            var provider = new ConsulConfigurationStore();

            var result = provider.Get("global", "content_service", "hotel_content", "applicable_supplier_families");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetAllConfugurationFromConsul()
        {
            var provider = new ConsulConfigurationStore();

            var result = await provider.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >0);
        }


        [TestMethod]
        public async Task LoadTest_Consul_GetAll() {
            var provider = new ConsulConfigurationStore();
            var sTime = DateTime.Now;
            int load =1;
            var tasks = new Task<bool>[load];
            for (int i = 0; i < load; i++)
            {
               tasks[i] = Task.Run(async () => await GetAllAsync());
            }

            var continuation = Task.WhenAll(tasks);
            try
            {
                continuation.Wait();
            }
            catch (AggregateException)
            { }
          

            if (continuation.Status == TaskStatus.RanToCompletion)
            {
                Console.WriteLine(DateTime.Now.Subtract(sTime));
            }

          }

        public async Task<bool> GetAllAsync() {
            var provider = new ConsulConfigurationStore();
            Console.WriteLine("hello");
            var result = await provider.GetAllAsync();

            return result != null;

        }
    }


}
