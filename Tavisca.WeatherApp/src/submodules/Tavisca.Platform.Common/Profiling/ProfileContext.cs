using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Tavisca.Platform.Common.Context;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Profiling
{
    public class ProfileContext : IDisposable
    {
        private readonly bool _skipStatCollection;
        private readonly Stopwatch _timer = new Stopwatch();
        private readonly bool _isRoot;
        public event Action<ProfileTreeNode> OnDispose;
        public static AsyncLocal<Dictionary<string, string>> MacroLogInfos { get; private set; } = new AsyncLocal<Dictionary<string, string>>() { };

        public void SetMacroLog(params string[] data)
        {
            if (MacroLogInfos.Value != null)
            {
                var startTime = _currentScopeNode.Value.PerformanceLog.StartPointInMilliseconds;
                var timeTaken = (_timer.ElapsedMilliseconds - startTime);
                MacroLogInfos.Value[_currentScopeNode.Value.PerformanceLog.Info]
                    = $"startTime {startTime}|timeTaken {timeTaken / (double)1000}|{string.Join("|", data)}";
            }
        }

        public ProfileContext(string description, bool ensureRoot = false)
        {
            if (!IsProfilingEnabled())
                return;
            if (ensureRoot)
            {
                MacroLogInfos.Value = new Dictionary<string, string>();
            }

            if (ensureRoot && CurrentScopeNode != null)
                throw new InvalidOperationException("Cannot instantiate a root node. Non-null profiling context found.");

            _isRoot = ensureRoot;
            var parentNode = GetPendingUplineNode();
            if (!_isRoot && parentNode == null)
            {
                _skipStatCollection = true;
                return;
            }

            var newScopeNode = new ProfileTreeNode()
            {
                PerformanceLog = new PerformanceLog()
                {
                    Info = description
                },
                ParentNode = parentNode
            };

            if (parentNode != null)
                parentNode.ChildNodes.AddLast(newScopeNode);

            _currentScopeNode.Value = newScopeNode;
            _timer.Start();
        }

        internal static AsyncLocal<ProfileTreeNode> _currentScopeNode { get; set; } = new AsyncLocal<ProfileTreeNode>();
        public static ProfileTreeNode CurrentScopeNode => _currentScopeNode.Value;

        public void Dispose()
        {
            if (!IsProfilingEnabled())
                return;

            _timer.Stop();
            if (_skipStatCollection)
                return;
            var nodeToDispose = GetPendingUplineNode();

            if (nodeToDispose == null)
                return;

            nodeToDispose.PerformanceLog.TotalExecutionTime = _timer.Elapsed;
            if (_isRoot)
            {
                var timeStampReduceAmount = nodeToDispose.PerformanceLog.StartTimeStamp;
                AdjustStartPointForSubtree(nodeToDispose, timeStampReduceAmount);
            }
            if (OnDispose != null)
                OnDispose(nodeToDispose);

            _currentScopeNode.Value = _currentScopeNode.Value?.ParentNode;
        }

        internal ProfileTreeNode GetPendingUplineNode()
        {
            if (_currentScopeNode.Value?.PerformanceLog == null)
                return null;

            if (_currentScopeNode?.Value?.PerformanceLog != null && !_currentScopeNode.Value.PerformanceLog.TotalExecutionTime.HasValue)
                return _currentScopeNode.Value;

            var parent = _currentScopeNode?.Value?.ParentNode;
            while (true)
            {
                if (parent == null || parent?.PerformanceLog?.TotalExecutionTime == null)
                    return parent;
                parent = parent.ParentNode;
            }
        }

        private static void AdjustStartPointForSubtree(ProfileTreeNode node, long reductionMagnitude)
        {
            node.PerformanceLog.StartTimeStamp -= reductionMagnitude;
            if (node.ChildNodes.Any())
            {
                foreach (var childNode in node.ChildNodes)
                    AdjustStartPointForSubtree(childNode, reductionMagnitude);
            }
        }

        private static bool IsProfilingEnabled()
        {
            return CallContext.Current != null
                && CallContext.Current.IsProfilingEnabled;
        }
    }
}
