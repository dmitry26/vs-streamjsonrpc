// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace StreamJsonRpc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft;
	using System.Data.JsonRpc;

    internal sealed class TargetMethod
    {
        private readonly HashSet<string> errorMessages = new HashSet<string>(StringComparer.Ordinal);
        private readonly JsonRpcRequest request;
        private readonly object target;
        private readonly MethodInfo method;
        private readonly object[] parameters;

        internal TargetMethod(
            JsonRpcRequest request,
            IEnumerable<MethodSignatureAndTarget> candidateMethodTargets)
        {
            Requires.NotNull(request, nameof(request));
            Requires.NotNull(candidateMethodTargets, nameof(candidateMethodTargets));

            this.request = request;

            var targetMethods = new Dictionary<MethodSignatureAndTarget, object[]>();
            foreach (var method in candidateMethodTargets)
            {
                this.TryAddMethod(request, targetMethods, method);
            }

            KeyValuePair<MethodSignatureAndTarget, object[]> methodWithParameters = targetMethods.FirstOrDefault();
            if (methodWithParameters.Key.Signature != null)
            {
                this.target = methodWithParameters.Key.Target;
                this.method = methodWithParameters.Key.Signature.MethodInfo;
                this.parameters = methodWithParameters.Value;
                this.AcceptsCancellationToken = methodWithParameters.Key.Signature.HasCancellationTokenParameter;
            }
        }

        internal bool IsFound => this.method != null;

        internal bool AcceptsCancellationToken { get; private set; }

        internal string LookupErrorMessage
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.UnableToFindMethod,
                    this.request.Method,
                    this.request.ParameterCount,
                    this.target?.GetType().FullName ?? "{no object}",
                    string.Join("; ", this.errorMessages));
            }
        }

        internal object Invoke(CancellationToken cancellationToken)
        {
            if (this.method == null)
            {
                throw new InvalidOperationException(this.LookupErrorMessage);
            }

            if (cancellationToken.CanBeCanceled && this.AcceptsCancellationToken)
            {
                for (int i = this.parameters.Length - 1; i >= 0; i--)
                {
                    if (this.parameters[i] is CancellationToken)
                    {
                        this.parameters[i] = cancellationToken;
                        break;
                    }
                }
            }

            return this.method.Invoke(!this.method.IsStatic ? this.target : null, this.parameters);
        }

        private static object[] TryGetParameters(JsonRpcRequest request, MethodSignature method, HashSet<string> errors, string requestMethodName)
        {
            Requires.NotNull(request, nameof(request));
            Requires.NotNull(method, nameof(method));
            Requires.NotNull(errors, nameof(errors));
            Requires.NotNullOrEmpty(requestMethodName, nameof(requestMethodName));

            // ref and out parameters aren't supported.
            if (method.HasOutOrRefParameters)
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.MethodHasRefOrOutParameters, method));
                return null;
            }

            if (request.HasParamsByName)
            {
                // If the parameter passed is an object, then we want to find the matching method with the same name and the method only takes a JToken as a parameter,
                // and possibly a CancellationToken
                if (method.Parameters.Length < 1 || method.Parameters[0].ParameterType != typeof(JsonObject))
                {
                    return null;
                }

                var args = new List<object>(2);
                args.Add(request.ParametersByName);

                if (method.Parameters.Length > 1 && method.Parameters[1].ParameterType == typeof(CancellationToken))
                {
                    args.Add(CancellationToken.None);
                }

                if (method.Parameters.Length > 2)
                {
                    // We don't support methods with more than two parameters.
                    return null;
                }

                return args.ToArray();
            }

            // The number of parameters must fall within required and total parameters.
            int paramCount = request.ParameterCount;
            if (paramCount < method.RequiredParamCount || paramCount > method.TotalParamCountExcludingCancellationToken)
            {
                string methodParameterCount;
                if (method.RequiredParamCount == method.TotalParamCountExcludingCancellationToken)
                {
                    methodParameterCount = method.RequiredParamCount.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    methodParameterCount = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", method.RequiredParamCount, method.TotalParamCountExcludingCancellationToken);
                }

                errors.Add(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MethodParameterCountDoesNotMatch,
                    method,
                    methodParameterCount,
                    request.ParameterCount));

                return null;
            }

            // Parameters must be compatible
            try
            {
                return GetParameters(request, method.Parameters);
            }
            catch (Exception exception)
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resources.MethodParametersNotCompatible, method, exception.Message));
                return null;
            }
        }

		private static object[] GetParameters(JsonRpcRequest request,ParameterInfo[] parameterInfos)
		{
			Requires.NotNull(parameterInfos,nameof(parameterInfos));

			if (parameterInfos.Length == 0)
				return new object[0];

			int index = 0;
			var result = new List<object>(parameterInfos.Length);

			foreach (var param in request.ParametersByPosition ?? new object[0])
			{
				var type = typeof(object);

				if (index < parameterInfos.Length)
				{
					type = parameterInfos[index].ParameterType;
					index++;
				}

				if (param is null)
				{
					if (type.IsValueType && !type.IsNullable())
						throw new InvalidOperationException($"The parameter is null, and not compatible with merthod parameter {type}");

					result.Add(param);
				}
				else if (type.IsAssignableFrom(param.GetType()))
					result.Add(param);
				else
					throw new InvalidOperationException($"The parameter {param.GetType()} is not compatible with merthod parameter {type}");
			}

			for (; index < parameterInfos.Length; index++)
			{
				object value =
				  parameterInfos[index].HasDefaultValue ? parameterInfos[index].DefaultValue :
				  parameterInfos[index].ParameterType == typeof(CancellationToken) ? (CancellationToken?)CancellationToken.None :
				  null;
				result.Add(value);
			}

			return result.ToArray();
		}

        private bool TryAddMethod(JsonRpcRequest request, Dictionary<MethodSignatureAndTarget, object[]> targetMethods, MethodSignatureAndTarget method)
        {
            Requires.NotNull(request, nameof(request));
            Requires.NotNull(targetMethods, nameof(targetMethods));

            object[] parameters = TryGetParameters(request, method.Signature, this.errorMessages, request.Method);
            if (parameters != null)
            {
                targetMethods.Add(method, parameters);
                return true;
            }

            return false;
        }
    }
}