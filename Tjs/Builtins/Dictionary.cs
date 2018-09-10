using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IronTjs.Builtins
{
    public class Dictionary : Runtime.DynamicStorage, IDictionary<string, object>, IDictionary
	{
		public Dictionary() { }

		public Dictionary(IEnumerable<KeyValuePair<string, object>> collection)
		{
			foreach (var kvp in collection)
				_members[kvp.Key] = kvp.Value;
		}

		Dictionary<string, object> _members = new Dictionary<string, object>();

		public static Runtime.Class GetClass() { return new DictionaryClass(); }

		protected override bool TryGetValue(object key, bool direct, out object member)
		{
			if (key != null && _members.TryGetValue(key.ToString(), out member))
			{
				if (!direct)
				{
					var prop = member as Runtime.Property;
					if (prop != null)
						member = prop.Value;
				}
				return true;
			}
			member = Void.Value;
			return true;
		}

		protected override bool TrySetValue(object key, bool direct, bool forceCreate, object value)
		{
			if (key != null)
			{
				object member;
				if (TryGetValue(key, true, out member))
				{
					Runtime.Property prop;
					if (direct || (prop = member as Runtime.Property) == null)
						_members[key.ToString()] = value;
					else
						prop.Value = value;
					return true;
				}
				else if (forceCreate)
				{
					Version++;
					_members[key.ToString()] = value;
					return true;
				}
			}
			return false;
		}

		protected override bool TryDeleteValue(object key)
		{
			if (key != null)
			{
				var result = _members.Remove(key.ToString());
				if (result)
					Version++;
				return result;
			}
			return false;
		}

		protected override IEnumerable<string> GetMemberNames() { return _members.Keys; }

		void IDictionary<string, object>.Add(string key, object value) { TrySetValue(key, false, true, value); }

		bool IDictionary<string, object>.ContainsKey(string key)
		{
			object dummy;
			return TryGetValue(key, false, out dummy);
		}

		ICollection<string> IDictionary<string, object>.Keys { get { return GetMemberNames().ToArray(); } }

		bool IDictionary<string, object>.Remove(string key) { return TryDeleteValue(key); }

		bool IDictionary<string, object>.TryGetValue(string key, out object value) { return TryGetValue(key, false, out value); }

		ICollection<object> IDictionary<string, object>.Values { get { return this.Select(x => x.Value).ToArray(); } }

		object IDictionary<string, object>.this[string key]
		{
			get
			{
				object v;
				if (TryGetValue(key, false, out v))
					return v;
				else
					throw new KeyNotFoundException();
			}
			set { TrySetValue(key, false, true, value); }
		}

		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) { TrySetValue(item.Key, false, true, item.Value); }

		void ICollection<KeyValuePair<string, object>>.Clear()
		{
			if (_members.Count > 0)
			{
				_members.Clear();
				Version++;
			}
		}

		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
		{
			object v;
			return TryGetValue(item.Key, false, out v) && EqualityComparer<object>.Default.Equals(item.Value, v);
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			foreach (var kvp in this)
				array[arrayIndex++] = kvp;
		}

		int ICollection<KeyValuePair<string, object>>.Count { get { return GetMemberNames().Count(); } }

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { return false; } }

		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			object v;
			if (TryGetValue(item.Key, false, out v) && EqualityComparer<object>.Default.Equals(item.Value, v))
				return TryDeleteValue(item.Key);
			return false;
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() { return new Enumerator(this); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return new Enumerator(this); }

		void IDictionary.Add(object key, object value) { ((IDictionary<string, object>)this).Add((string)Runtime.Binding.TjsBinder.ConvertInternal(key, typeof(string)), value); }

		void IDictionary.Clear() { ((IDictionary<string, object>)this).Clear(); }

		bool IDictionary.Contains(object key) { return ((IDictionary<string, object>)this).ContainsKey((string)Runtime.Binding.TjsBinder.ConvertInternal(key, typeof(string))); }

		IDictionaryEnumerator IDictionary.GetEnumerator() { return new Enumerator(this); }

		bool IDictionary.IsFixedSize { get { return false; } }

		bool IDictionary.IsReadOnly { get { return false; } }

		ICollection IDictionary.Keys { get { return GetMemberNames().ToArray(); } }

		void IDictionary.Remove(object key) { ((IDictionary<string, object>)this).Remove((string)Runtime.Binding.TjsBinder.ConvertInternal(key, typeof(string))); }

		ICollection IDictionary.Values { get { return this.Select(x => x.Value).ToArray(); } }

		object IDictionary.this[object key]
		{
			get { return ((IDictionary<string, object>)this)[(string)Runtime.Binding.TjsBinder.ConvertInternal(key, typeof(string))]; }
			set { ((IDictionary<string, object>)this)[(string)Runtime.Binding.TjsBinder.ConvertInternal(key, typeof(string))] = value; }
		}

		void ICollection.CopyTo(System.Array array, int index)
		{
			foreach (var kvp in this)
				array.SetValue(new DictionaryEntry(kvp.Key, kvp.Value), index++);
		}

		int ICollection.Count { get { return ((IDictionary<string, object>)this).Count; } }

		bool ICollection.IsSynchronized { get { return false; } }

		object ICollection.SyncRoot { get { return _members; } }

		struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IDictionaryEnumerator
		{
			public Enumerator(Dictionary owner)
			{
				_keyEnumerator = owner.GetMemberNames().GetEnumerator();
				_dict = owner;
				_value = null;
			}

			IDictionary<string, object> _dict;
			IEnumerator<string> _keyEnumerator;
			object _value;

			public KeyValuePair<string, object> Current { get { return new KeyValuePair<string, object>(_keyEnumerator.Current, _value); } }

			public void Dispose() { _keyEnumerator.Dispose(); }

			object IEnumerator.Current { get { return Current; } }

			public bool MoveNext()
			{
				if (_keyEnumerator.MoveNext())
				{
					_value = _dict[_keyEnumerator.Current];
					return true;
				}
				return false;
			}

			public void Reset() { _keyEnumerator.Reset(); }

			public DictionaryEntry Entry { get { return new DictionaryEntry(Current.Key, Current.Value); } }

			public object Key { get { return Current.Key; } }

			public object Value { get { return Current.Value; } }
		}

		class DictionaryClass : Runtime.Class
		{
			public DictionaryClass() : base(typeof(Dictionary).Name, Enumerable.Empty<Func<Runtime.Class>>(), PopulateMembers(), Enumerable.Empty<KeyValuePair<string, Func<object, object>>>()) { }

			static IEnumerable<KeyValuePair<string, object>> PopulateMembers()
			{
				yield return new KeyValuePair<string, object>("saveStruct", new Runtime.Function(saveStruct, null, null));
				yield return new KeyValuePair<string, object>("assign", new Runtime.Function(assign, null, null));
				yield return new KeyValuePair<string, object>("assignStruct", new Runtime.Function(assignStruct, null, null));
				yield return new KeyValuePair<string, object>("clear", new Runtime.Function(clear, null, null));
			}

			static object saveStruct(object global, object self, object[] args)
			{
				string fileName = (string)Runtime.Binding.TjsBinder.ConvertInternal(args[0], typeof(string));
				System.IO.File.WriteAllText(fileName, Utils.ConvertToExpression((IEnumerable)self, 0));
				return self;
			}

			static object assign(object global, object self, object[] args)
			{
				var source = args.Length > 0 ? args[0] as IEnumerable : null;
				var clearContents = args.Length > 1 ? (args[1] == Void.Value ? true : (bool)Runtime.Binding.TjsBinder.ConvertInternal(args[1], typeof(bool))) : true;
				if (clearContents)
					clear(global, self, new object[0]);
				var dictSource = source as IDictionary;
				if (dictSource != null)
				{
					var de = dictSource.GetEnumerator();
					while (de.MoveNext())
						((IDictionary)self)[de.Key] = de.Value;
				}
				else
				{
					object key = null;
					foreach (var item in source)
					{
						if (key == null)
							key = item;
						else
						{
							((IDictionary)self)[key] = item;
							key = null;
						}
					}
				}
				return Void.Value;
			}

			static void assignStructInternal(IDictionary self, IEnumerable source)
			{
				var dictSource = source as IDictionary;
				if (dictSource != null)
				{
					var de = dictSource.GetEnumerator();
					while (de.MoveNext())
					{
						var subSource = de.Value as IEnumerable;
						if (subSource != null)
						{
							var subDict = new Dictionary();
							assignStructInternal(subDict, subSource);
							self[de.Key] = subDict;
						}
						else
							self[de.Key] = de.Value;
					}
				}
				else
				{
					object key = null;
					foreach (var item in source)
					{
						if (key == null)
							key = item;
						else
						{
							var subSource = item as IEnumerable;
							if (subSource != null)
							{
								var subDict = new Dictionary();
								assignStructInternal(subDict, subSource);
								self[key] = subDict;
							}
							else
								self[key] = item;
							key = null;
						}
					}
				}
			}

			static object assignStruct(object global, object self, object[] args)
			{
				var source = args.Length > 0 ? args[0] as IEnumerable : null;
				assignStructInternal((IDictionary)self, source);
				return Void.Value;
			}

			static object clear(object global, object self, object[] args)
			{
				((IDictionary)self).Clear();
				return Void.Value;
			}

			protected override bool TryCreateInstance(object[] args, out object result)
			{
				result = new Dictionary();
				return true;
			}
		}
	}
}
