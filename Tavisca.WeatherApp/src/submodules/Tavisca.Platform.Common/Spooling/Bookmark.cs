using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tavisca.Platform.Common.Spooling
{
    public class Bookmark
    {
        public Bookmark(string trnasactionId, int totalCount, int[] indices)
        {
            TransactionId = trnasactionId;
            TotalCount = totalCount;
            Indices = indices;
        }

        public string TransactionId { get; }

        public int TotalCount { get; }

        public int[] Indices { get; }

        public static bool TryParse(string value, out Bookmark bookmark)
        {
            try
            {
                var decoded = value.Base64Decode();
                var parts = decoded.Split('|');
                var transactionId = parts[0];
                var totalCount = Convert.ToInt32(parts[1]);
                var indices = parts[2].Split(',').Where(x => !String.IsNullOrWhiteSpace(x)).Select(i => Convert.ToInt32(i)).ToArray();
                bookmark = new Bookmark(parts[0], totalCount, indices);

            }
            catch
            {
                bookmark = null;
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return ($"{TransactionId}|{TotalCount}|{string.Join(",", Indices)}").Base64Encode();
        }

        public Spooler<TIn, TOut, TResult> CreateSpooler<TIn, TOut, TResult>(IStateManager stateManager, int batchSize)
        {
            var spooler = new Spooler<TIn, TOut, TResult>(stateManager, this, batchSize);
            return spooler;
        }
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
