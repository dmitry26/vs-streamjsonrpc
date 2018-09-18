namespace StreamJsonRpc
{
    using System;
    using System.Collections.Generic;
    using System.Data.JsonRpc;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class JsonRpcErrorData
    {
        public JsonRpcErrorData(string code, string stack, object data)
        {
            this.ErrorCode = code;
            this.ErrorStack = stack;
            this.ErrorData = data;
        }

        public string ErrorStack { get; }

        public string ErrorCode { get; }

        public object ErrorData { get; }
    }
}
