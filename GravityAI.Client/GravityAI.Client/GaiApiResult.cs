using System.Collections.Generic;
using System.Linq;

namespace GravityAI.Client
{

    internal class GaiApiResult
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }

    internal class GaiApiResult<T> : GaiApiResult where T : class
    {
        public T Data { get; set; }
    }

}
