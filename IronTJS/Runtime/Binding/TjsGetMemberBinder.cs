using System.Dynamic;

namespace IronTJS.Runtime.Binding
{
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    class TjsGetMemberBinder : GetMemberBinder, IDirectAccessible
	{
		public TjsGetMemberBinder(TjsContext context, string name, bool ignoreCase, bool direct) : base(name, ignoreCase)
		{
			Context = context;
			DirectAccess = direct;
		}

		public TjsContext Context { get; private set; }

		public bool DirectAccess { get; private set; }

		public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			DynamicMetaObject result;
			if (DirectAccess)
				result = Context.Binder.GetMemberDirect(Name, target, new TjsOverloadResolverFactory(Context.Binder), false, errorSuggestion);
			else
				result = Context.Binder.GetMember(Name, target, new TjsOverloadResolverFactory(Context.Binder), false, errorSuggestion);
			if (result.Expression.Type.IsValueType)
				result = new DynamicMetaObject(AstUtils.Convert(result.Expression, typeof(object)), result.Restrictions);
			return result;
		}
	}
}
