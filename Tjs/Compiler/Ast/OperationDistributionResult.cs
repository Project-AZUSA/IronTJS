namespace IronTjs.Compiler.Ast
{
    struct OperationDistributionResult
	{
		public OperationDistributionResult(System.Linq.Expressions.ExpressionType type) : this()
		{
			ExpressionType = type;
			IsStandardOperation = true;
		}

		public OperationDistributionResult(IronTjs.Runtime.Binding.TjsOperationKind kind) : this()
		{
			OperationKind = kind;
			IsStandardOperation = false;
		}

		public System.Linq.Expressions.ExpressionType ExpressionType { get; private set; }

		public IronTjs.Runtime.Binding.TjsOperationKind OperationKind { get; private set; }

		public bool IsStandardOperation { get; private set; }
	}
}
