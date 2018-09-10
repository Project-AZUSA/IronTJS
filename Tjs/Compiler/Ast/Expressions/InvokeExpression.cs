using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
    public class InvokeExpression : Expression
	{
		public InvokeExpression(Expression target, IEnumerable<InvocationArgument> arguments, bool inheritArguments)
		{
			Target = target;
			Arguments = arguments.ToReadOnly();
			InheritArguments = inheritArguments;
			Target.Parent = this;
			foreach (var arg in Arguments)
				arg.Parent = this;
		}

		public Expression Target { get; private set; }

		public ReadOnlyCollection<InvocationArgument> Arguments { get; private set; }

		public bool InheritArguments { get; private set; }

		public Tuple<IEnumerable<System.Linq.Expressions.Expression>, CallSignature> TransformTargetAndArguments()
		{
			var targetExp = Target.TransformRead();
			if (InheritArguments)
			{
				var func = SearchFunction(Parent);
				if (func != null)
					return new Tuple<IEnumerable<System.Linq.Expressions.Expression>, CallSignature>(
						new[] { targetExp, func.Arguments },
						new CallSignature(ArgumentType.List)
					);
				else
					throw new Microsoft.Scripting.SyntaxErrorException("引数の省略が使用されましたが、このコードを含む関数が見つかりません。");
			}
			else
				return new Tuple<IEnumerable<System.Linq.Expressions.Expression>, CallSignature>(
					Enumerable.Repeat(targetExp, 1).Concat(Arguments.Select(x => x.Transform())),
					new CallSignature(Arguments.Select(x => x.IsSpread ? ArgumentType.List : ArgumentType.Simple).ToArray())
				);
		}

		public override System.Linq.Expressions.Expression TransformRead()
		{
			var args = TransformTargetAndArguments();
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateInvokeBinder(args.Item2), typeof(object), args.Item1);
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return Microsoft.Scripting.Ast.Utils.Void(TransformRead()); }

		internal static FunctionDefinition SearchFunction(Node node)
		{
			while (node != null)
			{
				var func = node as FunctionDefinition;
				if (func != null)
					return func;
				node = node.Parent;
			}
			return null;
		}
	}

	public class InvocationArgument : Node
	{
		public InvocationArgument(Expression value, bool spread)
		{
			Value = value;
			IsSpread = spread;
			if (Value != null)
				Value.Parent = this;
		}

		public Expression Value { get; private set; }

		public bool IsSpread { get; private set; }

		public System.Linq.Expressions.Expression Transform()
		{
			if (Value != null)
				return Value.TransformRead();
			else if (IsSpread) // 名前のない *
			{
				var func = InvokeExpression.SearchFunction(Parent);
				if (func != null)
				{
					var unnamedParam = func.Parameters.FirstOrDefault(x => x.ParameterVariable.Name == null);
					if (unnamedParam.ExpandToArray)
						return unnamedParam.ParameterVariable;
					throw new Microsoft.Scripting.SyntaxErrorException("名前のない * が使用されましたが、このコードを含む関数に名前のない * の引数定義がありません。");
				}
				throw new Microsoft.Scripting.SyntaxErrorException("名前のない * が使用されましたが、このコードを含む関数が見つかりません。");
			}
			else
				return System.Linq.Expressions.Expression.Constant(Builtins.Void.Value);
		}
	}
}
