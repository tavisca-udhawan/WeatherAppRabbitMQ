using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Spooling;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class SpoolerFixture
    {
        private readonly IStateManager _stateManager = new StubStateManager();
        private readonly List<int> _requests = new List<int> { 8, 2, 5, 5, 7, 3, 6, 1, 5, 9, 7 };

        private Tuple<int, string> ResultConvertor(SpoolerResult<int, int> spoolerResult)
        {
            return new Tuple<int, string>(spoolerResult.Input, spoolerResult.Result.ToString());

        }
        private async Task<int> Double(int input)
        {
            await Task.Delay(input * 100);
            return 2 * input;
        }

        [Fact]
        public async Task CreateSpoolerWithTasks_GivesValidBookMark()
        {
            Spooler<int, int, Tuple<int, string>> spooler = await Spooler<int, int, Tuple<int, string>>.StartNewAsync(_stateManager, _requests.ToArray(), Double, ResultConvertor);
            var bookmark = spooler.GetBookmark();
            Assert.NotNull(bookmark);
            Assert.Equal(11, bookmark.TotalCount);
        }

        [Fact]
        public async Task CreateSpoolerFromBookmark_GivesResults()
        {
            Spooler<int, int, Tuple<int, string>> spooler = await Spooler<int, int, Tuple<int, string>>.StartNewAsync(_stateManager, _requests.ToArray(), Double, ResultConvertor);
            var bookmark = spooler.GetBookmark();

            var newSpooler = bookmark.CreateSpooler<int, int, Tuple<int, string>>(_stateManager, 2);

            var results = new List<string>();
            SpoolerResults<Tuple<int, string>> result;
            do
            {
                result = await newSpooler.GetResultsAsync(bookmark);
                bookmark = result.Bookmark;
                results.AddRange(result.Results.Select(x => x.Item2));
            } while (result.MoreResultsAvailable);

            Assert.Equal(11, results.Count);
        }

        [Fact]
        public async Task CreateSpoolerWithTasks_GivesTasks()
        {
            Spooler<int, int, Tuple<int, string>> spooler = await Spooler<int, int, Tuple<int, string>>.StartNewAsync(_stateManager, _requests.ToArray(), Double, ResultConvertor, 5);
            var tasks = spooler.GetTasks();
            Assert.Equal(11, tasks.Count);
        }
    }
}
