using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace IronTJS.Runtime.Binding
{
    sealed class TjsOverloadResolverFactory : OverloadResolverFactory
    {
        public TjsOverloadResolverFactory(TjsBinder binder) { _binder = binder; }

        readonly TjsBinder _binder;

        public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType)
        {
            if (signature.ArgumentCount < args.Count && callType != CallTypes.ImplicitInstance) //FIX: when CallTypes != ImplicitInstance, remove the first arg (self)
            {
                //The self arg is added in `DefaultBinder.GetTargetInfo`
                args = new List<DynamicMetaObject>(args.Skip(1));
            }
            return new TjsOverloadResolver(_binder, args, signature, callType);
        }
    }

    public sealed class TjsOverloadResolver : DefaultOverloadResolver
    {
        public TjsOverloadResolver(TjsBinder binder, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType) : base(binder, args, signature, callType) { }
    }
}
