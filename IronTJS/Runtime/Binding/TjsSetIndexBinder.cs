using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace IronTJS.Runtime.Binding
{
    class TjsSetIndexBinder : SetIndexBinder, IDirectAccessible
	{
		public TjsSetIndexBinder(TjsContext context, CallInfo callInfo, bool direct) : base(callInfo)
		{
			_context = context;
			DirectAccess = direct;
		}

		readonly TjsContext _context;

		public bool DirectAccess { get; private set; }

		public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			var arguments = ArrayUtils.Append(ArrayUtils.Insert(target, indexes), value);
			var result = _context.Binder.SetIndex(new TjsOverloadResolverFactory(_context.Binder), arguments);
			if (result != null)
			{
				if (result.Expression.Type.IsValueType)
					return new DynamicMetaObject(Expression.Convert(result.Expression, typeof(object)), result.Restrictions);
				else
					return result;
			}
			if (indexes[0].LimitType == typeof(string))
			{
				return new DynamicMetaObject(
					Expression.Dynamic(new TjsSetMemberBinder(_context, (string)indexes[0].Value, false, true, DirectAccess), ReturnType, target.Expression, value.Expression),
					BindingRestrictions.Combine(arguments).Merge(
						BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value)
					).Merge(
						BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType)
					)
				);
			}
			return errorSuggestion ?? new DynamicMetaObject(
				Expression.Throw(Expression.Constant(new MissingMemberException(indexes[0].Value.ToString())), typeof(object)),
				BindingRestrictions.Combine(arguments).Merge(BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType))
			);
		}
	}
}
