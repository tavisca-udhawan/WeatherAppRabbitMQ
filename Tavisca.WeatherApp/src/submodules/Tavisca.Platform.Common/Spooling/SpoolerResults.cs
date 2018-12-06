using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Spooling
{
    public class SpoolerResults<T>
    {
        public T[] Results { get; set; }

        public Bookmark Bookmark { get; set; }

        public bool MoreResultsAvailable { get; set; }
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
