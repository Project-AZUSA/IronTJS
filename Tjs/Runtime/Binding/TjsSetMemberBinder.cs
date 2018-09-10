using System.Dynamic;

namespace IronTjs.Runtime.Binding
{
    using AstUtils = Microsoft.Scripting.Ast.Utils;

    class TjsSetMemberBinder : SetMemberBinder, IForceMemberCreatable, IDirectAccessible
	{
		public TjsSetMemberBinder(TjsContext context, string name, bool ignoreCase, bool forceCreate, bool direct) : base(name, ignoreCase)
		{
			Context = context;
			ForceCreate = forceCreate;
			DirectAccess = direct;
		}

		public TjsContext Context { get; private set; }

		public bool ForceCreate { get; private set; }

		public bool DirectAccess { get; private set; }

		public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			var result = Context.Binder.SetMember(Name, target, value, errorSuggestion, new TjsOverloadResolverFactory(Context.Binder));
			if (result.Expression.Type.IsValueType)
				result = new DynamicMetaObject(AstUtils.Convert(result.Expression, typeof(object)), result.Restrictions);
			return result;
		}
	}
}
