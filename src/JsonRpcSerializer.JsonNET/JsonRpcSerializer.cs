// © Alexander Kozlenko. Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace System.Data.JsonRpc
{
	/// <summary>Serializes and deserializes JSON-RPC messages into and from the JSON format.</summary>
	public sealed partial class JsonRpcSerializer : IJsonRpcSerializer
	{
		private const int _messageBufferSize = 64;
		private const int _streamBufferSize = 1024;

		private static readonly Encoding _streamEncoding = new UTF8Encoding(false);
		private static readonly IArrayPool<char> _jsonBufferPool = new JsonBufferPool();

		/// <summary>Initializes a new instance of the <see cref="JsonRpcSerializer" /> class.</summary>
		/// <param name="contractResolver">The JSON-RPC message contract resolver instance.</param>
		/// <param name="jsonSerializer">The JSON serializer instance.</param>
		/// <param name="compatibilityLevel">The JSON-RPC protocol compatibility level.</param>
		public JsonRpcSerializer(IJsonRpcContractResolver contractResolver = null,JsonSerializer jsonSerializer = null,JsonRpcCompatibilityLevel compatibilityLevel = default)
		{
			_contractResolver = contractResolver;
			_jsonSerializer = jsonSerializer ?? JsonSerializer.CreateDefault(_jsonSerializerSettings);
			_compatibilityLevel = (compatibilityLevel == JsonRpcCompatibilityLevel.Default)
				? JsonRpcCompatibilityLevel.Level2 : compatibilityLevel;
		}

		/// <summary>
		/// 
		/// </summary>
		public JsonSerializer JsonSerializer
		{
			get => _jsonSerializer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public IJsonRpcInfo<JsonRpcMessage> DeserializeMessages(string json,CancellationToken cancelToken = default)
		{
			if (json == null)			
				throw new ArgumentNullException(nameof(json));			

			using (var stringReader = new StringReader(json))
			{
				using (var jsonReader = new JsonTextReader(stringReader))
				{
					jsonReader.ArrayPool = _jsonBufferPool;					
					return DeserializeMessages(jsonReader,cancelToken);
				}
			}
		}		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public string SerializeMessage(JsonRpcMessage msg)
		{
			if (msg == null) throw new ArgumentNullException(nameof(msg));

			using (var stringWriter = new StringWriter(new StringBuilder(_messageBufferSize),CultureInfo.InvariantCulture))
			{
				using (var jsonWriter = new JsonTextWriter(stringWriter))
				{
					jsonWriter.AutoCompleteOnClose = false;
					jsonWriter.ArrayPool = _jsonBufferPool;
					jsonWriter.Formatting = _jsonSerializer.Formatting;

					if (msg is JsonRpcRequest req)
						SerializeRequest(jsonWriter,req);
					else if (msg is JsonRpcResponse rsp)
						SerializeResponse(jsonWriter,rsp);
					else throw new InvalidOperationException("Invalid JsonRpcMessage");
				}

				return stringWriter.ToString();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="messages"></param>
		/// <param name="cancelToken"></param>
		/// <returns></returns>
		public string SerializeMessages(IReadOnlyList<JsonRpcMessage> messages,CancellationToken cancelToken = default)
		{
			if (messages == null) throw new ArgumentNullException(nameof(messages));

			if (messages is IReadOnlyList<JsonRpcRequest> reqs)
				return SerializeRequests(reqs,cancelToken);

			if (messages is IReadOnlyList<JsonRpcResponse> rsps)
				return SerializeResponses(rsps,cancelToken);

			var msg = messages.FirstOrDefault();

			if (msg is JsonRpcRequest req)
				return SerializeRequests(messages.Cast<JsonRpcRequest>().ToArray(),cancelToken);

			return SerializeResponses(messages.Cast<JsonRpcResponse>().ToArray(),cancelToken);
		}

		/// <summary>Serializes the specified collection of JSON-RPC requests to a JSON string.</summary>
		/// <param name="requests">The collection of JSON-RPC requests to serialize.</param>
		/// <returns>A JSON string representation of the specified collection of JSON-RPC requests.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="requests" /> is <see langword="null" />.</exception>
		/// <exception cref="JsonException">An error occurred during JSON serialization.</exception>
		/// <exception cref="JsonRpcException">An error occurred during JSON-RPC serialization.</exception>
		public string SerializeRequests(IReadOnlyList<JsonRpcRequest> requests,CancellationToken cancelToken = default)
		{
			if (requests == null) throw new ArgumentNullException(nameof(requests));			

			using (var stringWriter = new StringWriter(new StringBuilder(_messageBufferSize * requests.Count),CultureInfo.InvariantCulture))
			{
				using (var jsonWriter = new JsonTextWriter(stringWriter))
				{
					jsonWriter.AutoCompleteOnClose = false;
					jsonWriter.ArrayPool = _jsonBufferPool;
					jsonWriter.Formatting = _jsonSerializer.Formatting;

					SerializeRequests(jsonWriter,requests,cancelToken);
				}

				return stringWriter.ToString();
			}
		}

		/// <summary>Serializes the specified collection of JSON-RPC responses to a JSON string.</summary>
		/// <param name="responses">The collection of JSON-RPC responses to serialize.</param>
		/// <returns>A JSON string representation of the specified collection of JSON-RPC responses.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="responses" /> is <see langword="null" />.</exception>
		/// <exception cref="JsonException">An error occurred during JSON serialization.</exception>
		/// <exception cref="JsonRpcException">An error occurred during JSON-RPC serialization.</exception>
		public string SerializeResponses(IReadOnlyList<JsonRpcResponse> responses,CancellationToken cancelToken = default)
		{
			if (responses == null) throw new ArgumentNullException(nameof(responses));			

			using (var stringWriter = new StringWriter(new StringBuilder(_messageBufferSize * responses.Count),CultureInfo.InvariantCulture))
			{
				using (var jsonWriter = new JsonTextWriter(stringWriter))
				{
					jsonWriter.AutoCompleteOnClose = false;
					jsonWriter.ArrayPool = _jsonBufferPool;
					jsonWriter.Formatting = _jsonSerializer.Formatting;
					
					SerializeResponses(jsonWriter,responses,cancelToken);
				}

				return stringWriter.ToString();
			}
		}
	}
}