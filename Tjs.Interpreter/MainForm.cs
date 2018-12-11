using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Controls;
using IronTJS.Compiler;
using IronTJS.Runtime;

namespace IronTJS
{
    public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			InitializeLanguageContext();
		}

        TjsContext context;
		string savedFileName = null;

		void InitializeLanguageContext()
		{
			var options = new Dictionary<string, object>();
			context = new TjsContext(
				new Microsoft.Scripting.Runtime.ScriptDomainManager(
					new DefaultHostingProvider(),
					new Microsoft.Scripting.Runtime.DlrConfiguration(false, false, options)
				),
				options
			);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			rtbSource.LanguageOption = RichTextBoxLanguageOptions.UIFonts;
			rtbSource.HighlightTokenizer = new HighlightTokenizer(rtbSource);
			rtbSource.SetTabStops(16);
			var dir = new DirectoryInfo("samples");
			if (dir.Exists)
			{
				var topItem = new ToolStripMenuItem("TJSサンプルスクリプト(&T)");
				tsmiFile.DropDownItems.Insert(2, topItem);
				foreach (var item in dir.EnumerateFiles("*.tjs", SearchOption.TopDirectoryOnly))
				{
					var targetItem = topItem.DropDownItems.Add(item.Name);
					targetItem.Tag = item.FullName;
					targetItem.Click += SampleScript_Click;
				}
			}
		}

		void SampleScript_Click(object sender, EventArgs e)
		{
			var file = ((ToolStripItem)sender).Tag.ToString();
			if (File.Exists(file))
				OpenFile(file);
		}

		class DefaultHostingProvider : Microsoft.Scripting.Runtime.DynamicRuntimeHostingProvider
		{
			public override Microsoft.Scripting.PlatformAdaptationLayer PlatformAdaptationLayer { get { return Microsoft.Scripting.PlatformAdaptationLayer.Default; } }
		}

		class HighlightTokenizer : IHighlightTokenizer
		{
			public HighlightTokenizer(TextBoxBase textBox) { this.textBox = textBox; }

			TextBoxBase textBox;

			public IEnumerable<HighlightToken> GetTokens(string text)
			{
                Tokenizer tokenizer = new Tokenizer();
				int end = 0;
				tokenizer.ErrorSink = Microsoft.Scripting.ErrorSink.Null;
				tokenizer.Initialize(null, new StringReader(text), null, Microsoft.Scripting.SourceLocation.MinValue);
				var tokens = new List<Token>();
				while (tokenizer.NextToken.Type != TokenType.EndOfStream)
					tokens.Add(tokenizer.Read());
				foreach (var token in tokens)
				{
					var start = token.Span.Start.Index;
					if (end != start)
						yield return new HighlightToken(end, start - end, Color.Green, Color.Empty);
					end = token.Span.End.Index;
					switch (token.Type)
					{
						case TokenType.KeywordClass:
						case TokenType.KeywordConst:
						case TokenType.KeywordEnum:
						case TokenType.KeywordExtends:
						case TokenType.KeywordFunction:
						case TokenType.KeywordPrivate:
						case TokenType.KeywordProperty:
						case TokenType.KeywordProtected:
						case TokenType.KeywordPublic:
						case TokenType.KeywordStatic:
						case TokenType.KeywordSynchronized:
						case TokenType.KeywordInt:
						case TokenType.KeywordOctet:
						case TokenType.KeywordReal:
						case TokenType.KeywordString:
						case TokenType.KeywordVar:
						case TokenType.KeywordBreak:
						case TokenType.KeywordCase:
						case TokenType.KeywordCatch:
						case TokenType.KeywordContinue:
						case TokenType.KeywordDebugger:
						case TokenType.KeywordDefault:
						case TokenType.KeywordDelete:
						case TokenType.KeywordDo:
						case TokenType.KeywordElse:
						case TokenType.KeywordExport:
						case TokenType.KeywordFinally:
						case TokenType.KeywordFor:
						case TokenType.KeywordGetter:
						case TokenType.KeywordGlobal:
						case TokenType.KeywordGoTo:
						case TokenType.KeywordIf:
						case TokenType.KeywordImport:
						case TokenType.KeywordIn:
						case TokenType.KeywordInContextOf:
						case TokenType.KeywordInstanceOf:
						case TokenType.KeywordInvalidate:
						case TokenType.KeywordIsValid:
						case TokenType.KeywordNew:
						case TokenType.KeywordReturn:
						case TokenType.KeywordSetter:
						case TokenType.KeywordSuper:
						case TokenType.KeywordSwitch:
						case TokenType.KeywordThis:
						case TokenType.KeywordThrow:
						case TokenType.KeywordTry:
						case TokenType.KeywordTypeOf:
						case TokenType.KeywordWhile:
						case TokenType.KeywordWith:
						case TokenType.KeywordFalse:
						case TokenType.KeywordNull:
						case TokenType.KeywordTrue:
						case TokenType.KeywordVoid:
							yield return new HighlightToken(start, end - start, Color.Blue, Color.Empty);
							break;
						case TokenType.LiteralString:
							yield return new HighlightToken(start, end - start, Color.FromArgb(163, 21, 21), Color.Empty);
							break;
						case TokenType.Unknown:
							yield return new HighlightToken(start, end - start, Color.White, Color.Red);
							break;
					}
				}
				if (end != text.Length)
					yield return new HighlightToken(end, text.Length - end, Color.Green, Color.Empty);
			}
		}

		class ListErrorSink : Microsoft.Scripting.ErrorSink
		{
			public ListErrorSink(ListView listView) { lv = listView; }

			ListView lv;

			public override void Add(Microsoft.Scripting.SourceUnit source, string message, Microsoft.Scripting.SourceSpan span, int errorCode, Microsoft.Scripting.Severity severity)
			{
				Add(message, span);
			}

			public override void Add(string message, string path, string code, string line, Microsoft.Scripting.SourceSpan span, int errorCode, Microsoft.Scripting.Severity severity)
			{
				Add(message, span);
			}

			void Add(string message, Microsoft.Scripting.SourceSpan span)
			{
				lv.Items.Add(new ListViewItem(new[] { message, span.Start.Line.ToString(), span.Start.Column.ToString() }) { Tag = span });
			}
		}

		void OpenFile(string fileName)
		{
			rtbSource.Text = File.ReadAllText(fileName, Encoding.UTF8);
			savedFileName = fileName;
		}

		void tsmiNew_Click(object sender, EventArgs e) { rtbSource.Clear(); }

		void tsmiOpen_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = "TJSソースコード|*.tjs";
				if (dialog.ShowDialog() == DialogResult.OK)
					OpenFile(dialog.FileName);
			}
		}

		void tsmiSave_Click(object sender, EventArgs e)
		{
			if (savedFileName == null)
				tsmiSaveAs_Click(sender, e);
			else
				File.WriteAllText(savedFileName, rtbSource.Text, Encoding.UTF8);
		}

		void tsmiSaveAs_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.Filter = "TJSソースコード|*.tjs";
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					File.WriteAllText(dialog.FileName, rtbSource.Text, Encoding.UTF8);
					savedFileName = dialog.FileName;
				}
			}
		}

		void tsmiExit_Click(object sender, EventArgs e) { Close(); }

		void tsmiUndo_Click(object sender, EventArgs e) { rtbSource.Undo(); }

		void tsmiRedo_Click(object sender, EventArgs e) { rtbSource.Redo(); }

		void tsmiCut_Click(object sender, EventArgs e) { rtbSource.Cut(); }

		void tsmiCopy_Click(object sender, EventArgs e) { rtbSource.Copy(); }

		void tsmiPaste_Click(object sender, EventArgs e) { rtbSource.Paste(); }

		void tsmiSelectAll_Click(object sender, EventArgs e) { rtbSource.SelectAll(); }

		void tsmiStartDebug_Click(object sender, EventArgs e)
		{
			try
			{
				lvParseResults.Items.Clear();
				var sourceUnit = context.CreateSnippet(rtbSource.Text, Microsoft.Scripting.SourceCodeKind.File);
				var scriptCode = context.CompileSourceCode(sourceUnit, new Microsoft.Scripting.CompilerOptions(), new ListErrorSink(lvParseResults));
				if (scriptCode != null)
				{
					var scope = scriptCode.CreateScope();
					InitializeStorage(scope.Storage);
					scriptCode.Run(scope);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, ex.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		void InitializeStorage(dynamic storage)
		{
			storage.print = new Function((global, ctx, args) =>
			{
				if (args.Length <= 0)
					MessageBox.Show(string.Empty);
				else if (args.Length <= 1)
					MessageBox.Show(string.Concat(args[0]));
				else if (args[0] != null)
					MessageBox.Show(string.Format(args[0].ToString(), Microsoft.Scripting.Utils.ArrayUtils.RemoveFirst(args)));
				return Builtins.Void.Value;
			}, null, null);
			storage.scan = new Function((global, ctx, args) =>
			{
				using (InputBox box = new InputBox())
				{
					if (args.Length > 0)
						box.Description = string.Concat(args[0]);
					box.ShowDialog(this);
					return box.InputText;
				}
			}, null, null);
			storage.Array = Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(Builtins.Array));
			storage.Dictionary = Builtins.Dictionary.GetClass();
			storage.Exception = Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(Exception));
			storage.Math = Microsoft.Scripting.Actions.MemberTracker.FromMemberInfo(typeof(Builtins.Math));
		}

		void lvParseResults_DoubleClick(object sender, EventArgs e)
		{
			if (lvParseResults.SelectedItems.Count > 0)
			{
				var span = (Microsoft.Scripting.SourceSpan)lvParseResults.SelectedItems[0].Tag;
				rtbSource.Select(span.Start.Index, 0);
				rtbSource.Select();
			}
		}
	}
}
