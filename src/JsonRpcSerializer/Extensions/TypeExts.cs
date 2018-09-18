using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.JsonRpc
{
    public static partial class TypeExts
    {
		public static bool IsNullable(this Type type) => (type ?? throw new ArgumentNullException(nameof(type)))
			.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
	}
}
