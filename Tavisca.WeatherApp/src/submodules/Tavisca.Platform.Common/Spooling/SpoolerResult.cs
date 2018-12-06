using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Spooling
{
    public class SpoolerResult<TIn, TOut>
    {
        public TIn Input { get; set; }

        public TOut Result { get; set; }

        public Exception Fault { get; set; }

        public bool IsFaulted => Fault != null;
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
