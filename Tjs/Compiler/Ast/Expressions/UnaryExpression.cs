using System;
using IronTjs.Runtime.Binding;

namespace IronTjs.Compiler.Ast
{
    public class UnaryExpression : Expression
	{
		public UnaryExpression(Expression operand, UnaryOperator expressionType)
		{
			Operand = operand;
			ExpressionType = expressionType;
			Operand.Parent = this;
		}

		public Expression Operand { get; private set; }

		public UnaryOperator ExpressionType { get; private set; }

		static OperationDistributionResult DistributeOperation(UnaryOperator unary)
		{
			switch (unary)
			{
				case UnaryOperator.CharCodeToChar:
					return new OperationDistributionResult(TjsOperationKind.CharCodeToChar);
				case UnaryOperator.CharToCharCode:
					return new OperationDistributionResult(TjsOperationKind.CharToCharCode);
				case UnaryOperator.Evaluate:
					return new OperationDistributionResult(TjsOperationKind.Evaluate);
				case UnaryOperator.Invalidate:
					return new OperationDistributionResult(TjsOperationKind.Invalidate);
				case UnaryOperator.InvokePropertyHandler:
					return new OperationDistributionResult(TjsOperationKind.InvokePropertyHandler);
				case UnaryOperator.IsValid:
					return new OperationDistributionResult(TjsOperationKind.IsValid);
				case UnaryOperator.Negate:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Negate);
				case UnaryOperator.Not:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Not);
				case UnaryOperator.OnesComplement:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.OnesComplement);
				case UnaryOperator.PostDecrementAssign:
				case UnaryOperator.PreDecrementAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Decrement);
				case UnaryOperator.PostIncrementAssign:
				case UnaryOperator.PreIncrementAssign:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.Increment);
				case UnaryOperator.TypeOf:
					return new OperationDistributionResult(TjsOperationKind.TypeOf);
				case UnaryOperator.UnaryPlus:
					return new OperationDistributionResult(System.Linq.Expressions.ExpressionType.UnaryPlus);
				default:
					throw new ArgumentException("演算子に対応する動的操作が存在しません。");
			}
		}

		static System.Linq.Expressions.Expression DoUnaryOperation(Runtime.TjsContext context, UnaryOperator operation, params System.Linq.Expressions.Expression[] args)
		{
			var res = DistributeOperation(operation);
			if (res.IsStandardOperation)
				return System.Linq.Expressions.Expression.Dynamic(context.CreateUnaryOperationBinder(res.ExpressionType), typeof(object), args);
			else
				return System.Linq.Expressions.Expression.Dynamic(context.CreateOperationBinder(res.OperationKind), typeof(object), args);
		}

		public override System.Linq.Expressions.Expression TransformRead()
		{
			if (ExpressionType == UnaryOperator.New)
			{
				var ie = Operand as InvokeExpression;
				if (ie != null)
					return System.Linq.Expressions.Expression.Dynamic(
						LanguageContext.CreateCreateBinder(new System.Dynamic.CallInfo(ie.Arguments.Count)),
						typeof(object),
						ie.TransformTargetAndArguments().Item1
					);
				else
					throw new InvalidOperationException("new 演算子を適用できるのは関数呼び出しのみです。");
			}
			if (ExpressionType == UnaryOperator.Delete)
				return Operand.TransformDelete();
			if (ExpressionType == UnaryOperator.AccessPropertyObject)
				return Operand.TransformGetProperty();
			var target = Operand.TransformRead();
			if (ExpressionType == UnaryOperator.PostDecrementAssign || ExpressionType == UnaryOperator.PostIncrementAssign)
			{
				var v = System.Linq.Expressions.Expression.Variable(typeof(object));
				return System.Linq.Expressions.Expression.Block(new[] { v },
					System.Linq.Expressions.Expression.Assign(v, target),
					Operand.TransformWrite(DoUnaryOperation(LanguageContext, ExpressionType, v)),
					v
				);
			}
			else
			{
				var exp = DoUnaryOperation(LanguageContext, ExpressionType, target);
				if (ExpressionType == UnaryOperator.PreIncrementAssign || ExpressionType == UnaryOperator.PreDecrementAssign)
					return Operand.TransformWrite(exp);
				return exp;
			}
		}

		// & * は代入可能
		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			switch (ExpressionType)
			{
				case UnaryOperator.AccessPropertyObject:
					return Operand.TransformSetProperty(value);
				case UnaryOperator.InvokePropertyHandler:
					return DoUnaryOperation(LanguageContext, ExpressionType, Operand.TransformRead(), value);
				default:
					throw new InvalidOperationException(string.Format("単項演算子 '{0}' は左辺値になることはできません。", ExpressionType));
			}
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			switch (ExpressionType)
			{
				case UnaryOperator.Delete:
					return Microsoft.Scripting.Ast.Utils.Void(Operand.TransformDelete());
				case UnaryOperator.CharCodeToChar:
				case UnaryOperator.CharToCharCode:
				case UnaryOperator.InvokePropertyHandler:
				case UnaryOperator.IsValid:
				case UnaryOperator.Negate:
				case UnaryOperator.Not:
				case UnaryOperator.OnesComplement:
				case UnaryOperator.AccessPropertyObject:
				case UnaryOperator.TypeOf:
				case UnaryOperator.UnaryPlus:
					return System.Linq.Expressions.Expression.Empty();
			}
			var exp = DoUnaryOperation(LanguageContext, ExpressionType, Operand.TransformRead());
			if (ExpressionType == UnaryOperator.PostDecrementAssign || ExpressionType == UnaryOperator.PostIncrementAssign ||
				ExpressionType == UnaryOperator.PreDecrementAssign || ExpressionType == UnaryOperator.PreIncrementAssign)
				return Microsoft.Scripting.Ast.Utils.Void(Operand.TransformWrite(exp));
			return exp;
		}
	}

	public enum UnaryOperator
	{
		// Unary
		AccessPropertyObject,
		CharCodeToChar,
		CharToCharCode,
		Delete,
		Evaluate,
		Invalidate,
		InvokePropertyHandler,
		IsValid,
		Negate,
		New,
		Not,
		OnesComplement,
		TypeOf,
		UnaryPlus,

		// Unary (Composite)
		PostDecrementAssign,
		PostIncrementAssign,
		PreDecrementAssign,
		PreIncrementAssign,
	}
}
