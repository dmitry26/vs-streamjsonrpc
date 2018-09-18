// © Alexander Kozlenko. Licensed under the MIT License.

namespace System.Data.JsonRpc
{
    /// <summary>Represents a JSON-RPC message deserialization result.</summary>
    /// <typeparam name="T">The type of the JSON-RPC message.</typeparam>
    public readonly struct JsonRpcMessageInfo<T> : IJsonRpcMessageInfo<T>
		where T : JsonRpcMessage
    {
        private readonly object _value;

        internal JsonRpcMessageInfo(T value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

		internal JsonRpcMessageInfo(JsonRpcSerializationException value)
		{
			_value = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>Gets a JSON-RPC message for successful deserialization.</summary>
		public T Message
        {
            get => _value as T;
        }

        /// <summary>Gets an exception for unsuccessful deserialization.</summary>
        public JsonRpcSerializationException Exception
        {
            get => _value as JsonRpcSerializationException;
        }

        /// <summary>Gets a value indicating whether the deserialization was successful.</summary>
        public bool IsValid
        {
            get => _value is JsonRpcMessage;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IJsonRpcMessageInfo<out T> where T : JsonRpcMessage
	{
		/// <summary>Gets an exception for unsuccessful deserialization.</summary>
		JsonRpcSerializationException Exception { get; }

		/// <summary>Gets a value indicating whether the deserialization was successful.</summary>
		bool IsValid { get; }
		
		/// <summary>Gets a JSON-RPC message for successful deserialization.</summary>
		T Message { get; }
	}
}