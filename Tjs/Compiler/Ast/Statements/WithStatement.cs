namespace IronTjs.Compiler.Ast
{
    public class WithStatement : Statement
	{
		public WithStatement(Expression objectExpression, Statement body)
		{
			ObjectExpression = objectExpression;
			Body = body;
			ObjectExpression.Parent = Body.Parent = this;
		}

		System.Linq.Expressions.ParameterExpression accessibleVariable = System.Linq.Expressions.Expression.Parameter(typeof(object));

		public Expression ObjectExpression { get; private set; }

		public Statement Body { get; private set; }

		public System.Linq.Expressions.ParameterExpression AccessibleVariable { get { return accessibleVariable; } }

		public override System.Linq.Expressions.Expression Transform()
		{
			var objExp = ObjectExpression.TransformRead();
			var body = Body.Transform();
			return System.Linq.Expressions.Expression.Block(new[] { accessibleVariable },
				System.Linq.Expressions.Expression.Assign(accessibleVariable, objExp),
				body
			);
		}
	}
}
