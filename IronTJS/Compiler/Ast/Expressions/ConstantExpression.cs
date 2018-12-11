namespace IronTJS.Compiler.Ast
{
    public class ConstantExpression : Expression
	{
		public ConstantExpression(object value) { Value = value; }

		public object Value { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead() { return System.Linq.Expressions.Expression.Constant(Value, typeof(object)); }

		public override System.Linq.Expressions.Expression TransformVoid() { return System.Linq.Expressions.Expression.Empty(); }
	}
}
