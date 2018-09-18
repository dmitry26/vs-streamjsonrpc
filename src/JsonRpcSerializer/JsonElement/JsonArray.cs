using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace System.Data.JsonRpc
{
	public class JsonArray : JsonElement, IReadOnlyList<JsonElement>
	{
		internal JsonArray(IReadOnlyList<JsonElement> list)
		{
			_list = list ?? throw new ArgumentNullException(nameof(list));
		}
		
		private IReadOnlyList<JsonElement> _list { get; }

		public override JsonElementType JsonType => JsonElementType.Array;

		public override bool TryGetIndex(GetIndexBinder binder,object[] indexes,out object result)
		{
			var idx = (int)indexes[0];
			result = _list[idx];
			return true;
		}

		public override bool TryBinaryOperation(BinaryOperationBinder binder,object arg,out object result)
		{
			return base.TryBinaryOperation(binder,arg,out result);
		}

		public override bool TryConvert(ConvertBinder binder,out object result)
		{
			var binderType = binder.Type;

			if (binderType.IsAssignableFrom(_list.GetType()))
			{
				result = _list;
				return true;
			}

			if (binderType.IsArray)
			{
				var elemType = binderType.GetElementType();
				var array = Array.CreateInstance(elemType,_list.Count);				

				for (int i = 0; i < _list.Count; ++i)
				{
					array.SetValue(ChangeType(_list[i],elemType),i);
				}

				result = array;
				return true;
			}
			
			if (typeof(IList).IsAssignableFrom(binderType))
			{
				var ctor = binderType.GetCtorDelegate();
				var target = (IList)ctor();
				var genArgType = binderType.GetGenericArguments()[0];

				foreach (var v in _list)
				{
					target.Add(ChangeType(v,genArgType));
				}

				result = target;
				return true;
			}

			return base.TryConvert(binder,out result);
		}

		public JsonElement this[int index] => _list[index];

		public int Count => _list.Count;

		public IEnumerator<JsonElement> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

		public IEnumerable<T> Cast<T>() =>
			_list.Select(x => (T)ChangeType(x,typeof(T)));

		public T[] ToArray<T>()
		{
			return _list.Select(x => (T)ChangeType((JsonValue)x,typeof(T))).ToArray();
		}
	}
}
