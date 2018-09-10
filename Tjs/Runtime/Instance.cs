using System;
using System.Collections.Generic;

namespace IronTjs.Runtime
{
    public class Instance : DynamicStorage
	{
		public Instance(Class type, params object[] args)
		{
			// 1. 空のオブジェクトの作成
			Type = type;
			Members = new Dictionary<string, object>();
			// 2. メソッド、プロパティの登録 (帰りがけ順)
			RegisterMembers(type);
			// 3. フィールドの登録および初期化
			RegisterFields(type);
			// 4. コンストラクタの呼び出し
			object member;
			Function ctor;
			if (TryGetValue(type.Name, true, out member) && (ctor = member as Function) != null)
				ctor.Invoke(args);
		}

		void RegisterMembers(Class type)
		{
			foreach (var parent in type.BaseClasses)
				RegisterMembers(parent());
			foreach (var member in type.Members)
			{
				var cc = member.Value as IContextChangeable;
				if (cc != null)
					Members[member.Key] = cc.ChangeContext(this);
				else
					Members[member.Key] = member.Value;
			}
		}

		void RegisterFields(Class type)
		{
			foreach (var parent in type.BaseClasses)
				RegisterFields(parent());
			foreach (var member in type.Fields)
				Members[member.Key] = member.Value(this);
		}

		public Dictionary<string, object> Members { get; private set; }

		public Class Type { get; private set; }

		protected override bool TryGetValue(object key, bool direct, out object member)
		{
			var name = key as string;
			if (name != null && Members.TryGetValue(name, out member))
			{
				if (!direct)
				{
					var prop = member as Property;
					if (prop != null)
						member = prop.Value;
				}
				return true;
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

		protected override bool TryCheckInstance(string className, out bool result)
		{
			result = string.Equals(Type.Name, className, StringComparison.Ordinal);
			return true;
		}

		protected override IEnumerable<string> GetMemberNames() { return Members.Keys; }

		public override string ToString() { return Type.Name; }
	}
}
