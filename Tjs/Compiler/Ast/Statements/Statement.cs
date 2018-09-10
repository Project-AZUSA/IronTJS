namespace IronTjs.Compiler.Ast
{
    public abstract class Statement : Node
	{
		public abstract System.Linq.Expressions.Expression Transform();
	}
}
