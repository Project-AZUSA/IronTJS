using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronTJS.Runtime.Binding
{
    class TjsOperationBinder : DynamicMetaObjectBinder
	{
		public TjsOperationBinder(TjsContext context, TjsOperationKind operation)
		{
			Context = context;
			OperationKind = operation;
		}

		public TjsContext Context { get; private set; }

		public TjsOperationKind OperationKind { get; private set; }

		public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
		{
			var ito = target as ITjsOperable;
			if (ito != null)
				return ito.BindOperation(this, args);
			else
				return FallbackOperation(target, args, null);
		}

		static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj)
		{
			if (obj.RuntimeType == null)
				return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
			else
				return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.RuntimeType);
		}

		public DynamicMetaObject FallbackOperation(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
		{
			Expression convertedTarget = Expression.Convert(target.Expression, target.LimitType);
			Expression[] convertedArgs = args.Select(x => Expression.Convert(x.Expression, x.LimitType)).ToArray();
			Expression exp = null;
			var restrictions = target.Restrictions.Merge(GetTypeRestriction(target));
			int usedArgs = 0;
			switch (OperationKind)
			{
				// Unary
				case TjsOperationKind.CharCodeToChar:
					exp = Expression.Call(Context.Convert(convertedTarget, typeof(char)), "ToString", null);
					break;
				case TjsOperationKind.CharToCharCode:
					exp = Expression.Call(new Func<string, long>(TjsOperationHelper.CharToCharCode).Method, Context.Convert(convertedTarget, typeof(string)));
					break;
				case TjsOperationKind.Evaluate:
				case TjsOperationKind.Invalidate:
				case TjsOperationKind.IsValid:
                    //MARK: not implemented?
					if (errorSuggestion == null)
						errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new NotImplementedException())), BindingRestrictions.Empty);
					break;
				case TjsOperationKind.TypeOf:
					if (target.RuntimeType == null)
						exp = Expression.Constant("Object");
					else if (target.RuntimeType == typeof(Builtins.Void))
						exp = Expression.Constant("void");
					else if (target.RuntimeType == typeof(string))
						exp = Expression.Constant("String");
					else if (Binders.IsInteger(target.RuntimeType))
						exp = Expression.Constant("Integer");
					else if (Binders.IsFloatingPoint(target.RuntimeType))
						exp = Expression.Constant("Real");
					else
						exp = Expression.Constant("Object");
					break;
				// Unary (Special)
				case TjsOperationKind.InvokePropertyHandler:
					if (target.LimitType == typeof(Property))
					{
						if (args.Length == 0)
							exp = Expression.Property(convertedTarget, (System.Reflection.PropertyInfo)Utils.GetMember<Property>(x => x.Value));
						else if (args.Length == 1)
						{
							exp = Expression.Assign(Expression.Property(convertedTarget, (System.Reflection.PropertyInfo)Utils.GetMember<Property>(x => x.Value)), args[0].Expression);
							usedArgs = 1;
						}
						else
						{
							if (errorSuggestion == null)
								errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("引数の数が * 演算子に適用できる許容範囲を超えています。"))), BindingRestrictions.Empty);
						}
					}
					else if (target.LimitType == typeof(BoundMemberTracker))
					{
						var tracker = (BoundMemberTracker)target.Value;
						if (tracker.Instance == null)
							tracker = new BoundMemberTracker(tracker.BoundTo, new DynamicMetaObject(Expression.Constant(tracker.ObjectInstance), BindingRestrictions.Empty, tracker.ObjectInstance));
						var type = tracker.Instance.GetLimitType();
						if (args.Length == 0)
						{
							var res = tracker.GetValue(new TjsOverloadResolverFactory(Context.Binder), Context.Binder, type);
							if (res != null)
							{
								exp = res.Expression;
								restrictions = restrictions.Merge(res.Restrictions);
							}
							else
							{
								if (errorSuggestion == null)
									errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("メンバの取得に失敗しました。"))), BindingRestrictions.Empty);
							}
						}
						else if (args.Length == 1)
						{
							var res = tracker.SetValue(new TjsOverloadResolverFactory(Context.Binder), Context.Binder, type, args[0]);
							if (res != null)
							{
								exp = res.Expression;
								restrictions = restrictions.Merge(res.Restrictions);
								usedArgs = 1;
							}
							else
							{
								if (errorSuggestion == null)
									errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("メンバの設定に失敗しました。"))), BindingRestrictions.Empty);
							}
						}
						else
						{
							if (errorSuggestion == null)
								errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("引数の数が * 演算子に適用できる許容範囲を超えています。"))), BindingRestrictions.Empty);
						}
					}
					else
					{
						if (errorSuggestion == null)
							errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("プロパティ以外に対して * 演算子が使用されました。"))), BindingRestrictions.Empty);
					}
					break;
				// Binary (Arithmetic & Logical)
				case TjsOperationKind.FloorDivide:
					exp = Expression.Divide(Context.Convert(convertedTarget, typeof(long)), Context.Convert(convertedArgs[0], typeof(long)));
					usedArgs = 1;
					break;
				case TjsOperationKind.RightShiftLogical:
					exp = Expression.Convert(Expression.RightShift(Context.Convert(convertedTarget, typeof(ulong)), Context.Convert(convertedArgs[0], typeof(int))), typeof(long));
					usedArgs = 1;
					break;
				// Binary (Comparison)
				case TjsOperationKind.DistinctEqual:
					if (convertedTarget.Type == convertedArgs[0].Type)
						exp = Expression.Condition(Expression.Equal(convertedTarget, convertedArgs[0]), Expression.Constant(1L), Expression.Constant(0L));
					else
						exp = Expression.Constant(0L);
					usedArgs = 1;
					break;
				case TjsOperationKind.DistinctNotEqual:
					if (convertedTarget.Type == convertedArgs[0].Type)
						exp = Expression.Condition(Expression.Equal(convertedTarget, convertedArgs[0]), Expression.Constant(0L), Expression.Constant(1L));
					else
						exp = Expression.Constant(1L);
					usedArgs = 1;
					break;
				// Binary (Special)
				case TjsOperationKind.InstanceOf:
					exp = Expression.Condition(Expression.Equal(Expression.Constant(convertedTarget.Type.Name), Context.Convert(convertedArgs[0], typeof(string))),
						Expression.Constant(1L),
						Expression.Constant(0L)
					);
					usedArgs = 1;
					break;
				case TjsOperationKind.InContextOf:
					if (target.Value is IContextChangeable)
						exp = Expression.Call(Expression.Convert(convertedTarget, typeof(IContextChangeable)), "ChangeContext", null, args[0].Expression);
					else if (target.LimitType == typeof(BoundMemberTracker))
						exp = Expression.New((System.Reflection.ConstructorInfo)Utils.GetMember(() => new BoundMemberTracker(null, (object)null)), Expression.PropertyOrField(convertedTarget, "BoundTo"), args[0].Expression);
					else
					{
						if (errorSuggestion == null)
							errorSuggestion = new DynamicMetaObject(Expression.Throw(Expression.Constant(new InvalidOperationException("incontextof 演算子は指定されたオブジェクトに適用できません。"))), BindingRestrictions.Empty);
					}
					usedArgs = 1;
					break;
			}
			for (int i = 0; i < usedArgs; i++)
				restrictions = restrictions.Merge(args[i].Restrictions).Merge(GetTypeRestriction(args[i]));
			if (exp == null)
				return new DynamicMetaObject(errorSuggestion.Expression, errorSuggestion.Restrictions.Merge(restrictions));
			if (exp.Type != typeof(object))
				exp = Expression.Convert(exp, typeof(object));
			return new DynamicMetaObject(exp, restrictions);
		}
	}

	static class TjsOperationHelper
	{
		public static long CharToCharCode(string s) { return s.Length > 0 ? s[0] : 0; }
	}
}
