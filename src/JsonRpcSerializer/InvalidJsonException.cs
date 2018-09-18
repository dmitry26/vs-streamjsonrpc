using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.JsonRpc
{
	/// <summary>
	/// 
	/// </summary>
	public class InvalidJsonException : Exception
	{
		/// <summary>
		/// 
		/// </summary>
		public InvalidJsonException()
			: base()
		{
		}

		/// <summary>Initializes a new instance of the <see cref="JsonRpcException" /> class.</summary>
		/// <param name="message">The message that describes the error.</param>
		public InvalidJsonException(string message)
			: base(message)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="JsonRpcException" /> class.</summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public InvalidJsonException(string message,Exception innerException)
			: base(message,innerException)
		{
		}
	}
}
