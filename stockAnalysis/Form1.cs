using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace stockAnalysis
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void openFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();          
            dialog.Multiselect = true;
            if(uxOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in uxOpenFileDialog.FileNames)
                {
                    string path = file;
                    csvHandler.GetDataTabletFromCSVFile(path);
                }
            }

        }
    }
}
