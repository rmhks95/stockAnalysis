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
using System.Data.SqlClient;

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

            var cb = new SqlConnectionStringBuilder();
            cb.DataSource = "tcp:cis625.database.windows.net,1433";
            cb.UserID = "admin123";
            cb.Password = "Nimda123";
            cb.InitialCatalog = "625data";
            cb.MultipleActiveResultSets = true;
            SqlConnection myConnection = new SqlConnection(cb.ConnectionString);
            myConnection.Open();

            using (SqlCommand myCmd = new SqlCommand("EXEC updateData",myConnection))
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.CommandTimeout = 0;
                myCmd.ExecuteNonQuery();
            }
            Close();

        }
    }
}
