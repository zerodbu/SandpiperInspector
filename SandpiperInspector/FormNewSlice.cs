using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SandpiperInspector
{
    public partial class FormNewSlice : Form
    {
        public event EventHandler createButtonClick;
        
        public string titleText { set { this.Text = value; } }
        
        public string sliceDescription { get { return textBoxSliceDescription.Text; } set { textBoxSliceDescription.Text = value; } }
        public string sliceType { get { return textBoxSliceType.Text; } set { textBoxSliceType.Text = value; } }
        public string sliceMetadata { get { return textBoxMetadata.Text; } set { textBoxMetadata.Text = value; } }

        public string goButttonText { set { buttonNewSliceCreate.Text = value; } }




        public FormNewSlice()
        {
            InitializeComponent();
        }

        private void FormNewSlice_Load(object sender, EventArgs e)
        {

        }

        private void buttonNewSliceCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }

        private void buttonNewSliceCreate_Click(object sender, EventArgs e)
        {
            /* 
             * https://stackoverflow.com/questions/1665533/communicate-between-two-windows-forms-in-c-sharp
            */

            createButtonClick?.Invoke(this, EventArgs.Empty);
            this.DialogResult = DialogResult.OK;
            this.Hide();

        }

    }
}
