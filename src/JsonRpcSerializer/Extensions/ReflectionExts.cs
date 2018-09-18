using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Data.JsonRpc
{
	public delegate object CtorDelegate(params object[] args);

	public static partial class ReflectionExts
	{
		public static CtorDelegate GetCtorDelegate(this Type src,params Type[] ctorArgTypes) =>
			GetCtorDelegate(new CtorInfoKey(src,null,ctorArgTypes));

		public static CtorDelegate GetCtorDelegate(this Type src,IReadOnlyList<Type> genArgTypes,params Type[] ctorArgTypes) =>
			GetCtorDelegate(new CtorInfoKey(src,genArgTypes,ctorArgTypes));

		public class CtorInfoKey : IEquatable<CtorInfoKey>
		{
			public CtorInfoKey(Type objType,IReadOnlyList<Type> genArgTypes,IReadOnlyList<Type> ctorArgTypes)
			{
				ObjectType = objType ?? throw new ArgumentNullException(nameof(objType));
				GenArgTypes = objType.IsGenericTypeDefinition ? genArgTypes ?? throw new ArgumentNullException(nameof(genArgTypes)) : null;
				CtorArgTypes = ctorArgTypes;
			}

			public Type ObjectType { get; }

			public IReadOnlyList<Type> GenArgTypes { get; }

			public IReadOnlyList<Type> CtorArgTypes { get; }

			public bool IsGenericTypeDefinition => ObjectType.IsGenericTypeDefinition;

			public bool Equals(CtorInfoKey other)
			{
				if (other is null)
					return false;

				if (ReferenceEquals(this,other))
					return true;

				if (ObjectType is null)
				{
					if (!(other.ObjectType is null))
						return false;
				}
				else if (!ObjectType.Equals(other.ObjectType))
					return false;

				if (!SequenceEqual(GenArgTypes,other.GenArgTypes))
					return false;

				return SequenceEqual(CtorArgTypes,other.CtorArgTypes);

				bool SequenceEqual(IReadOnlyList<Type> types,IReadOnlyList<Type> otherTypes)
				{
					if (types is null)
						return otherTypes is null;

					if (otherTypes is null)
						return false;

					if (types.Count != otherTypes.Count)
						return false;

					for (int i = 0; i < types.Count; ++i)
					{
						var type = types[i];
						var otherType = otherTypes[i];

						if (type is null)
						{
							if (!(otherType is null))
								return false;
						}
						else if (!type.Equals(otherType))
							return false;
					}

					return true;
				}
			}

			public override bool Equals(object obj) =>
				obj is CtorInfoKey otherKey ? Equals(otherKey) : false;

			public static bool operator ==(CtorInfoKey obj1,CtorInfoKey obj2) =>
			obj1 is null ? obj2 is null : obj1.Equals(obj2);

			public static bool operator !=(CtorInfoKey obj1,CtorInfoKey obj2) =>
				obj1 is null ? !(obj2 is null) : !obj1.Equals(obj2);
			
			public override int GetHashCode()
			{
				var hc = ObjectType?.GetHashCode() ?? 0;
				Aggregate(GenArgTypes);
				Aggregate(CtorArgTypes);
				return hc;

				void Aggregate(IReadOnlyList<Type> types)
				{
					if (types is null || types.Count == 0)
					{
						hc = ((hc << 5) + hc);
						return;
					}

					for (int i = 0; i < types.Count; ++i)
					{
						hc = ((hc << 5) + hc) ^ (types[i]?.GetHashCode() ?? 0);
					}
				}
			}
		}

		public static CtorDelegate GetCtorDelegate<T>(params Type[] ctorArgTypes) =>
			GetCtorDelegate(new CtorInfoKey(typeof(T),null,ctorArgTypes));

		private static Lazy<ConcurrentDictionary<CtorInfoKey,CtorDelegate>> _ctorDelCache = new Lazy<ConcurrentDictionary<CtorInfoKey,CtorDelegate>>();

		private static readonly Dictionary<CtorInfoKey,CtorDelegate> _ctorDelCache2 = new Dictionary<CtorInfoKey,CtorDelegate>();

		public static CtorDelegate GetCtorDelegate(this ConstructorInfo ctor) =>
			GetCtorDelegate(new CtorInfoKey(ctor.DeclaringType,null,ctor.GetParameters().Select(x => x.ParameterType).ToArray()));

		private static CtorDelegate GetCtorDelegate(CtorInfoKey key) =>
			_ctorDelCache.Value.GetOrAdd(key,CreateCtorDelegate);

		private static CtorDelegate CreateCtorDelegate(CtorInfoKey key)
		{
			var objType = key.IsGenericTypeDefinition
				? key.ObjectType.MakeGenericType(key.GenArgTypes.AsArray())
				: key.ObjectType;
			var ctor = objType.GetConstructor(key.CtorArgTypes.AsArray())
				?? throw new InvalidOperationException("Constructor not found");
			return ctor.CreateCtorDelegate();
		}

		public static CtorDelegate CreateCtorDelegate(this ConstructorInfo ctor)
		{
			var paramsInfo = (ctor ?? throw new ArgumentNullException(nameof(ctor))).GetParameters();
			var param = Expression.Parameter(typeof(object[]),"args");

			var argsExp = paramsInfo.Select((pi,idx) =>
				Expression.Convert(Expression.ArrayIndex(param,Expression.Constant(idx)),pi.ParameterType)
			).ToArray();

			var newExp = Expression.New(ctor,argsExp);
			return (CtorDelegate)Expression.Lambda(typeof(CtorDelegate),newExp,param).Compile();
		}

		public static CtorDelegate GetListCtorDel(Type genArgType) =>
			GetCtorDelegate(new CtorInfoKey(typeof(List<>),new[] { genArgType },
				new[] { typeof(List<>).MakeGenericType(genArgType) }));


		public static CtorDelegate GetListCtorDel(Type genArgType,Type ctorArgType) =>
			GetCtorDelegate(new CtorInfoKey(typeof(List<>),new[] { genArgType },
				new[] { ctorArgType }));

		public static CtorDelegate GetListDefCtorDel(Type genArgType) =>
			GetCtorDelegate(new CtorInfoKey(typeof(List<>),new[] { genArgType },null));

		public static CtorDelegate GetDictDefCtorDel(Type genKeyType,Type genValType) =>
			GetCtorDelegate(new CtorInfoKey(typeof(Dictionary<,>),new[] { genKeyType,genValType },null));		
	}	
}
