using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace IronTJS.Compiler.Ast
{
    public class SequenceExpression : Expression
	{
		public SequenceExpression(IEnumerable<Expression> expressions)
		{
			Expressions = expressions.ToReadOnly();
			foreach (var exp in Expressions)
				exp.Parent = this;
		}

		public ReadOnlyCollection<Expression> Expressions { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			var transformed = Expressions.Take(Expressions.Count - 1).Select(x => x.TransformVoid()).ToArray();
			var transformedLast = Expressions.Last().TransformRead();
			return System.Linq.Expressions.Expression.Block(transformed.Concat(Enumerable.Repeat(transformedLast, 1)));
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			var transformed = Expressions.Select(x => x.TransformVoid()).ToArray();
			return System.Linq.Expressions.Expression.Block(transformed);
		}
	}
}
