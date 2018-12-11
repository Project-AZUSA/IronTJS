namespace IronTJS
{
	partial class MainForm
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.msMain = new System.Windows.Forms.MenuStrip();
			this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiNew = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.tssFile1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.tssFile2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiUndo = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiRedo = new System.Windows.Forms.ToolStripMenuItem();
			this.tssEdit1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsmiCut = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.tssEdit2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsmiSelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiDebug = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiStartDebug = new System.Windows.Forms.ToolStripMenuItem();
			this.tsMain = new System.Windows.Forms.ToolStrip();
			this.tsbNew = new System.Windows.Forms.ToolStripButton();
			this.tsbOpen = new System.Windows.Forms.ToolStripButton();
			this.tsbSave = new System.Windows.Forms.ToolStripButton();
			this.tssMain1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbCut = new System.Windows.Forms.ToolStripButton();
			this.tsbCopy = new System.Windows.Forms.ToolStripButton();
			this.tsbPaste = new System.Windows.Forms.ToolStripButton();
			this.tssMain2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbStartDebug = new System.Windows.Forms.ToolStripButton();
			this.rtbSource = new Controls.WindowsForms.SyntaxHighlightingTextBox();
			this.lvParseResults = new System.Windows.Forms.ListView();
			this.clmDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.clmLine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.clmColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.splMain = new System.Windows.Forms.SplitContainer();
			this.msMain.SuspendLayout();
			this.tsMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splMain)).BeginInit();
			this.splMain.Panel1.SuspendLayout();
			this.splMain.Panel2.SuspendLayout();
			this.splMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// msMain
			// 
			this.msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.tsmiEdit,
            this.tsmiDebug});
			this.msMain.Location = new System.Drawing.Point(0, 0);
			this.msMain.Name = "msMain";
			this.msMain.Size = new System.Drawing.Size(383, 26);
			this.msMain.TabIndex = 0;
			this.msMain.Text = "menuStrip1";
			// 
			// tsmiFile
			// 
			this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNew,
            this.tsmiOpen,
            this.tssFile1,
            this.tsmiSave,
            this.tsmiSaveAs,
            this.tssFile2,
            this.tsmiExit});
			this.tsmiFile.Name = "tsmiFile";
			this.tsmiFile.Size = new System.Drawing.Size(85, 22);
			this.tsmiFile.Text = "ファイル(&F)";
			// 
			// tsmiNew
			// 
			this.tsmiNew.Image = ((System.Drawing.Image)(resources.GetObject("tsmiNew.Image")));
			this.tsmiNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsmiNew.Name = "tsmiNew";
			this.tsmiNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.tsmiNew.Size = new System.Drawing.Size(201, 22);
			this.tsmiNew.Text = "新規作成(&N)";
			this.tsmiNew.Click += new System.EventHandler(this.tsmiNew_Click);
			// 
			// tsmiOpen
			// 
			this.tsmiOpen.Image = ((System.Drawing.Image)(resources.GetObject("tsmiOpen.Image")));
			this.tsmiOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsmiOpen.Name = "tsmiOpen";
			this.tsmiOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.tsmiOpen.Size = new System.Drawing.Size(202, 22);
			this.tsmiOpen.Text = "開く(&O)";
			this.tsmiOpen.Click += new System.EventHandler(this.tsmiOpen_Click);
			// 
			// tssFile1
			// 
			this.tssFile1.Name = "tssFile1";
			this.tssFile1.Size = new System.Drawing.Size(199, 6);
			// 
			// tsmiSave
			// 
			this.tsmiSave.Image = ((System.Drawing.Image)(resources.GetObject("tsmiSave.Image")));
			this.tsmiSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsmiSave.Name = "tsmiSave";
			this.tsmiSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.tsmiSave.Size = new System.Drawing.Size(202, 22);
			this.tsmiSave.Text = "上書き保存(&S)";
			this.tsmiSave.Click += new System.EventHandler(this.tsmiSave_Click);
			// 
			// tsmiSaveAs
			// 
			this.tsmiSaveAs.Name = "tsmiSaveAs";
			this.tsmiSaveAs.Size = new System.Drawing.Size(202, 22);
			this.tsmiSaveAs.Text = "名前を付けて保存(&A)";
			this.tsmiSaveAs.Click += new System.EventHandler(this.tsmiSaveAs_Click);
			// 
			// tssFile2
			// 
			this.tssFile2.Name = "tssFile2";
			this.tssFile2.Size = new System.Drawing.Size(199, 6);
			// 
			// tsmiExit
			// 
			this.tsmiExit.Name = "tsmiExit";
			this.tsmiExit.Size = new System.Drawing.Size(202, 22);
			this.tsmiExit.Text = "終了(&X)";
			this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
			// 
			// tsmiEdit
			// 
			this.tsmiEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiUndo,
            this.tsmiRedo,
            this.tssEdit1,
            this.tsmiCut,
            this.tsmiCopy,
            this.tsmiPaste,
            this.tssEdit2,
            this.tsmiSelectAll});
			this.tsmiEdit.Name = "tsmiEdit";
			this.tsmiEdit.Size = new System.Drawing.Size(61, 22);
			this.tsmiEdit.Text = "編集(&E)";
			// 
			// tsmiUndo
			// 
			this.tsmiUndo.Name = "tsmiUndo";
			this.tsmiUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.tsmiUndo.Size = new System.Drawing.Size(190, 22);
			this.tsmiUndo.Text = "元に戻す(&U)";
			this.tsmiUndo.Click += new System.EventHandler(this.tsmiUndo_Click);
			// 
			// tsmiRedo
			// 
			this.tsmiRedo.Name = "tsmiRedo";
			this.tsmiRedo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.tsmiRedo.Size = new System.Drawing.Size(190, 22);
			this.tsmiRedo.Text = "やり直し(&R)";
			this.tsmiRedo.Click += new System.EventHandler(this.tsmiRedo_Click);
			// 
			// tssEdit1
			// 
			this.tssEdit1.Name = "tssEdit1";
			this.tssEdit1.Size = new System.Drawing.Size(187, 6);
			// 
			// tsmiCut
			// 
			this.tsmiCut.Image = ((System.Drawing.Image)(resources.GetObject("tsmiCut.Image")));
			this.tsmiCut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsmiCut.Name = "tsmiCut";
			this.tsmiCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.tsmiCut.Size = new System.Drawing.Size(190, 22);
			this.tsmiCut.Text = "切り取り(&T)";
			this.tsmiCut.Click += new System.EventHandler(this.tsmiCut_Click);
			// 
			// tsmiCopy
			// 
			this.tsmiCopy.Image = ((System.Drawing.Image)(resources.GetObject("tsmiCopy.Image")));
			this.tsmiCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsmiCopy.Name = "tsmiCopy";
			this.tsmiCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.tsmiCopy.Size = new System.Drawing.Size(190, 22);
			this.tsmiCopy.Text = "コピー(&C)";
			this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
			// 
			// tsmiPaste
			// 
			this.tsmiPaste.Image = ((System.Drawing.Image)(resources.GetObject("tsmiPaste.Image")));
			this.tsmiPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsmiPaste.Name = "tsmiPaste";
			this.tsmiPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.tsmiPaste.Size = new System.Drawing.Size(190, 22);
			this.tsmiPaste.Text = "貼り付け(&P)";
			this.tsmiPaste.Click += new System.EventHandler(this.tsmiPaste_Click);
			// 
			// tssEdit2
			// 
			this.tssEdit2.Name = "tssEdit2";
			this.tssEdit2.Size = new System.Drawing.Size(187, 6);
			// 
			// tsmiSelectAll
			// 
			this.tsmiSelectAll.Name = "tsmiSelectAll";
			this.tsmiSelectAll.Size = new System.Drawing.Size(190, 22);
			this.tsmiSelectAll.Text = "すべて選択(&A)";
			this.tsmiSelectAll.Click += new System.EventHandler(this.tsmiSelectAll_Click);
			// 
			// tsmiDebug
			// 
			this.tsmiDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiStartDebug});
			this.tsmiDebug.Name = "tsmiDebug";
			this.tsmiDebug.Size = new System.Drawing.Size(87, 22);
			this.tsmiDebug.Text = "デバッグ(&D)";
			// 
			// tsmiStartDebug
			// 
			this.tsmiStartDebug.Image = ((System.Drawing.Image)(resources.GetObject("tsmiStartDebug.Image")));
			this.tsmiStartDebug.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.tsmiStartDebug.Name = "tsmiStartDebug";
			this.tsmiStartDebug.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.tsmiStartDebug.Size = new System.Drawing.Size(188, 22);
			this.tsmiStartDebug.Text = "デバッグ開始(&S)";
			this.tsmiStartDebug.Click += new System.EventHandler(this.tsmiStartDebug_Click);
			// 
			// tsMain
			// 
			this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNew,
            this.tsbOpen,
            this.tsbSave,
            this.tssMain1,
            this.tsbCut,
            this.tsbCopy,
            this.tsbPaste,
            this.tssMain2,
            this.tsbStartDebug});
			this.tsMain.Location = new System.Drawing.Point(0, 26);
			this.tsMain.Name = "tsMain";
			this.tsMain.Size = new System.Drawing.Size(383, 25);
			this.tsMain.TabIndex = 1;
			this.tsMain.Text = "toolStrip1";
			// 
			// tsbNew
			// 
			this.tsbNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbNew.Image = ((System.Drawing.Image)(resources.GetObject("tsbNew.Image")));
			this.tsbNew.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbNew.Name = "tsbNew";
			this.tsbNew.Size = new System.Drawing.Size(23, 22);
			this.tsbNew.Text = "新規作成(&N)";
			this.tsbNew.Click += new System.EventHandler(this.tsmiNew_Click);
			// 
			// tsbOpen
			// 
			this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbOpen.Image = ((System.Drawing.Image)(resources.GetObject("tsbOpen.Image")));
			this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbOpen.Name = "tsbOpen";
			this.tsbOpen.Size = new System.Drawing.Size(23, 22);
			this.tsbOpen.Text = "開く(&O)";
			this.tsbOpen.Click += new System.EventHandler(this.tsmiOpen_Click);
			// 
			// tsbSave
			// 
			this.tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSave.Image = ((System.Drawing.Image)(resources.GetObject("tsbSave.Image")));
			this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbSave.Name = "tsbSave";
			this.tsbSave.Size = new System.Drawing.Size(23, 22);
			this.tsbSave.Text = "上書き保存(&S)";
			this.tsbSave.Click += new System.EventHandler(this.tsmiSave_Click);
			// 
			// tssMain1
			// 
			this.tssMain1.Name = "tssMain1";
			this.tssMain1.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbCut
			// 
			this.tsbCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbCut.Image = ((System.Drawing.Image)(resources.GetObject("tsbCut.Image")));
			this.tsbCut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbCut.Name = "tsbCut";
			this.tsbCut.Size = new System.Drawing.Size(23, 22);
			this.tsbCut.Text = "切り取り(&U)";
			this.tsbCut.Click += new System.EventHandler(this.tsmiCut_Click);
			// 
			// tsbCopy
			// 
			this.tsbCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbCopy.Image = ((System.Drawing.Image)(resources.GetObject("tsbCopy.Image")));
			this.tsbCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbCopy.Name = "tsbCopy";
			this.tsbCopy.Size = new System.Drawing.Size(23, 22);
			this.tsbCopy.Text = "コピー(&C)";
			this.tsbCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
			// 
			// tsbPaste
			// 
			this.tsbPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPaste.Image = ((System.Drawing.Image)(resources.GetObject("tsbPaste.Image")));
			this.tsbPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPaste.Name = "tsbPaste";
			this.tsbPaste.Size = new System.Drawing.Size(23, 22);
			this.tsbPaste.Text = "貼り付け(&P)";
			this.tsbPaste.Click += new System.EventHandler(this.tsmiPaste_Click);
			// 
			// tssMain2
			// 
			this.tssMain2.Name = "tssMain2";
			this.tssMain2.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbStartDebug
			// 
			this.tsbStartDebug.Image = ((System.Drawing.Image)(resources.GetObject("tsbStartDebug.Image")));
			this.tsbStartDebug.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbStartDebug.Name = "tsbStartDebug";
			this.tsbStartDebug.Size = new System.Drawing.Size(52, 22);
			this.tsbStartDebug.Text = "開始";
			this.tsbStartDebug.Click += new System.EventHandler(this.tsmiStartDebug_Click);
			// 
			// rtbSource
			// 
			this.rtbSource.AcceptsTab = true;
			this.rtbSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.rtbSource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtbSource.Font = new System.Drawing.Font("ＭＳ ゴシック", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtbSource.HighlightTokenizer = null;
			this.rtbSource.Location = new System.Drawing.Point(0, 0);
			this.rtbSource.Name = "rtbSource";
			this.rtbSource.Size = new System.Drawing.Size(383, 111);
			this.rtbSource.TabIndex = 0;
			this.rtbSource.Text = "";
			// 
			// lvParseResults
			// 
			this.lvParseResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmDescription,
            this.clmLine,
            this.clmColumn});
			this.lvParseResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvParseResults.FullRowSelect = true;
			this.lvParseResults.Location = new System.Drawing.Point(0, 0);
			this.lvParseResults.Name = "lvParseResults";
			this.lvParseResults.Size = new System.Drawing.Size(383, 96);
			this.lvParseResults.TabIndex = 0;
			this.lvParseResults.UseCompatibleStateImageBehavior = false;
			this.lvParseResults.View = System.Windows.Forms.View.Details;
			this.lvParseResults.DoubleClick += new System.EventHandler(this.lvParseResults_DoubleClick);
			// 
			// clmDescription
			// 
			this.clmDescription.Text = "説明";
			this.clmDescription.Width = 300;
			// 
			// clmLine
			// 
			this.clmLine.Text = "行";
			this.clmLine.Width = 30;
			// 
			// clmColumn
			// 
			this.clmColumn.Text = "列";
			this.clmColumn.Width = 30;
			// 
			// splMain
			// 
			this.splMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splMain.Location = new System.Drawing.Point(0, 51);
			this.splMain.Name = "splMain";
			this.splMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splMain.Panel1
			// 
			this.splMain.Panel1.Controls.Add(this.rtbSource);
			// 
			// splMain.Panel2
			// 
			this.splMain.Panel2.Controls.Add(this.lvParseResults);
			this.splMain.Size = new System.Drawing.Size(383, 211);
			this.splMain.SplitterDistance = 111;
			this.splMain.TabIndex = 4;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(383, 262);
			this.Controls.Add(this.splMain);
			this.Controls.Add(this.tsMain);
			this.Controls.Add(this.msMain);
			this.MainMenuStrip = this.msMain;
			this.Name = "MainForm";
			this.Text = "Interpreter";
			this.msMain.ResumeLayout(false);
			this.msMain.PerformLayout();
			this.tsMain.ResumeLayout(false);
			this.tsMain.PerformLayout();
			this.splMain.Panel1.ResumeLayout(false);
			this.splMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splMain)).EndInit();
			this.splMain.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip msMain;
		private System.Windows.Forms.ToolStripMenuItem tsmiFile;
		private System.Windows.Forms.ToolStripMenuItem tsmiNew;
		private System.Windows.Forms.ToolStripMenuItem tsmiOpen;
		private System.Windows.Forms.ToolStripSeparator tssFile1;
		private System.Windows.Forms.ToolStripMenuItem tsmiSave;
		private System.Windows.Forms.ToolStripMenuItem tsmiSaveAs;
		private System.Windows.Forms.ToolStripSeparator tssFile2;
		private System.Windows.Forms.ToolStripMenuItem tsmiExit;
		private System.Windows.Forms.ToolStripMenuItem tsmiEdit;
		private System.Windows.Forms.ToolStripMenuItem tsmiUndo;
		private System.Windows.Forms.ToolStripMenuItem tsmiRedo;
		private System.Windows.Forms.ToolStripSeparator tssEdit1;
		private System.Windows.Forms.ToolStripMenuItem tsmiCut;
		private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
		private System.Windows.Forms.ToolStripMenuItem tsmiPaste;
		private System.Windows.Forms.ToolStripSeparator tssEdit2;
		private System.Windows.Forms.ToolStripMenuItem tsmiSelectAll;
		private System.Windows.Forms.ToolStrip tsMain;
		private System.Windows.Forms.ToolStripButton tsbNew;
		private System.Windows.Forms.ToolStripButton tsbOpen;
		private System.Windows.Forms.ToolStripButton tsbSave;
		private System.Windows.Forms.ToolStripSeparator tssMain1;
		private System.Windows.Forms.ToolStripButton tsbCut;
		private System.Windows.Forms.ToolStripButton tsbCopy;
		private System.Windows.Forms.ToolStripButton tsbPaste;
		private Controls.WindowsForms.SyntaxHighlightingTextBox rtbSource;
		private System.Windows.Forms.ToolStripMenuItem tsmiDebug;
		private System.Windows.Forms.ToolStripMenuItem tsmiStartDebug;
		private System.Windows.Forms.ToolStripSeparator tssMain2;
		private System.Windows.Forms.ToolStripButton tsbStartDebug;
		private System.Windows.Forms.ListView lvParseResults;
		private System.Windows.Forms.ColumnHeader clmDescription;
		private System.Windows.Forms.ColumnHeader clmLine;
		private System.Windows.Forms.ColumnHeader clmColumn;
		private System.Windows.Forms.SplitContainer splMain;
	}
}

