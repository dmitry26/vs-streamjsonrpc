using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Data.JsonRpc
{
	internal static class JsonElementWriter
	{
		public static void WriteElement(JsonWriter writer,JsonElement elm,JsonSerializer serializer)
		{
			switch (elm.JsonType)
			{
				case JsonElementType.Array:
					writer.WriteStartArray();
					foreach (var item in (IReadOnlyList<JsonElement>)elm)
					{
						WriteElement(writer,item,serializer);
					}
					writer.WriteEndArray();
					break;
				case JsonElementType.Object:
					writer.WriteStartObject();
					foreach (var kvp in (IReadOnlyDictionary<string,JsonElement>)elm)
					{
						writer.WritePropertyName(kvp.Key);
						WriteElement(writer,kvp.Value,serializer);
					}
					writer.WriteEndObject();
					break;
				case JsonElementType.Value:
					WriteValue(writer,(JsonValue)elm,serializer);
					break;
			}
		}

		private static void WriteValue(JsonWriter writer,JsonValue val,JsonSerializer serializer)
		{
			if (val is null || !val.HasValue)
				writer.WriteNull();
			else
				serializer.Serialize(writer,val.Value);
		}
	}
}
