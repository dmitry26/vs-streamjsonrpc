using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.JsonRpc
{
	public static partial class LinqExts
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static T[] AsArray<T>(this IReadOnlyList<T> src) =>
			(src ?? new T[0]) is T[] arr ? arr : src.ToArray();

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> src) =>
			(src ?? new T[0]) is IReadOnlyList<T> list ? list : src.ToArray();
	}
}
