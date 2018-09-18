// © Alexander Kozlenko. Licensed under the MIT License.

using System.Collections.Generic;

namespace System.Data.JsonRpc
{
	/// <summary>Represents a JSON-RPC request message.</summary>
	public sealed class JsonRpcRequest : JsonRpcMessage
	{
		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequest" /> class.</summary>
		/// <param name="method">The string containing the name of the JSON-RPC method to be invoked.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method" /> is <see langword="null" />.</exception>
		public JsonRpcRequest(string method) =>
			Method = method ?? throw new ArgumentNullException(nameof(method));

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequest" /> class.</summary>
		/// <param name="method">The string containing the name of the JSON-RPC method to be invoked.</param>
		/// <param name="parameters">The parameters to be used during the invocation of the JSON-RPC method, provided by position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method" /> or <paramref name="parameters" /> is <see langword="null" />.</exception>
		public JsonRpcRequest(string method,IReadOnlyList<object> parameters)
			: this(method) =>
			ParametersByPosition = parameters ?? throw new ArgumentNullException(nameof(parameters));

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequest" /> class.</summary>
		/// <param name="method">The string containing the name of the JSON-RPC method to be invoked.</param>
		/// <param name="parameters">The parameters to be used during the invocation of the JSON-RPC method, provided by name.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method" /> or <paramref name="parameters" /> is <see langword="null" />.</exception>
		public JsonRpcRequest(string method,IReadOnlyDictionary<string,object> parameters)
			: this(method) =>
			ParametersByName = parameters ?? throw new ArgumentNullException(nameof(parameters));

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequest" /> class.</summary>
		/// <param name="method">The string containing the name of the JSON-RPC method to be invoked.</param>
		/// <param name="id">The identifier established by the client.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method" /> is <see langword="null" />.</exception>
		public JsonRpcRequest(string method,in JsonRpcId id)
			: base(id) =>
			Method = method ?? throw new ArgumentNullException(nameof(method));

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequest" /> class.</summary>
		/// <param name="method">The string containing the name of the JSON-RPC method to be invoked.</param>
		/// <param name="id">The identifier established by the client.</param>
		/// <param name="parameters">The parameters to be used during the invocation of the JSON-RPC method, provided by position.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method" /> or <paramref name="parameters" /> is <see langword="null" />.</exception>
		public JsonRpcRequest(string method,in JsonRpcId id,IReadOnlyList<object> parameters)
			: this(method,id) =>
			ParametersByPosition = parameters ?? throw new ArgumentNullException(nameof(parameters));

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequest" /> class.</summary>
		/// <param name="method">The string containing the name of the JSON-RPC method to be invoked.</param>
		/// <param name="id">The identifier established by the client.</param>
		/// <param name="parameters">The parameters to be used during the invocation of the JSON-RPC method, provided by name.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method" /> or <paramref name="parameters" /> is <see langword="null" />.</exception>
		public JsonRpcRequest(string method,in JsonRpcId id,IReadOnlyDictionary<string,object> parameters)
			: this(method,id) =>
			ParametersByName = parameters ?? throw new ArgumentNullException(nameof(parameters));

		/// <summary>Gets a string containing the name of the JSON-RPC method to be invoked.</summary>
		public string Method { get; }

		/// <summary>Gets the JSON-RPC method parameters, provided by name.</summary>
		public IReadOnlyDictionary<string,object> ParametersByName { get; }

		/// <summary>Gets the JSON-RPC method parameters, provided by position.</summary>
		public IReadOnlyList<object> ParametersByPosition { get; }

		/// <summary>
		/// Gets the number of parameters
		/// </summary>
		public int ParameterCount => !(ParametersByPosition is null)
			? ParametersByPosition.Count
			: !(ParametersByName is null)
				? ParametersByName.Count
				: 0;

		/// <summary>Gets a value indicating whether the request parameters are by name.</summary>
		public bool HasParamsByName => !(ParametersByName is null);

		/// <summary>Gets a value indicating whether the request parameters are by position.</summary>
		public bool HasParamsByPosition => !(ParametersByPosition is null);

		/// <summary>Gets a value indicating whether the JSON-RPC request is a notification.</summary>
		public bool IsNotification => Id.Type == JsonRpcIdType.None;

		/// <summary>Gets a value indicating whether the JSON-RPC request is a system extension request.</summary>
		public bool IsSystem =>	JsonRpcProtocol.IsSystemMethod(Method);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static JsonRpcRequest CreateNotification(string method) =>		
			new JsonRpcRequest(method);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static JsonRpcRequest CreateNotification(string method,IReadOnlyList<object> parameters) =>
			new JsonRpcRequest(method,parameters);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="method"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static JsonRpcRequest CreateNotification(string method,IReadOnlyDictionary<string,object> parameters) =>
			new JsonRpcRequest(method,parameters);
	}
}