namespace IronTJS.Compiler.Ast
{
    public class ThrowStatement : Statement
	{
		public ThrowStatement(Expression expression)
		{
			Expression = expression;
			Expression.Parent = this;
		}

		public Expression Expression { get; private set; }

		public override System.Linq.Expressions.Expression Transform()
		{
			var exp = Expression.TransformRead();
			return System.Linq.Expressions.Expression.Throw(exp);
		}
	}
}
