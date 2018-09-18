// © Alexander Kozlenko. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Data.JsonRpc
{
	/// <summary>Represents a JSON-RPC message contract resolver.</summary>
	public sealed class JsonRpcContractResolver : IJsonRpcContractResolver
	{
		private readonly ConcurrentDictionary<string,IReadOnlyList<JsonRpcRequestContract>> _staticRequestContracts = new ConcurrentDictionary<string,IReadOnlyList<JsonRpcRequestContract>>(StringComparer.Ordinal);
		private readonly ConcurrentDictionary<string,JsonRpcResponseContract> _staticResponseContracts = new ConcurrentDictionary<string,JsonRpcResponseContract>(StringComparer.Ordinal);
		private readonly ConcurrentDictionary<JsonRpcId,JsonRpcResponseContract> _dynamicResponseContracts = new ConcurrentDictionary<JsonRpcId,JsonRpcResponseContract>();
		private readonly IDictionary<JsonRpcId,string> _staticResponseBindings = new Dictionary<JsonRpcId,string>();

		/// <summary>Initializes a new instance of the <see cref="JsonRpcContractResolver" /> class.</summary>
		public JsonRpcContractResolver()
		{			
		}

		IReadOnlyList<JsonRpcRequestContract> IJsonRpcContractResolver.GetRequestContracts(string method)
		{
			if (method == null)			
				throw new ArgumentNullException(nameof(method));

			_staticRequestContracts.TryGetValue(method,out var contracts);
			return contracts;
		}

		JsonRpcResponseContract IJsonRpcContractResolver.GetResponseContract(in JsonRpcId messageId)
		{
			_dynamicResponseContracts.TryGetValue(messageId,out JsonRpcResponseContract contract);
			return contract;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="paramTypes"></param>		
		public void AddRequestContracts(string method,IEnumerable<(IReadOnlyList<Type> t,int n)> paramTypes)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			if (paramTypes == null)			
				throw new ArgumentNullException(nameof(paramTypes));			

			_staticRequestContracts[method] = paramTypes.Select(p => new JsonRpcRequestContract(p.t,p.n)).ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="paramsByName"></param>
		public void AddRequestContract(string method,IReadOnlyDictionary<string,Type> paramsByName)
		{
			if (method == null)
				throw new ArgumentNullException(nameof(method));

			if (paramsByName == null)
				throw new ArgumentNullException(nameof(paramsByName));

			_staticRequestContracts[method] = new[]
			{
				new JsonRpcRequestContract(paramsByName)
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messageId"></param>
		/// <param name="resultType"></param>
		public void AddResponseContract(in JsonRpcId messageId,Type resultType)
		{
			if (resultType == null)			
				throw new ArgumentNullException(nameof(resultType));			

			_dynamicResponseContracts[messageId] = new JsonRpcResponseContract(resultType);
		}		
				
		/// <summary>Removes the corresponding JSON-RPC response contract.</summary>
		/// <param name="messageId">The JSON-RPC message identifier.</param>
		public void RemoveResponseContract(in JsonRpcId messageId)
		{
			_dynamicResponseContracts.TryRemove(messageId,out _);
		}		
		
		/// <summary>Removes all JSON-RPC response contracts.</summary>
		public void ClearResponseContracts()
		{
			_dynamicResponseContracts.Clear();
		}		
	}
}