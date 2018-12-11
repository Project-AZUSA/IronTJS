using System.Dynamic;

namespace IronTJS.Runtime.Binding
{
    interface ITjsOperable
	{
		DynamicMetaObject BindOperation(TjsOperationBinder binder, DynamicMetaObject[] args);
	}
}
