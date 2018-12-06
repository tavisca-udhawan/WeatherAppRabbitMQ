using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aerospike;
using Tavisca.Common.Plugins.Configuration;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class CounterProviderTests
    {
        private const int _expirySeconds = 30;
        Random _random = new Random();
        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task CounterCreationShouldSucceedWithDefaultValueOfZero()
        {
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds));
            var val = await counter.GetCurrentValue(key);
            Assert.Equal(0, val);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task CounterCreationShouldSucceedWithGivenInitialValue()
        {
            var counterVal = 3;
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds), counterVal);
            var val = await counter.GetCurrentValue(key);
            Assert.Equal(counterVal, val);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task CounterCreationShouldFailInCaseCounterAlreadyExists()
        {
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds));
            await Assert.ThrowsAsync(typeof(AerospikeProviderException), async () => await counter.Create(key, TimeSpan.FromSeconds(1)));
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestDefaultCounterIncrementByOne()
        {
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds));
            var val = await counter.Increment(key);
            Assert.Equal(1, val);
            val = await counter.Increment(key);
            Assert.Equal(2, val);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestDefaultCounterDecrementByOne()
        {
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds));
            var val = await counter.Increment(key);
            Assert.Equal(1, val);
            val = await counter.Increment(key);
            Assert.Equal(2, val);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestCounterIncrementSpecifiedFactor()
        {
            var incrementFactor = 3;
            var currentValue = 0;
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds));

            currentValue += incrementFactor;
            var val = await counter.Increment(key, incrementFactor);
            Assert.Equal(currentValue, val);

            currentValue += incrementFactor;
            val = await counter.Increment(key, incrementFactor);
            Assert.Equal(currentValue, val);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestCounterDecrementSpecifiedFactor()
        {
            var decrementFactor = 3;
            var currentValue = 10;
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds), currentValue);

            currentValue -= decrementFactor;
            var val = await counter.Decrement(key, decrementFactor);
            Assert.Equal(currentValue, val);

            currentValue -= decrementFactor;
            val = await counter.Decrement(key, decrementFactor);
            Assert.Equal(currentValue, val);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestCouncurrentIncrementOperationsAtomicity()
        {
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds));

            var incrementTasks = new List<Task>();
            for (var i = 0; i < 100; i++)
                incrementTasks.Add(Task.Run(() => counter.Increment(key)));

            await Task.WhenAll(incrementTasks.ToArray());

            var counterVal = await counter.GetCurrentValue(key);
            Assert.Equal(incrementTasks.Count, counterVal);
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestCouncurrentDecrementOperationsAtomicity()
        {
            var key = Guid.NewGuid().ToString();
            var counter = new AerospikeAtomicCounter(GetClientFactory(), GetSettingsProvider());

            var initialValue = 1002;
            await counter.Create(key, TimeSpan.FromSeconds(_expirySeconds), initialValue);

            var decrementTasks = new List<Task>();
            for (var i = 0; i < 50; i++)
                decrementTasks.Add(Task.Run(() => counter.Decrement(key)));

            await Task.WhenAll(decrementTasks.ToArray());

            var counterVal = await counter.GetCurrentValue(key);
            Assert.Equal(initialValue - decrementTasks.Count, counterVal);
        }

        private static IAerospikeClientFactory GetClientFactory()
        {
            return new AerospikeClientFactory();
        }

        private static ICounterSettings _settingsProvider;
        private static ICounterSettings GetSettingsProvider()
        {
            return _settingsProvider ?? (_settingsProvider = new AerospikeSettingsProvider(new ConfigurationProvider("test_application")));
        }
    }
}
