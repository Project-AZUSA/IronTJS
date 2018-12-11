using System;
using IronTJS.Hosting;
using IronTJS.Runtime;
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
        scope.SetVariable("print", new Function((global, context, args) =>
        {
            if (args.Length <= 0)
                ConsoleIO.WriteLine();
            else if (args.Length <= 1)
                ConsoleIO.WriteLine(string.Concat(args[0]), Style.Out);
            else if (args[0] != null)
                ConsoleIO.WriteLine(string.Format(args[0].ToString(), ArrayUtils.RemoveFirst(args)), Style.Out);
            return IronTJS.Builtins.Void.Value;
        }, null, null));
        scope.SetVariable("scan", new Function((global, context, args) =>
        {
            if (args.Length > 0)
                ConsoleIO.Write(string.Concat(args[0]), Style.Prompt);
            return ConsoleIO.ReadLine(-1);
        }, null, null));
        scope.SetVariable("Array", Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(IronTJS.Builtins.Array)));
        scope.SetVariable("Dictionary", IronTJS.Builtins.Dictionary.GetClass());
        scope.SetVariable("Exception", Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(Exception)));
        scope.SetVariable("Math", Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(IronTJS.Builtins.Math)));
        scope.SetVariable("System", Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(IronTJS.Builtins.KrSystem)));
    }

    static int Main(string[] args)
    {
        return new TjsConsoleHost().Run(args);
    }
}
