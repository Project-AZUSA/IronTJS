using System.Diagnostics;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Runtime
{
    class TjsScopeExtension : ScopeExtension
	{
		public TjsScopeExtension(Scope scope) : base(scope)
		{
			Debug.Assert(scope.Storage is ScopeStorage);
			GlobalObject = new TjsScopeStorage(((ScopeStorage)scope.Storage).GetItems());
		}

		public TjsScopeStorage GlobalObject { get; private set; }
	}
}
