using System;
using System.Linq.Expressions;
using Microsoft.Scripting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;

namespace IronTJS.Runtime
{
    class TjsScriptCode : ScriptCode
	{
		public TjsScriptCode(Expression<Func<object, object>> code, SourceUnit sourceUnit) : base(sourceUnit) { _code = code; }

		readonly Expression<Func<object, object>> _code;

		Func<object, object> _target;

		Func<object, object> Target
		{
			get
			{
				if (_target == null)
				{
					Func<object, object> compiledMethod;
					if (SourceUnit.LanguageContext.Options.NoAdaptiveCompilation)
						compiledMethod = _code.Compile();
					else
						compiledMethod = _code.LightCompile(SourceUnit.LanguageContext.Options.CompilationThreshold);
					System.Threading.Interlocked.CompareExchange(ref _target, compiledMethod, null);
				}
				return _target;
			}
		}

		public override object Run(Scope scope)
		{
			var extension = (TjsScopeExtension)scope.GetExtension(LanguageContext.ContextId);
			if (extension == null)
				scope.SetExtension(LanguageContext.ContextId, extension = new TjsScopeExtension(scope));
			return Target(extension.GlobalObject);
		}
	}
}
