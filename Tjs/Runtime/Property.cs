using System;

namespace IronTjs.Runtime
{
    public class Property : IContextChangeable
	{
		public Property(Func<object, object, object[], object> getter, Func<object, object, object[], object> setter, object global, object context)
		{
			_getter = getter;
			_setter = setter;
			_global = global;
			Context = context;
		}

		Func<object, object, object[], object> _getter;
		Func<object, object, object[], object> _setter;
		object _global;

		public object Context { get; private set; }

		public object Value
		{
			get
			{
				if (_getter == null)
					throw new InvalidOperationException("プロパティに getter が存在しないため右辺値となることができません。");
				return _getter(_global, Context, new object[0]);
			}
			set
			{
				if (_setter == null)
					throw new InvalidOperationException("プロパティに setter が存在しないため左辺値となることができません。");
				_setter(_global, Context, new[] { value });
			}
		}

		public Property ChangeContext(object context) { return new Property(_getter, _setter, _global, context); }

		IContextChangeable IContextChangeable.ChangeContext(object context) { return ChangeContext(context); }
	}
}
