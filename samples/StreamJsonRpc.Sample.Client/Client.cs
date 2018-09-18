using System;
using System.Data.JsonRpc;
using System.IO.Pipes;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace StreamJsonRpc.Sample.Client
{
    class Client
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            Console.WriteLine("Connecting to server...");
            using (var stream = new NamedPipeClientStream(".", "StreamJsonRpcSamplePipe", PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                await stream.ConnectAsync();
                Console.WriteLine("Connected. Sending request...");

				//var clientRpc = new JsonRpc(streams.Item1,streams.Item1);
				//var client1 = clientRpc..Attach<IServer>();

				var jsonRpc = JsonRpc.Attach(stream,cr => new JsonRpcSerializer(cr));
				var calc = jsonRpc.Attach<ICalculator>();
				var sum2 = await calc.Add(2,4);
				Console.WriteLine($"2 + 4 = {sum2}");

				int sum = await jsonRpc.InvokeAsync<int>("Add", 3, 5);
                Console.WriteLine($"3 + 5 = {sum}");
                Console.WriteLine("Terminating stream...");
            }
        }
    }

	public interface ICalculator
	{
		Task<int> Add(int a,int b);		
	}
}
