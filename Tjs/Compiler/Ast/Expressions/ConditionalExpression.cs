namespace IronTjs.Compiler.Ast
{
    public class ConditionalExpression : Expression
	{
		public ConditionalExpression(Expression condition, Expression ifTrue, Expression ifFalse)
		{
			Condition = condition;
			IfTrue = ifTrue;
			IfFalse = ifFalse;
			Condition.Parent = IfTrue.Parent = IfFalse.Parent = this;
		}

		public Expression Condition { get; private set; }

		public Expression IfTrue { get; private set; }

		public Expression IfFalse { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead() { return System.Linq.Expressions.Expression.Condition(Condition.TransformReadAsBoolean(), IfTrue.TransformRead(), IfFalse.TransformRead()); }

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value)
		{
			var v = System.Linq.Expressions.Expression.Variable(typeof(object));
			return System.Linq.Expressions.Expression.Block(new[] { v },
				System.Linq.Expressions.Expression.Assign(v, value),
				System.Linq.Expressions.Expression.Condition(Condition.TransformReadAsBoolean(),
					IfTrue.TransformWrite(v),
					IfFalse.TransformWrite(v)
				)
			);
		}

		public override System.Linq.Expressions.Expression TransformDelete() { return System.Linq.Expressions.Expression.Condition(Condition.TransformReadAsBoolean(), IfTrue.TransformDelete(), IfFalse.TransformDelete()); }

		public override System.Linq.Expressions.Expression TransformGetProperty() { return System.Linq.Expressions.Expression.Condition(Condition.TransformReadAsBoolean(), IfTrue.TransformGetProperty(), IfFalse.TransformGetProperty()); }

		public override System.Linq.Expressions.Expression TransformSetProperty(System.Linq.Expressions.Expression value)
		{
			var v = System.Linq.Expressions.Expression.Variable(typeof(object));
			return System.Linq.Expressions.Expression.Block(new[] { v },
				System.Linq.Expressions.Expression.Assign(v, value),
				System.Linq.Expressions.Expression.Condition(Condition.TransformReadAsBoolean(),
					IfTrue.TransformSetProperty(v),
					IfFalse.TransformSetProperty(v)
				)
			);
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.IfThenElse(Condition.TransformReadAsBoolean(), IfTrue.TransformVoid(), IfFalse.TransformVoid()); }
	}
}
