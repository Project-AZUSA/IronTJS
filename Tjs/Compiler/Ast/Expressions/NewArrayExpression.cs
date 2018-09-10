using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
    public class NewArrayExpression : Expression
	{
		public NewArrayExpression(IEnumerable<Expression> expressions)
		{
			Expressions = expressions.ToReadOnly();
			foreach (var exp in Expressions)
				exp.Parent = this;
		}

		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			return System.Linq.Expressions.Expression.New(
				typeof(Builtins.Array).GetConstructor(new[] { typeof(IEnumerable<object>) }),
				System.Linq.Expressions.Expression.NewArrayInit(typeof(object), Expressions.Select(x => x.TransformRead()))
			);
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Block(Expressions.Select(x => x.TransformVoid())); }
	}
}
