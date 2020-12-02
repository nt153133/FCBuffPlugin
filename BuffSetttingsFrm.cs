using System;
using System.Windows.Forms;

namespace LlamaLibrary
{
    public partial class BuffSetttingsFrm : Form
    {
        public BuffSetttingsFrm()
        {
            InitializeComponent();
        }

        private void BuffSetttingsFrm_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = FCActionPlugin.Settings;
        }
    }
}