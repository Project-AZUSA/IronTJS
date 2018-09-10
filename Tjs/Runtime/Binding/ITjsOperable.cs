using System.Dynamic;

namespace IronTjs.Runtime.Binding
{
    interface ITjsOperable
	{
		DynamicMetaObject BindOperation(TjsOperationBinder binder, DynamicMetaObject[] args);
	}
}
