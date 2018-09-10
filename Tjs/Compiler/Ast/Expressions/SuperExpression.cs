using System;

namespace IronTjs.Compiler.Ast
{
    public class SuperExpression : Expression
	{
		public override System.Linq.Expressions.Expression TransformRead()
		{
			throw new NotImplementedException();
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
