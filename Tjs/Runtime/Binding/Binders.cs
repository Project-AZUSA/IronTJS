using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using IronTjs.Compiler;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;

namespace IronTjs.Runtime.Binding
{
    static class Binders
	{
		public static Expression Convert(this TjsContext context, Expression expression, Type toType) { return Expression.Dynamic(context.CreateConvertBinder(toType, true), toType, expression); }

		public static CallSignature GetCallSignatureForCallInfo(CallInfo callInfo)
		{
			return new CallSignature(Enumerable.Repeat(new Argument(ArgumentType.Simple), callInfo.ArgumentCount - callInfo.ArgumentNames.Count)
				.Concat(callInfo.ArgumentNames.Select(x => new Argument(ArgumentType.Named, x))).ToArray());
		}

		public static CallInfo GetCallInfoForCallSignature(CallSignature signature) { return new CallInfo(signature.ArgumentCount, signature.GetArgumentNames()); }

		public static bool IsFloatingPoint(Type type)
		{
			if (type.IsEnum)
				return false;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}

		public static bool IsInteger(Type type)
		{
			if (type.IsEnum)
				return false;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
			}
		}

		public static bool IsNumber(Type type)
		{
			if (type.IsEnum)
				return false;
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
			}
		}

		public static Type GetNonNullableType(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? type.GetGenericArguments()[0] : type;
		}

		public static bool TryConvertInt64(string s, out long value)
		{
			using (var reader = new StringReader(s))
			{
				var tokenizer = new Tokenizer();
				tokenizer.Initialize(null, reader, null, SourceLocation.MinValue);
				if (tokenizer.NextToken.Type == TokenType.LiteralInteger)
				{
					value = (long)tokenizer.NextToken.Value;
					return true;
				}
				else if (tokenizer.NextToken.Type == TokenType.LiteralReal)
				{
					value = (long)(double)tokenizer.NextToken.Value;
					return true;
				}
				value = 0;
				return false;
			}
		}

		public static bool TryConvertDouble(string s, out double value)
		{
			using (var reader = new StringReader(s))
			{
				var tokenizer = new Tokenizer();
				tokenizer.Initialize(null, reader, null, SourceLocation.MinValue);
				if (tokenizer.NextToken.Type == TokenType.LiteralInteger)
				{
					value = (long)tokenizer.NextToken.Value;
					return true;
				}
				else if (tokenizer.NextToken.Type == TokenType.LiteralReal)
				{
					value = (double)tokenizer.NextToken.Value;
					return true;
				}
				value = 0;
				return false;
			}
		}

		public static object ConvertNumber(string s)
		{
			using (var reader = new StringReader(s))
			{
				var tokenizer = new Tokenizer();
				tokenizer.Initialize(null, reader, null, SourceLocation.MinValue);
				if (tokenizer.NextToken.Type == TokenType.LiteralInteger || tokenizer.NextToken.Type == TokenType.LiteralReal)
					return tokenizer.NextToken.Value;
				return 0;
			}
		}
	}
}
