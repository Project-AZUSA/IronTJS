using System.Collections.Generic;
using System.Linq;

namespace IronTJS.Runtime
{
    public class TjsScopeStorage : DynamicStorage
	{
		public TjsScopeStorage() { }

		public TjsScopeStorage(IEnumerable<KeyValuePair<string, object>> pairs)
		{
			foreach (var pair in pairs)
				_members.Add(pair.Key, pair.Value);
		}

		Dictionary<string, dynamic> _members = new Dictionary<string, dynamic>();

		protected override bool TryGetValue(object key, bool direct, out object member)
		{
			var name = key as string;
			if (name == null)
			{
				member = null;
				return false;
			}
			lock (_members)
			{
				if (_members.TryGetValue(name, out member))
				{
					if (!direct)
					{
						var prop = member as Property;
						if (prop != null)
							member = prop.Value;
					}
					return true;
				}
				return false;
			}
		}

		protected override bool TrySetValue(object key, bool direct, bool forceCreate, object value)
		{
			string name = key as string;
			if (name == null)
				return false;
			lock (_members)
			{
				object member;
				if (_members.TryGetValue(name, out member))
				{
					Property prop;
					if (direct || (prop = member as Property) == null)
						_members[name] = value;
					else
						prop.Value = value;
					return true;
				}
				else if (forceCreate)
				{
					Version++;
					_members[name] = value;
					return true;
				}
				return false;
			}
		}

		protected override bool TryDeleteValue(object key)
		{
			lock (_members)
			{
				var name = key as string;
				if (name == null)
					return false;
				var result = _members.Remove(name);
				if (result)
					Version++;
				return result;
			}
		}

		protected override IEnumerable<string> GetMemberNames() { lock (_members) return _members.Keys.ToArray(); }
	}
}
