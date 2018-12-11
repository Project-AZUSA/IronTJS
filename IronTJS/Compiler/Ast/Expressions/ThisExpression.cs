namespace IronTJS.Compiler.Ast
{
    public class ThisExpression : Expression
	{
		public override System.Linq.Expressions.Expression TransformRead()
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				var holder = node as IContextHolder;
				if (holder != null)
					return holder.Context;
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
