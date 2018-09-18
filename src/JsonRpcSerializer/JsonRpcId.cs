// © Alexander Kozlenko. Licensed under the MIT License.

using System.Data.JsonRpc.Resources;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Data.JsonRpc
{
	/// <summary>Represents a JSON-RPC message identifier.</summary>
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct JsonRpcId : IEquatable<JsonRpcId>, IComparable<JsonRpcId>
	{		
		[FieldOffset(0)]
		private readonly JsonRpcIdType _type;

		[FieldOffset(8)]
		private readonly long _intVal;

		[FieldOffset(8)]
		private readonly double _floatVal;

		[FieldOffset(16)]
		private readonly string _strVal;

		/// <summary>Initializes a new instance of the <see cref="JsonRpcId" /> structure.</summary>
		/// <param name="value">The identifier value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
		public JsonRpcId(string value) : this()
		{
			_type = JsonRpcIdType.String;
			_strVal = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>Initializes a new instance of the <see cref="JsonRpcId" /> structure.</summary>
		/// <param name="value">The identifier value.</param>
		public JsonRpcId(long value) : this()
		{
			_type = JsonRpcIdType.Integer;
			_intVal = value;
		}		

		/// <summary>Initializes a new instance of the <see cref="JsonRpcId" /> structure.</summary>
		/// <param name="value">The identifier value.</param>
		/// <exception cref="ArgumentException"><paramref name="value" /> is <see cref="double.NaN" />, or <see cref="double.NegativeInfinity" />, or <see cref="double.PositiveInfinity" />.</exception>
		public JsonRpcId(double value) : this()
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				throw new ArgumentException(Strings.GetString("id.invalid_float"),nameof(value));

			_type = JsonRpcIdType.Float;
			_floatVal = value;
		}

		/// <summary>Gets the JSON-RPC message identifier type.</summary>
		public JsonRpcIdType Type => _type;

		bool IEquatable<JsonRpcId>.Equals(JsonRpcId other) => Equals(other);

		int IComparable<JsonRpcId>.CompareTo(JsonRpcId other) => CompareTo(other);

		internal string UnsafeAsString() => _strVal;

		internal long UnsafeAsInteger() => _intVal;

		internal double UnsafeAsFloat() => _floatVal;

		/// <summary>Compares the current <see cref="JsonRpcId" /> with another <see cref="JsonRpcId" /> and returns an integer that indicates whether the current <see cref="JsonRpcId" /> precedes, follows, or occurs in the same position in the sort order as the other <see cref="JsonRpcId" />.</summary>
		/// <param name="other">A <see cref="JsonRpcId" /> to compare with the current <see cref="JsonRpcId" />.</param>
		/// <returns>A value that indicates the relative order of the objects being compared.</returns>
		[CLSCompliant(false)]
		public int CompareTo(in JsonRpcId other)
		{
			var result = ((int)_type).CompareTo((int)other._type);

			if (result != 0) return result;

			switch (_type)
            {
                case JsonRpcIdType.String:
                    return string.CompareOrdinal(_strVal,other._strVal);
                case JsonRpcIdType.Integer:
                    return _intVal.CompareTo(other._intVal);
                case JsonRpcIdType.Float:
                    return _floatVal.CompareTo(other._floatVal);
            }

            return result;
		}

		/// <summary>Indicates whether the current <see cref="JsonRpcId" /> is equal to another <see cref="JsonRpcId" />.</summary>
		/// <param name="other">A <see cref="JsonRpcId" /> to compare with the current <see cref="JsonRpcId" />.</param>
		/// <returns><see langword="true" /> if the current <see cref="JsonRpcId" /> is equal to the other <see cref="JsonRpcId" />; otherwise, <see langword="false" />.</returns>
		[CLSCompliant(false)]
		public bool Equals(in JsonRpcId other)
		{
			if (_type != other._type)
				return false;

			switch (_type)
			{
				case JsonRpcIdType.String:
					return _strVal.Equals(other._strVal);
				case JsonRpcIdType.Integer:
					return _intVal.Equals(other._intVal);
				case JsonRpcIdType.Float:
					return _floatVal.Equals(other._floatVal);
				default:
					return true;
			}
		}

		/// <summary>Indicates whether the current <see cref="JsonRpcId" /> is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with the current <see cref="JsonRpcId" />.</param>
		/// <returns><see langword="true" /> if the current <see cref="JsonRpcId" /> is equal to the specified object; otherwise, <see langword="false" />.</returns>
		public override bool Equals(object obj)
		{
			switch (obj)
			{
				case JsonRpcId other:
					return Equals(other);
				case string otherVal:
					return _type == JsonRpcIdType.String && _strVal.Equals(otherVal);
				case long otherVal:
					return _type == JsonRpcIdType.Integer && _intVal.Equals(otherVal);
				case double otherVal:
					return _type == JsonRpcIdType.Float && _floatVal.Equals(otherVal);
				default:
					return false;
			}
		}

		/// <summary>Returns the hash code for the current <see cref="JsonRpcId" />.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			switch (Type)
			{
				case JsonRpcIdType.String:
					return _strVal.GetHashCode();
				case JsonRpcIdType.Integer:
					return _intVal.GetHashCode();
				case JsonRpcIdType.Float:
					return _floatVal.GetHashCode();
				default:
					return 0;
			}
		}

		/// <summary>Converts the current <see cref="JsonRpcId" /> to its equivalent string representation.</summary>
		/// <returns>The string representation of the current <see cref="JsonRpcId" />.</returns>
		public override string ToString()
		{
			switch (Type)
			{
				case JsonRpcIdType.String:
					return _strVal;
				case JsonRpcIdType.Integer:
					return _intVal.ToString(CultureInfo.InvariantCulture);
				case JsonRpcIdType.Float:
					// Writes at least 1 precision digit out of 16 possible
					return _floatVal.ToString("0.0###############",CultureInfo.InvariantCulture);
				default:
					return string.Empty;
			}
		}

		/// <summary>Indicates whether the left <see cref="JsonRpcId" /> is equal to the right <see cref="JsonRpcId" />.</summary>
		/// <param name="obj1">The left <see cref="JsonRpcId" /> operand.</param>
		/// <param name="obj2">The right <see cref="JsonRpcId" /> operand.</param>
		/// <returns><see langword="true" /> if the left <see cref="JsonRpcId" /> is equal to the right <see cref="JsonRpcId" />; otherwise, <see langword="false" />.</returns>
		public static bool operator ==(in JsonRpcId obj1,in JsonRpcId obj2) => obj1.Equals(obj2);

		/// <summary>Indicates whether the left <see cref="JsonRpcId" /> is not equal to the right <see cref="JsonRpcId" />.</summary>
		/// <param name="obj1">The left <see cref="JsonRpcId" /> operand.</param>
		/// <param name="obj2">The right <see cref="JsonRpcId" /> operand.</param>
		/// <returns><see langword="true" /> if the left <see cref="JsonRpcId" /> is not equal to the right <see cref="JsonRpcId" />; otherwise, <see langword="false" />.</returns>
		public static bool operator !=(in JsonRpcId obj1,in JsonRpcId obj2) => !obj1.Equals(obj2);

		/// <summary>Performs an implicit conversion from <see cref="string" /> to <see cref="JsonRpcId" />.</summary>
		/// <param name="value">The value to create a <see cref="JsonRpcId" /> from.</param>
		public static implicit operator JsonRpcId(string value) => new JsonRpcId(value);

		/// <summary>Performs an implicit conversion from <see cref="ulong" /> to <see cref="JsonRpcId" />.</summary>
		/// <param name="value">The value to create a <see cref="JsonRpcId" /> from.</param>
		public static implicit operator JsonRpcId(long value) => new JsonRpcId(value);

		/// <summary>Performs an implicit conversion from <see cref="double" /> to <see cref="JsonRpcId" />.</summary>
		/// <param name="value">The value to create a <see cref="JsonRpcId" /> from.</param>
		public static implicit operator JsonRpcId(double value) => new JsonRpcId(value);

		/// <summary>Performs an implicit conversion from <see cref="JsonRpcId" /> to <see cref="string" />.</summary>
		/// <param name="value">The identifier to get a <see cref="string" /> value from.</param>
		/// <exception cref="InvalidCastException">The underlying value is not of type <see cref="string" />.</exception>
		public static explicit operator string(in JsonRpcId value) =>
			value.Type == JsonRpcIdType.String
			? value._strVal
			: throw new InvalidCastException(string.Format(Strings.GetString("id.invalid_cast"),typeof(JsonRpcId),typeof(string)));

		/// <summary>Performs an implicit conversion from <see cref="JsonRpcId" /> to <see cref="long" />.</summary>
		/// <param name="value">The identifier to get a <see cref="long" /> value from.</param>
		/// <exception cref="InvalidCastException">The underlying value is not of type <see cref="long" />.</exception>
		public static explicit operator long(in JsonRpcId value) =>
			value.Type == JsonRpcIdType.Integer
			? value._intVal
			: throw new InvalidCastException(string.Format(Strings.GetString("id.invalid_cast"),typeof(JsonRpcId),typeof(long)));

		/// <summary>Performs an implicit conversion from <see cref="JsonRpcId" /> to <see cref="double" />.</summary>
		/// <param name="value">The identifier to get a <see cref="double" /> value from.</param>
		/// <exception cref="InvalidCastException">The underlying value is not of type <see cref="double" />.</exception>
		public static explicit operator double(in JsonRpcId value) =>
			value.Type == JsonRpcIdType.Float
			? value._floatVal
			: throw new InvalidCastException(string.Format(Strings.GetString("id.invalid_cast"),typeof(JsonRpcId),typeof(double)));

		/// <summary>Indicates whether the current JsonRpcId object has a value.</summary>
		public bool HasValue => _type != JsonRpcIdType.None;
	}
}