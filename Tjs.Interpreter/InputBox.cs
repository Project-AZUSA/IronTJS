using System.Windows.Forms;

namespace IronTJS
{
    public partial class InputBox : Form
	{
		public InputBox()
		{
			InitializeComponent();
		}

		public string Description
		{
			get { return lblDescription.Text; }
			set { lblDescription.Text = value; }
		}

		public string InputText { get { return txtInput.Text; } }
	}
}
