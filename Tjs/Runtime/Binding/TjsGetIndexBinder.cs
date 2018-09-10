using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace IronTjs.Runtime.Binding
{
    class TjsGetIndexBinder : GetIndexBinder, IDirectAccessible
	{
		public TjsGetIndexBinder(TjsContext context, CallInfo callInfo, bool direct) : base(callInfo)
		{
			_context = context;
			DirectAccess = direct;
		}

		readonly TjsContext _context;

		public bool DirectAccess { get; private set; }

		public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
		{
			var result = _context.Binder.GetIndex(new TjsOverloadResolverFactory(_context.Binder), ArrayUtils.Insert(target, indexes));
			if (result != null)
			{
				if (result.Expression.Type.IsValueType)
					return new DynamicMetaObject(Expression.Convert(result.Expression, typeof(object)), result.Restrictions);
				else
					return result;
			}
			if (indexes[0].LimitType == typeof(string))
			{
				result = target.BindGetMember(new TjsGetMemberBinder(_context, (string)indexes[0].Value, false, DirectAccess));
				return new DynamicMetaObject(result.Expression, result.Restrictions.Merge(
					BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value)
				).Merge(
					BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType)
				));
			}
			return errorSuggestion ?? new DynamicMetaObject(
				Expression.Throw(Expression.Constant(new MissingMemberException(indexes[0].Value.ToString())), typeof(object)),
				BindingRestrictions.Combine(ArrayUtils.Insert(target, indexes)).Merge(BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType))
			);
		}
	}
}
