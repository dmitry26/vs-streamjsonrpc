namespace StreamJsonRpc
{
    using System;
    using System.Collections.Generic;
    using System.Data.JsonRpc;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class JsonRpcErrorExts
    {
        public static string ErrorStack(this JsonElement elem)
        {
            if (elem?.JsonType == JsonElementType.Object)
            {
                if (((dynamic)elem).stack is JsonValue val)
                {
                    return val;
                }
            }

            return null;
        }

        public static string ErrorCode(this JsonElement elem)
        {
            if (elem?.JsonType == JsonElementType.Object)
            {
                if (((dynamic)elem).code is JsonValue val)
                {
                    return val;
                }
            }

            return null;
        }

        public static object ErrorData(this JsonElement elem)
        {
            if (elem?.JsonType == JsonElementType.Value)
            {
                return ((JsonValue)elem).Value;
            }

            return elem;
        }
    }
}
