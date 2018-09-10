using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
    class TjsCreateInstanceBinder : CreateInstanceBinder
	{
		public TjsCreateInstanceBinder(TjsContext context, CallInfo callInfo) : base(callInfo) { Context = context; }

		public TjsContext Context { get; private set; }

		public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			var t = target.Value as Type;
			if (t == null)
			{
				var tt = target.Value as TypeTracker;
				if (tt != null)
					t = tt.Type;
			}
			if (t != null)
			{
				return Context.Binder.CallMethod(
					new TjsOverloadResolver(Context.Binder, args, Binders.GetCallSignatureForCallInfo(CallInfo), Microsoft.Scripting.Runtime.CallTypes.None),
					Microsoft.Scripting.Generation.CompilerHelpers.GetConstructors(t, false),
					target.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value))
				);
			}
			return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.Constant(new NotImplementedException()), typeof(object)), target.Restrictions.Merge(BindingRestrictions.Combine(args)));
		}
	}
}
