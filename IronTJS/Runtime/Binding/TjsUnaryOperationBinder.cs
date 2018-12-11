using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;

namespace IronTJS.Runtime.Binding
{
    class TjsUnaryOperationBinder : UnaryOperationBinder
	{
		public TjsUnaryOperationBinder(TjsContext context, ExpressionType operation) : base(operation) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			var arg = Expression.Convert(target.Expression, target.LimitType);
			var restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target);
			Expression res = null;
			switch (Operation)
			{
				case ExpressionType.Decrement:
					if (Binders.IsFloatingPoint(target.LimitType))
						res = Expression.Decrement(_context.Convert(arg, typeof(double)));
					else
						res = Expression.Decrement(_context.Convert(arg, typeof(long)));
					break;
				case ExpressionType.Increment:
					if (Binders.IsFloatingPoint(target.LimitType))
						res = Expression.Increment(_context.Convert(arg, typeof(double)));
					else
						res = Expression.Increment(_context.Convert(arg, typeof(long)));
					break;
				case ExpressionType.Negate:
					if (Binders.IsFloatingPoint(target.LimitType))
						res = Expression.Negate(_context.Convert(arg, typeof(double)));
					else
						res = Expression.Negate(_context.Convert(arg, typeof(long)));
					break;
				case ExpressionType.Not:
					res = Expression.Condition(_context.Convert(arg, typeof(bool)), Expression.Constant(0L), Expression.Constant(1L));
					break;
				case ExpressionType.OnesComplement:
					res = Expression.OnesComplement(_context.Convert(arg, typeof(long)));
					break;
				case ExpressionType.UnaryPlus:
					if (Binders.IsFloatingPoint(target.LimitType))
						res = _context.Convert(arg, typeof(double));
					else if (Binders.IsInteger(target.LimitType))
						res = _context.Convert(arg, typeof(long));
					else if (target.LimitType == typeof(string))
						res = Expression.Call(new Func<string, object>(Binders.ConvertNumber).Method, arg);
					break;
			}
			if (res != null)
			{
				if (res.Type != typeof(object))
					return new DynamicMetaObject(Expression.Convert(res, typeof(object)), restrictions);
				else
					return new DynamicMetaObject(res, restrictions);
			}
			else
				return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("不正な単項演算です。")), typeof(object)), restrictions);
		}
	}
}
