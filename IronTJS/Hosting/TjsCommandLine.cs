using System;
using System.Linq;
using IronTJS.Compiler;
using IronTJS.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;

namespace IronTJS.Hosting
{
    public class TjsCommandLine : CommandLine
	{
		public TjsCommandLine(Action<ScriptScope> scopeInitializer) { _scopeInitializer = scopeInitializer; }

		Action<ScriptScope> _scopeInitializer;

		protected override string Logo
		{
			get
			{
				return string.Format("{0} {1} on .NET {2}\nTJS2 Interpreter for .NET Framework\n\n", TjsContext.DisplayName, TjsContext.TjsVersion.ToString(), Environment.Version);
			}
		}

		protected override int Run()
		{
			ScriptScope = Engine.CreateScope();
			_scopeInitializer(ScriptScope);
			return base.Run();
		}

		protected override void ExecuteCommand(string command)
		{
			var result = ExecuteCommand(Engine.CreateScriptSourceFromString(command, SourceCodeKind.InteractiveCode));
			if (result == null)
				Console.WriteLine("null", Style.Out);
			else if (result.GetType() == typeof(Builtins.Void))
				Console.WriteLine("void", Style.Out);
			else if (result.GetType() == typeof(string))
				Console.WriteLine(string.Format("\"{0}\"", result), Style.Out);
			else
				Console.WriteLine(result.ToString(), Style.Out);
		}

		protected override int GetNextAutoIndentSize(string text)
		{
			var lastLine = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
			if (lastLine == null)
				return 0;
			return Parser.GetNextAutoIndentSize(Language.CreateSnippet(lastLine, SourceCodeKind.InteractiveCode), Options.AutoIndentSize);
		}
	}
}
