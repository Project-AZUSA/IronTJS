using System.Collections.Generic;

namespace IronTJS.Compiler.Ast
{
    public class WhileStatement : Statement, ILoopStatement
	{
		public WhileStatement(Expression condition, Statement body)
		{
			Condition = condition;
			Body = body;
			Condition.Parent = Body.Parent = this;
		}

		System.Linq.Expressions.LabelTarget _breakLabel = System.Linq.Expressions.Expression.Label();

		public Expression Condition { get; private set; }

		public Statement Body { get; private set; }

		public System.Linq.Expressions.LabelTarget ContinueLabel { get; set; }

		public System.Linq.Expressions.LabelTarget BreakLabel { get { return _breakLabel; } }

		public override System.Linq.Expressions.Expression Transform()
		{
			var condition = Condition.TransformReadAsBoolean();
			var body = Body.Transform();
			return Microsoft.Scripting.Ast.Utils.While(condition, body, null, BreakLabel, ContinueLabel);
		}
	}

	public class DoWhileStatement : Statement, ILoopStatement
	{
		public DoWhileStatement(Statement body, Expression condition)
		{
			Body = body;
			Condition = condition;
			Body.Parent = Condition.Parent = this;
		}

		System.Linq.Expressions.LabelTarget _breakLabel = System.Linq.Expressions.Expression.Label();

		public Statement Body { get; private set; }

		public Expression Condition { get; private set; }

		public System.Linq.Expressions.LabelTarget ContinueLabel { get; set; }

		public System.Linq.Expressions.LabelTarget BreakLabel { get { return _breakLabel; } }

		public override System.Linq.Expressions.Expression Transform()
		{
			var body = Body.Transform();
			var condition = Condition.TransformReadAsBoolean();
			return System.Linq.Expressions.Expression.Loop(
				System.Linq.Expressions.Expression.Block(
					body,
					System.Linq.Expressions.Expression.IfThen(System.Linq.Expressions.Expression.Not(condition),
						System.Linq.Expressions.Expression.Break(BreakLabel)
					)
				),
				BreakLabel, ContinueLabel
			);
		}
	}

	public class ForStatement : Statement, ILoopStatement, INameResolver
	{
		public ForStatement(Expression initialization, Expression condition, Expression update, Statement body)
		{
			Initialization = initialization;
			Condition = condition;
			Update = update;
			Body = body;
			if (Initialization != null)
				Initialization.Parent = this;
			if (Condition != null)
				Condition.Parent = this;
			if (Update != null)
				Update.Parent = this;
			Body.Parent = this;
		}

		System.Linq.Expressions.LabelTarget _breakLabel = System.Linq.Expressions.Expression.Label();
		Dictionary<string, System.Linq.Expressions.ParameterExpression> variables = new Dictionary<string, System.Linq.Expressions.ParameterExpression>();

		public Expression Initialization { get; private set; }

		public Expression Condition { get; private set; }

		public Expression Update { get; private set; }

		public Statement Body { get; private set; }

		public System.Linq.Expressions.LabelTarget ContinueLabel { get; set; }

		public System.Linq.Expressions.LabelTarget BreakLabel { get { return _breakLabel; } }

		public System.Linq.Expressions.Expression ResolveForRead(string name, bool direct)
		{
			System.Linq.Expressions.ParameterExpression param;
			return variables.TryGetValue(name, out param) ? param : null;
		}

		public System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value, bool direct)
		{
			System.Linq.Expressions.ParameterExpression param;
			return variables.TryGetValue(name, out param) ? System.Linq.Expressions.Expression.Assign(param, value) : null;
		}

		public System.Linq.Expressions.Expression ResolveForDelete(string name) { return variables.ContainsKey(name) ? System.Linq.Expressions.Expression.Constant(0L) : null; }

		public System.Linq.Expressions.Expression DeclareVariable(string name, System.Linq.Expressions.Expression value)
		{
			System.Linq.Expressions.ParameterExpression param;
			if (!variables.TryGetValue(name, out param))
				variables[name] = param = System.Linq.Expressions.Expression.Variable(typeof(object), name);
			return System.Linq.Expressions.Expression.Assign(param, value);
		}

		public override System.Linq.Expressions.Expression Transform()
		{
			var init = Initialization != null ? Initialization.TransformVoid() : null;
			var cond = Condition != null ? Condition.TransformReadAsBoolean() : null;
			var update = Update != null ? Update.TransformVoid() : null;
			var body = Body.Transform();
			return System.Linq.Expressions.Expression.Block(variables.Values, init, Microsoft.Scripting.Ast.Utils.Loop(cond, update, body, null, BreakLabel, ContinueLabel));
		}
	}

	public interface IBreakableStatement
	{
		System.Linq.Expressions.LabelTarget BreakLabel { get; }
	}

	public interface ILoopStatement : IBreakableStatement
	{
		System.Linq.Expressions.LabelTarget ContinueLabel { get; set; }
	}

	public class BreakStatement : Statement
	{
		public override System.Linq.Expressions.Expression Transform()
		{
			var node = Parent;
			IBreakableStatement breakable = null;
			while ((breakable = node as IBreakableStatement) == null)
				node = node.Parent;
			return System.Linq.Expressions.Expression.Break(breakable.BreakLabel);
		}
	}

	public class ContinueStatement : Statement
	{
		public override System.Linq.Expressions.Expression Transform()
		{
			var node = Parent;
			ILoopStatement loop = null;
			while ((loop = node as ILoopStatement) == null)
				node = node.Parent;
			return System.Linq.Expressions.Expression.Continue(loop.ContinueLabel);
		}
	}
}
