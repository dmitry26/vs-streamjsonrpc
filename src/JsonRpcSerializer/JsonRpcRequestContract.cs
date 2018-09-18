// © Alexander Kozlenko. Licensed under the MIT License.

using System.Collections.Generic;

namespace System.Data.JsonRpc
{
    /// <summary>Represents a type contract for JSON-RPC request deserialization.</summary>
    public sealed class JsonRpcRequestContract : JsonRpcMessageContract
    {

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequestContract" /> class.</summary>
		public JsonRpcRequestContract()
        {
        }

		/// <summary>Initializes a new instance of the <see cref="JsonRpcRequestContract" /> class.</summary>
		/// <param name="parameters">The contract for parameters, provided by position.</param>
		/// <param name="requiredCount">The count of required parameters.</param>
		/// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
		public JsonRpcRequestContract(IReadOnlyList<Type> parameters,int requiredCount = -1)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

			if (parameters.Count > 0)
			{
				ParametersType = JsonRpcParametersType.ByPosition;
				ParametersByPosition = parameters;
				RequiredParamCount = requiredCount >= 0 && requiredCount < parameters.Count
					? requiredCount : parameters.Count;
			}
        }

        /// <summary>Initializes a new instance of the <see cref="JsonRpcRequestContract" /> class.</summary>
        /// <param name="parameters">The contract for parameters, provided by name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
        public JsonRpcRequestContract(IReadOnlyDictionary<string, Type> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

			if (parameters.Count > 0)
			{
				ParametersType = JsonRpcParametersType.ByName;
				ParametersByName = parameters;
				RequiredParamCount = parameters.Count;
			}
        }

        /// <summary>Gets the JSON-RPC method parameters type.</summary>
        public JsonRpcParametersType ParametersType { get; }        

        /// <summary>Gets the types of JSON-RPC method parameters, provided by name.</summary>
        public IReadOnlyDictionary<string, Type> ParametersByName { get; } 
        
        /// <summary>Gets the types of JSON-RPC method parameters, provided by position.</summary>
        public IReadOnlyList<Type> ParametersByPosition { get; }
        
		/// <summary>
		/// 
		/// </summary>
		public int RequiredParamCount { get; }

	}
}