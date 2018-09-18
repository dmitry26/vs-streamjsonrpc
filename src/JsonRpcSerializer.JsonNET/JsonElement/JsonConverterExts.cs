using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace System.Data.JsonRpc
{
	internal static class JsonConverterExts
	{
		public static bool MoveToContent(this JsonReader reader)
		{
			var t = reader.TokenType;

			while (t == JsonToken.None || t == JsonToken.Comment)
			{
				if (!reader.Read())
					return false;

				t = reader.TokenType;
			}

			return true;
		}

		public static bool IsPrimitiveToken(this JsonReader reader)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Undefined:
				case JsonToken.Null:
				case JsonToken.Date:
				case JsonToken.Bytes:
					return true;
				default:
					return false;
			}
		}

		internal static JsonSerializationException CreateException(this JsonReader reader,string message) =>
			new JsonSerializationException(FormatMessage(reader as IJsonLineInfo,reader?.Path,message));

		internal static JsonSerializationException CreateException(this JsonWriter writer,string message) =>
			new JsonSerializationException(FormatMessage(null,writer?.Path,message));

		internal static string FormatMessage(IJsonLineInfo lineInfo,string path,string message)
		{
			var sb = new StringBuilder(256);

			// don't add a fullstop and space when message ends with a new line
			if (!message.EndsWith(Environment.NewLine,StringComparison.Ordinal))
			{
				sb.Append(message.Trim());

				if (message.Length > 0)
				{
					if (message[message.Length - 1] != '.')
						sb.Append('.');

					sb.Append(' ');
				}
			}
			else sb.Append(message);

			sb.Append(FormattableString.Invariant($"Path '{path}'"));

			if (lineInfo != null && lineInfo.HasLineInfo())
				sb.Append(FormattableString.Invariant($", line {lineInfo.LineNumber}, position {lineInfo.LinePosition}"));

			sb.Append('.');

			return sb.ToString();
		}
	}
}
