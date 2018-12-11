using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IronTJS.Compiler;
using IronTJS.Runtime.Binding;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronTJS.Runtime
{
    public sealed class TjsContext : LanguageContext
    {
        public TjsContext(ScriptDomainManager manager, IDictionary<string, object> options) : base(manager)
        {
            Binder = new TjsBinder();
            DefaultContext.InitializeDefaults(this);
        }

        public static string DisplayName => "IronTJS";

        public static Version TjsVersion => typeof(TjsContext).Assembly.GetName().Version;

        // Comparison
        CallSite<Func<CallSite, object, object, object>> _distinctEqualSite, _distinctNotEqualSite;
        CallSite<Func<CallSite, object, object, object>> _equalSite, _notEqualSite;
        CallSite<Func<CallSite, object, object, object>> _lessThanSite, _lessThanEqualSite, _greaterThanSite, _greaterThanEqualSite;

        // Conversion
        Dictionary<Type, CallSite> _convertSites;

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            var context = new CompilerContext(sourceUnit, options, errorSink);
            var ast = new Parser().Parse(context);
            if (ast == null)
                return null;
            var exp = ast.Transform<Func<object, object>>();
            return new TjsScriptCode(exp, sourceUnit);
        }

        public override CompilerOptions GetCompilerOptions() => new CompilerOptions();

        public override Guid LanguageGuid => new Guid("B6632EF3-555B-454A-816C-7430ADA873C8");

        public override Version LanguageVersion => TjsVersion;

        public TjsBinder Binder { get; private set; }

        public override CreateInstanceBinder CreateCreateBinder(CallInfo callInfo) => new TjsCreateInstanceBinder(this, callInfo);

        public override ConvertBinder CreateConvertBinder(Type toType, bool? explicitCast) => new TjsConvertBinder(this, toType, explicitCast ?? true);

        public override InvokeBinder CreateInvokeBinder(CallInfo callInfo) => new TjsInvokeBinder(this, Binders.GetCallSignatureForCallInfo(callInfo));

        public InvokeBinder CreateInvokeBinder(CallSignature signature) => new TjsInvokeBinder(this, signature);

        public override UnaryOperationBinder CreateUnaryOperationBinder(ExpressionType operation) => new TjsUnaryOperationBinder(this, operation);

        public override BinaryOperationBinder CreateBinaryOperationBinder(ExpressionType operation) => new TjsBinaryOperationBinder(this, operation);

        public override GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase) => CreateGetMemberBinder(name, ignoreCase, false);

        public GetMemberBinder CreateGetMemberBinder(string name, bool ignoreCase, bool direct) => new TjsGetMemberBinder(this, name, ignoreCase, direct);

        public override SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase) => CreateSetMemberBinder(name, ignoreCase, true, false);

        public SetMemberBinder CreateSetMemberBinder(string name, bool ignoreCase, bool forceCreate, bool direct) => new TjsSetMemberBinder(this, name, ignoreCase, forceCreate, direct);

        public override DeleteMemberBinder CreateDeleteMemberBinder(string name, bool ignoreCase) => new CompatibilityDeleteMemberBinder(this, name, ignoreCase);

        public DynamicMetaObjectBinder CreateDeleteMemberBinder(string name, bool ignoreCase, bool noThrow)
        {
            if (noThrow)
                return new TjsDeleteMemberBinder(this, name, ignoreCase);
            else
                return CreateDeleteMemberBinder(name, ignoreCase);
        }

        public GetIndexBinder CreateGetIndexBinder(CallInfo callInfo, bool direct) => new TjsGetIndexBinder(this, callInfo, direct);

        public SetIndexBinder CreateSetIndexBinder(CallInfo callInfo, bool direct) => new TjsSetIndexBinder(this, callInfo, direct);

        public DynamicMetaObjectBinder CreateDeleteIndexBinder(CallInfo callInfo) => new TjsDeleteIndexBinder(this, callInfo);

        public DynamicMetaObjectBinder CreateOperationBinder(TjsOperationKind operation) => new TjsOperationBinder(this, operation);

#if !DEBUG
		public override string FormatException(Exception exception)
		{
			if (exception is NotImplementedException)
				return base.FormatException(exception);
			if (exception is MissingMemberException)
				return string.Format("\"{0}\" という名前のオブジェクトは指定されたスコープに存在しません。", exception.Message);
			return exception.GetType().ToString() + ": " + exception.Message;
		}
#endif

        internal bool DistinctEqual(object left, object right) { return Compare(left, right, CreateOperationBinder(TjsOperationKind.DistinctEqual), ref _distinctEqualSite); }

        internal bool DistinctNotEqual(object left, object right) { return Compare(left, right, CreateOperationBinder(TjsOperationKind.DistinctNotEqual), ref _distinctNotEqualSite); }

        internal bool Equal(object left, object right) { return Compare(left, right, CreateBinaryOperationBinder(ExpressionType.Equal), ref _equalSite); }

        internal bool NotEqual(object left, object right) { return Compare(left, right, CreateBinaryOperationBinder(ExpressionType.NotEqual), ref _notEqualSite); }

        internal bool LessThan(object left, object right) { return Compare(left, right, CreateBinaryOperationBinder(ExpressionType.LessThan), ref _lessThanSite); }

        internal bool LessThanOrEqual(object left, object right) { return Compare(left, right, CreateBinaryOperationBinder(ExpressionType.LessThanOrEqual), ref _lessThanEqualSite); }

        internal bool GreaterThan(object left, object right) { return Compare(left, right, CreateBinaryOperationBinder(ExpressionType.GreaterThan), ref _greaterThanSite); }

        internal bool GreaterThanOrEqual(object left, object right) { return Compare(left, right, CreateBinaryOperationBinder(ExpressionType.GreaterThanOrEqual), ref _greaterThanEqualSite); }

        bool Compare(object left, object right, DynamicMetaObjectBinder compareBinder, ref CallSite<Func<CallSite, object, object, object>> compareSite)
        {
            if (compareSite == null)
                System.Threading.Interlocked.CompareExchange(
                    ref compareSite,
                    CallSite<Func<CallSite, object, object, object>>.Create(compareBinder),
                    null
                );
            return Convert<bool>(compareSite.Target(compareSite, left, right));
        }

        internal T Convert<T>(object target)
        {
            if (_convertSites == null)
                System.Threading.Interlocked.CompareExchange(ref _convertSites, new Dictionary<Type, CallSite>(), null);
            var type = typeof(T);
            if (!_convertSites.TryGetValue(type, out var site))
                _convertSites[type] = site = CallSite<Func<CallSite, object, T>>.Create(CreateConvertBinder(typeof(T), true));
            return ((CallSite<Func<CallSite, object, T>>)site).Target(site, target);
        }
    }
}
