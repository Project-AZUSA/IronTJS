using System;
using MSAst = System.Linq.Expressions;

namespace IronTjs.Compiler.Ast
{
    public class PropertyDefinition : Node
	{
		public PropertyDefinition(string name, FunctionDefinition getter, FunctionDefinition setter)
		{
			Name = name;
			Getter = getter;
			Setter = setter;
			if (Getter != null)
				Getter.Parent = this;
			if (Setter != null)
				Setter.Parent = this;
		}

		public string Name { get; private set; }

		public FunctionDefinition Getter { get; private set; }

		public FunctionDefinition Setter { get; private set; }

		public MSAst.Expression TransformProperty(MSAst.Expression context)
		{
			MSAst.Expression getter;
			if (Getter != null)
				getter = Getter.TransformLambda();
			else
				getter = MSAst.Expression.Constant(null, typeof(Func<object, object, object[], object>));
			MSAst.Expression setter;
			if (Setter != null)
				setter = Setter.TransformLambda();
			else
				setter = MSAst.Expression.Constant(null, typeof(Func<object, object, object[], object>));
			return MSAst.Expression.New((System.Reflection.ConstructorInfo)Utils.GetMember(() => new Runtime.Property(null, null, null, null)),
				getter,
				setter,
				GlobalParent.Context,
				context
			);
		}

		public MSAst.Expression Register(MSAst.Expression registeredTo)
		{
			return MSAst.Expression.Dynamic(LanguageContext.CreateSetMemberBinder(Name, false, true, true), typeof(object), registeredTo, TransformProperty(registeredTo));
		}
	}
}
