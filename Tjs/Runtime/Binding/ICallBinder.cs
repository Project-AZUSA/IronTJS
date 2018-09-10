using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
    public interface ICallBinder
	{
		CallSignature Signature { get; }
	}
}
