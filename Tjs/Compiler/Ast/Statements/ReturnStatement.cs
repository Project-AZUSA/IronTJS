namespace IronTjs.Compiler.Ast
{
    public class ReturnStatement : Statement
	{
		public ReturnStatement(Expression expression)
		{
			Expression = expression;
			if (Expression != null)
				Expression.Parent = this;
		}

		public Expression Expression { get; private set; }

		public override System.Linq.Expressions.Expression Transform()
		{
			var exp = Expression != null ? Expression.TransformRead() : System.Linq.Expressions.Expression.Constant(IronTjs.Builtins.Void.Value);
			var node = Parent;
			FunctionDefinition function = null;
			while ((function = node as FunctionDefinition) == null)
				node = node.Parent;
			return System.Linq.Expressions.Expression.Return(function.ReturnLabel, exp);
		}
	}
}
