// © Alexander Kozlenko. Licensed under the MIT License.

using System.Collections.Generic;

namespace System.Data.JsonRpc
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IJsonRpcInfo<out T> where T : JsonRpcMessage
	{
		/// <summary>Gets a value indicating whether the data is a batch.</summary>
		bool IsBatch { get; }

		/// <summary>Gets a JSON-RPC message deserialization result for non-batch data.</summary>
		IJsonRpcMessageInfo<T> Message { get; }

		/// <summary>Gets a JSON-RPC message deserialization result for non-batch data.</summary>
		IReadOnlyList<IJsonRpcMessageInfo<T>> Messages { get; }
	}

	/// <summary>Represents a JSON-RPC data deserialization result.</summary>
	/// <typeparam name="T">The type of the JSON-RPC message.</typeparam>
	public sealed class JsonRpcInfo<T> : IJsonRpcInfo<T>
		where T : JsonRpcMessage
	{
		internal JsonRpcInfo(IJsonRpcMessageInfo<T> message) =>
			Message = message ?? throw new ArgumentNullException(nameof(message));

		internal JsonRpcInfo(IReadOnlyList<IJsonRpcMessageInfo<T>> messages) =>
			Messages = (messages?.Count ?? 0) > 0 ? messages : throw new ArgumentException("Null or empty",nameof(messages));

		/// <summary>Gets a value indicating whether the data is a batch.</summary>
		public bool IsBatch => !(Messages is null);

		/// <summary>Gets a JSON-RPC message deserialization result for non-batch data.</summary>
		public IJsonRpcMessageInfo<T> Message { get; }
        
        /// <summary>Gets a collection of JSON-RPC message deserialization results for batch data.</summary>
        public IReadOnlyList<IJsonRpcMessageInfo<T>> Messages { get; }       
    }
}