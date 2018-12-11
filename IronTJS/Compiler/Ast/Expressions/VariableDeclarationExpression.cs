using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Utils;

namespace IronTJS.Compiler.Ast
{
    public class VariableDeclarationExpression : Expression
	{
		public VariableDeclarationExpression(IEnumerable<KeyValuePair<string, Expression>> initializers)
		{
			Initializers = initializers.ToReadOnly();
			foreach (var init in Initializers)
			{
				if (init.Value != null)
					init.Value.Parent = this;
			}
		}

		public ReadOnlyCollection<KeyValuePair<string, Expression>> Initializers { get; private set; }

		List<System.Linq.Expressions.Expression> TransformInternal()
		{
			INameResolver resolver = null;
			Node node = Parent;
			while ((resolver = node as INameResolver) == null)
				node = node.Parent;
			System.Linq.Expressions.Expression exp;
			if (resolver == null)
				throw new InvalidOperationException("変数を宣言できるスコープが見つかりません。");
			List<System.Linq.Expressions.Expression> exps = new List<System.Linq.Expressions.Expression>();
			foreach (var initializer in Initializers)
			{
				if (initializer.Value != null)
					exp = resolver.DeclareVariable(initializer.Key, initializer.Value.TransformRead());
				else
					exp = resolver.DeclareVariable(initializer.Key, System.Linq.Expressions.Expression.Constant(Builtins.Void.Value));
				if (exp == null)
					throw new InvalidOperationException(string.Format("スコープに変数 \"{0}\" を宣言できません。", initializer.Key));
				exps.Add(exp);
			}
			return exps;
		}

		public override System.Linq.Expressions.Expression TransformRead()
		{
			var exps = TransformInternal();
			if (exps.Count == 1)
				return exps[0];
			else
				return System.Linq.Expressions.Expression.Block(exps);
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			var exps = TransformInternal();
			exps.Add(System.Linq.Expressions.Expression.Empty());
			return System.Linq.Expressions.Expression.Block(exps);
		}
	}
}
