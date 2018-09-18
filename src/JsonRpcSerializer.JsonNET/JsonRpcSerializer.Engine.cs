// © Alexander Kozlenko. Licensed under the MIT License.

using System.Collections.Generic;
using System.Data.JsonRpc.Resources;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Data.JsonRpc
{
	public partial class JsonRpcSerializer
	{
		private static readonly JsonLoadSettings _jsonLoadSettings = CreateJsonLoadSettings();
		private static readonly JsonSerializerSettings _jsonSerializerSettings = CreateJsonSerializerSettings();

		private readonly IJsonRpcContractResolver _contractResolver;
		private readonly JsonSerializer _jsonSerializer;
		private readonly JsonRpcCompatibilityLevel _compatibilityLevel;

		private static JsonLoadSettings CreateJsonLoadSettings()
		{
			return new JsonLoadSettings
			{
				LineInfoHandling = LineInfoHandling.Ignore
			};
		}

		private static JsonSerializerSettings CreateJsonSerializerSettings()
		{
			return new JsonSerializerSettings
			{
				MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
				ContractResolver = new JsonElementContractResolver()				
			};
		}

		private JsonRpcInfo<JsonRpcMessage> DeserializeMessages(JsonTextReader reader,CancellationToken cancellationToken = default)
		{
			if (_contractResolver == null)
			{
				throw new InvalidOperationException(Strings.GetString("core.deserialize.resolver.undefined"));
			}

			var messages = default(List<IJsonRpcMessageInfo<JsonRpcMessage>>);

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.StartObject)
				{
					if (messages == null)
					{
						return new JsonRpcInfo<JsonRpcMessage>(DeserializeMessage(reader));
					}
					else
					{
						cancellationToken.ThrowIfCancellationRequested();
						messages.Add(DeserializeMessage(reader));
					}
				}
				else if (reader.TokenType == JsonToken.StartArray)
				{
					messages = new List<IJsonRpcMessageInfo<JsonRpcMessage>>();
				}
				else if (reader.TokenType == JsonToken.EndArray)
				{
					if (messages.Count == 0)
					{
						throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.batch.empty"));
					}

					return new JsonRpcInfo<JsonRpcMessage>(messages.ToArray());
				}
				else
				{
					if (messages == null)
					{
						break;
					}
					else
					{
						cancellationToken.ThrowIfCancellationRequested();

						var exceptionMessage = string.Format(Strings.GetString("core.batch.invalid_item"),messages.Count);
						var exception = new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,exceptionMessage);

						messages.Add(new JsonRpcMessageInfo<JsonRpcMessage>(exception));
					}
				}
			}

			if (reader.TokenType == JsonToken.None)
			{
				throw new JsonReaderException(Strings.GetString("core.deserialize.json_issue"));
			}
			else
			{
				throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.input.invalid_structure"));
			}
		}

		private static class MessageType
		{
			public const int None = 0;			
			public const int HasMethod = 1;
			public const int HasParams = 1 << 1;
			public const int HasResult = 1 << 2;
			public const int HasError = 1 << 3;			
			public const int Request = HasMethod|HasParams;
			public const int Response = HasResult|HasError;
		}
		
		private IJsonRpcMessageInfo<JsonRpcMessage> DeserializeMessage(JsonTextReader reader)
		{
			try
			{
				var protocolVersion = default(string);
				var requestMethod = default(string);
				var msgId = default(JsonRpcId);
				var requestParamsetersToken = default(JContainer);
				var msgType = MessageType.None;

				JsonRpcError rpcError = null;
				JToken rspResultToken = default;
				long? rspErrorCode = default;
				string rspErrorMessage = default;
				JToken rspErrorDataToken = default;
				bool rspErrorSetAsNull = default;

				JsonRpcSerializationException CreateExceptionWithMsgId(long code,string name,bool skipFirst = true)
				{
					return new JsonRpcSerializationException(code,Strings.GetString(name),requestMethod,
						msgId.Type == JsonRpcIdType.None ? FindMsgId(reader,skipFirst) : msgId);
				}

				bool IsRequest() => (msgType & MessageType.Request) > 0;
				bool IsResponse() => (msgType & MessageType.Response) > 0;
				bool HasMethod() => (msgType & MessageType.HasMethod) > 0;
				bool HasParams() => (msgType & MessageType.HasParams) > 0;
				bool HasError() => (msgType & MessageType.HasError) > 0;
				bool HasResult() => (msgType & MessageType.HasResult) > 0;

				while (reader.Read())
				{
					if (reader.TokenType == JsonToken.PropertyName)
					{
						var jsonPropertyName = (string)reader.Value;
						reader.Read();

						switch (jsonPropertyName)
						{
							case "jsonrpc":
								if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
								{
									if (reader.TokenType != JsonToken.String)
										throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.request.protocol.invalid_property");
									protocolVersion = (string)reader.Value;
								}
								else
									throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.request.protocol.invalid_property");
								break;
							case "method":
								if (IsResponse() || HasMethod())
									throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.request.method.invalid_property");
								msgType |= MessageType.HasMethod;
								if (reader.TokenType != JsonToken.String)
									throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.request.method.invalid_property");
								requestMethod = (string)reader.Value;
								break;
							case "params":
								if (IsResponse() || HasParams())
									throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.request.params.invalid_property");
								msgType |= MessageType.HasParams;
								switch (reader.TokenType)
								{
									case JsonToken.StartArray:
										requestParamsetersToken = JArray.Load(reader,_jsonLoadSettings);
										break;
									case JsonToken.StartObject:
										requestParamsetersToken = JObject.Load(reader,_jsonLoadSettings);
										break;
									default:
										throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.request.params.invalid_property");
								}
								break;
							case "id":
								msgId = ReadMsgId(reader,true);
								break;
							case "result":
								if (IsRequest() || HasResult())
									throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidResponse,"core.deserialize.response.invalid_properties");
								msgType |= MessageType.HasResult;
								rspResultToken = JToken.ReadFrom(reader,_jsonLoadSettings);
								break;
							case "error":
								if (IsRequest() || HasError())
									throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidResponse,"core.deserialize.response.invalid_properties");
								msgType |= MessageType.HasError;
								(rspErrorCode, rspErrorMessage, rspErrorDataToken, rspErrorSetAsNull) = ReadResponseError(reader,msgId);
								break;
							default:
								reader.Skip();
								break;
						}
					}
					else if (reader.TokenType == JsonToken.EndObject)
						break;
					else
						reader.Skip();
				}

				if (IsRequest())
				{
					if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
					{
						if (protocolVersion != "2.0")
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.request.protocol.invalid_property"),requestMethod,msgId);
					}

					if (requestMethod == null)
						throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.request.method.invalid_property"),msgId);

					var requestContracts = _contractResolver.GetRequestContracts(requestMethod);

					if (requestContracts == null)
					{
						var exceptionMessage = string.Format(Strings.GetString("core.deserialize.request.method.unsupported"),requestMethod);
						throw new JsonRpcSerializationException(JsonRpcErrorCode.MethodNotFound,exceptionMessage,requestMethod,msgId);
					}
					
					var request = requestContracts.Select(c => GetRpcRequest(requestMethod,msgId,requestParamsetersToken,c)).FirstOrDefault(p => !(p is null));

					if (request == null)
					{
						var exceptionMessage = string.Format(Strings.GetString("core.deserialize.request.method.unsupported"),requestMethod);
						throw new JsonRpcSerializationException(JsonRpcErrorCode.MethodNotFound,exceptionMessage,requestMethod,msgId);
					}

					return new JsonRpcMessageInfo<JsonRpcRequest>(request);
				}

				if (IsResponse())
				{
					var responseSuccess = false;

					if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
					{
						if (protocolVersion != "2.0")
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.protocol.invalid_property"),msgId);

						//if both present or absent
						if (!(HasResult() ^ HasError()))
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.invalid_properties"),msgId);

						responseSuccess = HasResult();
					}
					else
					{
						if (!HasResult() || !HasError())
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.invalid_properties"),msgId);

						responseSuccess = rpcError == JsonRpcError.JsonNull;
					}

					if (responseSuccess)
					{
						var responseContract = _contractResolver.GetResponseContract(msgId) ??
							throw new JsonRpcSerializationException(JsonRpcErrorCode.MethodNotFound,Strings.GetString("core.deserialize.response.method.contract.undefined"),msgId);

						var responseResult = default(object);

						if (responseContract.ResultType != null)
						{
							try
							{
								responseResult = ToObject(rspResultToken,responseContract.ResultType);
							}
							catch (Exception e)
							{
								throw new InvalidJsonException(Strings.GetString("core.deserialize.json_issue"),e);
							}
						}

						return new JsonRpcMessageInfo<JsonRpcResponse>(new JsonRpcResponse(responseResult,msgId));
					}

					if (rspErrorSetAsNull)
					{
						if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.error.invalid_type"),msgId);

						return new JsonRpcMessageInfo<JsonRpcResponse>(new JsonRpcResponse(JsonRpcError.JsonNull,msgId));
					}

					if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
					{
						if (rspErrorCode == null)
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.error.code.invalid_property"),msgId);

						if (rspErrorMessage == null)
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.error.message.invalid_property"),msgId);
					}

					if (rspErrorDataToken != null)
					{
						Type rspErrorDataType;

						if (msgId.Type == JsonRpcIdType.None)
							rspErrorDataType = _contractResolver.GetResponseContract(default)?.ErrorDataType;
						else
						{
							var responseContract = _contractResolver.GetResponseContract(msgId) ??
								throw new JsonRpcSerializationException(JsonRpcErrorCode.MethodNotFound,Strings.GetString("core.deserialize.response.method.contract.undefined"),msgId);

							rspErrorDataType = responseContract.ErrorDataType;
						}

						var responseErrorData = default(object);

						if (rspErrorDataType != null)
						{
							try
							{
								responseErrorData = ToObject(rspErrorDataToken,rspErrorDataType);
							}
							catch (Exception e)
							{
								throw new InvalidJsonException(Strings.GetString("core.deserialize.json_issue"),e);
							}
						}

						JsonRpcError rspError;

						try
						{
							rspError = new JsonRpcError(rspErrorCode ?? 0L,rspErrorMessage ?? string.Empty,responseErrorData);
						}
						catch (ArgumentOutOfRangeException e)
						{
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.error.code.invalid_range"),msgId);
						}

						return new JsonRpcMessageInfo<JsonRpcResponse>(new JsonRpcResponse(rspError,msgId));
					}
					else
					{
						JsonRpcError rspError;

						try
						{
							rspError = new JsonRpcError(rspErrorCode ?? 0L,rspErrorMessage ?? string.Empty);
						}
						catch (ArgumentOutOfRangeException e)
						{
							throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.error.code.invalid_range"),msgId);
						}

						return new JsonRpcMessageInfo<JsonRpcResponse>(new JsonRpcResponse(rspError,msgId));
					}
				}

				throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.response.invalid_properties"),msgId);
			}
			catch (JsonRpcSerializationException e)
			{
				return new JsonRpcMessageInfo<JsonRpcMessage>(e);
			}
		}

		private  object ToObject(JToken token,Type objectType) =>		
			token.ToObject(objectType,_jsonSerializer);		

		private JsonRpcId FindMsgId(JsonTextReader reader,bool skipFirst)
		{
			try
			{
				if (skipFirst)
					reader.Skip();

				while (reader.Read())
				{
					if (reader.TokenType == JsonToken.PropertyName)
					{
						var jsonPropertyName = (string)reader.Value;
						reader.Read();

						if (jsonPropertyName == "id")
							return ReadMsgId(reader,false);
						else
							reader.Skip();
					}
					else if (reader.TokenType == JsonToken.EndObject)
						break;
					else
						reader.Skip();
				}
			}
			catch { }

			return default;
		}

		private JsonRpcId ReadMsgId(JsonTextReader reader,bool throwIfErr)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Null:
					break;
				case JsonToken.String:
					return new JsonRpcId((string)reader.Value);
				case JsonToken.Integer:
					return new JsonRpcId((long)reader.Value);
				case JsonToken.Float:
					return new JsonRpcId((double)reader.Value);
				default:
					if (throwIfErr)
						throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.deserialize.request.id.invalid_property"));
					break;
			}

			return default;
		}

		private (long?, string, JToken, bool) ReadResponseError(JsonTextReader reader,in JsonRpcId msgId)
		{
			long? errorCode = default;
			string errorMessage = default;
			JToken errorDataToken = default;
			bool errorSetAsNull = default;

			JsonRpcSerializationException CreateExceptionWithMsgId(long code,string name,in JsonRpcId id, bool skipFirst = true)
			{
				return new JsonRpcSerializationException(code,Strings.GetString(name),
					id.Type != JsonRpcIdType.None ? id : FindMsgId(reader,skipFirst));
			}

			switch (reader.TokenType)
			{
				case JsonToken.StartObject:
					while (reader.Read())
					{
						if (reader.TokenType == JsonToken.PropertyName)
						{
							var jsonPropertyName = (string)reader.Value;
							reader.Read();

							switch (jsonPropertyName)
							{
								case "code":
									if (reader.TokenType == JsonToken.Integer)
										errorCode = (long)reader.Value;
									else if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
										throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.response.error.code.invalid_property",msgId);
									else reader.Skip();
									break;
								case "message":
									if (reader.TokenType == JsonToken.String)
										errorMessage = (string)reader.Value;
									else if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
										throw CreateExceptionWithMsgId(JsonRpcErrorCode.InvalidRequest,"core.deserialize.response.error.message.invalid_property",msgId);
									else reader.Skip();
									break;
								case "data":
									errorDataToken = JToken.ReadFrom(reader,_jsonLoadSettings);
									break;
								default:
									reader.Skip();
									break;
							}
						}
						else if (reader.TokenType == JsonToken.EndObject)
							break;
						else
							reader.Skip();
					}
					break;
				case JsonToken.Null:
					errorSetAsNull = true;
					break;
				default:
					reader.Skip();
					break;
			}

			return (errorCode, errorMessage, errorDataToken, errorSetAsNull);
		}

		private JsonRpcRequest GetRpcRequest(string requestMethod,in JsonRpcId requestId,JContainer requestParamsetersToken,JsonRpcRequestContract requestContract)
		{
			switch (requestContract.ParametersType)
			{
				case JsonRpcParametersType.ByPosition:
					if (requestParamsetersToken is JObject requestParamsByName)
					{
						if (requestContract.RequiredParamCount == 1 && requestContract.ParametersByPosition.Count == 1)
						{
							var paramType = typeof(JsonElement);

							if (requestContract.ParametersByPosition[0] == paramType)
								return new JsonRpcRequest(requestMethod,requestId,new[] { ToObject(requestParamsByName,paramType) });
						}

						return null;
					}

					if (!(requestParamsetersToken is JArray requestParametersJArray))
						return null;

					if (requestParametersJArray.Count < requestContract.RequiredParamCount || requestParametersJArray.Count > requestContract.ParametersByPosition.Count)
						return null;

					var paramesByPos = new object[requestParametersJArray.Count];

					try
					{
						for (var i = 0; i < requestParametersJArray.Count; i++)
						{
							paramesByPos[i] = ToObject(requestParametersJArray[i],requestContract.ParametersByPosition[i]);
						}
					}
					catch (Exception e)
					{
						return null;
					}

					return new JsonRpcRequest(requestMethod,requestId,paramesByPos);
				case JsonRpcParametersType.ByName:
					if (!(requestParamsetersToken is JObject requestParametersJObject))
						return null;

					var paramsByName = new Dictionary<string,object>(requestContract.ParametersByName.Count,StringComparer.Ordinal);

					try
					{
						foreach (var kvp in requestContract.ParametersByName)
						{
							if (!requestParametersJObject.TryGetValue(kvp.Key,StringComparison.Ordinal,out var requestParameterToken))
								continue;

							paramsByName[kvp.Key] = requestParameterToken.ToObject(kvp.Value);
						}
					}
					catch (Exception e)
					{
						return null;
					}

					return new JsonRpcRequest(requestMethod,requestId,paramsByName);
				case JsonRpcParametersType.None:
					if (requestParamsetersToken == null || !requestParamsetersToken.HasValues)
						return new JsonRpcRequest(requestMethod,requestId);
					break;
			}

			return null;
		}

		private void SerializeRequest(JsonTextWriter writer,JsonRpcRequest request)
		{
			writer.WriteStartObject();

			if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
			{
				writer.WritePropertyName("jsonrpc");
				writer.WriteValue("2.0");
			}

			WriteMsgId(writer,request.Id);

			writer.WritePropertyName("method");
			writer.WriteValue(request.Method);

			if (request.HasParamsByPosition)
			{
				if (request.ParametersByPosition.Count > 0)
				{
					writer.WritePropertyName("params");
					writer.WriteStartArray();

					try
					{
						for (var i = 0; i < request.ParametersByPosition.Count; i++)
						{
							_jsonSerializer.Serialize(writer,request.ParametersByPosition[i]);
						}
					}
					catch (Exception e)
					{
						throw new InvalidJsonException(Strings.GetString("core.serialize.json_issue"),e);
					}

					writer.WriteEndArray();
				}
				else if (_compatibilityLevel == JsonRpcCompatibilityLevel.Level1)
				{
					writer.WritePropertyName("params");
					writer.WriteStartArray();
					writer.WriteEndArray();
				}
			}
			else if (request.HasParamsByName)
			{
				if (_compatibilityLevel == JsonRpcCompatibilityLevel.Level1)
					throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.serialize.request.params.unsupported_structure"),request.Id);

				writer.WritePropertyName("params");
				writer.WriteStartObject();

				try
				{
					foreach (var kvp in request.ParametersByName)
					{
						writer.WritePropertyName(kvp.Key);
						_jsonSerializer.Serialize(writer,kvp.Value);
					}
				}
				catch (Exception e)
				{
					throw new InvalidJsonException(Strings.GetString("core.serialize.json_issue"),e);
				}

				writer.WriteEndObject();
			}
			else if (_compatibilityLevel == JsonRpcCompatibilityLevel.Level1)
			{
				writer.WritePropertyName("params");
				writer.WriteStartArray();
				writer.WriteEndArray();
			}

			writer.WriteEndObject();
		}

		private void WriteMsgId(JsonTextWriter writer,in JsonRpcId id)
		{
			if (!id.HasValue && _compatibilityLevel == JsonRpcCompatibilityLevel.Level2)
				return;

			writer.WritePropertyName("id");

			switch (id.Type)
			{
				case JsonRpcIdType.String:
					writer.WriteValue(id.UnsafeAsString());
					break;
				case JsonRpcIdType.Integer:
					writer.WriteValue(id.UnsafeAsInteger());
					break;
				case JsonRpcIdType.Float:
					writer.WriteValue(id.UnsafeAsFloat());
					break;
				default:
					writer.WriteNull();
					break;
			}
		}

		private void SerializeResponse(JsonTextWriter writer,JsonRpcResponse response)
		{
			writer.WriteStartObject();

			if (_compatibilityLevel != JsonRpcCompatibilityLevel.Level1)
			{
				writer.WritePropertyName("jsonrpc");
				writer.WriteValue("2.0");
			}

			WriteMsgId(writer,response.Id);

			if (response.Success)
			{
				writer.WritePropertyName("result");

				try
				{
					_jsonSerializer.Serialize(writer,response.Result);
				}
				catch (Exception e)
				{
					throw new InvalidJsonException(Strings.GetString("core.serialize.json_issue"),e);
				}

				if (_compatibilityLevel == JsonRpcCompatibilityLevel.Level1)
				{
					writer.WritePropertyName("error");
					writer.WriteNull();
				}
			}
			else
			{
				if (_compatibilityLevel == JsonRpcCompatibilityLevel.Level1)
				{
					writer.WritePropertyName("result");
					writer.WriteNull();
				}

				writer.WritePropertyName("error");
				writer.WriteStartObject();
				writer.WritePropertyName("code");
				writer.WriteValue(response.Error.Code);
				writer.WritePropertyName("message");
				writer.WriteValue(response.Error.Message);

				if (response.Error.HasData)
				{
					writer.WritePropertyName("data");

					try
					{
						_jsonSerializer.Serialize(writer,response.Error.Data);
					}
					catch (Exception e)
					{
						throw new InvalidJsonException(Strings.GetString("core.serialize.json_issue"),e);
					}
				}

				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}

		private void SerializeRequests(JsonTextWriter writer,IReadOnlyList<JsonRpcRequest> requests,CancellationToken cancellationToken = default)
		{
			if (requests.Count == 0)
			{
				throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.batch.empty"));
			}

			writer.WriteStartArray();

			for (var i = 0; i < requests.Count; i++)
			{
				if (requests[i] == null)
				{
					throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,string.Format(Strings.GetString("core.batch.invalid_item"),i));
				}

				cancellationToken.ThrowIfCancellationRequested();

				SerializeRequest(writer,requests[i]);
			}

			writer.WriteEndArray();
		}

		private void SerializeResponses(JsonTextWriter writer,IReadOnlyList<JsonRpcResponse> responses,CancellationToken cancellationToken = default)
		{
			if (responses.Count == 0)
			{
				throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,Strings.GetString("core.batch.empty"));
			}

			writer.WriteStartArray();

			for (var i = 0; i < responses.Count; i++)
			{
				if (responses[i] == null)
				{
					throw new JsonRpcSerializationException(JsonRpcErrorCode.InvalidRequest,string.Format(Strings.GetString("core.batch.invalid_item"),i));
				}

				cancellationToken.ThrowIfCancellationRequested();

				SerializeResponse(writer,responses[i]);
			}

			writer.WriteEndArray();
		}

	}
}
