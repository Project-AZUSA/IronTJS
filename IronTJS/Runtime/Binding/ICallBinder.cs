using Microsoft.Scripting.Actions;

namespace IronTJS.Runtime.Binding
{
    public interface ICallBinder
	{
		CallSignature Signature { get; }
	}
}
