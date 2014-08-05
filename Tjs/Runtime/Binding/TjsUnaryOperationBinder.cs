﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Runtime.Binding
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
					if (Binders.IsReal(target.LimitType))
						res = Expression.Decrement(_context.Convert(arg, typeof(double)));
					else
						res = Expression.Decrement(_context.Convert(arg, typeof(long)));
					break;
				case ExpressionType.Increment:
					if (Binders.IsReal(target.LimitType))
						res = Expression.Increment(_context.Convert(arg, typeof(double)));
					else
						res = Expression.Increment(_context.Convert(arg, typeof(long)));
					break;
				case ExpressionType.Negate:
					if (Binders.IsReal(target.LimitType))
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
					if (Binders.IsReal(target.LimitType))
						res = _context.Convert(arg, typeof(double));
					else if (Binders.IsInteger(target.LimitType))
						res = _context.Convert(arg, typeof(long));
					else if (target.LimitType == typeof(string))
						res = _context.Convert(arg, typeof(double));
					break;
			}
			if (res != null)
				return new DynamicMetaObject(Expression.Convert(res, typeof(object)), restrictions);
			else
				return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("不正な単項演算です。")), typeof(object)), restrictions);
		}
	}
}
