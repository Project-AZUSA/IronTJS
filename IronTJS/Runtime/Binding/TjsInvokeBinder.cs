using System.Dynamic;
using Microsoft.Scripting.Actions;

namespace IronTJS.Runtime.Binding
{
    class TjsInvokeBinder : InvokeBinder, ICallBinder
    {
        public TjsInvokeBinder(TjsContext context, CallSignature signature) : base(Binders.GetCallInfoForCallSignature(signature))
        {
            _context = context;
            Signature = signature;
        }

        readonly TjsContext _context;

        public CallSignature Signature { get; private set; }
        
        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            return _context.Binder.Call(
                Signature,
                errorSuggestion,
                new TjsOverloadResolverFactory(_context.Binder),
                target,
                args
            );
        }
    }
}
