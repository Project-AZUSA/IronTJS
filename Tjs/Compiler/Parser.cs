using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronTjs.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronTjs.Compiler
{
    public class Parser
	{
		[Serializable]
		class InternalInvalidSyntaxException : Exception { }

		Tokenizer _tokenizer;
		CompilerContext _context;
		bool _parsedIncompleteIf = false;

		bool Accept(TokenType type)
		{
			if (_tokenizer.NextToken.Type == type)
			{
				_tokenizer.Read();
				return true;
			}
			return false;
		}

		Token Expect(TokenType type)
		{
			if (_tokenizer.NextToken.Type == type)
				return _tokenizer.Read();
			var errorSpan = new SourceSpan(_tokenizer.NextToken.Span.Start, _tokenizer.NextToken.Span.Start);
			_tokenizer.ErrorSink.Add(_context.SourceUnit, "Unexpected token: " + _tokenizer.NextToken.Type, errorSpan, -1, Severity.Error);
			throw new InternalInvalidSyntaxException();
			//object dummyValue = null;
			//switch (types[0])
			//{
			//	case TokenType.Identifier:
			//	case TokenType.LiteralString:
			//		dummyValue = "__dummy__";
			//		break;
			//	case TokenType.LiteralInteger:
			//		dummyValue = 0;
			//		break;
			//	case TokenType.LiteralReal:
			//		dummyValue = 0.0;
			//		break;
			//}
			//return new Token(types[0], dummyValue, errorSpan);
		}

		public static int GetNextAutoIndentSize(SourceUnit sourceUnit, int autoIndentTabWidth)
		{
			new Parser().Parse(new CompilerContext(sourceUnit, new CompilerOptions(), ErrorSink.Null));
			var autoIndentSize = sourceUnit.GetCode().TakeWhile(x => x == ' ' || x == '\t').Aggregate(0, (x, y) => y == ' ' ? x + 1 : x + autoIndentTabWidth);
			if (sourceUnit.CodeProperties != ScriptCodeParseResult.Complete)
				autoIndentSize += autoIndentTabWidth;
			return autoIndentSize;
		}

		public SourceUnitTree Parse(CompilerContext context)
		{
			_tokenizer = new Tokenizer();
			_context = context;
			ErrorCounter counter = new ErrorCounter(context.Errors);
			_tokenizer.ErrorSink = counter;
			using (var reader = context.SourceUnit.GetReader())
			{
				_tokenizer.Initialize(null, reader, context.SourceUnit, SourceLocation.MinValue);
				SourceUnitTree ast;
				try { ast = Parse(); }
				catch (InternalInvalidSyntaxException) { ast = null; }
				finally
				{
					ScriptCodeParseResult props;
					if (counter.AnyError)
					{
						ast = null;
						if (_tokenizer.NextToken.Type == TokenType.EndOfStream)
							props = ScriptCodeParseResult.IncompleteStatement;
						else
							props = ScriptCodeParseResult.Invalid;
					}
					else if (_parsedIncompleteIf)
						props = ScriptCodeParseResult.IncompleteStatement;
					else
						props = ScriptCodeParseResult.Complete;
					context.SourceUnit.CodeProperties = props;
				}
				return ast;
			}
		}

		SourceUnitTree Parse()
		{
			List<ClassDefinition> classes = new List<ClassDefinition>();
			List<FunctionDefinition> functions = new List<FunctionDefinition>();
			List<PropertyDefinition> properties = new List<PropertyDefinition>();
			List<Statement> statements = new List<Statement>();
			while (_tokenizer.NextToken.Type != TokenType.EndOfStream)
			{
				if (Accept(TokenType.KeywordClass))
				{
					classes.Add(ParseClassDefinition());
					_parsedIncompleteIf = false;
				}
				else if (Accept(TokenType.KeywordFunction))
				{
					functions.Add(ParseFunctionDefinition());
					_parsedIncompleteIf = false;
				}
				else if (Accept(TokenType.KeywordProperty))
				{
					properties.Add(ParsePropertyDefinition());
					_parsedIncompleteIf = false;
				}
				else
					statements.Add(ParseStatement());
			}
			return new SourceUnitTree(classes, functions, properties, statements, _context);
		}

		ClassDefinition ParseClassDefinition()
		{
			var name = Expect(TokenType.Identifier).Value.ToString();
			List<string> baseClasses = new List<string>();
			if (Accept(TokenType.KeywordExtends))
			{
				do
					baseClasses.Add(Expect(TokenType.Identifier).Value.ToString());
				while (Accept(TokenType.SymbolComma));
			}
			List<ClassDefinition> classes = new List<ClassDefinition>();
			List<FunctionDefinition> functions = new List<FunctionDefinition>();
			List<PropertyDefinition> properties = new List<PropertyDefinition>();
			List<VariableDeclarationExpression> variableDeclarations = new List<VariableDeclarationExpression>();
			Expect(TokenType.SymbolOpenBrace);
			while (true)
			{
				if (Accept(TokenType.KeywordClass))
					classes.Add(ParseClassDefinition());
				else if (Accept(TokenType.KeywordFunction))
					functions.Add(ParseFunctionDefinition());
				else if (Accept(TokenType.KeywordProperty))
					properties.Add(ParsePropertyDefinition());
				else if (Accept(TokenType.KeywordVar))
				{
					var varDec = ParseVariableDeclaration();
					Expect(TokenType.SymbolSemicolon);
					variableDeclarations.Add(varDec);
				}
				else
					break;
			}
			Expect(TokenType.SymbolCloseBrace);
			return new ClassDefinition(name, baseClasses, classes, functions, properties, variableDeclarations);
		}

		FunctionDefinition ParseFunctionDefinition()
		{
			var name = Expect(TokenType.Identifier).Value.ToString();
			List<ParameterDefinition> parameters = new List<ParameterDefinition>();
			if (Accept(TokenType.SymbolOpenParenthesis))
			{
				while (true)
				{
					if (Accept(TokenType.SymbolCloseParenthesis))
						break;
					else
						Accept(TokenType.SymbolComma);
					if (Accept(TokenType.SymbolAsterisk))
						parameters.Add(new ParameterDefinition(null, true));
					else
					{
						var paramName = Expect(TokenType.Identifier).Value.ToString();
						if (Accept(TokenType.SymbolEquals))
						{
							var start = _tokenizer.NextToken.Span.Start;
							var exp = new SourceUnitTree(
								Enumerable.Empty<ClassDefinition>(),
								Enumerable.Empty<FunctionDefinition>(),
								Enumerable.Empty<PropertyDefinition>(),
								new[] { new ExpressionStatement(ParseAssignmentExpression()) },
								_context
							);
							var end = _tokenizer.NextToken.Span.End;
							try
							{
								parameters.Add(new ParameterDefinition(paramName, exp.Transform<Func<object, object>>().Compile()(null)));
							}
							catch (MissingMemberException)
							{
								_tokenizer.ErrorSink.Add(_context.SourceUnit, "関数の既定値には定数のみが使用できます。", new SourceSpan(start, end), -2, Severity.Error);
								throw new InternalInvalidSyntaxException();
							}
						}
						else if (Accept(TokenType.SymbolAsterisk))
							parameters.Add(new ParameterDefinition(paramName, true));
						else
							parameters.Add(new ParameterDefinition(paramName, false));
					}
				}
			}
			List<Statement> statements = new List<Statement>();
			Expect(TokenType.SymbolOpenBrace);
			while (!Accept(TokenType.SymbolCloseBrace))
				statements.Add(ParseStatement());
			return new FunctionDefinition(name, parameters, statements);
		}

		PropertyDefinition ParsePropertyDefinition()
		{
			var name = Expect(TokenType.Identifier).Value.ToString();
			FunctionDefinition getter = null;
			FunctionDefinition setter = null;
			Expect(TokenType.SymbolOpenBrace);
			if (Accept(TokenType.KeywordGetter))
			{
				getter = ParsePropertyGetter();
				if (Accept(TokenType.KeywordSetter))
					setter = ParsePropertySetter();
			}
			else
			{
				Expect(TokenType.KeywordSetter);
				setter = ParsePropertySetter();
				if (Accept(TokenType.KeywordGetter))
					getter = ParsePropertyGetter();
			}
			Expect(TokenType.SymbolCloseBrace);
			return new PropertyDefinition(name, getter, setter);
		}

		FunctionDefinition ParsePropertyGetter()
		{
			if (Accept(TokenType.SymbolOpenParenthesis))
				Expect(TokenType.SymbolCloseParenthesis);
			List<Statement> body = new List<Statement>();
			Expect(TokenType.SymbolOpenBrace);
			while (!Accept(TokenType.SymbolCloseBrace))
				body.Add(ParseStatement());
			return new FunctionDefinition("getter", new ParameterDefinition[0], body);
		}

		FunctionDefinition ParsePropertySetter()
		{
			Expect(TokenType.SymbolOpenParenthesis);
			var name = Expect(TokenType.Identifier).Value.ToString();
			Expect(TokenType.SymbolCloseParenthesis);
			List<Statement> body = new List<Statement>();
			Expect(TokenType.SymbolOpenBrace);
			while (!Accept(TokenType.SymbolCloseBrace))
				body.Add(ParseStatement());
			return new FunctionDefinition("setter", new[] { new ParameterDefinition(name, false) } , body);
		}

		Statement ParseStatement()
		{
			if (Accept(TokenType.SymbolOpenBrace))
			{
				List<Statement> statements = new List<Statement>();
				while (!Accept(TokenType.SymbolCloseBrace))
					statements.Add(ParseStatement());
				_parsedIncompleteIf = false;
				return new Block(statements);
			}
			else if (Accept(TokenType.KeywordIf))
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var test = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				var ifTrue = ParseStatement();
				_parsedIncompleteIf = true;
				Statement ifFalse = null;
				if (Accept(TokenType.KeywordElse))
				{
					ifFalse = ParseStatement();
					_parsedIncompleteIf = false;
				}
				return new IfStatement(test, ifTrue, ifFalse);
			}
			else if (Accept(TokenType.KeywordSwitch))
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var cond = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				Expect(TokenType.SymbolOpenBrace);
				List<SwitchCase> cases = new List<SwitchCase>();
				List<Expression> testExpressions = new List<Expression>();
				List<Statement> statements = new List<Statement>();
				bool containsDefault = false;
				while (true)
				{
					TokenType? type = null;
					if (Accept(TokenType.KeywordCase))
						type = TokenType.KeywordCase;
					else if (Accept(TokenType.KeywordDefault))
						type = TokenType.KeywordDefault;
					else if (Accept(TokenType.SymbolCloseBrace))
						type = TokenType.SymbolCloseBrace;
					if (type != null && statements.Count > 0)
					{
						cases.Add(new SwitchCase(testExpressions, containsDefault, statements));
						testExpressions.Clear();
						statements.Clear();
						containsDefault = false;
					}
					if (type == null)
						statements.Add(ParseStatement());
					else if (type == TokenType.KeywordCase)
					{
						testExpressions.Add(ParseExpression());
						Expect(TokenType.SymbolColon);
					}
					else if (type == TokenType.KeywordDefault)
					{
						containsDefault = true;
						Expect(TokenType.SymbolColon);
					}
					else
					{
						_parsedIncompleteIf = false;
						return new SwitchStatement(cond, cases);
					}
				}
			}
			else if (Accept(TokenType.KeywordWhile))
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var cond = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				var body = ParseStatement();
				_parsedIncompleteIf = false;
				return new WhileStatement(cond, body);
			}
			else if (Accept(TokenType.KeywordDo))
			{
				var body = ParseStatement();
				Expect(TokenType.KeywordWhile);
				Expect(TokenType.SymbolOpenParenthesis);
				var cond = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				Expect(TokenType.SymbolSemicolon);
				_parsedIncompleteIf = false;
				return new DoWhileStatement(body, cond);
			}
			else if (Accept(TokenType.KeywordFor))
			{
				Expression init = null;
				Expression condition = null;
				Expression update = null;
				Expect(TokenType.SymbolOpenParenthesis);
				if (!Accept(TokenType.SymbolSemicolon))
				{
					if (Accept(TokenType.KeywordVar))
						init = ParseVariableDeclaration();
					else
						init = ParseExpression();
					Expect(TokenType.SymbolSemicolon);
				}
				if (!Accept(TokenType.SymbolSemicolon))
				{
					condition = ParseExpression();
					Expect(TokenType.SymbolSemicolon);
				}
				if (!Accept(TokenType.SymbolCloseParenthesis))
				{
					update = ParseExpression();
					Expect(TokenType.SymbolCloseParenthesis);
				}
				var body = ParseStatement();
				_parsedIncompleteIf = false;
				return new ForStatement(init, condition, update, body);
			}
			else if (Accept(TokenType.KeywordTry))
			{
				var body = ParseStatement();
				Expect(TokenType.KeywordCatch);
				string catchVariableName = null;
				if (Accept(TokenType.SymbolOpenParenthesis))
				{
					catchVariableName = Expect(TokenType.Identifier).Value.ToString();
					Expect(TokenType.SymbolCloseParenthesis);
				}
				var catchBody = ParseStatement();
				_parsedIncompleteIf = false;
				return new TryStatement(body, new CatchBlock(catchVariableName, catchBody));
			}
			else if (Accept(TokenType.KeywordWith))
			{
				Expect(TokenType.SymbolOpenParenthesis);
				var exp = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				var body = ParseStatement();
				_parsedIncompleteIf = false;
				return new WithStatement(exp, body);
			}
			else if (Accept(TokenType.KeywordBreak))
			{
				Expect(TokenType.SymbolSemicolon);
				return new BreakStatement();
			}
			else if (Accept(TokenType.KeywordContinue))
			{
				Expect(TokenType.SymbolSemicolon);
				return new ContinueStatement();
			}
			else if (Accept(TokenType.KeywordReturn))
			{
				Expression exp = null;
				if (!Accept(TokenType.SymbolSemicolon))
				{
					exp = ParseExpression();
					Expect(TokenType.SymbolSemicolon);
				}
				return new ReturnStatement(exp);
			}
			else if (Accept(TokenType.KeywordThrow))
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolSemicolon);
				return new ThrowStatement(exp);
			}
			else if (Accept(TokenType.KeywordVar))
			{
				var varDec = ParseVariableDeclaration();
				Expect(TokenType.SymbolSemicolon);
				return new ExpressionStatement(varDec);
			}
			else if (Accept(TokenType.SymbolSemicolon))
				return new EmptyStatement();
			else
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolSemicolon);
				return new ExpressionStatement(exp);
			}
		}

		VariableDeclarationExpression ParseVariableDeclaration()
		{
			List<KeyValuePair<string, Expression>> initializers = new List<KeyValuePair<string, Expression>>();
			do
			{
				var name = Expect(TokenType.Identifier).Value.ToString();
				Expression exp = null;
				if (Accept(TokenType.SymbolEquals))
					exp = ParseAssignmentExpression();
				initializers.Add(new KeyValuePair<string, Expression>(name, exp));
			} while (Accept(TokenType.SymbolComma));
			return new VariableDeclarationExpression(initializers);
		}

		Expression ParseExpression()
		{
			var exp = ParseSequenceExpression();
			if (Accept(TokenType.KeywordIf))
				return new IfExpression(exp, ParseSequenceExpression());
			return exp;
		}

		Expression ParseSequenceExpression()
		{
			List<Expression> exps = new List<Expression>();
			exps.Add(ParseAssignmentExpression());
			while (Accept(TokenType.SymbolComma))
				exps.Add(ParseAssignmentExpression());
			if (exps.Count > 1)
				return new SequenceExpression(exps);
			else
				return exps[0];
		}

		Expression ParseAssignmentExpression()
		{
			var exp = ParseConditionalExpression();
			if (Accept(TokenType.SymbolEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.Assign);
			else if (Accept(TokenType.SymbolAsteriskEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.MultiplyAssign);
			else if (Accept(TokenType.SymbolSlashEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.DivideAssign);
			else if (Accept(TokenType.SymbolBackSlashEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.FloorDivideAssign);
			else if (Accept(TokenType.SymbolPercentEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.ModuloAssign);
			else if (Accept(TokenType.SymbolPlusEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.AddAssign);
			else if (Accept(TokenType.SymbolMinusEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.SubtractAssign);
			else if (Accept(TokenType.SymbolDoubleLessThanEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.LeftShiftAssign);
			else if (Accept(TokenType.SymbolDoubleGreaterThanEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.RightShiftArithmeticAssign);
			else if (Accept(TokenType.SymbolTripleGreaterThanEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.RightShiftLogicalAssign);
			else if (Accept(TokenType.SymbolAmpersandEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.AndAssign);
			else if (Accept(TokenType.SymbolCircumflexEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.ExclusiveOrAssign);
			else if (Accept(TokenType.SymbolVerticalLineEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.OrAssign);
			else if (Accept(TokenType.SymbolDoubleAmpersandEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.AndAlsoAssign);
			else if (Accept(TokenType.SymbolDoubleVerticalLineEquals))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.OrElseAssign);
			else if (Accept(TokenType.SymbolLessThanMinusGreaterThan))
				return new BinaryExpression(exp, ParseAssignmentExpression(), BinaryOperator.Exchange);
			else
				return exp;
		}

		Expression ParseConditionalExpression()
		{
			var exp = ParseLogicalOrExpression();
			if (Accept(TokenType.SymbolQuestion))
			{
				var ifTrue = ParseAssignmentExpression();
				Expect(TokenType.SymbolColon);
				var ifFalse = ParseAssignmentExpression();
				return new ConditionalExpression(exp, ifTrue, ifFalse);
			}
			return exp;
		}

		Expression ParseLogicalOrExpression()
		{
			var exp = ParseLogicalAndExpression();
			while (Accept(TokenType.SymbolDoubleVerticalLine))
				exp = new BinaryExpression(exp, ParseLogicalAndExpression(), BinaryOperator.OrElse);
			return exp;
		}

		Expression ParseLogicalAndExpression()
		{
			var exp = ParseBitwiseOrExpression();
			while (Accept(TokenType.SymbolDoubleAmpersand))
				exp = new BinaryExpression(exp, ParseBitwiseOrExpression(), BinaryOperator.AndAlso);
			return exp;
		}

		Expression ParseBitwiseOrExpression()
		{
			var exp = ParseBitwiseXorExpression();
			while (Accept(TokenType.SymbolVerticalLine))
				exp = new BinaryExpression(exp, ParseBitwiseXorExpression(), BinaryOperator.Or);
			return exp;
		}

		Expression ParseBitwiseXorExpression()
		{
			var exp = ParseBitwiseAndExpression();
			while (Accept(TokenType.SymbolCircumflex))
				exp = new BinaryExpression(exp, ParseBitwiseAndExpression(), BinaryOperator.ExclusiveOr);
			return exp;
		}

		Expression ParseBitwiseAndExpression()
		{
			var exp = ParseEqualityExpression();
			while (Accept(TokenType.SymbolAmpersand))
				exp = new BinaryExpression(exp, ParseEqualityExpression(), BinaryOperator.And);
			return exp;
		}

		Expression ParseEqualityExpression()
		{
			var exp = ParseRelationalExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolDoubleEquals))
					exp = new BinaryExpression(exp, ParseRelationalExpression(), BinaryOperator.Equal);
				else if (Accept(TokenType.SymbolExclamationEquals))
					exp = new BinaryExpression(exp, ParseRelationalExpression(), BinaryOperator.NotEqual);
				else if (Accept(TokenType.SymbolTripleEquals))
					exp = new BinaryExpression(exp, ParseRelationalExpression(), BinaryOperator.DistinctEqual);
				else if (Accept(TokenType.SymbolExclamationDoubleEquals))
					exp = new BinaryExpression(exp, ParseRelationalExpression(), BinaryOperator.DistinctNotEqual);
				else
					break;
			}
			return exp;
		}

		Expression ParseRelationalExpression()
		{
			var exp = ParseShiftExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolLessThan))
					exp = new BinaryExpression(exp, ParseShiftExpression(), BinaryOperator.LessThan);
				else if (Accept(TokenType.SymbolLessThanEquals))
					exp = new BinaryExpression(exp, ParseShiftExpression(), BinaryOperator.LessThanOrEqual);
				else if (Accept(TokenType.SymbolGreaterThan))
					exp = new BinaryExpression(exp, ParseShiftExpression(), BinaryOperator.GreaterThan);
				else if (Accept(TokenType.SymbolGreaterThanEquals))
					exp = new BinaryExpression(exp, ParseShiftExpression(), BinaryOperator.GreaterThanOrEqual);
				else
					break;
			}
			return exp;
		}

		Expression ParseShiftExpression()
		{
			var exp = ParseAdditiveExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolDoubleLessThan))
					exp = new BinaryExpression(exp, ParseAdditiveExpression(), BinaryOperator.LeftShift);
				else if (Accept(TokenType.SymbolDoubleGreaterThan))
					exp = new BinaryExpression(exp, ParseAdditiveExpression(), BinaryOperator.RightShiftArithmetic);
				else if (Accept(TokenType.SymbolTripleGreaterThan))
					exp = new BinaryExpression(exp, ParseAdditiveExpression(), BinaryOperator.RightShiftLogical);
				else
					break;
			}
			return exp;
		}

		Expression ParseAdditiveExpression()
		{
			var exp = ParseMultiplicativeExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolPlus))
					exp = new BinaryExpression(exp, ParseMultiplicativeExpression(), BinaryOperator.Add);
				else if (Accept(TokenType.SymbolMinus))
					exp = new BinaryExpression(exp, ParseMultiplicativeExpression(), BinaryOperator.Subtract);
				else
					break;
			}
			return exp;
		}

		Expression ParseMultiplicativeExpression()
		{
			var exp = ParsePrefixExpression();
			while (true)
			{
				if (_tokenizer.GetNextToken(0).Type == TokenType.SymbolAsterisk &&
					_tokenizer.GetNextToken(1).Type != TokenType.SymbolCloseParenthesis &&
					_tokenizer.GetNextToken(1).Type != TokenType.SymbolComma)
				{
					_tokenizer.Read();
					exp = new BinaryExpression(exp, ParsePrefixExpression(), BinaryOperator.Multiply);
				}
				else if (Accept(TokenType.SymbolSlash))
					exp = new BinaryExpression(exp, ParsePrefixExpression(), BinaryOperator.Divide);
				else if (Accept(TokenType.SymbolBackSlash))
					exp = new BinaryExpression(exp, ParsePrefixExpression(), BinaryOperator.FloorDivide);
				else if (Accept(TokenType.SymbolPercent))
					exp = new BinaryExpression(exp, ParsePrefixExpression(), BinaryOperator.Modulo);
				else
					break;
			}
			return exp;
		}

		Expression ParsePrefixExpression()
		{
			if (Accept(TokenType.SymbolExclamation))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.Not);
			else if (Accept(TokenType.SymbolTilde))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.OnesComplement);
			else if (Accept(TokenType.SymbolNumberSign))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.CharToCharCode);
			else if (Accept(TokenType.SymbolDollarSign))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.CharCodeToChar);
			else if (Accept(TokenType.SymbolPlus))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.UnaryPlus);
			else if (Accept(TokenType.SymbolMinus))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.Negate);
			else if (Accept(TokenType.SymbolAmpersand))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.AccessPropertyObject);
			else if (_tokenizer.GetNextToken(0).Type == TokenType.SymbolAsterisk &&
				_tokenizer.GetNextToken(1).Type != TokenType.SymbolCloseParenthesis &&
				_tokenizer.GetNextToken(1).Type != TokenType.SymbolComma)
			{
				_tokenizer.Read();
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.InvokePropertyHandler);
			}
			else if (Accept(TokenType.SymbolDoublePlus))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.PreIncrementAssign);
			else if (Accept(TokenType.SymbolDoubleMinus))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.PreDecrementAssign);
			else if (Accept(TokenType.KeywordNew))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.New);
			else if (Accept(TokenType.KeywordInvalidate))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.Invalidate);
			else if (Accept(TokenType.KeywordIsValid))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.IsValid);
			else if (Accept(TokenType.KeywordDelete))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.Delete);
			else if (Accept(TokenType.KeywordTypeOf))
				return new UnaryExpression(ParsePrefixExpression(), UnaryOperator.TypeOf);
			else if (Accept(TokenType.KeywordInt))
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.Integer);
			else if (Accept(TokenType.KeywordReal))
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.Real);
			else if (Accept(TokenType.KeywordString))
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.String);
			else if (_tokenizer.GetNextToken(0).Type == TokenType.SymbolOpenParenthesis &&
				_tokenizer.GetNextToken(1).Type == TokenType.KeywordInt &&
				_tokenizer.GetNextToken(2).Type == TokenType.SymbolCloseParenthesis)
			{
				_tokenizer.Read();
				_tokenizer.Read();
				_tokenizer.Read();
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.Integer);
			}
			else if (_tokenizer.GetNextToken(0).Type == TokenType.SymbolOpenParenthesis &&
				_tokenizer.GetNextToken(1).Type == TokenType.KeywordReal &&
				_tokenizer.GetNextToken(2).Type == TokenType.SymbolCloseParenthesis)
			{
				_tokenizer.Read();
				_tokenizer.Read();
				_tokenizer.Read();
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.Real);
			}
			else if (_tokenizer.GetNextToken(0).Type == TokenType.SymbolOpenParenthesis &&
				_tokenizer.GetNextToken(1).Type == TokenType.KeywordString &&
				_tokenizer.GetNextToken(2).Type == TokenType.SymbolCloseParenthesis)
			{
				_tokenizer.Read();
				_tokenizer.Read();
				_tokenizer.Read();
				return new ConvertExpression(ParsePrefixExpression(), ConvertType.String);
			}
			else
				return ParseClassInstanceExpression();
		}

		Expression ParseClassInstanceExpression()
		{
			var exp = ParseValidationExpression();
			while (Accept(TokenType.KeywordInstanceOf))
				exp = new BinaryExpression(exp, ParseValidationExpression(), BinaryOperator.InstanceOf);
			return exp;
		}

		Expression ParseValidationExpression()
		{
			var exp = ParseContextExpression();
			if (Accept(TokenType.KeywordIsValid))
				return new UnaryExpression(exp, UnaryOperator.IsValid);
			return exp;
		}

		Expression ParseContextExpression()
		{
			var exp = ParsePostfixExpression();
			while (Accept(TokenType.KeywordInContextOf))
				exp = new BinaryExpression(exp, ParsePostfixExpression(), BinaryOperator.InContextOf);
			return exp;
		}

		Expression ParsePostfixExpression()
		{
			var exp = ParseMemberExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolDoublePlus))
					exp = new UnaryExpression(exp, UnaryOperator.PostIncrementAssign);
				else if (Accept(TokenType.SymbolDoubleMinus))
					exp = new UnaryExpression(exp, UnaryOperator.PostDecrementAssign);
				else if (Accept(TokenType.SymbolExclamation))
					exp = new UnaryExpression(exp, UnaryOperator.Evaluate);
				else
					break;
			}
			return exp;
		}

		Expression ParseMemberExpression()
		{
			Expression exp;
			if (Accept(TokenType.SymbolPeriod))
				exp = new DirectMemberAccessExpression(null, Expect(TokenType.Identifier).Value.ToString());
			else
				exp = ParsePrimaryExpression();
			while (true)
			{
				if (Accept(TokenType.SymbolPeriod))
					exp = new DirectMemberAccessExpression(exp, Expect(TokenType.Identifier).Value.ToString());
				else if (Accept(TokenType.SymbolOpenBracket))
				{
					exp = new IndirectMemberAccessExpression(exp, ParseExpression());
					Expect(TokenType.SymbolCloseBracket);
				}
				else if (Accept(TokenType.SymbolOpenParenthesis))
				{
					bool inheritArgs = false;
					List<InvocationArgument> arguments = new List<InvocationArgument>();
					if (!Accept(TokenType.SymbolCloseParenthesis))
					{
						if (Accept(TokenType.SymbolTriplePeriod))
							inheritArgs = true;
						else
						{
							arguments.Add(ParseInvocationArgument());
							while (Accept(TokenType.SymbolComma))
								arguments.Add(ParseInvocationArgument());
						}
						Expect(TokenType.SymbolCloseParenthesis);
					}
					exp = new InvokeExpression(exp, arguments, inheritArgs);
				}
				else
					break;
			}
			return exp;
		}

		InvocationArgument ParseInvocationArgument()
		{
			if (_tokenizer.GetNextToken(0).Type == TokenType.SymbolAsterisk &&
				(_tokenizer.GetNextToken(1).Type == TokenType.SymbolCloseParenthesis ||
				_tokenizer.GetNextToken(1).Type == TokenType.SymbolComma))
			{
				_tokenizer.Read();
				return new InvocationArgument(null, true);
			}
			else if (_tokenizer.NextToken.Type != TokenType.SymbolComma && _tokenizer.NextToken.Type != TokenType.SymbolCloseParenthesis)
			{
				var exp = ParseAssignmentExpression();
				if (Accept(TokenType.SymbolAsterisk))
					return new InvocationArgument(exp, true);
				else
					return new InvocationArgument(exp, false);
			}
			else
				return new InvocationArgument(null, false);
		}

		Expression ParsePrimaryExpression()
		{
			if (_tokenizer.NextToken.Type == TokenType.Identifier)
				return new IdentifierExpression(_tokenizer.Read().Value.ToString());
			else if (Accept(TokenType.KeywordSuper))
				return new SuperExpression();
			else if (Accept(TokenType.KeywordGlobal))
				return new GlobalExpression();
			else if (Accept(TokenType.KeywordThis))
				return new ThisExpression();
			else if (Accept(TokenType.SymbolOpenBracket))
			{
				List<Expression> exps = new List<Expression>();
				if (!Accept(TokenType.SymbolCloseBracket))
				{
					exps.Add(ParseAssignmentExpression());
					while (Accept(TokenType.SymbolComma))
						exps.Add(ParseAssignmentExpression());
					Expect(TokenType.SymbolCloseBracket);
				}
				return new NewArrayExpression(exps);
			}
			else if (Accept(TokenType.SymbolPercent))
			{
				Expect(TokenType.SymbolOpenBracket);
				List<DictionaryInitializationEntry> exps = new List<DictionaryInitializationEntry>();
				if (!Accept(TokenType.SymbolCloseBracket))
				{
					exps.Add(ParseDictionaryInitializationEntry());
					while (Accept(TokenType.SymbolComma))
						exps.Add(ParseDictionaryInitializationEntry());
					Expect(TokenType.SymbolCloseBracket);
				}
				return new NewDictionaryExpression(exps);
			}
			else if (Accept(TokenType.SymbolOpenParenthesis))
			{
				var exp = ParseExpression();
				Expect(TokenType.SymbolCloseParenthesis);
				return exp;
			}
			else
				return new ConstantExpression(ParseLiteral());
		}

		DictionaryInitializationEntry ParseDictionaryInitializationEntry()
		{
			var key = ParseAssignmentExpression();
			Expect(TokenType.SymbolEqualsGreaterThan);
			var value = ParseAssignmentExpression();
			return new DictionaryInitializationEntry(key, value);
		}

		object ParseLiteral()
		{
			if (Accept(TokenType.KeywordTrue))
				return 1L;
			else if (Accept(TokenType.KeywordFalse))
				return 0L;
			else if (Accept(TokenType.KeywordNull))
				return null;
			else if (Accept(TokenType.KeywordVoid))
				return IronTjs.Builtins.Void.Value;
			else if (_tokenizer.NextToken.Type == TokenType.LiteralInteger)
				return _tokenizer.Read().Value;
			else if (_tokenizer.NextToken.Type == TokenType.LiteralReal)
				return _tokenizer.Read().Value;
			else
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(Expect(TokenType.LiteralString).Value.ToString());
				while (_tokenizer.NextToken.Type == TokenType.LiteralString)
					sb.Append(_tokenizer.Read().Value.ToString());
				return sb.ToString();
			}
		}
	}
}
