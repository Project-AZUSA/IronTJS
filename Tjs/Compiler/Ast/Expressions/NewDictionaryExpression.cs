using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Scripting.Utils;

namespace IronTjs.Compiler.Ast
{
    public class NewDictionaryExpression : Expression
	{
		public NewDictionaryExpression(IEnumerable<DictionaryInitializationEntry> entries)
		{
			Entries = entries.ToReadOnly();
			foreach (var entry in Entries)
			{
				entry.Key.Parent = this;
				entry.Value.Parent = this;
			}
		}

		public ReadOnlyCollection<DictionaryInitializationEntry> Entries { get; private set; }

		public override System.Linq.Expressions.Expression TransformRead()
		{
			return System.Linq.Expressions.Expression.New(
				(System.Reflection.ConstructorInfo)Utils.GetMember(() => new Builtins.Dictionary(null)),
				System.Linq.Expressions.Expression.NewArrayInit(typeof(KeyValuePair<string, object>), Entries.Select(x => x.TransformRead(LanguageContext)))
			);
		}

		public override System.Linq.Expressions.Expression TransformVoid()
		{
			return System.Linq.Expressions.Expression.Block(
				Entries.SelectMany(x => new[] { x.Key.TransformVoid(), x.Value.TransformVoid() })
			);
		}
	}

	public class DictionaryInitializationEntry
	{
		public DictionaryInitializationEntry(Expression key, Expression value)
		{
			Key = key;
			Value = value;
		}

		public Expression Key { get; private set; }

		public Expression Value { get; private set; }

		public System.Linq.Expressions.Expression TransformRead(Runtime.TjsContext context)
		{
			return System.Linq.Expressions.Expression.New((System.Reflection.ConstructorInfo)Utils.GetMember(() => new KeyValuePair<string, object>(null, null)),
				Runtime.Binding.Binders.Convert(context, Key.TransformRead(), typeof(string)),
				Runtime.Binding.Binders.Convert(context, Value.TransformRead(), typeof(object))
			);
		}
	}
}
