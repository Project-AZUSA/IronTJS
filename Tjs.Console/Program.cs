﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronTjs.Hosting;
using IronTjs.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Utils;

sealed class TjsConsoleHost : ConsoleHost
{
	protected override Type Provider { get { return typeof(TjsContext); } }

	protected override CommandLine CreateCommandLine() { return new TjsCommandLine(InitializeScope); }

	protected override IConsole CreateConsole(ScriptEngine engine, CommandLine commandLine, ConsoleOptions options)
	{
		ContractUtils.RequiresNotNull(options, "options");
		return new TjsConsole(options.ColorfulConsole, options.AutoIndentSize);
	}

	protected override OptionsParser CreateOptionsParser()
	{
		var parser = new OptionsParser<ConsoleOptions>();
		parser.CommonConsoleOptions.AutoIndent = true;
		parser.CommonConsoleOptions.ColorfulConsole = true;
		return parser;
	}

	void InitializeScope(ScriptScope scope)
	{
		scope.SetVariable("print", new Function((context, args) =>
		{
			if (args.Length <= 0)
				ConsoleIO.WriteLine();
			else if (args.Length <= 1)
				ConsoleIO.WriteLine(string.Concat(args[0]), Style.Out);
			else if (args[0] != null)
				ConsoleIO.WriteLine(string.Format(args[0].ToString(), Microsoft.Scripting.Utils.ArrayUtils.RemoveFirst(args)), Style.Out);
			return IronTjs.Builtins.Void.Value;
		}, null));
		scope.SetVariable("scan", new Function((context, args) =>
		{
			if (args.Length > 0)
				ConsoleIO.Write(string.Concat(args[0]), Style.Prompt);
			return ConsoleIO.ReadLine(-1);
		}, null));
		scope.SetVariable("Array", Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(IronTjs.Builtins.Array)));
	}

	static int Main(string[] args)
	{
		return new TjsConsoleHost().Run(args);
	}
}
