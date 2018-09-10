namespace IronTjs.Compiler.Ast
{
    public interface IContextHolder
	{
		System.Linq.Expressions.Expression Context { get; }

		System.Linq.Expressions.Expression GlobalContext { get; }
	}
}
