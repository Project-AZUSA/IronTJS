using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Runtime.Binding
{
    class TjsBinaryOperationBinder : BinaryOperationBinder
	{
		public TjsBinaryOperationBinder(TjsContext context, ExpressionType operation) : base(operation) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
		{
			var left = Expression.Convert(target.Expression, target.LimitType);
			var right = Expression.Convert(arg.Expression, arg.LimitType);
			Expression res = null;
			switch (Operation)
			{
				case ExpressionType.Add:
					if (target.LimitType == typeof(string) || arg.LimitType == typeof(string))
						res = Expression.Call(new Func<string, string, string>(string.Concat).Method, _context.Convert(left, typeof(string)), _context.Convert(right, typeof(string)));
					else if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
						res = Expression.Add(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
					else
						res = Expression.Add(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
				case ExpressionType.And:
					res = Expression.And(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
				case ExpressionType.Divide:
					res = Expression.Divide(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
					break;
				case ExpressionType.Equal:
					res = Expression.Condition(Equal(left, right), Expression.Constant(1L), Expression.Constant(0L));
					break;
				case ExpressionType.ExclusiveOr:
					res = Expression.ExclusiveOr(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
				case ExpressionType.GreaterThan:
					{
						Expression exp;
						if (target.LimitType == typeof(string) && arg.LimitType == typeof(string))
							exp = Expression.GreaterThan(Expression.Call(new Func<string, string, int>(string.CompareOrdinal).Method, left, right), Expression.Constant(0));
						else if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
							exp = Expression.GreaterThan(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
						else
							exp = Expression.GreaterThan(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
						res = Expression.Condition(exp, Expression.Constant(1L), Expression.Constant(0L));
					}
					break;
				case ExpressionType.GreaterThanOrEqual:
					{
						Expression exp;
						if (target.LimitType == typeof(string) && arg.LimitType == typeof(string))
							exp = Expression.GreaterThanOrEqual(Expression.Call(new Func<string, string, int>(string.CompareOrdinal).Method, left, right), Expression.Constant(0));
						else if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
							exp = Expression.GreaterThanOrEqual(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
						else
							exp = Expression.GreaterThanOrEqual(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
						res = Expression.Condition(exp, Expression.Constant(1L), Expression.Constant(0L));
					}
					break;
				case ExpressionType.LeftShift:
					res = Expression.LeftShift(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(int)));
					break;
				case ExpressionType.LessThan:
					{
						Expression exp;
						if (target.LimitType == typeof(string) && arg.LimitType == typeof(string))
							exp = Expression.LessThan(Expression.Call(new Func<string, string, int>(string.CompareOrdinal).Method, left, right), Expression.Constant(0));
						else if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
							exp = Expression.LessThan(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
						else
							exp = Expression.LessThan(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
						res = Expression.Condition(exp, Expression.Constant(1L), Expression.Constant(0L));
					}
					break;
				case ExpressionType.LessThanOrEqual:
					{
						Expression exp;
						if (target.LimitType == typeof(string) && arg.LimitType == typeof(string))
							exp = Expression.LessThanOrEqual(Expression.Call(new Func<string, string, int>(string.CompareOrdinal).Method, left, right), Expression.Constant(0));
						else if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
							exp = Expression.LessThanOrEqual(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
						else
							exp = Expression.LessThanOrEqual(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
						res = Expression.Condition(exp, Expression.Constant(1L), Expression.Constant(0L));
					}
					break;
				case ExpressionType.Modulo:
					res = Expression.Modulo(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
				case ExpressionType.Multiply:
					if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
						res = Expression.Multiply(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
					else
						res = Expression.Multiply(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
				case ExpressionType.NotEqual:
					res = Expression.Condition(Equal(left, right), Expression.Constant(0L), Expression.Constant(1L));
					break;
				case ExpressionType.Or:
					res = Expression.Or(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
				case ExpressionType.RightShift:
					res = Expression.RightShift(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(int)));
					break;
				case ExpressionType.Subtract:
					if (Binders.IsFloatingPoint(target.LimitType) || Binders.IsFloatingPoint(arg.LimitType))
						res = Expression.Subtract(_context.Convert(left, typeof(double)), _context.Convert(right, typeof(double)));
					else
						res = Expression.Subtract(_context.Convert(left, typeof(long)), _context.Convert(right, typeof(long)));
					break;
			}
			var restrictions = BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target).Merge(BindingRestrictionsHelpers.GetRuntimeTypeRestriction(arg));
			if (res != null)
				return new DynamicMetaObject(Expression.Convert(res, typeof(object)), restrictions);
			else
				return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("不正な二項演算です。")), typeof(object)), restrictions);
		}

		static Expression Equal(Expression left, Expression right)
		{
			List<Expression> exps = new List<Expression>();
			// Variable
			var l = Expression.Variable(left.Type);
			var r = Expression.Variable(right.Type);
			var succeeded = Expression.Variable(typeof(bool));
			exps.Add(Expression.Assign(l, left));
			exps.Add(Expression.Assign(r, right));
			// Left -> Right
			var leftToRight = TjsConvertBinder.TryConvertExpression(l, right.Type, succeeded);
			if (leftToRight != null)
			{
				var res = Expression.Variable(right.Type);
				// ((res = Convert(right, leftType, out succeeded)), succeeded && res == r)
				leftToRight = Expression.Block(new[] { res },
					Expression.Assign(res, leftToRight),
					Expression.AndAlso(succeeded, Expression.Equal(res, r))
				);
			}
			// Right -> Left
			var rightToLeft = TjsConvertBinder.TryConvertExpression(r, left.Type, succeeded);
			if (rightToLeft != null)
			{
				var res = Expression.Variable(left.Type);
				// ((res = Convert(left, rightType, out succeeded)), succeeded && l == res)
				rightToLeft = Expression.Block(new[] { res },
					Expression.Assign(res, rightToLeft),
					Expression.AndAlso(succeeded, Expression.Equal(l, res))
				);
			}
			if (leftToRight == null && rightToLeft == null)
				return Expression.Constant(false);
			else if (leftToRight == null)
				exps.Add(rightToLeft);
			else if (rightToLeft == null)
				exps.Add(leftToRight);
			else
				exps.Add(Expression.OrElse(leftToRight, rightToLeft));
			return Expression.Block(new[] { l, r, succeeded }, exps);
		}
	}
}
