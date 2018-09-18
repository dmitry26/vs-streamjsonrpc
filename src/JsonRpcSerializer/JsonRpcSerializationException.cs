// © Alexander Kozlenko. Licensed under the MIT License.

namespace System.Data.JsonRpc
{
    /// <summary>Represents an error that occurs during JSON-RPC message serialization or deserialization.</summary>
    public sealed class JsonRpcSerializationException : JsonRpcException
    {
        private readonly JsonRpcId _messageId;

		internal JsonRpcSerializationException(long errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
		}

		internal JsonRpcSerializationException(long errorCode,string message,in JsonRpcId messageId,Exception innerException = null)
			: this(errorCode,message,null,messageId,innerException)
		{
		}

		internal JsonRpcSerializationException(long errorCode, string message,string method, in JsonRpcId messageId, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
			Method = method;
			_messageId = messageId;			
        }

		/// <summary>Gets the identifier of the related JSON-RPC message.</summary>
		public ref readonly JsonRpcId MessageId => ref _messageId;		
        
        /// <summary>Gets the corresponding JSON-RPC error code.</summary>
        public long ErrorCode { get; }

		/// <summary>Gets a string containing the name of the JSON-RPC method.</summary>
		public string Method { get; }		

		/// <summary>Gets a value indicating whether the JSON-RPC message is a requwst.</summary>
		public bool IsRequest => !(Method is null);		

		/// <summary>Gets a value indicating whether the JSON-RPC request is a notification.</summary>
		public bool IsNotification => IsRequest && _messageId.Type == JsonRpcIdType.None;

		/// <summary>Gets a value indicating whether the JSON-RPC request is a system extension request.</summary>
		public bool IsSystem => IsRequest && JsonRpcProtocol.IsSystemMethod(Method);
	}
}