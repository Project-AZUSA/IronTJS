using System.Collections.Generic;
using System.Dynamic;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Runtime.Binding
{
    sealed class TjsOverloadResolverFactory : OverloadResolverFactory
	{
		public TjsOverloadResolverFactory(TjsBinder binder) { _binder = binder; }

		readonly TjsBinder _binder;

		public override DefaultOverloadResolver CreateOverloadResolver(IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType) { return new TjsOverloadResolver(_binder, args, signature, callType); }
	}

	public sealed class TjsOverloadResolver : DefaultOverloadResolver
	{
		public TjsOverloadResolver(TjsBinder binder, IList<DynamicMetaObject> args, CallSignature signature, CallTypes callType) : base(binder, args, signature, callType) { }
	}
}
