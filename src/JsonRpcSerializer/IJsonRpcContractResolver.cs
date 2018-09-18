// © Alexander Kozlenko. Licensed under the MIT License.
using System.Collections.Generic;

namespace System.Data.JsonRpc
{
    /// <summary>Defines a JSON-RPC message contract resolver.</summary>
    public interface IJsonRpcContractResolver
    {
		/// <summary>Gets the JSON-RPC request contract.</summary>
		/// <param name="method">The name of a JSON-RPC method.</param>
		/// <returns>The corresponding request contract or <see langword="null" />.</returns>
		IReadOnlyList<JsonRpcRequestContract> GetRequestContracts(string method);

        /// <summary>Gets the JSON-RPC response contract.</summary>
        /// <param name="messageId">The JSON-RPC message identifier.</param>
        /// <returns>The corresponding response contract or <see langword="null" />.</returns>
        JsonRpcResponseContract GetResponseContract(in JsonRpcId messageId);
    }
}