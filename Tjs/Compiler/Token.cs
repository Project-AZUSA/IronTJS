using System.Collections.Generic;
using Microsoft.Scripting;

namespace IronTjs.Compiler
{
    public class Token
	{
		public Token(TokenType type, object value, SourceSpan span)
		{
			Type = type;
			Value = value;
			Span = span;
		}

		public TokenType Type { get; private set; }

		public object Value { get; private set; }

		public SourceSpan Span { get; private set; }

		public override string ToString() { return string.Format("{0}, \"{1}\"", Type.ToString(), Value); }

		public TokenInfo ToTokenInfo()
		{
			TokenCategory category = TokenCategory.None;
			if (Type >= TokenType.KeywordBreak && Type <= TokenType.KeywordWith)
				category = TokenCategory.Keyword;
			switch (Type)
			{
				case TokenType.EndOfStream:
					category = TokenCategory.EndOfStream;
					break;
				case TokenType.Identifier:
					category = TokenCategory.Identifier;
					break;
				case TokenType.LiteralInteger:
				case TokenType.LiteralReal:
					category = TokenCategory.NumericLiteral;
					break;
				case TokenType.LiteralString:
					category = TokenCategory.StringLiteral;
					break;
				case TokenType.SymbolAmpersand:
				case TokenType.SymbolAmpersandEquals:
				case TokenType.SymbolAsterisk:
				case TokenType.SymbolAsteriskEquals:
				case TokenType.SymbolBackSlash:
				case TokenType.SymbolBackSlashEquals:
				case TokenType.SymbolCircumflex:
				case TokenType.SymbolCircumflexEquals:
				case TokenType.SymbolDollarSign:
				case TokenType.SymbolDoubleAmpersand:
				case TokenType.SymbolDoubleAmpersandEquals:
				case TokenType.SymbolDoubleEquals:
				case TokenType.SymbolDoubleGreaterThan:
				case TokenType.SymbolDoubleGreaterThanEquals:
				case TokenType.SymbolDoubleLessThan:
				case TokenType.SymbolDoubleLessThanEquals:
				case TokenType.SymbolDoubleMinus:
				case TokenType.SymbolDoublePlus:
				case TokenType.SymbolDoubleVerticalLine:
				case TokenType.SymbolDoubleVerticalLineEquals:
				case TokenType.SymbolEquals:
				case TokenType.SymbolExclamation:
				case TokenType.SymbolExclamationDoubleEquals:
				case TokenType.SymbolExclamationEquals:
				case TokenType.SymbolGreaterThan:
				case TokenType.SymbolGreaterThanEquals:
				case TokenType.SymbolLessThan:
				case TokenType.SymbolLessThanEquals:
				case TokenType.SymbolLessThanMinusGreaterThan:
				case TokenType.SymbolMinus:
				case TokenType.SymbolMinusEquals:
				case TokenType.SymbolNumberSign:
				case TokenType.SymbolPercent:
				case TokenType.SymbolPercentEquals:
				case TokenType.SymbolPeriod:
				case TokenType.SymbolPlus:
				case TokenType.SymbolPlusEquals:
				case TokenType.SymbolQuestion:
				case TokenType.SymbolSlash:
				case TokenType.SymbolSlashEquals:
				case TokenType.SymbolTilde:
				case TokenType.SymbolTripleEquals:
				case TokenType.SymbolTripleGreaterThan:
				case TokenType.SymbolTripleGreaterThanEquals:
				case TokenType.SymbolVerticalLine:
				case TokenType.SymbolVerticalLineEquals:
					category = TokenCategory.Operator;
					break;
				case TokenType.SymbolColon:
				case TokenType.SymbolComma:
				case TokenType.SymbolSemicolon:
				case TokenType.SymbolEqualsGreaterThan:
				case TokenType.SymbolTriplePeriod:
					category = TokenCategory.Delimiter;
					break;
				case TokenType.SymbolAsteriskSlash:
				case TokenType.SymbolSlashAsterisk:
					category = TokenCategory.Comment;
					break;
				case TokenType.SymbolDoubleSlash:
					category = TokenCategory.LineComment;
					break;
				case TokenType.SymbolCloseBrace:
				case TokenType.SymbolCloseBracket:
				case TokenType.SymbolCloseParenthesis:
				case TokenType.SymbolOpenBrace:
				case TokenType.SymbolOpenBracket:
				case TokenType.SymbolOpenParenthesis:
					category = TokenCategory.Grouping;
					break;
			}
			return new TokenInfo(Span, category, TokenTriggers.None);
		}

		static readonly Dictionary<string, TokenType> mappings = new Dictionary<string, TokenType>()
		{
			{ "break", TokenType.KeywordBreak },
			{ "continue", TokenType.KeywordContinue },
			{ "const", TokenType.KeywordConst },
			{ "catch", TokenType.KeywordCatch },
			{ "class", TokenType.KeywordClass },
			{ "case", TokenType.KeywordCase },
			{ "debugger", TokenType.KeywordDebugger },
			{ "default", TokenType.KeywordDefault },
			{ "delete", TokenType.KeywordDelete },
			{ "do", TokenType.KeywordDo },
			{ "extends", TokenType.KeywordExtends },
			{ "export", TokenType.KeywordExport },
			{ "enum", TokenType.KeywordEnum },
			{ "else", TokenType.KeywordElse },
			{ "function", TokenType.KeywordFunction },
			{ "finally", TokenType.KeywordFinally },
			{ "false", TokenType.KeywordFalse },
			{ "for", TokenType.KeywordFor },
			{ "global", TokenType.KeywordGlobal },
			{ "getter", TokenType.KeywordGetter },
			{ "goto", TokenType.KeywordGoTo },
			{ "incontextof", TokenType.KeywordInContextOf },
			{ "Infinity", TokenType.KeywordInfinity },
			{ "invalidate", TokenType.KeywordInvalidate },
			{ "instanceof", TokenType.KeywordInstanceOf },
			{ "isvalid", TokenType.KeywordIsValid },
			{ "import", TokenType.KeywordImport },
			{ "int", TokenType.KeywordInt },
			{ "in", TokenType.KeywordIn },
			{ "if", TokenType.KeywordIf },
			{ "NaN", TokenType.KeywordNaN },
			{ "null", TokenType.KeywordNull },
			{ "new", TokenType.KeywordNew },
			{ "octet", TokenType.KeywordOctet },
			{ "protected", TokenType.KeywordProtected },
			{ "property", TokenType.KeywordProperty },
			{ "private", TokenType.KeywordPrivate },
			{ "public", TokenType.KeywordPublic },
			{ "return", TokenType.KeywordReturn },
			{ "real", TokenType.KeywordReal },
			{ "synchronized", TokenType.KeywordSynchronized },
			{ "switch", TokenType.KeywordSwitch },
			{ "static", TokenType.KeywordStatic },
			{ "setter", TokenType.KeywordSetter },
			{ "string", TokenType.KeywordString },
			{ "super", TokenType.KeywordSuper },
			{ "typeof", TokenType.KeywordTypeOf },
			{ "throw", TokenType.KeywordThrow },
			{ "this", TokenType.KeywordThis },
			{ "true", TokenType.KeywordTrue },
			{ "try", TokenType.KeywordTry },
			{ "void", TokenType.KeywordVoid },
			{ "var", TokenType.KeywordVar },
			{ "while", TokenType.KeywordWhile },
			{ "with", TokenType.KeywordWith },
			{ "+", TokenType.SymbolPlus },
			{ "-", TokenType.SymbolMinus },
			{ "*", TokenType.SymbolAsterisk },
			{ "/", TokenType.SymbolSlash },
			{ "\\", TokenType.SymbolBackSlash },
			{ "%", TokenType.SymbolPercent },
			{ "<<", TokenType.SymbolDoubleLessThan },
			{ ">>", TokenType.SymbolDoubleGreaterThan },
			{ ">>>", TokenType.SymbolTripleGreaterThan },
			{ "&", TokenType.SymbolAmpersand },
			{ "^", TokenType.SymbolCircumflex },
			{ "|", TokenType.SymbolVerticalLine },
			{ "<", TokenType.SymbolLessThan },
			{ ">", TokenType.SymbolGreaterThan },
			{ "&&", TokenType.SymbolDoubleAmpersand },
			{ "||", TokenType.SymbolDoubleVerticalLine },
			{ "<=", TokenType.SymbolLessThanEquals },
			{ ">=", TokenType.SymbolGreaterThanEquals },
			{ "==", TokenType.SymbolDoubleEquals },
			{ "!=", TokenType.SymbolExclamationEquals },
			{ "===", TokenType.SymbolTripleEquals },
			{ "!==", TokenType.SymbolExclamationDoubleEquals },
			{ "=", TokenType.SymbolEquals },
			{ "<->", TokenType.SymbolLessThanMinusGreaterThan },
			{ "+=", TokenType.SymbolPlusEquals },
			{ "-=", TokenType.SymbolMinusEquals },
			{ "*=", TokenType.SymbolAsteriskEquals },
			{ "/=", TokenType.SymbolSlashEquals },
			{ "\\=", TokenType.SymbolBackSlashEquals },
			{ "%=", TokenType.SymbolPercentEquals },
			{ "<<=", TokenType.SymbolDoubleLessThanEquals },
			{ ">>=", TokenType.SymbolDoubleGreaterThanEquals },
			{ ">>>=", TokenType.SymbolTripleGreaterThanEquals },
			{ "&=", TokenType.SymbolAmpersandEquals },
			{ "^=", TokenType.SymbolCircumflexEquals },
			{ "|=", TokenType.SymbolVerticalLineEquals },
			{ "&&=", TokenType.SymbolDoubleAmpersandEquals },
			{ "||=", TokenType.SymbolDoubleVerticalLineEquals },
			{ "!", TokenType.SymbolExclamation },
			{ "~", TokenType.SymbolTilde },
			{ "#", TokenType.SymbolNumberSign },
			{ "$", TokenType.SymbolDollarSign },
			{ "++", TokenType.SymbolDoublePlus },
			{ "--", TokenType.SymbolDoubleMinus },
			{ "=>", TokenType.SymbolEqualsGreaterThan },
			{ "...", TokenType.SymbolTriplePeriod },
			{ ":", TokenType.SymbolColon },
			{ ";", TokenType.SymbolSemicolon },
			{ ",", TokenType.SymbolComma },
			{ ".", TokenType.SymbolPeriod },
			{ "?", TokenType.SymbolQuestion },
			{ "{", TokenType.SymbolOpenBrace },
			{ "}", TokenType.SymbolCloseBrace },
			{ "[", TokenType.SymbolOpenBracket },
			{ "]", TokenType.SymbolCloseBracket },
			{ "(", TokenType.SymbolOpenParenthesis },
			{ ")", TokenType.SymbolCloseParenthesis },
			{ "//", TokenType.SymbolDoubleSlash },
			{ "/*", TokenType.SymbolSlashAsterisk },
			{ "*/", TokenType.SymbolAsteriskSlash },
		};

		public static TokenType GetTokenTypeForWord(string word)
		{
			TokenType type;
			if (mappings.TryGetValue(word, out type))
				return type;
			return TokenType.Identifier;
		}

		public static TokenType GetTokenTypeForSymbol(string symbol)
		{
			TokenType type;
			if (mappings.TryGetValue(symbol, out type))
				return type;
			return TokenType.Unknown;
		}

		public static string GetStringForToken(TokenType type)
		{
			if (type == TokenType.Identifier || type == TokenType.LiteralInteger ||
				type == TokenType.LiteralReal || type == TokenType.LiteralString ||
				type == TokenType.EndOfStream)
				return type.ToString();
			foreach (var kvp in mappings)
			{
				if (kvp.Value == type)
					return kvp.Key;
			}
			return string.Empty;
		}
	}

	public enum TokenType
	{
		Unknown,
		EndOfStream,
		LiteralReal,
		LiteralInteger,
		LiteralString,
		KeywordBreak,
		KeywordContinue,
		KeywordConst,
		KeywordCatch,
		KeywordClass,
		KeywordCase,
		KeywordDebugger,
		KeywordDefault,
		KeywordDelete,
		KeywordDo,
		KeywordExtends,
		KeywordExport,
		KeywordEnum,
		KeywordElse,
		KeywordFunction,
		KeywordFinally,
		KeywordFalse,
		KeywordFor,
		KeywordGlobal,
		KeywordGetter,
		KeywordGoTo,
		KeywordInContextOf,
		KeywordInfinity,
		KeywordInvalidate,
		KeywordInstanceOf,
		KeywordIsValid,
		KeywordImport,
		KeywordInt,
		KeywordIn,
		KeywordIf,
		KeywordNaN,
		KeywordNull,
		KeywordNew,
		KeywordOctet,
		KeywordProtected,
		KeywordProperty,
		KeywordPrivate,
		KeywordPublic,
		KeywordReturn,
		KeywordReal,
		KeywordSynchronized,
		KeywordSwitch,
		KeywordStatic,
		KeywordSetter,
		KeywordString,
		KeywordSuper,
		KeywordTypeOf,
		KeywordThrow,
		KeywordThis,
		KeywordTrue,
		KeywordTry,
		KeywordVoid,
		KeywordVar,
		KeywordWhile,
		KeywordWith,
		Identifier,
		SymbolPlus,
		SymbolMinus,
		SymbolAsterisk,
		SymbolSlash,
		SymbolBackSlash,
		SymbolPercent,
		SymbolDoubleLessThan,
		SymbolDoubleGreaterThan,
		SymbolTripleGreaterThan,
		SymbolAmpersand,
		SymbolCircumflex,
		SymbolVerticalLine,
		SymbolLessThan,
		SymbolGreaterThan,
		SymbolDoubleAmpersand,
		SymbolDoubleVerticalLine,
		SymbolLessThanEquals,
		SymbolGreaterThanEquals,
		SymbolDoubleEquals,
		SymbolExclamationEquals,
		SymbolTripleEquals,
		SymbolExclamationDoubleEquals,
		SymbolEquals,
		SymbolPlusEquals,
		SymbolMinusEquals,
		SymbolAsteriskEquals,
		SymbolSlashEquals,
		SymbolBackSlashEquals,
		SymbolPercentEquals,
		SymbolDoubleLessThanEquals,
		SymbolDoubleGreaterThanEquals,
		SymbolTripleGreaterThanEquals,
		SymbolAmpersandEquals,
		SymbolCircumflexEquals,
		SymbolVerticalLineEquals,
		SymbolDoubleAmpersandEquals,
		SymbolDoubleVerticalLineEquals,
		SymbolLessThanMinusGreaterThan,
		SymbolExclamation,
		SymbolTilde,
		SymbolNumberSign,
		SymbolDollarSign,
		SymbolDoublePlus,
		SymbolDoubleMinus,
		SymbolEqualsGreaterThan,
		SymbolTriplePeriod,
		SymbolColon,
		SymbolSemicolon,
		SymbolComma,
		SymbolPeriod,
		SymbolQuestion,
		SymbolOpenBrace,
		SymbolCloseBrace,
		SymbolOpenBracket,
		SymbolCloseBracket,
		SymbolOpenParenthesis,
		SymbolCloseParenthesis,
		SymbolDoubleSlash,
		SymbolSlashAsterisk,
		SymbolAsteriskSlash,
	}
}
