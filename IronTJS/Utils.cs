using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace IronTJS
{
    static class Utils
	{
		public static Type[] GetDelegateSignature(Type delegateType)
		{
			var invokeMethod = delegateType.GetMethod("Invoke");
			return invokeMethod.GetParameters().Select(x => x.ParameterType).Concat(Enumerable.Repeat(invokeMethod.ReturnType, 1)).ToArray();
		}

		public static MemberInfo GetMemberEx<TDelegate>(Expression<TDelegate> lambda)
		{
			Expression exp = lambda.Body;
			if (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked)
				exp = ((UnaryExpression)exp).Operand;
			if (exp.NodeType == ExpressionType.New)
				return ((NewExpression)exp).Constructor;
			if (exp.NodeType == ExpressionType.Call)
				return ((MethodCallExpression)exp).Method;
			if (exp.NodeType == ExpressionType.MemberAccess)
				return ((MemberExpression)exp).Member;
			return null;
		}

		public static MemberInfo GetMember(Expression<Func<object>> lambda) { return GetMemberEx(lambda); }

		public static MemberInfo GetMember(Expression<Action> lambda) { return GetMemberEx(lambda); }

		public static MemberInfo GetMember<T>(Expression<Func<T, object>> lambda) { return GetMemberEx(lambda); }

		public static MemberInfo GetMember<T>(Expression<Action<T>> lambda) { return GetMemberEx(lambda); }

		internal static string ConvertToExpression(IEnumerable self, int indent)
		{
			StringBuilder sb = new StringBuilder();
			indent += 4;
			var dict = self as IDictionary;
			if (dict != null)
			{
				sb.Append("(const) %[");
				string sep = "";
				var iter = dict.GetEnumerator();
				while (iter.MoveNext())
				{
					sb.AppendFormat(sep + Environment.NewLine + new string(' ', indent) + "\"{0}\" => ", iter.Key);
					WriteValue(sb, indent, iter.Value);
					sep = ",";
				}
			}
			else
			{
				sb.Append("(const) [");
				string sep = "";
				foreach (var item in self)
				{
					sb.Append(sep + Environment.NewLine + new string(' ', indent));
					WriteValue(sb, indent, item);
					sep = ",";
				}
			}
			indent -= 4;
			sb.Append(Environment.NewLine + new string(' ', indent) + "]");
			return sb.ToString();
		}

		static void WriteValue(StringBuilder sb, int indent, object value)
		{
			if (value == null)
				sb.Append("void");
			else if (value.GetType() == typeof(string))
				sb.AppendFormat("\"{0}\"", value);
			else if (Runtime.Binding.Binders.IsNumber(value.GetType()))
				sb.AppendFormat("{0}", value);
			else
			{
				var enumerable = value as IEnumerable;
				if (enumerable != null)
					sb.Append(ConvertToExpression(enumerable, indent));
				else
					sb.Append("void");
			}
		}
	}
}
