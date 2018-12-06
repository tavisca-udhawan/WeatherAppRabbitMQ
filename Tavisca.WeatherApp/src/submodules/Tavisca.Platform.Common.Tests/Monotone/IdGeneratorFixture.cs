using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Monotone;
using Xunit;
using Xunit.Abstractions;

namespace Tavisca.Platform.Common.Tests
{
    public class IdGeneratorFixture
    {
        private readonly ITestOutputHelper _output;

        public IdGeneratorFixture(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ZeroIdTest()
        {
            var gen = new IdGenerator(0,0,0);
            Assert.Equal(0u, gen.CreateId(0u).Value);
        }

        [Fact]
        public void BigIdParsingShouldBeCaseInsensitiveTest()
        {
            BigId id1, id2;
            BigId.TryParse("Demo", out id1);
            BigId.TryParse("demo", out id2);
            Assert.Equal(id1, id2);
        }

        [Fact]
        public void BigIdParsingShouldReturnFalseForIdWhoseLongEquivalentIsGreaterThanMaxValue()
        {
            BigId id;
            Assert.False(BigId.TryParse("n4hsl87dnjk2e", out id));
        }

        [Fact]
        public void BigIdParsingShouldReturnTrueForIdWhoseLongEquivalentIsEqualToMaxValue()
        {
            BigId id;
            Assert.True(BigId.TryParse(new BigId(long.MaxValue).ToString(), out id));
        }

        [Fact]
        public void DifferentInstancesGenerateDifferentIdsTest()
        {
            var gen1 = new IdGenerator(0,0,0);
            var gen2 = new IdGenerator(0,0,1);
            Assert.NotEqual(gen1.CreateId(0u).Value, gen2.CreateId(0u).Value);
        }

        [Fact]
        public void OneIdTest()
        {
            var gen = new IdGenerator(0,0,0);
            Assert.Equal(1u << (IdGenerator.InstanceBits + IdGenerator.ZoneBits + IdGenerator.RegionBits), gen.CreateId(1u).Value);
        }

        [Fact]
        public void TwoInstancesWithZeroTimeTest()
        {
            var gen = new IdGenerator(0,0,1);
            Assert.Equal(1u, gen.CreateId(0).Value);
        }

        [Fact]
        public void TwoZonesWithZeroTimeTest()
        {
            var gen = new IdGenerator(0,1,0);
            Assert.Equal(1u << IdGenerator.InstanceBits, gen.CreateId(0).Value);
        }

        [Fact]
        public void TwoRegionsWithZeroTimeTest()
        {
            var gen = new IdGenerator(1,0,0);
            Assert.Equal(1u << (IdGenerator.InstanceBits + IdGenerator.ZoneBits), gen.CreateId(0).Value);
        }


        [Fact]
        public void IdGeneratorOverflowShouldThrowTest()
        {
            var value = (GetMask(64 - IdGenerator.InstanceBits - IdGenerator.RegionBits - IdGenerator.ZoneBits) & long.MaxValue) + 1;
            var gen = new IdGenerator(0,0,0);
            Assert.Throws<Exception>(() => gen.CreateId(value));
        }

        [Fact]
        public void ParallelIdGenerationShouldBeUniqueTest()
        {
            UniqueId.InitializeForSingleMachine();
            var ids = new string[1000];
            Parallel.For(0, 1000, new ParallelOptions() { MaxDegreeOfParallelism = 16 }, new Action<int>( i => ids[i] = UniqueId.NextId().ToString()));
            Assert.True(ids.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1000);
        }


        [Fact]
        public void ParallelIdGenerationPerformanceTest()
        {
            Stopwatch timer = Stopwatch.StartNew();
            var gen = new IdGenerator(0,0,0);
            for (int i = 0; i < 1000; i++)
            {
                gen.NextId();
            }
            timer.Stop();
            _output.WriteLine(((decimal)timer.ElapsedTicks / (decimal)Stopwatch.Frequency).ToString());
        }




        private long GetMask(int oneBits)
        {
            return (long.MaxValue << oneBits) ^ long.MaxValue;
        }
    }
}
