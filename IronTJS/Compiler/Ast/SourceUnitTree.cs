using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronTJS.Compiler.Ast
{
    public class SourceUnitTree : Node, INameResolver, IContextHolder
	{
		public SourceUnitTree(IEnumerable<ClassDefinition> classes, IEnumerable<FunctionDefinition> functions, IEnumerable<PropertyDefinition> properties, IEnumerable<Statement> statements, CompilerContext context)
		{
			Classes = classes.ToReadOnly();
			foreach (var cls in Classes)
				cls.Parent = this;
			Functions = functions.ToReadOnly();
			foreach (var function in Functions)
				function.Parent = this;
			Properties = properties.ToReadOnly();
			foreach (var prop in Properties)
				prop.Parent = this;
			Statements = statements.ToReadOnly();
			foreach (var statement in Statements)
				statement.Parent = this;
			_context = System.Linq.Expressions.Expression.Parameter(typeof(object), "global");
			CompilerContext = context;
		}

		System.Linq.Expressions.ParameterExpression _context;

		public CompilerContext CompilerContext { get; private set; }

		public ReadOnlyCollection<ClassDefinition> Classes { get; private set; }

		public ReadOnlyCollection<FunctionDefinition> Functions { get; private set; }

		public ReadOnlyCollection<PropertyDefinition> Properties { get; private set; }

		public ReadOnlyCollection<Statement> Statements { get; private set; }

		public System.Linq.Expressions.Expression Context { get { return _context; } }

		public System.Linq.Expressions.Expression GlobalContext { get { return _context; } }

		public System.Linq.Expressions.Expression<TDelegate> Transform<TDelegate>() where TDelegate : class
		{
			var signature = Utils.GetDelegateSignature(typeof(TDelegate));
			bool hasResult;
			if (signature.Length == 2 && signature[0] == typeof(object) && ((hasResult = signature[1] == typeof(object)) || signature[1] == typeof(void)))
			{
				List<System.Linq.Expressions.Expression> exps = new List<System.Linq.Expressions.Expression>();
				foreach (var cls in Classes)
					exps.Add(cls.Register(Context));
				foreach (var func in Functions)
					exps.Add(func.Register(Context));
				foreach (var prop in Properties)
					exps.Add(prop.Register(Context));
				for (int i = 0; i < Statements.Count - 1; i++)
					exps.Add(Statements[i].Transform());
				if (Statements.Count > 0)
				{
					ExpressionStatement lastExp;
					if (!hasResult)
						exps.Add(Statements[Statements.Count - 1].Transform());
					else if ((lastExp = Statements[Statements.Count - 1] as ExpressionStatement) != null)
						exps.Add(lastExp.Expression.TransformRead());
					else
					{
						exps.Add(Statements[Statements.Count - 1].Transform());
						exps.Add(System.Linq.Expressions.Expression.Constant(Builtins.Void.Value));
					}
				}
				System.Linq.Expressions.Expression body;
				if (exps.Count > 1)
					body = System.Linq.Expressions.Expression.Block(exps);
				else if (exps.Count == 1)
					body = exps[0];
				else if (hasResult)
					body = System.Linq.Expressions.Expression.Constant(Builtins.Void.Value);
				else
					body = System.Linq.Expressions.Expression.Empty();
				return System.Linq.Expressions.Expression.Lambda<TDelegate>(body, _context);
			}
			throw new ArgumentException("無効なデリゲート型です。");
		}

		public System.Linq.Expressions.Expression ResolveForRead(string name, bool direct)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateGetMemberBinder(name, false, direct), typeof(object), Context);
		}

		public System.Linq.Expressions.Expression ResolveForWrite(string name, System.Linq.Expressions.Expression value, bool direct)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(name, false, false, direct), typeof(object), Context, value);
		}

		public System.Linq.Expressions.Expression ResolveForDelete(string name)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateDeleteMemberBinder(name, false, true), typeof(object), Context);
		}

		public System.Linq.Expressions.Expression DeclareVariable(string name, System.Linq.Expressions.Expression value)
		{
			return System.Linq.Expressions.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(name, false, true, true), typeof(object), Context, value);
		}
	}
}
