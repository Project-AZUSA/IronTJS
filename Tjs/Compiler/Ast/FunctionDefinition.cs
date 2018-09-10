using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using IronTjs.Runtime.Binding;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
    using Ast = System.Linq.Expressions.Expression;

    public class FunctionDefinition : Node, INameResolver, IContextHolder
	{
		public FunctionDefinition(string name, IEnumerable<ParameterDefinition> parameters, IEnumerable<Statement> body)
		{
			Name = name;
			Parameters = parameters.ToReadOnly();
			foreach (var param in Parameters)
				param.Parent = this;
			Body = body.ToReadOnly();
			foreach (var statement in Body)
				statement.Parent = this;
			ReturnLabel = Ast.Label(typeof(object));
		}
		
		Dictionary<string, System.Linq.Expressions.ParameterExpression> _variables = new Dictionary<string, System.Linq.Expressions.ParameterExpression>();
		// actual formal parameters 
		System.Linq.Expressions.ParameterExpression _global = Ast.Parameter(typeof(object), "global");
		System.Linq.Expressions.ParameterExpression _context = Ast.Parameter(typeof(object), "self");
		System.Linq.Expressions.ParameterExpression _arguments = Ast.Parameter(typeof(object[]), "args");

		public Ast GlobalContext { get { return _global; } }

		public Ast Context { get { return _context; } }

		public Ast Arguments { get { return _arguments; } }

		public string Name { get; private set; }

		public ReadOnlyCollection<ParameterDefinition> Parameters { get; private set; }

		public ReadOnlyCollection<Statement> Body { get; private set; }

		public System.Linq.Expressions.LabelTarget ReturnLabel { get; private set; }
		
		public Ast ResolveForRead(string name, bool direct)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			if (param != null || _variables.TryGetValue(name, out param))
				return param;
			else
				return Ast.Dynamic(new ThisProxyMemberAccessBinder(
					LanguageContext, name, false,
					direct ? MemberAccessKind.Get | MemberAccessKind.Direct : MemberAccessKind.Get
				), typeof(object), _context, _global);
		}

		public Ast ResolveForWrite(string name, Ast value, bool direct)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			if (param != null || _variables.TryGetValue(name, out param))
				return Ast.Assign(param, value);
			else
				return Ast.Dynamic(new ThisProxyMemberAccessBinder(
					LanguageContext, name, false,
					direct ? MemberAccessKind.Set | MemberAccessKind.Direct : MemberAccessKind.Set
				), typeof(object), _context, _global, value);
		}

		public Ast ResolveForDelete(string name)
		{
			var param = Parameters.Select(x => x.ParameterVariable).FirstOrDefault(x => x != null && x.Name == name);
			if (param != null || _variables.TryGetValue(name, out param))
				return Ast.Constant(0L);
			else
				return Ast.Dynamic(new ThisProxyMemberAccessBinder(
					LanguageContext, name, false, MemberAccessKind.Delete
				), typeof(object), _context, _global);
		}

		public Ast DeclareVariable(string name, Ast value)
		{
			System.Linq.Expressions.ParameterExpression param;
			if (!_variables.TryGetValue(name, out param))
				_variables[name] = param = Ast.Variable(typeof(object), name);
			return Ast.Assign(param, value);
		}

		// Function Body Code Generation
		//
		// object Func(object self, object[] args) {
		//     object p1 = ...;
		//     object p2 = ...;
		//     ...
		//     object pn = ...;
		//     (..Body..)
		// }
		//
		// <if has_default(p[i]) then>
		//     <p[i]> = <i> < args.Length ? (args[<i>] == void ? <default(p[i])> : args[<i>]) : <default(p[i])>;
		// <else if any_length_args(p[i]) then>
		//     <if has_name(p[i]) then>
		//         <p[i]> = new Array(args.Skip(<i>));
		//     <else>
		//         <p[i]> = new UnnamedSpreadArguments(args, <i>);
		//     <end if>
		// <else>
		//     <p[i]> = <i> < args.Length ? args[<i>] : void;
		// <end if>

		public System.Linq.Expressions.Expression<Func<object, object, object[], object>> TransformLambda()
		{
			List<Ast> body = new List<Ast>();
			for (int i = 0; i < Parameters.Count; i++)
			{
				var capCheck = Ast.LessThan(Ast.Constant(i), Ast.Property(_arguments, "Length"));
				Ast exp;
				if (Parameters[i].HasDefaultValue)
					exp = Ast.Condition(capCheck,
						Ast.Condition(Ast.TypeEqual(Ast.ArrayAccess(_arguments, Ast.Constant(i)), typeof(Builtins.Void)),
							Ast.Constant(Parameters[i].DefaultValue, typeof(object)),
							Ast.ArrayAccess(_arguments, Ast.Constant(i))
						),
						Ast.Constant(Parameters[i].DefaultValue, typeof(object))
					);
				else if (Parameters[i].ExpandToArray)
				{
					if (Parameters[i].ParameterVariable.Name != null)
						exp = Ast.New((System.Reflection.ConstructorInfo)Utils.GetMember(() => new Builtins.Array(null)),
							Ast.Call(new Func<IEnumerable<object>, int, IEnumerable<object>>(Enumerable.Skip).Method, _arguments, Ast.Constant(i))
						);
					else
						exp = Ast.New((System.Reflection.ConstructorInfo)Utils.GetMember(() => new Runtime.UnnamedSpreadArguments(null, 0)),
							_arguments,
							Ast.Constant(i)
						);
				}
				else
					exp = Ast.Condition(capCheck,
						Ast.ArrayAccess(_arguments, Ast.Constant(i)),
						Ast.Constant(Builtins.Void.Value, typeof(object))
					);
				body.Add(Ast.Assign(Parameters[i].ParameterVariable, exp));
			}
			foreach (var statement in Body)
				body.Add(statement.Transform());
			body.Add(Ast.Label(ReturnLabel, Ast.Constant(Builtins.Void.Value)));
			return Ast.Lambda<Func<object, object, object[], object>>(Ast.Block(_variables.Values.Concat(Parameters.Select(x => x.ParameterVariable)), body), Name, new[] { _global, _context, _arguments });
		}

		public Ast TransformFunction(Ast context)
		{
			var lambda = TransformLambda();
			return Ast.New((System.Reflection.ConstructorInfo)Utils.GetMember(() => new Runtime.Function(null, null, null)),
				lambda,
				GlobalParent.Context,
				context
			);
		}

		public Ast Register(Ast registeredTo)
		{
			return Ast.Dynamic(LanguageContext.CreateSetMemberBinder(Name, false, true, true), typeof(object), registeredTo,
				TransformFunction(registeredTo)
			);
		}
	}

	public class ParameterDefinition : Node
	{
		public ParameterDefinition(string name, object defaultValue) : this(name, false)
		{
			DefaultValue = defaultValue;
			HasDefaultValue = true;
		}

		public ParameterDefinition(string name, bool expandToArray)
		{
			ParameterVariable = Ast.Variable(typeof(object), name);
			ExpandToArray = expandToArray;
		}

		public System.Linq.Expressions.ParameterExpression ParameterVariable { get; private set; }

		public bool HasDefaultValue { get; private set; }

		public object DefaultValue { get; private set; }

		public bool ExpandToArray { get; private set; }
	}
}
