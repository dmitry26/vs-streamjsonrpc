using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.JsonRpc
{
	public abstract class JsonElement : DynamicObject
	{
		public abstract JsonElementType JsonType { get; }

		internal static T ChangeType<T>(object obj,IFormatProvider provider = null) =>
			(T)ChangeType(obj,typeof(T),provider);

		internal static object ChangeType(JsonElement obj,Type newType,IFormatProvider provider = null)
		{
			_ = newType ?? throw new ArgumentNullException(nameof(newType));

			var val = obj is null || obj.JsonType != JsonElementType.Value ? obj
				: ((JsonValue)obj).Value;

			return ChangeTypeCore(val,newType,provider);
		}

		internal static object ChangeType(object obj,Type newType,IFormatProvider provider = null)
		{
			_ = newType ?? throw new ArgumentNullException(nameof(newType));

			return ChangeTypeCore(obj is JsonValue val ? val.Value : obj,newType,provider);
		}

		private static object ChangeTypeCore(object val,Type newType,IFormatProvider provider)
		{
			if (val is null)
			{
				return newType.IsValueType && !newType.IsNullable() ? throw new InvalidCastException("Cannot cast null to ValueType")
					: (object)null;
			}

			var newUnderType = Nullable.GetUnderlyingType(newType);
			if (!(newUnderType is null)) newType = newUnderType;

			if (provider is null) provider = CultureInfo.InvariantCulture;

			if (val is IConvertible)
				return Convert.ChangeType(val,newType,provider);

			if (newType == typeof(Uri))
				return val is Uri uri ? uri : new Uri(Convert.ToString(val,provider));

			if (newType == typeof(TimeSpan) || newType == typeof(TimeSpan?))
				return val is TimeSpan ts ? ts : TimeSpan.Parse(Convert.ToString(val,provider));

			if (newType == typeof(Guid) || newType == typeof(Guid?))
				return val is Guid guid ? guid : Guid.Parse(Convert.ToString(val,provider));

			if (newType == typeof(byte[]))
				Convert.FromBase64String(Convert.ToString(val,provider));

			if (newType == typeof(object))
				return val;

			throw new InvalidCastException(FormattableString.Invariant($"Can not convert val, {val.GetType()} to {newType}"));
		}

		public static implicit operator bool? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator bool(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator int? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator int(JsonElement obj) =>
			 (JsonValue)obj;

		public static implicit operator long? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator long(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator float? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator float(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator decimal? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator decimal(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator double? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator double(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator string(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator DateTime? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator DateTime(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator TimeSpan? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator TimeSpan(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator Guid? (JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator Guid(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator Uri(JsonElement obj) =>
			(JsonValue)obj;

		public static implicit operator byte[] (JsonElement obj) =>
			(JsonValue)obj;
	}

	public enum JsonElementType
	{
		Value = 0,  // Primitive: String, Number (long, double), Boolean, Null
		Object,		// Structure 
		Array,		// Structure 		
	}
}
