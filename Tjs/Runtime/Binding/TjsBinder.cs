using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using AstUtils = Microsoft.Scripting.Ast.Utils;

namespace IronTjs.Runtime.Binding
{
    public sealed class TjsBinder : DefaultBinder
	{
		public override MemberGroup GetMember(MemberRequestKind action, Type type, string name)
		{
			if (type == typeof(string))
			{
				var method = typeof(Builtins.String).GetMethod(name);
				if (method != null)
					return new MemberGroup(MemberTracker.FromMemberInfo(method, typeof(string)));
				var property = typeof(Builtins.String).GetField(name + "Property");
				if (property != null && property.FieldType == typeof(ExtensionPropertyTracker))
					return new MemberGroup((ExtensionPropertyTracker)property.GetValue(null));
				return MemberGroup.EmptyGroup;
			}
			else if (type.IsSubclassOf(typeof(Exception)))
			{
				var property = typeof(Builtins.Exception).GetField(name + "Property");
				if (property != null && property.FieldType == typeof(ExtensionPropertyTracker))
					return new MemberGroup((ExtensionPropertyTracker)property.GetValue(null));
			}
			return base.GetMember(action, type, name);
		}

		internal static readonly object NoValue = new object();

		internal static object ConvertInternal(object obj, Type toType)
		{
			Type fromType;
			if (obj == null || (fromType = obj.GetType()) == typeof(Builtins.Void))
			{
				if (toType == typeof(string))
					return obj == null ? "null" : string.Empty;
				else
					return toType.IsValueType ? toType.GetConstructor(Type.EmptyTypes).Invoke(null) : null;
			}
			if (toType == typeof(object) || toType == fromType)
				return obj;
			if (toType == typeof(string))
				return obj.ToString();
			var nonNullableTo = Binders.GetNonNullableType(toType);
			if (Binders.IsNumber(nonNullableTo))
			{
				if (Binders.IsNumber(fromType))
				{
					var converted = System.Convert.ChangeType(obj, nonNullableTo);
					if (nonNullableTo == toType)
						return converted;
					else
						return Activator.CreateInstance(toType, converted);
				}
				else if (fromType == typeof(string))
				{
					var tryParse = nonNullableTo.GetMethod("TryParse", new[] { typeof(string), nonNullableTo.MakeByRefType() });
					var argument = new[] { obj, null };
					if ((bool)tryParse.Invoke(null, argument))
					{
						if (nonNullableTo == toType)
							return argument[1];
						else
							return Activator.CreateInstance(toType, argument[1]);
					}
					else
						return Activator.CreateInstance(toType);
				}
			}
			else if (nonNullableTo == typeof(bool))
			{
				object converted;
				if (Binders.IsNumber(fromType))
					converted = System.Convert.ChangeType(obj, nonNullableTo);
				else if (fromType == typeof(string))
				{
					long value;
					converted = long.TryParse((string)obj, out value) && value != 0;
				}
				else if (!fromType.IsValueType)
					converted = obj != null;
				else if (fromType.IsGenericType && fromType.GetGenericTypeDefinition() == typeof(Nullable<>))
					converted = fromType.GetProperty("HasValue").GetValue(obj);
				else
					converted = true;
				if (nonNullableTo == toType)
					return converted;
				else
					return new bool?((bool)converted);
			}
			return NoValue;
		}

		public override object Convert(object obj, Type toType)
		{
			var converted = ConvertInternal(obj, toType);
			if (converted == NoValue)
				return base.Convert(obj, toType);
			return converted;
		}

		public override Expression ConvertExpression(Expression expr, Type toType, ConversionResultKind kind, OverloadResolverFactory resolverFactory)
		{
			return TjsConvertBinder.TryConvertExpression(expr, toType, null) ?? base.ConvertExpression(expr, toType, kind, resolverFactory);
		}

		public override bool CanConvertFrom(Type fromType, Type toType, bool toNotNullable, NarrowingLevel level)
		{
			var nonNullable = Binders.GetNonNullableType(toType);
			return toType.IsAssignableFrom(fromType) ||
				fromType == typeof(Builtins.Void) ||
				toType == typeof(object) ||
				toType == typeof(string) ||
				nonNullable == typeof(bool) ||
				Binders.IsNumber(nonNullable) && (Binders.IsNumber(fromType) || fromType == typeof(string));
		}

		public DynamicMetaObject GetMemberDirect(string name, DynamicMetaObject target, OverloadResolverFactory resolverFactory, bool isNoThrow, DynamicMetaObject errorSuggestion)
		{
			ContractUtils.RequiresNotNull(name, "name");
			ContractUtils.RequiresNotNull(target, "target");
			ContractUtils.RequiresNotNull(resolverFactory, "resolverFactory");
			return MakeGetMemberTarget(new GetMemberInfo(name, resolverFactory, isNoThrow, errorSuggestion != null ? errorSuggestion.Expression : null), target);
		}

		DynamicMetaObject MakeGetMemberTarget(GetMemberInfo info, DynamicMetaObject target)
		{
			var type = target.GetLimitType();
			var restrictions = target.Restrictions;
			var self = target;
			target = target.Restrict(target.GetLimitType());
			var members = MemberGroup.EmptyGroup;
			// メンバ取得対象が TypeTracker である場合
			if (typeof(TypeTracker).IsAssignableFrom(type))
			{
				restrictions = restrictions.Merge(BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value));
				var tg = target.Value as TypeGroup;
				if (tg == null || tg.TypesByArity.ContainsKey(0))
				{
					var targetedType = ((TypeTracker)target.Value).Type;
					members = GetMember(MemberRequestKind.Get, targetedType, info.Name);
					if (members.Count > 0)
					{
						type = targetedType;
						self = null;
					}
				}
			}
			// 通常のメンバ一覧を検索
			if (members.Count == 0)
				members = GetMember(MemberRequestKind.Get, type, info.Name);
			// インターフェイスの場合、object メンバを検索
			if (members.Count == 0 && type.IsInterface)
				members = GetMember(MemberRequestKind.Get, type = typeof(object), info.Name);
			// プロパティ・フィールド用に StrongBox を展開し、そこから検索
			var expandedSelf = self;
			if (members.Count == 0 && typeof(IStrongBox).IsAssignableFrom(type) && expandedSelf != null)
			{
				expandedSelf = new DynamicMetaObject(Expression.Field(AstUtils.Convert(expandedSelf.Expression, type), type.GetField("Value")), expandedSelf.Restrictions, ((IStrongBox)expandedSelf.Value).Value);
				type = type.GetGenericArguments()[0];
				members = GetMember(MemberRequestKind.Get, type, info.Name);
			}
			MakeBodyHelper(info, self, expandedSelf, type, members);
			return info.Body.GetMetaObject(restrictions);
		}

		void MakeBodyHelper(GetMemberInfo info, DynamicMetaObject self, DynamicMetaObject expandedSelf, Type type, MemberGroup members)
		{
			Expression error;
			var memberType = GetMemberType(members, out error);
			if (error == null)
			{
				switch (memberType)
				{
					case TrackerTypes.TypeGroup:
					case TrackerTypes.Type:
						info.Body.FinishCondition(members.Cast<TypeTracker>().Aggregate(TypeGroup.UpdateTypeEntity).GetValue(info.ResolutionFactory, this, type).Expression);
						break;
					case TrackerTypes.Method:
						// MethodGroup になる        
						MakeGenericBodyWorker(info, type, ReflectionCache.GetMethodGroup(info.Name, members), self);
						break;
					case TrackerTypes.Event:
					case TrackerTypes.Field:
					case TrackerTypes.Property:
					case TrackerTypes.Constructor:
					case TrackerTypes.Custom:
						// もし複数のメンバーが与えられたら、その型に一番近いメンバを探す
						MakeGenericBodyWorker(info, type, members.Aggregate((w, x) => !IsTrackerApplicableForType(type, x) && (x.DeclaringType.IsSubclassOf(w.DeclaringType) || !IsTrackerApplicableForType(type, w)) ? x : w), expandedSelf);
						break;
					case TrackerTypes.All:
						// どのメンバも見つからなかった
						if (self != null)
							MakeOperatorGetMemberBody(info, expandedSelf, type, "GetBoundMember");
						if (info.ErrorSuggestion != null)
							info.Body.FinishCondition(info.ErrorSuggestion);
						else if (info.IsNoThrow)
							info.Body.FinishCondition(MakeOperationFailed());
						else
							info.Body.FinishCondition(MakeError(MakeMissingMemberError(type, self, info.Name), typeof(object)).Expression);
						break;
					default:
						throw new InvalidOperationException(memberType.ToString());
				}
			}
			else
				info.Body.FinishCondition(info.ErrorSuggestion ?? error);
		}

		static bool IsTrackerApplicableForType(Type type, MemberTracker mt) { return mt.DeclaringType == type || type.IsSubclassOf(mt.DeclaringType); }

		void MakeGenericBodyWorker(GetMemberInfo info, Type type, MemberTracker tracker, DynamicMetaObject instance)
		{
			if (instance != null)
				tracker = tracker.BindToInstance(instance);
			info.Body.FinishCondition(ReturnMemberTracker(type, tracker).Expression);
		}

		void MakeOperatorGetMemberBody(GetMemberInfo info, DynamicMetaObject instance, Type type, string name)
		{
			var getMem = GetMethod(type, name);
			if (getMem != null)
			{
				var tmp = Expression.Variable(typeof(object), "getVal");
				info.Body.AddVariable(tmp);
				info.Body.AddCondition(
					Expression.NotEqual(
						Expression.Assign(
							tmp,
							MakeCallExpression(
								info.ResolutionFactory,
								getMem,
								new DynamicMetaObject(Expression.Convert(instance.Expression, type), instance.Restrictions, instance.Value),
								new DynamicMetaObject(Expression.Constant(info.Name), BindingRestrictions.Empty, info.Name)
							).Expression
						),
						MakeOperationFailed()
					),
					tmp
				);
			}
		}

		static MemberExpression MakeOperationFailed() { return Expression.Field(null, typeof(OperationFailed).GetField("Value")); }

		sealed class GetMemberInfo
		{
			public readonly string Name;
			public readonly OverloadResolverFactory ResolutionFactory;
			public readonly bool IsNoThrow;
			public readonly ConditionalBuilder Body = new ConditionalBuilder();
			public readonly Expression ErrorSuggestion;

			public GetMemberInfo(string name, OverloadResolverFactory resolutionFactory, bool noThrow, Expression errorSuggestion)
			{
				Name = name;
				ResolutionFactory = resolutionFactory;
				IsNoThrow = noThrow;
				ErrorSuggestion = errorSuggestion;
			}
		}

		/// <summary>
		/// false 文がいまだ不明な場合に一連の条件式を構築します。
		/// 条件およびその条件に対する true 文は追加し続けることができます。
		/// それぞれの後続の条件式は以前の条件の false 文になります。
		/// 最後に条件式ではない終端ノードを追加する必要があります。
		/// </summary>
		class ConditionalBuilder
		{
			readonly List<Microsoft.Scripting.Ast.IfStatementTest> _tests = new List<Microsoft.Scripting.Ast.IfStatementTest>();
			readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
			Expression _body;

            /// <summary>新しい条件式と本体を追加します。最初の呼び出しは最上位の条件式に、後続の呼び出しは以前の条件式の false 文として追加されます。</summary>
            /// <param name="condition"><see cref="bool"/> 型の結果型をもつ条件式を指定します。</param>
            /// <param name="body"><paramref name="condition"/> が真の場合に実行される式を指定します。</param>
            public void AddCondition(Expression condition, Expression body)
			{
				Assert.NotNull(condition, body);
				_tests.Add(AstUtils.IfCondition(condition, body));
			}

			/// <summary>先行するすべての条件が満たされない場合に実行される式を追加します。</summary>
			/// <param name="body">先行するすべての条件が満たされない場合に実行される式を指定します。</param>
			public void FinishCondition(Expression body)
			{
				if (_body != null)
					throw new InvalidOperationException();
				for (int i = _tests.Count - 1; i >= 0; i--)
				{
					var t = _tests[i].Body.Type;
					if (t != body.Type)
					{
						if (t.IsSubclassOf(body.Type)) // サブクラス
							t = body.Type;
						else if (!body.Type.IsSubclassOf(t)) // 互換ではないため object に
							t = typeof(object);
					}
					body = Expression.Condition(_tests[i].Test, AstUtils.Convert(_tests[i].Body, t), AstUtils.Convert(body, t));
				}
				_body = Expression.Block(_variables, body);
			}

			/// <summary>
			/// この条件式を表す結果のメタオブジェクトを取得します。
			/// FinishCondition が呼び出されている必要があります。
			/// </summary>
			/// <param name="restrictions">結果の <see cref="DynamicMetaObject"/> への追加の制約を指定します。</param>
			/// <returns>この条件式を表す <see cref="DynamicMetaObject"/>。</returns>
			public DynamicMetaObject GetMetaObject(BindingRestrictions restrictions)
			{
				if (_body == null)
					throw new InvalidOperationException("FinishCondition should have been called");
				return new DynamicMetaObject(_body, restrictions);
			}

			/// <summary>最終式のレベルにスコープされた変数を追加します。</summary>
			/// <param name="var">この条件式に追加する変数を指定します。</param>
			public void AddVariable(ParameterExpression var) { _variables.Add(var); }
		}
	}
}
