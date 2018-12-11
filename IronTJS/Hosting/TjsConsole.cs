using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IronTJS.Compiler;
using Microsoft.Scripting.Hosting.Shell;

namespace IronTJS.Hosting
{
    public sealed class TjsConsole : BasicConsole
	{
		struct Cursor
		{
			int _anchorTop;
			int _anchorLeft;

			public void Anchor()
			{
				_anchorTop = Console.CursorTop;
				_anchorLeft = Console.CursorLeft;
			}

			public void Reset()
			{
				Console.CursorTop = _anchorTop;
				Console.CursorLeft = _anchorLeft;
			}

			public void Place(int index)
			{
				Console.CursorLeft = (_anchorLeft + index) % Console.BufferWidth;
				int cursorTop = _anchorTop + (_anchorLeft + index) / Console.BufferWidth;
				if (cursorTop >= Console.BufferHeight)
				{
					_anchorTop -= cursorTop - Console.BufferHeight + 1;
					cursorTop = Console.BufferHeight - 1;
				}
				Console.CursorTop = cursorTop;
			}
		}

		public TjsConsole(bool colorful, int tabWidth) : base(colorful) { _tabWidth = tabWidth; }

		StringBuilder _input = new StringBuilder();
		int _current;
		int _rendered;
		Cursor _cursor;
		Tuple<int, int>[] _inputToOutputTable = new[] { new Tuple<int, int>(0, 0) };
		int _tabWidth;
		bool _coloringInput;

		void Initialize()
		{
			_cursor.Anchor();
			_input.Length = 0;
			_current = 0;
			_rendered = 0;
		}

		void OnBackspace()
		{
			if (_input.Length > 0 && _current > 0)
			{
				_input.Remove(--_current, 1);
				Render();
			}
		}

		void OnDelete()
		{
			if (_input.Length > 0 && _current < _input.Length)
			{
				_input.Remove(_current, 1);
				Render();
			}
		}

		void Insert(char c)
		{
			_input.Insert(_current++, c);
			Render();
		}

		void Render()
		{
			_inputToOutputTable = new Tuple<int,int>[_input.Length + 1];
			_cursor.Reset();
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < _input.Length; i++)
			{
				_inputToOutputTable[i] = new Tuple<int,int>(output.Length, Encoding.GetEncoding(932).GetByteCount(output.ToString()));
				if (_input[i] == '\t')
				{
					for (int j = _tabWidth - output.Length % _tabWidth; j > 0; j--)
						output.Append(' ');
				}
				else if (char.IsControl(_input[i]))
				{
					if (_input[i] <= 26)
						output.AppendFormat("^{0}", (char)(_input[i] + 'A' - 1));
					else
						output.Append("^?");
				}
				else
					output.Append(_input[i]);
			}
			_inputToOutputTable[_input.Length] = new Tuple<int,int>(output.Length, Encoding.GetEncoding(932).GetByteCount(output.ToString()));
			var text = output.ToString();
			if (_coloringInput)
			{
				int end = 0;
				var tokenizer = new Tokenizer();
				tokenizer.ErrorSink = Microsoft.Scripting.ErrorSink.Null;
				tokenizer.Initialize(null, new StringReader(_input.ToString()), null, Microsoft.Scripting.SourceLocation.MinValue);
				List<Token> tokens = new List<Token>();
				while (tokenizer.NextToken.Type != TokenType.EndOfStream)
					tokens.Add(tokenizer.Read());
				foreach (var token in tokens)
				{
					var start = _inputToOutputTable[token.Span.Start.Index].Item1;
					if (end != start)
						WriteColor(Output, text.Substring(end, start - end), ConsoleColor.Cyan);
					end = _inputToOutputTable[token.Span.End.Index].Item1;
					ConsoleColor color;
					switch (token.Type)
					{
						case TokenType.KeywordClass:
						case TokenType.KeywordConst:
						case TokenType.KeywordEnum:
						case TokenType.KeywordExtends:
						case TokenType.KeywordFunction:
						case TokenType.KeywordPrivate:
						case TokenType.KeywordProperty:
						case TokenType.KeywordProtected:
						case TokenType.KeywordPublic:
						case TokenType.KeywordStatic:
						case TokenType.KeywordSynchronized:
						case TokenType.KeywordInt:
						case TokenType.KeywordOctet:
						case TokenType.KeywordReal:
						case TokenType.KeywordString:
						case TokenType.KeywordVar:
							color = ConsoleColor.Green;
							break;
						case TokenType.KeywordBreak:
						case TokenType.KeywordCase:
						case TokenType.KeywordCatch:
						case TokenType.KeywordContinue:
						case TokenType.KeywordDebugger:
						case TokenType.KeywordDefault:
						case TokenType.KeywordDelete:
						case TokenType.KeywordDo:
						case TokenType.KeywordElse:
						case TokenType.KeywordExport:
						case TokenType.KeywordFinally:
						case TokenType.KeywordFor:
						case TokenType.KeywordGetter:
						case TokenType.KeywordGlobal:
						case TokenType.KeywordGoTo:
						case TokenType.KeywordIf:
						case TokenType.KeywordImport:
						case TokenType.KeywordIn:
						case TokenType.KeywordInContextOf:
						case TokenType.KeywordInstanceOf:
						case TokenType.KeywordInvalidate:
						case TokenType.KeywordIsValid:
						case TokenType.KeywordNew:
						case TokenType.KeywordReturn:
						case TokenType.KeywordSetter:
						case TokenType.KeywordSuper:
						case TokenType.KeywordSwitch:
						case TokenType.KeywordThis:
						case TokenType.KeywordThrow:
						case TokenType.KeywordTry:
						case TokenType.KeywordTypeOf:
						case TokenType.KeywordWhile:
						case TokenType.KeywordWith:
							color = ConsoleColor.Yellow;
							break;
						case TokenType.KeywordFalse:
						case TokenType.KeywordInfinity:
						case TokenType.KeywordNaN:
						case TokenType.KeywordNull:
						case TokenType.KeywordTrue:
						case TokenType.KeywordVoid:
						case TokenType.LiteralInteger:
						case TokenType.LiteralReal:
						case TokenType.LiteralString:
							color = ConsoleColor.Magenta;
							break;
						case TokenType.Unknown:
							color = ConsoleColor.Red;
							break;
						default:
							color = ConsoleColor.White;
							break;
					}
					WriteColor(Output, text.Substring(start, end - start), color);
				}
				if (end != text.Length)
					WriteColor(Output, text.Substring(end), ConsoleColor.Cyan);
			}
			else
				WriteColor(Output, text, ConsoleColor.White);
			if (text.Length < _rendered)
				Output.Write(new string(' ', _rendered - text.Length));
			_rendered = text.Length;
			_cursor.Place(_inputToOutputTable[_current].Item2);
		}

		void MoveRight()
		{
			if (_current < _input.Length)
				_cursor.Place(_inputToOutputTable[++_current].Item2);
		}

		void MoveLeft()
		{
			if (_current > 0)
				_cursor.Place(_inputToOutputTable[--_current].Item2);
		}

		public override string ReadLine(int autoIndentSize)
		{
			_coloringInput = autoIndentSize >= 0;
			Initialize();
			for (int i = 0; i < autoIndentSize / _tabWidth; i++)
				Insert('\t');
			while (true)
			{
				var key = Console.ReadKey(true);
				switch (key.Key)
				{
					case ConsoleKey.Backspace:
						OnBackspace();
						break;
					case ConsoleKey.Delete:
						OnDelete();
						break;
					case ConsoleKey.Enter:
						return OnEnter();
					case ConsoleKey.RightArrow:
						MoveRight();
						break;
					case ConsoleKey.LeftArrow:
						MoveLeft();
						break;
					default:
						if (key.KeyChar == '\r')
							goto case ConsoleKey.Enter;      // Ctrl-M
						if (key.KeyChar == '\b')
							goto case ConsoleKey.Backspace;  // Ctrl-H
						if (key.KeyChar != '\0')
							Insert(key.KeyChar);
						break;
				}
			}
		}

		string OnEnter()
		{
			Output.Write("\n");
			var line = _input.ToString();
			if (line == FinalLineText)
				return null;
			return line;
		}

		string FinalLineText { get { return Environment.OSVersion.Platform != PlatformID.Unix ? "\x1A" : "\x04"; } }
	}
}