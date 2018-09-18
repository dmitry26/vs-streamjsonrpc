using System;
using System.Collections.Generic;
using System.Data.JsonRpc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StreamJsonRpc.Sample.Web.Controllers
{	
	public class HomeController : Controller
    {		
		public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Socket()
        {
            if (this.HttpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await this.HttpContext.WebSockets.AcceptWebSocketAsync();
				//var jsonRpc = new JsonRpc(new WebSocketMessageHandler(socket), new JsonRpcServer());
				var jsonRpc = new JsonRpc(new WebSocketMessageHandler(socket),cr => new JsonRpcSerializer(cr), new JsonRpcServer());
				jsonRpc.StartListening();
				
				try
				{
					await jsonRpc.Completion;
				}
				catch (Exception x)
				{
				}

                return new EmptyResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }	
}
