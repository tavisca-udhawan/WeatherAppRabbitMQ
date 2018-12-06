using StructureMap;
using System;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.StructureMap;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Profiling;
using Xunit;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace Tavisca.Platform.Common.Tests
{
    public class ProfilerTests
    {
        [Fact]
        public async Task TestProfileDataCollectionWhenProfilingIsEnabled()
        {
            using (new AmbientContextScope(new SampleContext() { IsProfilingEnabled = true }))
            {
                var container = GetContainer();
                var assertionsPassed = false;
                Action<ProfileTreeNode> assertions = (profileNode) =>
                {
                    Assert.NotNull(profileNode);
                    Assert.True(profileNode.PerformanceLog.Info.ToLower().Contains("sample profiling"));
                    Assert.Equal(1, profileNode.ChildNodes.Count);
                    var outerNode = profileNode.ChildNodes.First();
                    Assert.True(outerNode.PerformanceLog.Info.ToLower().Contains("outerstuff"));
                    Assert.Equal(2, outerNode.ChildNodes.Count);
                    foreach (var intermediateNode in outerNode.ChildNodes)
                    {
                        intermediateNode.PerformanceLog.Info.ToLower().Contains("intermediatestuff");
                        Assert.Equal(2, intermediateNode.ChildNodes.Count);
                        foreach (var innerNode in intermediateNode.ChildNodes)
                        {
                            innerNode.PerformanceLog.Info.ToLower().Contains("innerstuff");
                            Assert.Equal(0, innerNode.ChildNodes.Count);
                        }
                    }
                    assertionsPassed = true;
                };

                using (var context = new ProfileContext("Sample profiling", ensureRoot: true))
                {
                    context.OnDispose += assertions;
                    await container.GetInstance<IOuter>().DoOuterStuff().ConfigureAwait(false);
                }

                Assert.True(assertionsPassed);
            }
        }

        [Fact]
        public async Task TestProfileDataCollectionWhenProfilingDisabled()
        {
            bool disposeActionInvoked = false;
            using (new AmbientContextScope(new SampleContext() { IsProfilingEnabled = false }))
            {
                var container = GetContainer();

                Action<ProfileTreeNode> assertions = (profileNode) =>
                {
                    disposeActionInvoked = true;
                };

                using (var context = new ProfileContext("Sample profiling", ensureRoot: true))
                {
                    context.OnDispose += assertions;
                    await container.GetInstance<IOuter>().DoOuterStuff().ConfigureAwait(false);
                }

                Assert.False(disposeActionInvoked);
            }
        }

        [Fact]
        public async Task TestProfileCollectionSkipInCaseNoRootScopeHasBeenInstalled()
        {
            using (new AmbientContextScope(new SampleContext() { IsProfilingEnabled = false }))
            {
                var container = GetContainer();

                await container.GetInstance<IOuter>().DoOuterStuff().ConfigureAwait(false);

                Assert.Null(ProfileContext.CurrentScopeNode);
            }
        }

        [Fact]
        public void TestProfileScopeCreationFailureInCaseRootScopeIsOpenedMoreThanOnce()
        {
            using (new AmbientContextScope(new SampleContext() { IsProfilingEnabled = true }))
            {
                using (new ProfileContext(string.Empty, ensureRoot: true))
                {
                    Assert.Throws(typeof(InvalidOperationException), () =>
                    {
                        using (new ProfileContext(string.Empty, ensureRoot: true))
                        {
                        }
                    });
                }
            }
        }

        private static Container GetContainer()
        {
            var container = new Container(x =>
            {
                //x.Policies.Interceptors(new ProfilerInjectorPolicy());
                x.For<IOuter>().Use<Outer>();
                x.For<IIntermediate>().Use<Intermediate>();
                x.For<IInner>().Use<Inner>();
            });

            container.Configure(x => x.Policies.Interceptors(new ProfilerInjectorPolicy()));

            return container;
        }
    }

    #region private classes

    public interface IOuter
    {
        Task DoOuterStuff();
    }

    public interface IIntermediate
    {
        Task DoIntermediateStuff();
    }

    public interface IInner
    {
        Task DoInnerStuff();
    }

    public class Outer : IOuter
    {
        private IIntermediate _intermediate;
        public Outer(IIntermediate intermediate)
        {
            _intermediate = intermediate;
        }
        public async Task DoOuterStuff()
        {
            await _intermediate.DoIntermediateStuff();
            await _intermediate.DoIntermediateStuff();
        }
    }

    public class Intermediate : IIntermediate
    {
        private IInner _inner;
        public Intermediate(IInner inner)
        {
            _inner = inner;
        }
        public async Task DoIntermediateStuff()
        {
            await _inner.DoInnerStuff();
            await _inner.DoInnerStuff();
        }
    }

    public class Inner : IInner
    {
        public async Task DoInnerStuff()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public class SampleContext : CallContext
    {

    }

    #endregion
}
