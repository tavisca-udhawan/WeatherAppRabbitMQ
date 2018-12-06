using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public abstract class SuccessResponse
    {
        public List<Info> Warnings { get; } = new List<Info>();
    }
}
