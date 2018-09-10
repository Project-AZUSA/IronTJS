namespace IronTjs.Compiler.Ast
{
    public class ExpressionStatement : Statement
	{
		public ExpressionStatement(Expression expression)
		{
			Expression = expression;
			Expression.Parent = this;
		}

		public Expression Expression { get; private set; }

		public override System.Linq.Expressions.Expression Transform() { return Expression.TransformVoid(); }
	}
}
