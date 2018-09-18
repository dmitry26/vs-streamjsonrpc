using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace System.Data.JsonRpc
{
	public class JsonObject : JsonElement, IReadOnlyDictionary<string,JsonElement>
	{
		internal JsonObject(IReadOnlyDictionary<string,JsonElement> dict)
		{
			_dict = dict ?? throw new ArgumentNullException(nameof(dict));	
		}

		private IReadOnlyDictionary<string,JsonElement> _dict { get; }

		public override JsonElementType JsonType => JsonElementType.Object;

		public JsonElement this[string key] =>		
			!_dict.TryGetValue(key,out JsonElement value)
				? null : value;

		public override IEnumerable<string> GetDynamicMemberNames() => _dict.Keys;

		public override bool TryGetMember(GetMemberBinder binder,out object result)
		{
			result = (_dict.TryGetValue(binder.Name,out JsonElement val)) ? val : null;
			return true;
		}

		public IEnumerable<string> Keys => _dict.Keys;

		public IEnumerable<JsonElement> Values => _dict.Values;

		public int Count => _dict.Count;

		public bool ContainsKey(string key) => _dict.ContainsKey(key);
		public IEnumerator<KeyValuePair<string,JsonElement>> GetEnumerator() => _dict.GetEnumerator();
		public bool TryGetValue(string key,out JsonElement value) => _dict.TryGetValue(key,out value);
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
	}
}
