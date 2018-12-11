namespace IronTJS.Compiler.Ast
{
    public class IdentifierExpression : Expression
	{
		public IdentifierExpression(string identifier) { Identifier = identifier; }

		public string Identifier { get; private set; }

		System.Linq.Expressions.Expression TransformReadInternal(bool direct)
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				INameResolver resolver;
				if ((resolver = node as INameResolver) != null)
				{
					var exp = resolver.ResolveForRead(Identifier, direct);
					if (exp != null)
						return exp;
				}
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		System.Linq.Expressions.Expression TransformWriteInternal(System.Linq.Expressions.Expression value, bool direct)
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				INameResolver resolver;
				if ((resolver = node as INameResolver) != null)
				{
					var exp = resolver.ResolveForWrite(Identifier, value, direct);
					if (exp != null)
						return exp;
				}
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		public override System.Linq.Expressions.Expression TransformRead() { return TransformReadInternal(false); }

		public override System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { return TransformWriteInternal(value, false); }

		public override System.Linq.Expressions.Expression TransformDelete()
		{
			for (var node = Parent; node != null; node = node.Parent)
			{
				var resolver = node as INameResolver;
				if (resolver != null)
				{
					var exp = resolver.ResolveForDelete(Identifier);
					if (exp != null)
						return exp;
				}
			}
			throw Microsoft.Scripting.Utils.Assert.Unreachable;
		}

		public override System.Linq.Expressions.Expression TransformGetProperty() { return TransformReadInternal(true); }

		public override System.Linq.Expressions.Expression TransformSetProperty(System.Linq.Expressions.Expression value) { return TransformWriteInternal(value, true); }

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
