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



    public partial class FormSelectSlice : Form
    {

        public event EventHandler OKButtonClick;

        public string titleText { set { this.Text = value; } }

        public string headline { set {label1.Text=value; } }

        public string selectedSliceText { get { return listBoxSlices.Items[listBoxSlices.SelectedIndex].ToString(); } }
        
        public string listItemString { set { listBoxSlices.Items.Add(value); } }

        public bool applyAll { get { return checkBoxApplyToAll.Checked; } }



        public FormSelectSlice()
        {
            InitializeComponent();
        }

        private void FormSelectSlice_Load(object sender, EventArgs e)
        {
            resizeControls();
            if (listBoxSlices.Items.Count > 0)
            {
                listBoxSlices.SelectedIndex = 0;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            /* 
            * https://stackoverflow.com/questions/1665533/communicate-between-two-windows-forms-in-c-sharp
            */

            OKButtonClick?.Invoke(this, EventArgs.Empty);
            this.DialogResult = DialogResult.OK;
            this.Hide();

        }

        private void FormSelectSlice_Resize(object sender, EventArgs e)
        {
            resizeControls();
        }

        private void resizeControls()
        {
            listBoxSlices.Width = this.Width - 33;
            listBoxSlices.Height = this.Height - 105;
            buttonOk.Left = this.Width - 79;
            buttonOk.Top = listBoxSlices.Bottom + 6;
            checkBoxApplyToAll.Top = listBoxSlices.Bottom + 6;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
