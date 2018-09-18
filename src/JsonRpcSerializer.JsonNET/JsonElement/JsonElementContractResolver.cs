using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Data.JsonRpc
{
	/// <summary>
	/// 
	/// </summary>
	public class JsonElementContractResolver : DefaultContractResolver
	{
		private static readonly JsonConverter _converter = new JsonElementConverter();
		private static Type _type = typeof(JsonElement);

		protected override JsonConverter ResolveContractConverter(Type objectType) =>
			objectType == null || !_type.IsAssignableFrom(objectType)
			? base.ResolveContractConverter(objectType)
			: _converter;
	}
}
