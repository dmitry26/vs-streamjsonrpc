// © Alexander Kozlenko. Licensed under the MIT License.

using System.Data.JsonRpc.Resources;

namespace System.Data.JsonRpc
{
    /// <summary>Represents a JSON-RPC error.</summary>
    public sealed class JsonRpcError
    {
        /// <summary>Initializes a new instance of the <see cref="JsonRpcError" /> class.</summary>
        /// <param name="code">The number that indicates the error type that occurred.</param>
        /// <param name="message">The string providing a short description of the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="code" /> is outside the allowable range.</exception>
        public JsonRpcError(long code, string message)
        {           
            Code = JsonRpcProtocol.IsSystemErrorCode(code) && !JsonRpcProtocol.IsServerErrorCode(code) && !JsonRpcProtocol.IsStandardErrorCode(code)
			? throw new ArgumentOutOfRangeException(nameof(code),code,Strings.GetString("error.code.invalid_range"))
			: code;

            Message = message ?? throw new ArgumentNullException(nameof(message));
		}

        /// <summary>Initializes a new instance of the <see cref="JsonRpcError" /> class.</summary>
        /// <param name="code">The number that indicates the error type that occurred.</param>
        /// <param name="message">The string providing a short description of the error.</param>
        /// <param name="data">The primitive or structured value that contains additional information about the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="code" /> is outside the allowable range.</exception>
        public JsonRpcError(long code, string message, object data)
            : this(code, message)
        {
            Data = data;
            HasData = true;
        }

        /// <summary>Gets a number that indicates the error type that occurred.</summary>
        public long Code { get; }        

        /// <summary>Gets a string providing a short description of the error.</summary>
        public string Message { get; }        

        /// <summary>Gets an optional value that contains additional information about the error.</summary>
        public object Data { get; }
       
        /// <summary>Gets a value indicating whether the additional information about the error is specified.</summary>
        public bool HasData { get; }     

		/// <summary>Used when a JSON error value is set as null.</summary>
		public static JsonRpcError JsonNull { get; } = new JsonRpcError(default,string.Empty);
	}
}