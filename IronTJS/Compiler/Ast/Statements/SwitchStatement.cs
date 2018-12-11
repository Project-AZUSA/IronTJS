using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Utils;
using IronTJS.Runtime.Binding;

namespace IronTJS.Compiler.Ast
{
    public class SwitchStatement : Statement, IBreakableStatement
	{
		public SwitchStatement(Expression expression, IEnumerable<SwitchCase> cases)
		{
			Expression = expression;
			Cases = cases.ToReadOnly();
			if (Cases.Count(x => x.ContainsDefault) > 1)
				throw new ArgumentException("default ラベルは switch ステートメント内に複数存在できません。");
			Expression.Parent = this;
			foreach (var cs in Cases)
				cs.Parent = this;
		}

		System.Linq.Expressions.LabelTarget _breakLabel = System.Linq.Expressions.Expression.Label();

		public Expression Expression { get; private set; }

		public ReadOnlyCollection<SwitchCase> Cases { get; private set; }

		public System.Linq.Expressions.LabelTarget BreakLabel { get { return _breakLabel; } }

		public override System.Linq.Expressions.Expression Transform()
		{
			var transformedExp = Expression.TransformRead();
			var transformedCases = Cases.Select(x => x.Transform()).ToArray();
			var hiddenVar = System.Linq.Expressions.Expression.Variable(typeof(object), "__switchVariable__");
			List<System.Linq.Expressions.Expression> exp = new List<System.Linq.Expressions.Expression>();
			exp.Add(System.Linq.Expressions.Expression.Assign(hiddenVar, transformedExp));
			var defaultIndex = Cases.FindIndex(x => x.ContainsDefault);
			var builder = Microsoft.Scripting.Ast.Utils.If();
			for (int i = 0; i < transformedCases.Length; i++)
			{
				if (transformedCases[i].Item1.Length > 0)
				{
					var test = LanguageContext.Convert(BinaryExpression.TransformSimpleOperation(LanguageContext, hiddenVar, transformedCases[i].Item1[0], BinaryOperator.Equal), typeof(bool));
					for (int j = 1; j < transformedCases[i].Item1.Length; i++)
						test = System.Linq.Expressions.Expression.OrElse(test, LanguageContext.Convert(BinaryExpression.TransformSimpleOperation(LanguageContext, hiddenVar, transformedCases[i].Item1[i], BinaryOperator.Equal), typeof(bool)));
					builder.ElseIf(test, System.Linq.Expressions.Expression.Goto(Cases[i].CaseLabel));
				}
			}
			exp.Add(builder.Else(System.Linq.Expressions.Expression.Goto(defaultIndex >= 0 ? Cases[defaultIndex].CaseLabel : BreakLabel)));
			for (int i = 0; i < transformedCases.Length; i++)
			{
				exp.Add(System.Linq.Expressions.Expression.Label(Cases[i].CaseLabel));
				foreach (var transformedStatement in transformedCases[i].Item2)
					exp.Add(transformedStatement);
			}
			exp.Add(System.Linq.Expressions.Expression.Label(BreakLabel));
			return System.Linq.Expressions.Expression.Block(new[] { hiddenVar }, exp);
		}
	}

	public class SwitchCase : Node
	{
		public SwitchCase(IEnumerable<Expression> testExpressions, bool containsDefault, IEnumerable<Statement> body)
		{
			TestExpressions = testExpressions.ToReadOnly();
			if (TestExpressions.Count <= 0 && !containsDefault)
				throw new ArgumentException("case 節は 1 個以上のテスト式を含むか、default ラベルがなければなりません。");
			foreach (var testExp in TestExpressions)
				testExp.Parent = this;
			ContainsDefault = containsDefault;
			Body = body.ToReadOnly();
			foreach (var statement in Body)
				statement.Parent = this;
			CaseLabel = System.Linq.Expressions.Expression.Label();
		}

		public ReadOnlyCollection<Expression> TestExpressions { get; private set; }

		public bool ContainsDefault { get; private set; }

		public System.Linq.Expressions.LabelTarget CaseLabel { get; private set; }

		public ReadOnlyCollection<Statement> Body { get; private set; }

		public Tuple<System.Linq.Expressions.Expression[], System.Linq.Expressions.Expression[]> Transform()
		{
			var transformedTest = TestExpressions.Select(x => x.TransformRead()).ToArray();
			var transformedBody = Body.Select(x => x.Transform()).ToArray();
			return new Tuple<System.Linq.Expressions.Expression[], System.Linq.Expressions.Expression[]>(transformedTest, transformedBody);
		}
	}
}
