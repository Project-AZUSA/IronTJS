using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace IronTjs.Runtime
{
    public class Class : DynamicStorage
	{
		public Class(string name, IEnumerable<Func<Class>> baseClasses, IEnumerable<KeyValuePair<string, object>> members, IEnumerable<KeyValuePair<string, Func<object, object>>> fields)
		{
			Name = name;
			BaseClasses = baseClasses.ToReadOnly();
			Members = members.ToDictionary(x => x.Key, x => x.Value);
			Fields = fields.ToDictionary(x => x.Key, x => x.Value);
		}

		public string Name { get; private set; }

		public ReadOnlyCollection<Func<Class>> BaseClasses { get; private set; }

		public Dictionary<string, object> Members { get; private set; }

		public Dictionary<string, Func<object, object>> Fields { get; private set; }

		protected override bool TryGetValue(object key, bool direct, out object member)
		{
			var name = key as string;
			if (name != null)
			{
				if (Members.TryGetValue(name, out member))
				{
					if (!direct)
					{
						var prop = member as Property;
						if (prop != null)
							member = prop.Value;
					}
					return true;
				}
				foreach (var baseClass in BaseClasses.Reverse())
				{
					if (baseClass().TryGetValue(key, direct, out member))
						return true;
				}
			}
			member = null;
			return false;
		}

		protected override bool TrySetValue(object key, bool direct, bool forceCreate, object value)
		{
			string name = key as string;
			if (name != null)
			{
				object member;
				if (TryGetValue(key, true, out member))
				{
					Property prop;
					if (direct || (prop = member as Property) == null)
						Members[name] = value;
					else
						prop.Value = value;
					return true;
				}
				else if (forceCreate)
				{
					Version++;
					Members[name] = value;
					return true;
				}
			}
			return false;
		}

		protected override bool TryDeleteValue(object key)
		{
			var name = key as string;
			if (name != null)
			{
				var result = Members.Remove(name);
				if (result)
					Version++;
				return result;
			}
			return false;
		}

		protected override bool TryCreateInstance(object[] args, out object result)
		{
			result = new Instance(this, args);
			return true;
		}

		protected override IEnumerable<string> GetMemberNames() { return Members.Keys.Concat(BaseClasses.SelectMany(x => x().GetMemberNames())).Distinct(); }
	}
}
