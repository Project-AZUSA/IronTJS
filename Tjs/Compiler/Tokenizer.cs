using System;
using System.IO;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Compiler
{
    public class Tokenizer : TokenizerService
	{
		TextReader _reader;
		SourceUnit _sourceUnit;
		int _lastLength = 0;
		string _line = string.Empty;
		int _columnIndex = 0;
		SourceLocation _baseLocation;
		CircularBuffer<Token> _tokens = new CircularBuffer<Token>();
		bool _initial = true;

		public override void Initialize(object state, TextReader sourceReader, SourceUnit sourceUnit, SourceLocation initialLocation)
		{
			_reader = sourceReader;
			_sourceUnit = sourceUnit;
			_baseLocation = initialLocation;
			Read();
		}

		static string ReadLineWithLength(TextReader reader, out int length)
		{
			StringBuilder sb = new StringBuilder();
			length = 0;
			int ch;
			while (true)
			{
				ch = reader.Read();
				if (ch == -1)
				{
					if (sb.Length > 0)
						return sb.ToString();
					return null;
				}
				length++;
				if (ch == 13 && reader.Peek() == 10)
				{
					reader.Read();
					length++;
				}
				if (ch == 13 || ch == 10)
					return sb.ToString();
				sb.Append((char)ch);
			}
		}

		// ソースコード解析のための整形 (改行、空白、コメントの除去)
		Token CheckEndOfStream()
		{
			int commentLevel = 0;
			do
			{
				if (_line == null) // すでに行がない
					return new Token(TokenType.EndOfStream, null, new SourceSpan(_baseLocation, _baseLocation));
				if (_columnIndex >= _line.Length) // 行終わりなので次の行を読む
				{
					if (!_initial)
						_baseLocation = new SourceLocation(_baseLocation.Index + _lastLength, _baseLocation.Line + 1, 1);
					else
						_initial = false;
					_line = ReadLineWithLength(_reader, out _lastLength);
					if (_line == null)
						return new Token(TokenType.EndOfStream, null, new SourceSpan(_baseLocation, _baseLocation));
					_columnIndex = 0;
				}
				while (_columnIndex < _line.Length && char.IsWhiteSpace(_line[_columnIndex])) // 先頭の空白を飛ばす
					_columnIndex++;
				if (commentLevel == 1)
					commentLevel = 0;
				if (_columnIndex < _line.Length)
				{
					if (_line.IndexOf("/*", _columnIndex) == _columnIndex) // 複数行コメント開始
					{
						_columnIndex += 2;
						if (commentLevel++ == 0)
							commentLevel++;
					}
					else if (commentLevel > 0) // コメント中
					{
						if (_line.IndexOf("*/", _columnIndex) == _columnIndex) // 複数行コメント終了
						{
							_columnIndex += 2;
							--commentLevel;
						}
						else // その他
							_columnIndex++;
					}
					else if (_line.IndexOf("//", _columnIndex) == _columnIndex) // 単一行コメントを飛ばす
						_columnIndex = _line.Length;
				}
			} while (_columnIndex >= _line.Length || commentLevel > 0);
			return null;
		}

		// 識別子 or キーワード
		Token ParseIdentifierOrKeyword()
		{
			if (char.IsLetter(_line[_columnIndex]) || _line[_columnIndex] == '_')
			{
				var start = CurrentPosition;
				StringBuilder sb = new StringBuilder();
				while (_columnIndex < _line.Length && (char.IsLetterOrDigit(_line[_columnIndex]) || _line[_columnIndex] == '_'))
					sb.Append(_line[_columnIndex++]);
				return new Token(Token.GetTokenTypeForWord(sb.ToString()), sb.ToString(), new SourceSpan(start, CurrentPosition));
			}
			return null;
		}

		// 数値リテラル
		Token ParseNumericLiteral()
		{
			if (char.IsDigit(_line[_columnIndex]))
			{
				var start = CurrentPosition;
				int baseN = 10;
				long value = 0;
				if (_line[_columnIndex] == '0') // 10進数以外の数値リテラルプレフィックスの解析
				{
					_columnIndex++;
					if (_columnIndex < _line.Length)
					{
						if (_line[_columnIndex] == 'x' || _line[_columnIndex] == 'X') // 16進数
						{
							_columnIndex++;
							baseN = 16;
						}
						else if (_line[_columnIndex] == 'b' || _line[_columnIndex] == 'B') // 2進数
						{
							_columnIndex++;
							baseN = 2;
						}
						else if (IsBaseNDigit(_line[_columnIndex], 8)) // 8進数
							baseN = 8;
					}
				}
				while (_columnIndex < _line.Length && IsBaseNDigit(_line[_columnIndex], baseN)) // 整数部分
					value = value * baseN + ConvertBaseNDigit(_line[_columnIndex++]);
				Tuple<long, long> fractions = new Tuple<long, long>(0, 1);
				if (_columnIndex < _line.Length && _line[_columnIndex] == '.') // 小数部分
				{
					_columnIndex++;
					fractions = ParseNumericLiteralFraction(baseN);
				}
				var exponents = ParseNumericLiteralExponent(baseN);
				if (fractions.Item2 > 1 || exponents.Item1 != 0)
					return new Token(TokenType.LiteralReal, ((double)value + (double)fractions.Item1 / fractions.Item2) * Math.Pow(exponents.Item1, exponents.Item2), new SourceSpan(start, CurrentPosition));
				else
					return new Token(TokenType.LiteralInteger, value, new SourceSpan(start, CurrentPosition));
			}
			return null;
		}

		Tuple<long, long> ParseNumericLiteralFraction(int baseN)
		{
			long fraction = 0;
			long divisor = 1;
			while (_columnIndex < _line.Length && IsBaseNDigit(_line[_columnIndex], baseN))
			{
				fraction = fraction * baseN + ConvertBaseNDigit(_line[_columnIndex++]);
				divisor *= baseN;
			}
			return new Tuple<long, long>(fraction, divisor);
		}

		Tuple<long, long> ParseNumericLiteralExponent(int baseN)
		{
			long expBase = 0;
			long exponent = 0;
			if (_columnIndex < _line.Length) // 指数部
			{
				if (_line[_columnIndex] == 'e' || _line[_columnIndex] == 'E') // 10を底とする指数表現
				{
					_columnIndex++;
					expBase = 10;
				}
				else if (_line[_columnIndex] == 'p' || _line[_columnIndex] == 'P') // 2を底とする指数表現
				{
					_columnIndex++;
					expBase = 2;
				}
				if (expBase != 0)
				{
					bool negative = false;
					if (_columnIndex < _line.Length && _line[_columnIndex] == '+')
						_columnIndex++;
					else if (_columnIndex < _line.Length && _line[_columnIndex] == '-')
					{
						_columnIndex++;
						negative = true;
					}
					while (_columnIndex < _line.Length && IsBaseNDigit(_line[_columnIndex], baseN))
						exponent = exponent * baseN + ConvertBaseNDigit(_line[_columnIndex++]);
					if (negative)
						exponent = -exponent;
				}
			}
			return new Tuple<long, long>(expBase, exponent);
		}

		// 文字列リテラル (連続文字列の連結はパーサーで)
		Token ParseStringLiteral()
		{
			if (_line[_columnIndex] == '"' || _line[_columnIndex] == '\'')
			{
				var start = CurrentPosition;
				var quote = _line[_columnIndex++];
				StringBuilder sb = new StringBuilder();
				while (true)
				{
					if (_columnIndex >= _line.Length)
					{
						//AddError("文字列トークンが予期せず終了しました。", new SourceSpan(start, CurrentPosition), -1, Severity.Error); //FIX: can not input "
						return new Token(TokenType.Unknown, sb.ToString(), new SourceSpan(start, CurrentPosition));
					}
					else if (_line[_columnIndex] == quote)
					{
						_columnIndex++;
						return new Token(TokenType.LiteralString, sb.ToString(), new SourceSpan(start, CurrentPosition));
					}
					var ch = _line[_columnIndex++];
					if (_columnIndex < _line.Length && ch == '\\')
					{
						ch = _line[_columnIndex++];
						switch (ch)
						{
							case 'a':
								sb.Append('\a');
								break;
							case 'b':
								sb.Append('\b');
								break;
							case 'f':
								sb.Append('\f');
								break;
							case 'n':
								sb.Append('\n');
								break;
							case 'r':
								sb.Append('\r');
								break;
							case 't':
								sb.Append('\t');
								break;
							case 'v':
								sb.Append('\v');
								break;
							case 'x':
							case 'X':
								int charCode = 0;
								while (_columnIndex < _line.Length && charCode <= 0x0FFF && IsBaseNDigit(_line[_columnIndex], 16))
									charCode = charCode * 16 + ConvertBaseNDigit(_line[_columnIndex++]);
								sb.Append((char)charCode);
								break;
							default:
								sb.Append(ch);
								break;
						}
					}
					else
						sb.Append(ch);
				}
			}
			return null;
		}

		// 記号類 (演算子、セパレータなど)
		Token ParseSymbol()
		{
			var start = CurrentPosition;
			StringBuilder sb = new StringBuilder();
			string completeSymbol = null;
			int index = 0;
			while (_columnIndex < _line.Length && !char.IsWhiteSpace(_line[_columnIndex]))
			{
				sb.Append(_line[_columnIndex++]);
				var type = Token.GetTokenTypeForSymbol(sb.ToString());
				if (type != TokenType.Unknown)
				{
					completeSymbol = sb.ToString();
					index = _columnIndex;
					Tuple<long, long> frac;
					if (type == TokenType.SymbolPeriod && (frac = ParseNumericLiteralFraction(10)).Item2 > 1)
					{
						var exponents = ParseNumericLiteralExponent(10);
						return new Token(TokenType.LiteralReal, (double)frac.Item1 / frac.Item2 * Math.Pow(exponents.Item1, exponents.Item2), new SourceSpan(start, CurrentPosition));
					}
				}
			}
			if (completeSymbol != null)
			{
				_columnIndex = index; // 最長で解析できた場所にカーソルを戻す
				return new Token(Token.GetTokenTypeForSymbol(completeSymbol), completeSymbol, new SourceSpan(start, CurrentPosition));
			}
			else // ダメなら全部で1トークン
				return new Token(TokenType.Unknown, sb.ToString(), new SourceSpan(start, CurrentPosition));
		}

		public Token Read()
		{
			var parsedToken = CheckEndOfStream() ?? ParseIdentifierOrKeyword() ?? ParseNumericLiteral() ?? ParseStringLiteral() ?? ParseSymbol();
			Token token = null;
			if (_tokens.Count > 0)
			{
				token = _tokens[0];
				_tokens.RemoveAt(0);
			}
			_tokens.Add(parsedToken);
			return token;
		}

		public Token NextToken { get { return _tokens[0]; } }

		public Token GetNextToken(int level)
		{
			while (_tokens.Count <= level && _tokens[_tokens.Count - 1].Type != TokenType.EndOfStream)
				_tokens.Add(CheckEndOfStream() ?? ParseIdentifierOrKeyword() ?? ParseNumericLiteral() ?? ParseStringLiteral() ?? ParseSymbol());
			return _tokens[Math.Min(level, _tokens.Count - 1)];
		}

		public override SourceLocation CurrentPosition { get { return new SourceLocation(_baseLocation.Index + _columnIndex, _baseLocation.Line, _baseLocation.Column + _columnIndex); } }

		public override object CurrentState { get { return null; } }

		public override ErrorSink ErrorSink { get; set; }

		public override bool IsRestartable { get { return true; } }

		public override TokenInfo ReadToken() { return Read().ToTokenInfo(); }

		static bool IsBaseNDigit(char ch, int baseN)
		{
			if (baseN <= 10)
				return ch >= '0' && ch <= '0' + baseN - 1;
			else
				return ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'a' + baseN - 10 - 1 || ch >= 'A' && ch <= 'A' + baseN - 10 - 1;
		}

		static int ConvertBaseNDigit(char ch)
		{
			if (ch >= 'a')
				return ch - 'a' + 10;
			if (ch >= 'A')
				return ch - 'A' + 10;
			return ch - '0';
		}

		void AddError(string message, SourceSpan span, int errorCode, Severity severity)
		{
			if (_sourceUnit != null)
				ErrorSink.Add(_sourceUnit, message, span, errorCode, severity);
			else
				ErrorSink.Add(message, null, null, _line, span, errorCode, severity);
		}
	}
}
