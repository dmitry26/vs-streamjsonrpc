using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Data.JsonRpc
{	
	public class JsonValue : JsonElement, IEquatable<JsonValue>
	{
		private JsonValue()
		{
		}

		internal JsonValue(JsonValue other)
		{
			Value = other?.Value;
		}

		internal JsonValue(object value)
		{
			Value = IsJsonValue(value)
				? value
				: throw new ArgumentException($"It is not a valid json primitive type: {value.GetType()}");
		}

		public static JsonValue NullValue { get; } = new JsonValue();

		public override JsonElementType JsonType => JsonElementType.Value;

		private static bool IsJsonValue(object value)
		{
			if (value is null)
				return true;

			switch (value)
			{
				case IConvertible _:
					return true;				
				case byte[] _:
					return true;				
				default:
					break;
			}

			return false;
		}

		public object Value { get; }

		public bool HasValue => !(Value is null);

		public override string ToString() => Value?.ToString();

		public bool Equals(JsonValue other) =>
			Equals(Value,other.Value);

		public override bool Equals(object obj) =>
			(obj is JsonValue cv) && Equals(Value,cv.Value);

		public override int GetHashCode() => HasValue ? Value.GetHashCode() : 0;

		private static bool IsNull(JsonValue obj) => obj is null || obj.Value is null;

		public static bool operator ==(JsonValue obj1,JsonValue obj2) =>
			obj1 is null ? obj2 is null : obj1.Equals(obj2);

		public static bool operator !=(JsonValue obj1,JsonValue obj2) =>
			obj1 is null ? !(obj2 is null) : !obj1.Equals(obj2);

		public static implicit operator bool?(JsonValue obj) =>
			IsNull(obj) ? default(bool?) 
				: ((IConvertible)obj.Value).ToBoolean(CultureInfo.CurrentCulture);

		public static implicit operator bool(JsonValue obj) =>
			IsNull(obj) ? default
				: ((IConvertible)obj.Value).ToBoolean(CultureInfo.CurrentCulture);		

		public static implicit operator int?(JsonValue obj) =>
			IsNull(obj) ? default(int?)
			: Convert.ToInt32(obj.Value,CultureInfo.CurrentCulture);

		public static implicit operator int(JsonValue obj) =>
			 Convert.ToInt32(obj?.Value,CultureInfo.CurrentCulture);

		public static implicit operator long?(JsonValue obj) =>
			IsNull(obj) ? default(long?)
			: ((IConvertible)obj.Value).ToInt64(CultureInfo.CurrentCulture);

		public static implicit operator long(JsonValue obj) =>
			IsNull(obj) ? default
			: ((IConvertible)obj.Value).ToInt64(CultureInfo.CurrentCulture);

		public static implicit operator float?(JsonValue obj) =>
			IsNull(obj) ? default(float?)
			: ((IConvertible)obj.Value).ToSingle(CultureInfo.CurrentCulture);

		public static implicit operator float(JsonValue obj) =>
			IsNull(obj) ? default
			: ((IConvertible)obj.Value).ToSingle(CultureInfo.CurrentCulture);

		public static implicit operator decimal?(JsonValue obj) =>
			IsNull(obj) ? default(decimal?)
			: ((IConvertible)obj.Value).ToDecimal(CultureInfo.CurrentCulture);

		public static implicit operator decimal(JsonValue obj) =>
			IsNull(obj) ? default
			: ((IConvertible)obj.Value).ToDecimal(CultureInfo.CurrentCulture);

		public static implicit operator double?(JsonValue obj) =>
			IsNull(obj) ? default(double?)
			: ((IConvertible)obj.Value).ToDouble(CultureInfo.CurrentCulture);

		public static implicit operator double(JsonValue obj) =>
			IsNull(obj) ? default
			: ((IConvertible)obj.Value).ToDouble(CultureInfo.CurrentCulture);

		public static implicit operator string(JsonValue obj) =>
			IsNull(obj) ? default
			: (obj.Value is byte[] bytes
				? Convert.ToBase64String(bytes)
				: Convert.ToString(obj.Value,CultureInfo.InvariantCulture));

		public static implicit operator DateTime?(JsonValue obj) =>
			IsNull(obj) ? default(DateTime?) 
			: ((IConvertible)obj.Value).ToDateTime(CultureInfo.InvariantCulture);

		public static implicit operator DateTime(JsonValue obj) =>
			IsNull(obj) ? default
			: (obj.Value is DateTimeOffset offset ? offset.DateTime
				: ((IConvertible)obj.Value).ToDateTime(CultureInfo.InvariantCulture));

		public static implicit operator DateTimeOffset?(JsonValue obj) =>
			IsNull(obj) ? default(DateTimeOffset?)
			: (obj.Value is DateTimeOffset dto) ? dto
				: obj.Value is string s ? DateTimeOffset.Parse(s,CultureInfo.InvariantCulture)
					: new DateTimeOffset(Convert.ToDateTime(obj.Value,CultureInfo.InvariantCulture));

		public static implicit operator DateTimeOffset(JsonValue obj) =>
			IsNull(obj) ? default
			: (obj.Value is DateTimeOffset dto) ? dto 
				: obj.Value is string s ? DateTimeOffset.Parse(s,CultureInfo.InvariantCulture)
					: new DateTimeOffset(Convert.ToDateTime(obj.Value,CultureInfo.InvariantCulture));

		public static implicit operator TimeSpan?(JsonValue obj) =>
			IsNull(obj) ? default(TimeSpan?)
			: ((obj.Value is TimeSpan ts) ? ts : TimeSpan.Parse(Convert.ToString(obj.Value,CultureInfo.InvariantCulture)));

		public static implicit operator TimeSpan(JsonValue obj) =>
			IsNull(obj) ? default
			: ((obj.Value is TimeSpan ts) ? ts : TimeSpan.Parse(Convert.ToString(obj.Value,CultureInfo.InvariantCulture)));

		public static implicit operator Guid?(JsonValue obj) =>
			IsNull(obj) ? default(Guid?)
			: ((obj.Value is Guid guid) ? guid : Guid.Parse(Convert.ToString(obj.Value,CultureInfo.InvariantCulture)));

		public static implicit operator Guid(JsonValue obj) =>
			IsNull(obj) ? default
			: ((obj.Value is Guid guid) ? guid : Guid.Parse(Convert.ToString(obj.Value,CultureInfo.InvariantCulture)));

		public static implicit operator Uri(JsonValue obj) =>
			IsNull(obj) ? default
			: ((obj.Value is Uri uri) ? uri : new Uri(Convert.ToString(obj.Value,CultureInfo.InvariantCulture)));

		public static implicit operator byte[](JsonValue obj) =>
			IsNull(obj) ? default
			: (obj.Value is byte[] bytes) ? bytes
				: obj.Value is string s ? Convert.FromBase64String(Convert.ToString(obj.Value,CultureInfo.InvariantCulture))
					: throw new ArgumentException(FormattableString.Invariant($"Can not convert {obj.Value.GetType()} to byte array."));
	}
}
