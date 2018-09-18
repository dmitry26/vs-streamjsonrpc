﻿// © Alexander Kozlenko. Licensed under the MIT License.

using System.Data.JsonRpc.Resources;

namespace System.Data.JsonRpc
{
	/// <summary>Represents a JSON-RPC response message.</summary>
	public sealed class JsonRpcResponse : JsonRpcMessage
	{		
		/// <summary>Initializes a new instance of the <see cref="JsonRpcResponse" /> class.</summary>
		/// <param name="result">The produced result for successful request.</param>
		/// <param name="id">The identifier, which must be the same as the identifier in the JSON-RPC request.</param>
		/// <exception cref="ArgumentException"><paramref name="id" /> has undefined value.</exception>
		public JsonRpcResponse(object result,in JsonRpcId id)
			: base(id)
		{
			if (id.Type == JsonRpcIdType.None)
				throw new ArgumentException(Strings.GetString("response.undefined_id"),nameof(id));

			Result = result;
		}

		/// <summary>Initializes a new instance of the <see cref="JsonRpcResponse" /> class.</summary>
		/// <param name="error">The produced JSON-RPC error for unsuccessful request.</param>
		/// <param name="id">The identifier, which must be the same as the identifier in the JSON-RPC request.</param>
		/// <exception cref="ArgumentNullException"><paramref name="error" /> is <see langword="null" />.</exception>
		public JsonRpcResponse(JsonRpcError error,in JsonRpcId id)
			: base(id) =>
			Error = error ?? throw new ArgumentNullException(nameof(error));

		/// <summary>Gets the produced result for successful request.</summary>
		public object Result { get; }

		/// <summary>Gets the produced JSON-RPC error for unsuccessful request.</summary>
		public JsonRpcError Error { get; }
		
		/// <summary>Gets a value indicating whether the request was successful.</summary>
		public bool Success => Error is null;

		/// <summary>Gets a value indicating whether the response is an error.</summary>
		public bool IsError => !(Error is null);
	}
}