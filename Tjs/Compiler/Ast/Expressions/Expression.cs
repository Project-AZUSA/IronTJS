using System;

namespace IronTjs.Compiler.Ast
{
    public abstract class Expression : Node
	{
		// Type: typeof(object)
		public abstract System.Linq.Expressions.Expression TransformRead();

		// Type: typeof(bool)
		public System.Linq.Expressions.Expression TransformReadAsBoolean() { return Runtime.Binding.Binders.Convert(LanguageContext, TransformRead(), typeof(bool)); }

		// Type: typeof(object)
		public virtual System.Linq.Expressions.Expression TransformWrite(System.Linq.Expressions.Expression value) { throw new InvalidOperationException(string.Format("式 {0} は左辺値となることはできません。", GetType())); }

		// Type: typeof(object)
		public virtual System.Linq.Expressions.Expression TransformDelete() { throw new InvalidOperationException(string.Format("式 {0} に対して delete 演算を適用することはできません。", GetType())); }

		// Type: typeof(object)
		public virtual System.Linq.Expressions.Expression TransformGetProperty() { throw new InvalidOperationException(string.Format("式 {0} に対して & 演算を適用することはできません。", GetType())); }

		// Type: typeof(object)
		public virtual System.Linq.Expressions.Expression TransformSetProperty(System.Linq.Expressions.Expression value) { throw new InvalidOperationException(string.Format("式 {0} に対して & 演算を適用することはできません。", GetType())); }

		// Type: typeof(void)
		public abstract System.Linq.Expressions.Expression TransformVoid();
	}
}
