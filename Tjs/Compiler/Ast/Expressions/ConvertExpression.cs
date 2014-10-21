﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronTjs.Compiler.Ast
{
	public class ConvertExpression : Expression
	{
		public ConvertExpression(Expression operand, ConvertType toType)
		{
			Operand = operand;
			ToType = toType;
			Operand.Parent = this;
		}

		public Expression Operand { get; private set; }

		public ConvertType ToType { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			Type type;
			switch (ToType)
			{
				case ConvertType.Integer:
				default:
					type = typeof(long);
					break;
				case ConvertType.Real:
					type = typeof(double);
					break;
				case ConvertType.String:
					type = typeof(string);
					break;
			}
			return System.Linq.Expressions.Expression.Convert(IronTjs.Runtime.Binding.Binders.Convert(LanguageContext, Operand.TransformRead(), type), typeof(object));
		}

		public override System.Linq.Expressions.Expression TransformVoid() { return Operand.TransformVoid(); }
	}

	public enum ConvertType
	{
		Default = 0,
		Integer = 0,
		Real,
		String,
	}
}
