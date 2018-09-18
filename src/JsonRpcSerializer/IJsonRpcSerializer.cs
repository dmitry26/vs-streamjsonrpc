using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.JsonRpc
{
	public interface IJsonRpcSerializer
	{
		IJsonRpcInfo<JsonRpcMessage> DeserializeMessages(string json,CancellationToken cancelToken = default);		
		string SerializeMessage(JsonRpcMessage msg);
		string SerializeMessages(IReadOnlyList<JsonRpcMessage> messages,CancellationToken cancelToken = default);
		string SerializeRequests(IReadOnlyList<JsonRpcRequest> requests,CancellationToken cancelToken = default);
		string SerializeResponses(IReadOnlyList<JsonRpcResponse> responses,CancellationToken cancelToken = default);
	}
}