using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting.Runtime;

namespace IronTJS.Runtime.Binding
{
    class TjsConvertBinder : ConvertBinder
	{
		public TjsConvertBinder(TjsContext context, Type type, bool @explicit) : base(type, @explicit) { _context = context; }

		readonly TjsContext _context;

		public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			var newTarget = new DynamicMetaObject(Expression.Convert(target.Expression, target.LimitType), BindingRestrictions.GetTypeRestriction(target.Expression, target.LimitType), target.Value);
			var exp = TryConvertExpression(newTarget.Expression, Type, null);
			if (exp == null)
				exp = Expression.Throw(Expression.Constant(new InvalidCastException()), Type);
			if (ReturnType != Type)
				exp = Expression.Convert(exp, ReturnType);
			var ret = _context.Binder.ConvertTo(
				Type,
				Explicit ? Microsoft.Scripting.Actions.ConversionResultKind.ExplicitCast : Microsoft.Scripting.Actions.ConversionResultKind.ImplicitCast,
				newTarget,
				new TjsOverloadResolverFactory(_context.Binder),
				new DynamicMetaObject(exp, BindingRestrictionsHelpers.GetRuntimeTypeRestriction(target))
			);
			return ret;
		}

		static Expression NewNullableOrThrough(Expression exp, Type toType, Type nonNullableType)
		{
			if (toType == nonNullableType)
				return exp;
			else
				return Expression.New(toType.GetConstructor(new[] { nonNullableType }), exp);
		}

		internal static Expression TryConvertExpression(Expression expression, Type toType, ParameterExpression succeeded)
		{
			var nonNullable = Binders.GetNonNullableType(toType);
			if (toType == typeof(object))
				return InSuccess(Expression.Convert(expression, toType), succeeded);
			else if (toType == expression.Type)
				return InSuccess(expression, succeeded);
			else if (expression.Type == typeof(Builtins.Void))
			{
				if (toType == typeof(string))
					return InSuccess(Expression.Constant(string.Empty), succeeded);
				else
					return InSuccess(Expression.Default(toType), succeeded);
			}
			else if (Binders.IsNumber(nonNullable))
			{
				if (Binders.IsNumber(expression.Type))
					return InSuccess(NewNullableOrThrough(Expression.Convert(expression, nonNullable), toType, nonNullable), succeeded);
				else if (expression.Type == typeof(string))
				{
					ParameterExpression v;
					Expression test;
					if (Binders.IsInteger(nonNullable))
					{
						v = Expression.Variable(typeof(long));
						test = Expression.Call(typeof(Binders).GetMethod("TryConvertInt64"), expression, v);
					}
					else if (Binders.IsFloatingPoint(nonNullable))
					{
						v = Expression.Variable(typeof(double));
						test = Expression.Call(typeof(Binders).GetMethod("TryConvertDouble"), expression, v);
					}
					else
					{
						v = Expression.Variable(nonNullable);
						test = Expression.Call(nonNullable.GetMethod("TryParse", new[] { typeof(string), nonNullable.MakeByRefType() }), expression, v);
					}
					if (succeeded != null)
						test = Expression.Assign(succeeded, test);
					return Expression.Block(new[] { v },
						Expression.Condition(test,
							NewNullableOrThrough(v, toType, nonNullable),
							Expression.Default(toType)
						)
					);
				}
			}
			else if (toType == typeof(string))
			{
				if (expression.Type.IsValueType)
					return InSuccess(Expression.Call(expression, expression.Type.GetMethod("ToString", Type.EmptyTypes)), succeeded);
				var v = Expression.Variable(expression.Type);
				return InSuccess(Expression.Block(new[] { v },
					Expression.Condition(Expression.NotEqual(Expression.Assign(v, expression), Expression.Constant(null)),
						Expression.Call(v, expression.Type.GetMethod("ToString", Type.EmptyTypes)),
						Expression.Constant("null")
					)
				), succeeded);
			}
			else if (nonNullable == typeof(bool))
			{
				Expression exp;
				if (Binders.IsNumber(expression.Type))
					exp = Expression.NotEqual(expression, Expression.Default(expression.Type));
				else if (expression.Type == typeof(string))
				{
					var v = Expression.Variable(typeof(long));
					exp = Expression.Block(new[] { v },
						Expression.AndAlso(
							Expression.Call(typeof(long).GetMethod("TryParse", new[] { typeof(string), typeof(long).MakeByRefType() }), expression, v),
							Expression.NotEqual(v, Expression.Constant(0L))
						)
					);
				}
				else if (!expression.Type.IsValueType)
					exp = Expression.NotEqual(expression, Expression.Constant(null));
				else if (expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
					exp = Expression.Property(expression, "HasValue");
				else
					return InSuccess(nonNullable == toType ? Expression.Constant(true) : Expression.Constant(new bool?(true)), succeeded);
				return InSuccess(NewNullableOrThrough(exp, toType, nonNullable), succeeded);
			}
			return null;
		}

		static Expression InSuccess(Expression expression, ParameterExpression succeeded)
		{
			if (succeeded == null)
				return expression;
			else
				return Expression.Block(
					Expression.Assign(succeeded, Expression.Constant(true)),
					expression
				);
		}
	}
}
