using System;

namespace IronTJS.Compiler.Ast
{
    public class TryStatement : Statement
	{
		public TryStatement(Statement body, CatchBlock catchBlock)
		{
			Body = body;
			CatchBlock = catchBlock;
			Body.Parent = CatchBlock.Parent = this;
		}

		public Statement Body { get; private set; }

		public CatchBlock CatchBlock { get; private set; }

		public override System.Linq.Expressions.Expression Transform()
		{
			var body = Body.Transform();
			var catchBlock = CatchBlock.Transform();
			return System.Linq.Expressions.Expression.TryCatch(body, catchBlock);
		}
	}

	public class CatchBlock : Node, INameResolver
	{
		public CatchBlock(string variableName, Statement body)
		{
			Variable = System.Linq.Expressions.Expression.Variable(typeof(Exception), variableName);
			Body = body;
			Body.Parent = this;
		}

		public System.Linq.Expressions.ParameterExpression Variable { get; private set; }

		public Statement Body { get; private set; }

		public System.Linq.Expressions.CatchBlock Transform()
		{
			var body = Body.Transform();
			return System.Linq.Expressions.Expression.Catch(Variable, body);
		}

		public System.Linq.Expressions.Expression ResolveForRead(string name, bool direct) { return name == Variable.Name ? Variable : null; }

		public System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value, bool direct) { return null; }

		public System.Linq.Expressions.Expression ResolveForDelete(string name) { return Variable.Name == name ? System.Linq.Expressions.Expression.Constant(0L) : null; }

		public System.Linq.Expressions.Expression DeclareVariable(string name, System.Linq.Expressions.Expression value) { return null; }
	}
}
