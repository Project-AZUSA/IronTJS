using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime
{
    public class Function : IDynamicMetaObjectProvider, IContextChangeable
	{
		public Function(Func<object, object, object[], object> functionBody, object global, object context)
		{
			_functionBody = functionBody;
			_global = global;
			Context = context;
		}

		object _global;
		Func<object, object, object[], object> _functionBody;

		public object Context { get; private set; }

		public object Invoke(params object[] args) { return _functionBody(_global, Context, args); }

		public Function ChangeContext(object context) { return new Function(_functionBody, _global, context); }

		IContextChangeable IContextChangeable.ChangeContext(object context) { return ChangeContext(context); }

		public DynamicMetaObject GetMetaObject(Expression parameter) { return new Meta(parameter, BindingRestrictions.GetTypeRestriction(parameter, typeof(Function)), this); }

		class Meta : DynamicMetaObject
		{
			public Meta(Expression expression, BindingRestrictions restrictions, Function value) : base(expression, restrictions, value) { }

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				CallSignature signature;
				var callBinder = binder as Binding.ICallBinder;
				if (callBinder != null)
					signature = callBinder.Signature;
				else
					signature = Binding.Binders.GetCallSignatureForCallInfo(binder.CallInfo);
				var spreadsAr = new bool[signature.ArgumentCount];
				for (var i = 0; i < signature.ArgumentCount; i++)
					spreadsAr[i] = signature.GetArgumentKind(i) == ArgumentType.List;
				var spreads = Expression.Constant(spreadsAr);
				return new DynamicMetaObject(
					Expression.Call(
						Expression.Convert(Expression, typeof(Function)),
						typeof(Function).GetMethod("Invoke", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
						Expression.Call(
							new Func<object[], bool[], object[]>(GetActualArguments).Method,
							Expression.NewArrayInit(typeof(object), args.Select(x => x.Expression)),
							spreads
						)
					),
					Restrictions.Merge(BindingRestrictions.Combine(args))
				);
			}

			static object[] GetActualArguments(object[] passedArgs, bool[] spread)
			{
				List<object> res = new List<object>();
				for (int i = 0; i < passedArgs.Length; i++)
				{
					if (spread[i])
					{
						var enumerable = passedArgs[i] as IEnumerable;
						if (enumerable == null)
							throw new InvalidOperationException("指定された引数は展開できません。");
						foreach (var item in enumerable)
							res.Add(item);
					}
					else
						res.Add(passedArgs[i]);
				}
				return res.ToArray();
			}
		}
	}
}
