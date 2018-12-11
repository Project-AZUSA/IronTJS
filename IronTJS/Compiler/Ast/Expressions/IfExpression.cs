using System;

namespace IronTJS.Compiler.Ast
{
    public class IfExpression : Expression
	{
		public IfExpression(Expression body, Expression condition)
		{
			Body = body;
			Condition = condition;
			Body.Parent = Condition.Parent = this;
		}

		public Expression Body { get; private set; }

		public Expression Condition { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead() { throw new InvalidOperationException("if 式の結果を得ることはできません。"); }

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.IfThen(Condition.TransformReadAsBoolean(), Body.TransformVoid()); }
	}
}
