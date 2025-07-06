using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teste_Cliente
{
    public partial class IpPedid : Form
    {
        public string InputText { get; private set; } = "";
        public IpPedid()
        {
            InitializeComponent();
        }

        private void IpPedid_Load(object sender, EventArgs e)
        {

        }

        private void textBoxip_MouseDown(object sender, MouseEventArgs e)
        {
            textBoxip.Clear();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            InputText = textBoxip.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
