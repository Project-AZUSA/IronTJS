using System;
using IronTJS.Runtime.Binding;

namespace IronTJS.Compiler.Ast
{
    public class BinaryExpression : Expression
	{
		public BinaryExpression(Expression left, Expression right, BinaryOperator expressionType)
		{
			Left = left;
			Right = right;
			ExpressionType = expressionType;
			Left.Parent = Right.Parent = this;
		}

		public Expression Left { get; private set; }

		public Expression Right { get; private set; }

		public BinaryOperator ExpressionType { get; private set; }

		static OperationDistributionResult DistributeOperation(BinaryOperator binary)
		{
			switch (binary)
			{
				case BinaryOperator.Add:
				case BinaryOperator.AddAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Add);
				case BinaryOperator.And:
				case BinaryOperator.AndAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.And);
				case BinaryOperator.DistinctEqual:
					return new OperationDistributionResult(TjsOperationKind.DistinctEqual);
				case BinaryOperator.DistinctNotEqual:
					return new OperationDistributionResult(TjsOperationKind.DistinctNotEqual);
				case BinaryOperator.Divide:
				case BinaryOperator.DivideAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Divide);
				case BinaryOperator.Equal:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Equal);
				case BinaryOperator.ExclusiveOr:
				case BinaryOperator.ExclusiveOrAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.ExclusiveOr);
				case BinaryOperator.FloorDivide:
				case BinaryOperator.FloorDivideAssign:
					return new OperationDistributionResult(TjsOperationKind.FloorDivide);
				case BinaryOperator.GreaterThan:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.GreaterThan);
				case BinaryOperator.GreaterThanOrEqual:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.GreaterThanOrEqual);
				case BinaryOperator.InContextOf:
					return new OperationDistributionResult(TjsOperationKind.InContextOf);
				case BinaryOperator.InstanceOf:
					return new OperationDistributionResult(TjsOperationKind.InstanceOf);
				case BinaryOperator.LeftShift:
				case BinaryOperator.LeftShiftAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.LeftShift);
				case BinaryOperator.LessThan:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.LessThan);
				case BinaryOperator.LessThanOrEqual:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.LessThanOrEqual);
				case BinaryOperator.Modulo:
				case BinaryOperator.ModuloAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Modulo);
				case BinaryOperator.Multiply:
				case BinaryOperator.MultiplyAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Multiply);
				case BinaryOperator.NotEqual:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.NotEqual);
				case BinaryOperator.Or:
				case BinaryOperator.OrAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Or);
				case BinaryOperator.RightShiftArithmetic:
				case BinaryOperator.RightShiftArithmeticAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.RightShift);
				case BinaryOperator.RightShiftLogical:
				case BinaryOperator.RightShiftLogicalAssign:
					return new OperationDistributionResult(TjsOperationKind.RightShiftLogical);
				case BinaryOperator.Subtract:
				case BinaryOperator.SubtractAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Subtract);
				default:
					throw new ArgumentException("演算子に対応する動的操作が存在しません。");
			}
		}

		internal static System.Linq.Expressions.Expression TransformSimpleOperation(Runtime.TjsContext context, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right, BinaryOperator binary)
		{
			if (binary == BinaryOperator.Assign)
				return right;
			else if (binary == BinaryOperator.AndAlso || binary == BinaryOperator.AndAlsoAssign)
			{
				var v = System.Linq.Expressions.Expression.Variable(typeof(object));
				return System.Linq.Expressions.Expression.Block(new[] { v },
					System.Linq.Expressions.Expression.Assign(v, left),
					System.Linq.Expressions.Expression.Condition(Binders.Convert(context, v, typeof(bool)), right, v)
				);
			}
			else if (binary == BinaryOperator.OrElse || binary == BinaryOperator.OrElseAssign)
			{
				var v = System.Linq.Expressions.Expression.Variable(typeof(object));
				return System.Linq.Expressions.Expression.Block(new[] { v },
					System.Linq.Expressions.Expression.Assign(v, left),
					System.Linq.Expressions.Expression.Condition(Binders.Convert(context, v, typeof(bool)), v, right)
				);
			}
			else
				return DoBinaryOperation(context, left, right, binary);
		}

		static System.Linq.Expressions.Expression DoBinaryOperation(Runtime.TjsContext context, System.Linq.Expressions.Expression left, System.Linq.Expressions.Expression right, BinaryOperator operation)
		{
			var res = DistributeOperation(operation);
			if (res.IsStandardOperation)
				return System.Linq.Expressions.Expression.Dynamic(context.CreateBinaryOperationBinder(res.ExpressionType), typeof(object), left, right);
			else
				return System.Linq.Expressions.Expression.Dynamic(context.CreateOperationBinder(res.OperationKind), typeof(object), left, right);
		}

		public override System.Linq.Expressions.Expression TransformRead()
		{
			if (ExpressionType == BinaryOperator.Exchange)
				throw new InvalidOperationException("交換操作の結果を得ることはできません。");
			var exp = TransformSimpleOperation(LanguageContext, Left.TransformRead(), Right.TransformRead(), ExpressionType);
			switch (ExpressionType)
			{
				case BinaryOperator.Assign:
				case BinaryOperator.AddAssign:
				case BinaryOperator.AndAssign:
				case BinaryOperator.AndAlsoAssign:
				case BinaryOperator.DivideAssign:
				case BinaryOperator.ExclusiveOrAssign:
				case BinaryOperator.FloorDivideAssign:
				case BinaryOperator.LeftShiftAssign:
				case BinaryOperator.ModuloAssign:
				case BinaryOperator.MultiplyAssign:
				case BinaryOperator.OrAssign:
				case BinaryOperator.OrElseAssign:
				case BinaryOperator.RightShiftArithmeticAssign:
				case BinaryOperator.RightShiftLogicalAssign:
				case BinaryOperator.SubtractAssign:
					return Left.TransformWrite(exp);
				default:
					return exp;
			}
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			switch (ExpressionType)
			{
				case BinaryOperator.Add:
				case BinaryOperator.RightShiftArithmetic:
				case BinaryOperator.And:
				case BinaryOperator.Or:
				case BinaryOperator.ExclusiveOr:
				case BinaryOperator.Divide:
				case BinaryOperator.Equal:
				case BinaryOperator.FloorDivide:
				case BinaryOperator.GreaterThan:
				case BinaryOperator.GreaterThanOrEqual:
				case BinaryOperator.InContextOf:
				case BinaryOperator.InstanceOf:
				case BinaryOperator.LeftShift:
				case BinaryOperator.LessThan:
				case BinaryOperator.LessThanOrEqual:
				case BinaryOperator.RightShiftLogical:
				case BinaryOperator.Modulo:
				case BinaryOperator.Multiply:
				case BinaryOperator.NotEqual:
				case BinaryOperator.DistinctEqual:
				case BinaryOperator.DistinctNotEqual:
				case BinaryOperator.Subtract:
					return System.Linq.Expressions.Expression.Block(Left.TransformVoid(), Right.TransformVoid());
				case BinaryOperator.AndAlso:
					return System.Linq.Expressions.Expression.IfThen(Left.TransformReadAsBoolean(), Right.TransformVoid());
				case BinaryOperator.OrElse:
					return System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Not(Left.TransformReadAsBoolean()), Right.TransformVoid());
				case BinaryOperator.Exchange:
					{
						var v = System.Linq.Expressions.Expression.Variable(typeof(object));
						return System.Linq.Expressions.Expression.Block(new[] { v },
							System.Linq.Expressions.Expression.Assign(v, Left.TransformRead()),
							Left.TransformWrite(Right.TransformRead()),
							Right.TransformWrite(v),
							System.Linq.Expressions.Expression.Empty()
						);
					}
				case BinaryOperator.Assign:
				case BinaryOperator.AddAssign:
				case BinaryOperator.AndAssign:
				case BinaryOperator.AndAlsoAssign:
				case BinaryOperator.DivideAssign:
				case BinaryOperator.ExclusiveOrAssign:
				case BinaryOperator.FloorDivideAssign:
				case BinaryOperator.LeftShiftAssign:
				case BinaryOperator.ModuloAssign:
				case BinaryOperator.MultiplyAssign:
				case BinaryOperator.OrAssign:
				case BinaryOperator.OrElseAssign:
				case BinaryOperator.RightShiftArithmeticAssign:
				case BinaryOperator.RightShiftLogicalAssign:
				case BinaryOperator.SubtractAssign:
					return Microsoft.Scripting.Ast.Utils.Void(Left.TransformWrite(TransformSimpleOperation(LanguageContext, Left.TransformRead(), Right.TransformRead(), ExpressionType)));
				default:
					throw Microsoft.Scripting.Utils.Assert.Unreachable;
			}
		}
	}

	public enum BinaryOperator
	{
		// Arithmetic & Logical
		Add,
		And,
		AndAlso,
		Divide,
		ExclusiveOr,
		FloorDivide,
		LeftShift,
		Modulo,
		Multiply,
		Or,
		OrElse,
		RightShiftArithmetic,
		RightShiftLogical,
		Subtract,

		// Comparison
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		Equal,
		NotEqual,
		DistinctEqual,
		DistinctNotEqual,

		// Special
		Assign,
		Exchange,
		InstanceOf,
		InContextOf,

		// InPlace
		AddAssign,
		AndAssign,
		AndAlsoAssign,
		DivideAssign,
		ExclusiveOrAssign,
		FloorDivideAssign,
		LeftShiftAssign,
		ModuloAssign,
		MultiplyAssign,
		OrAssign,
		OrElseAssign,
		RightShiftArithmeticAssign,
		RightShiftLogicalAssign,
		SubtractAssign,
	}
}
