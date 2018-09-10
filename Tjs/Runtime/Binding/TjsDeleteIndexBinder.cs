using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Utils;

namespace IronTjs.Runtime.Binding
{
    class TjsDeleteIndexBinder : DynamicMetaObjectBinder
	{
		public TjsDeleteIndexBinder(TjsContext context, CallInfo callInfo)
		{
			_context = context;
			CallInfo = callInfo;
		}

		readonly TjsContext _context;

		public CallInfo CallInfo { get; private set; }

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			var obj = target.BindDeleteIndex(new CompatibilityDeleteIndexBinder(_context, CallInfo), args);
			return new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Try(
				obj.Expression,
				Expression.Constant(1L, typeof(object))
			).Catch(typeof(MissingMemberException),
				Expression.Constant(0L, typeof(object))
			), obj.Restrictions);
		}
	}

	class CompatibilityDeleteIndexBinder : DeleteIndexBinder
	{
		public CompatibilityDeleteIndexBinder(TjsContext context, CallInfo callInfo) : base(callInfo) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
		{
			if (indexes[0].LimitType == typeof(string))
			{
				return new DynamicMetaObject(
					Expression.Dynamic(new TjsDeleteMemberBinder(_context, (string)indexes[0].Value, false), ReturnType, target.Expression),
					BindingRestrictions.Combine(ArrayUtils.Insert(target, indexes)).Merge(
						BindingRestrictions.GetInstanceRestriction(indexes[0].Expression, indexes[0].Value)
					).Merge(
						BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType)
					)
				);
			}
			return errorSuggestion ?? new DynamicMetaObject(
				Expression.Throw(Expression.Constant(new MissingMemberException(indexes[0].Value.ToString())), typeof(object)),
				BindingRestrictions.Combine(ArrayUtils.Insert(target, indexes)).Merge(BindingRestrictions.GetTypeRestriction(indexes[0].Expression, indexes[0].LimitType))
			);
		}
	}
}
