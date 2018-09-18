using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace System.Data.JsonRpc
{
	internal static class JsonElementReader
	{
		public static object ReadFrom(JsonReader reader) =>
			ReadValue(reader);

		private static JsonElement ReadValue(JsonReader reader)
		{
			if (!reader.MoveToContent())
				throw reader.CreateException("Unexpected end when reading JsonElement.");

			switch (reader.TokenType)
			{
				case JsonToken.StartObject:
					return ReadObject(reader);
				case JsonToken.StartArray:
					return ReadList(reader);
				default:
					if (reader.IsPrimitiveToken())
						return new JsonValue(reader.Value);
					break;
			}

			throw reader.CreateException(FormattableString.Invariant($"Unexpected token when converting to ExpandoObject: {reader.TokenType}"));
		}

		private static JsonElement ReadList(JsonReader reader)
		{
			var list = new List<JsonElement>();

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.Comment:
						break;
					default:
						list.Add(ReadValue(reader));
						break;
					case JsonToken.EndArray:
						return new JsonArray(list);
				}
			}

			throw reader.CreateException("Unexpected end when reading JsonElement.");
		}

		private static JsonElement ReadObject(JsonReader reader)
		{
			var dict = new Dictionary<string,JsonElement>();

			while (reader.Read())
			{
				switch (reader.TokenType)
				{
					case JsonToken.PropertyName:
						string propertyName = reader.Value.ToString();
						if (!reader.Read())
							throw reader.CreateException("Unexpected end when reading JsonElement.");
						dict[propertyName] =  ReadValue(reader);
						break;
					case JsonToken.Comment:
						break;
					case JsonToken.EndObject:
						return new JsonObject(dict);
				}
			}

			throw reader.CreateException("Unexpected end when reading JsonElement.");
		}		
	}	
}
