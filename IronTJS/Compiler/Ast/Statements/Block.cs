using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace IronTJS.Compiler.Ast
{
    public class Block : Statement, INameResolver
	{
		public Block(IEnumerable<Statement> statements)
		{
			Statements = statements.ToReadOnly();
			foreach (var statement in statements)
				statement.Parent = this;
		}

		public ReadOnlyCollection<Statement> Statements { get; private set; }

		Dictionary<string, System.Linq.Expressions.ParameterExpression> variables = new Dictionary<string, System.Linq.Expressions.ParameterExpression>();

		public override System.Linq.Expressions.Expression Transform()
		{
			var transformed = Statements.Select(x => x.Transform()).ToArray();
			if (transformed.Length == 0)
				return System.Linq.Expressions.Expression.Empty();
			if (transformed.Length == 1 && variables.Count == 0)
				return transformed[0];
			return System.Linq.Expressions.Expression.Block(variables.Values, transformed);
		}

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
	}
}
