using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace IronTjs.Runtime.Binding
{
    class TjsDeleteMemberBinder : DynamicMetaObjectBinder
	{
		public TjsDeleteMemberBinder(TjsContext context, string name, bool ignoreCase)
		{
			_context = context;
			Name = name;
			IgnoreCase = ignoreCase;
		}

		readonly TjsContext _context;

		public string Name { get; private set; }

		public bool IgnoreCase { get; private set; }

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			return Bind(target, new CompatibilityDeleteMemberBinder(_context, Name, IgnoreCase));
		}

		internal static DynamicMetaObject Bind(DynamicMetaObject target, DeleteMemberBinder binder)
		{
			var obj = target.BindDeleteMember(binder);
			return new DynamicMetaObject(Microsoft.Scripting.Ast.Utils.Try(
				obj.Expression,
				Expression.Constant(1L, typeof(object))
			).Catch(typeof(MissingMemberException),
				Expression.Constant(0L, typeof(object))
			), obj.Restrictions);
		}
	}

	class CompatibilityDeleteMemberBinder : DeleteMemberBinder
	{
		public CompatibilityDeleteMemberBinder(TjsContext context, string name, bool ignoreCase) : base(name, ignoreCase) { Context = context; }

		public TjsContext Context { get; private set; }

		public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
		{
			return Context.Binder.DeleteMember(Name, target, new TjsOverloadResolverFactory(Context.Binder), errorSuggestion);
		}
	}
}
