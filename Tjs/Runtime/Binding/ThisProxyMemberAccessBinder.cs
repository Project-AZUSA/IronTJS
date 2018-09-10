using System;
using System.Dynamic;

namespace IronTjs.Runtime.Binding
{
    class ThisProxyMemberAccessBinder : DynamicMetaObjectBinder
	{
		public ThisProxyMemberAccessBinder(TjsContext context, string name, bool ignoreCase, MemberAccessKind accessKind)
		{
			_context = context;
			_name = name;
			_ignoreCase = ignoreCase;
			_accessKind = accessKind;
		}

		readonly TjsContext _context;
		readonly string _name;
		readonly bool _ignoreCase;
		readonly MemberAccessKind _accessKind;

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			switch (_accessKind & MemberAccessKind.AccessMask)
			{
				case MemberAccessKind.Get:
					return target.BindGetMember(new GetMemberBinderImpl(_context, _name, _ignoreCase, (_accessKind & MemberAccessKind.Direct) != 0, args[0]));
				case MemberAccessKind.Set:
					return target.BindSetMember(
						new SetMemberBinderImpl(
							_context, _name, _ignoreCase,
							(_accessKind & MemberAccessKind.Creatable) != 0,
							(_accessKind & MemberAccessKind.Direct) != 0,
							args[0]
						), args[1]
					);
				default:
					return TjsDeleteMemberBinder.Bind(target, new DeleteMemberBinderImpl(_context, _name, _ignoreCase, args[0]));
			}
		}

		class GetMemberBinderImpl : TjsGetMemberBinder
		{
			public GetMemberBinderImpl(TjsContext context, string name, bool ignoreCase, bool direct, DynamicMetaObject fallback) : base(context, name, ignoreCase, direct)
			{
				_fallback = fallback;
			}

			readonly DynamicMetaObject _fallback;

			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				return base.FallbackGetMember(target, _fallback.BindGetMember(Context.CreateGetMemberBinder(Name, IgnoreCase, DirectAccess)));
			}
		}

		class SetMemberBinderImpl : TjsSetMemberBinder
		{
			public SetMemberBinderImpl(TjsContext context, string name, bool ignoreCase, bool forceCreate, bool direct, DynamicMetaObject fallback)
				: base(context, name, ignoreCase, forceCreate, direct)
			{
				_fallback = fallback;
			}

			readonly DynamicMetaObject _fallback;

			public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
			{
				return base.FallbackSetMember(target, value, _fallback.BindSetMember(Context.CreateSetMemberBinder(Name, IgnoreCase, ForceCreate, DirectAccess), value));
			}
		}

		class DeleteMemberBinderImpl : CompatibilityDeleteMemberBinder
		{
			public DeleteMemberBinderImpl(TjsContext context, string name, bool ignoreCase, DynamicMetaObject fallback) : base(context, name, ignoreCase)
			{
				_fallback = fallback;
			}

			readonly DynamicMetaObject _fallback;

			public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
			{
				return base.FallbackDeleteMember(target, _fallback.BindDeleteMember(new CompatibilityDeleteMemberBinder(Context, Name, IgnoreCase)));
			}
		}
	}

	[Flags]
	enum MemberAccessKind
	{
		Get = 0x00,
		Set = 0x01,
		Delete = 0x02,
		Creatable = 0x04,
		Direct = 0x08,
		AccessMask = 0x03,
	}
}
