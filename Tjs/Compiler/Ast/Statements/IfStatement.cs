namespace IronTjs.Compiler.Ast
{
    public class IfStatement : Statement
	{
		public IfStatement(Expression condition, Statement ifTrue, Statement ifFalse)
		{
			Condition = condition;
			IfTrue = ifTrue;
			IfFalse = ifFalse;
			Condition.Parent = IfTrue.Parent = this;
			if (IfFalse != null)
				IfFalse.Parent = this;
		}

		public Expression Condition { get; private set; }

		public Statement IfTrue { get; private set; }

		public Statement IfFalse { get; private set; }

		public override System.Linq.Expressions.Expression Transform()
		{
			var condition = Condition.TransformReadAsBoolean();
			var ifTrue = IfTrue.Transform();
			var ifFalse = IfFalse != null ? IfFalse.Transform() : System.Linq.Expressions.Expression.Empty();
			return System.Linq.Expressions.Expression.IfThenElse(condition, ifTrue, ifFalse);
		}
	}
}
