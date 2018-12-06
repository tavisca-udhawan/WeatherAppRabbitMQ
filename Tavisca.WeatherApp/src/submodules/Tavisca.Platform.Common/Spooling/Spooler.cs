using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Platform.Common.Spooling
{
    public class Spooler<TIn, TOut, TResult>
    {

        #region private members
        private IStateManager _stateManager;
        private readonly Converter<SpoolerResult<TIn, TOut>, TResult> _outputConverter;
        private readonly Func<TIn, Task<TOut>> _process;
        private string _transactionId;
        private TIn[] _inputs;
        private int _workItems;
        private SpoolerKeyCollection _keys;
        private const int STARTED = 1;
        private const int COMPLETED = 2;
        private const int FAULTED = 3;
        private List<Task> tasks = new List<Task>();
        private List<int> readIndices;
        #endregion

        public int BatchSize { get; set; } = 1;

        internal Spooler(IStateManager stateManager, Bookmark bookmark, int batchSize = 1)
        {
            _stateManager = stateManager;
            _transactionId = bookmark.TransactionId;
            _keys = new SpoolerKeyCollection(_transactionId, bookmark.TotalCount);
            _workItems = bookmark.TotalCount;
            BatchSize = batchSize;
            readIndices = bookmark.Indices.ToList();
        }

        private Spooler(IStateManager stateManager, TIn[] inputs, Func<TIn, Task<TOut>> process, Converter<SpoolerResult<TIn, TOut>, TResult> outputConverter, int batchSize = 1)
        {
            _stateManager = stateManager;
            _transactionId = Guid.NewGuid().ToString();
            _inputs = inputs;
            _process = process;
            _outputConverter = outputConverter;
            _workItems = inputs.Length;
            _keys = new SpoolerKeyCollection(_transactionId, inputs.Length);
            BatchSize = BatchSize;
            readIndices = new List<int>();
        }

        /// <summary>
        /// Creates new instance of spooler and runs process delegate in separate tasks for all inputs
        /// </summary>
        /// <param name="stateManager"></param>
        /// <param name="inputs">list of inputs to run tasks for</param>
        /// <param name="process">process to run for all inputs</param>
        /// <param name="outputConverter">translator for save task results before saving output to stateManager</param>
        /// <returns>newly created spooler instance</returns>
        public static async Task<Spooler<TIn, TOut, TResult>> StartNewAsync(IStateManager stateManager, TIn[] inputs, Func<TIn, Task<TOut>> process, Converter<SpoolerResult<TIn, TOut>, TResult> outputConverter, int batchSize = 1)
        {
            var spooler = new Spooler<TIn, TOut, TResult>(stateManager, inputs, process, outputConverter, batchSize);
            await spooler.StartAsync();
            return spooler;
        }

        /// <summary>
        /// This can be used if you want to wait on all tasks started inside spooler.
        /// </summary>
        /// <returns></returns>
        public List<Task> GetTasks()
        {
            return tasks;
        }

        public Bookmark GetBookmark()
        {
            return new Bookmark(_transactionId, _workItems, new int[0]);
        }

        //todo: refactor-spooler is created from bookmark and has state information with it
        public async Task<SpoolerResults<TResult>> GetResultsAsync(Bookmark bookmark)
        {
            /*
             * Read the status state for status of items
             * Get items to read and return along with bookmark.
             * If all items have been read then return moreResults as false.
            */

            try
            {
                // Get status of work.
                var unreadKeys = Enumerable.Range(0, bookmark.TotalCount)
                          .Except(bookmark.Indices)
                          .Select(x => _keys.GetStatusKey(x))
                          .ToArray();

                //Find statuses of work that is completed
                KeyValuePair<string, int>[] statuses;
                using (var profilerContext = new ProfileContext("GetStatuses"))
                {
                    statuses = (await _stateManager.MultiGetAsync<int>(unreadKeys))
                                     .Where(x => x.Value == COMPLETED || x.Value == FAULTED)
                                     .ToArray();
                    profilerContext.SetMacroLog($"keysCount {unreadKeys.Length}");
                }

                //get faulted indices
                var faultedIndices = ArrayExtension.ConvertAll(Array.FindAll(statuses, x => x.Value == FAULTED), x => _keys.ResolveIndex(x.Key));

                //read only the completed results and batch them
                var keysToBeRead = statuses.Where(x => x.Value == COMPLETED)
                                       .Take(BatchSize)
                                       .Select(x => x.Key)
                                       .Select(x => _keys.ResolveIndex(x))
                                       .Select(x => _keys.GetResultKey(x))
                                       .ToArray();

                var indicesToBeRead = ArrayExtension.ConvertAll(keysToBeRead, x => _keys.ResolveIndex(x));
                var moreResultsAvailable = unreadKeys.Length > faultedIndices.Length + indicesToBeRead.Length;

                if (moreResultsAvailable == false)
                {
                    string[] faultedResultKeys = await GetFaultedResultKeys(bookmark);
                    if (faultedResultKeys != null && faultedResultKeys.Length > 0)
                        keysToBeRead = keysToBeRead.Union(faultedResultKeys).ToArray();
                }
                TResult[] resultStates;
                using (var profilerContext = new ProfileContext("GetResults"))
                {
                    resultStates = ArrayExtension.ConvertAll((await _stateManager.MultiGetAsync<TResult>(keysToBeRead)), x => x.Value);
                    profilerContext.SetMacroLog($"keysCount {keysToBeRead.Length}");

                }
                return new SpoolerResults<TResult>
                {
                    Results = resultStates,
                    MoreResultsAvailable = moreResultsAvailable,
                    Bookmark = new Bookmark(bookmark.TransactionId, bookmark.TotalCount, bookmark.Indices.Union(indicesToBeRead).Union(faultedIndices).ToArray()),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region private methods
        private async Task<string[]> GetFaultedResultKeys(Bookmark bookmark)
        {
            var allStatusKeys = Enumerable.Range(0, bookmark.TotalCount)
                                      .Select(x => _keys.GetStatusKey(x))
                                      .ToArray();

            var faultedResultKeys = (await _stateManager.MultiGetAsync<int>(allStatusKeys)) //get status
                        .Where(x => x.Value == FAULTED)                                     //select faulted
                        .Select(x => _keys.ResolveIndex(x.Key))                             //resolve index
                        .Select(x => _keys.GetResultKey(x))                                   //get result key
                        .ToArray();
            return faultedResultKeys;
        }
        private async Task RunAsync(Func<TIn, Task<TOut>> process, TIn input, string txnId, int index)
        {
            SpoolerResult<TIn, TOut> result = null;
            var statusKey = _keys.GetStatusKey(index);
            var resultKey = _keys.GetResultKey(index);
            bool isFaulted = false;
            try
            {

                await _stateManager.SetAsync(statusKey, STARTED);
                TOut output;
                using (new ProfileContext("SpoolerProcess"))
                {
                    output = await process(input);
                }
                result = new SpoolerResult<TIn, TOut> { Input = input, Result = output };
            }
            catch (Exception ex)
            {
                isFaulted = true;
                result = new SpoolerResult<TIn, TOut> { Input = input, Fault = ex };
            }
            finally
            {
                var state = _outputConverter(result);
                await _stateManager.SetAsync(resultKey, state);
                if (isFaulted == true)
                    await _stateManager.SetAsync(statusKey, FAULTED);
                else
                    await _stateManager.SetAsync(statusKey, COMPLETED);
            }
        }
        private async Task StartAsync()
        {
            // Run all the tasks in parallel.
            for (int i = 0; i < _inputs.Length; i++)
            {
                var index = i;
                tasks.Add(RunAsync(_process, _inputs[i], _transactionId, index));
            }
            return;
        }
        #endregion

    }
}


/*
Specification:
- Gets a delegate to invoke along with the input arg.
- Delegate will either run or fault.

- Should be able to create a spooler with an existing txn id.
    var spooler = new Spooler(txnId);

- Should get results based on a previous continuation token.
    var results = spooler.GetResults(bookmark);

*/


