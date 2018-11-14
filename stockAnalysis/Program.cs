using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stockAnalysis
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            List<Criteria> list = parseCriteria.ParseCriteria();

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            csvHandler.GetDataTabletFromCSVFile();
            //runDBConnect();
            
        }

        static void runDBConnect()
        {
            try
            {
                var cb = new SqlConnectionStringBuilder();
                cb.DataSource = "cis625.database.windows.net";
                cb.UserID = "admin123";
                cb.Password = "Nimda123";
                cb.InitialCatalog = "criteriaSets";

                using (var connection = new SqlConnection(cb.ConnectionString))
                {
                    connection.Open();

                    Submit_Tsql_NonQuery(connection, "2 - Create-Tables",
                       Build_2_Tsql_CreateTables());
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("View the report output here, then press any key to end the program...");
            //Console.ReadKey();
        }

        static void Submit_Tsql_NonQuery(
         SqlConnection connection,
         string tsqlPurpose,
         string tsqlSourceCode,
         string parameterName = null,
         string parameterValue = null
         )
        {
            Console.WriteLine();
            Console.WriteLine("=================================");
            Console.WriteLine("T-SQL to {0}...", tsqlPurpose);

            using (var command = new SqlCommand(tsqlSourceCode, connection))
            {
                if (parameterName != null)
                {
                    command.Parameters.AddWithValue(  // Or, use SqlParameter class.
                       parameterName,
                       parameterValue);
                }
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine(rowsAffected + " = rows affected.");
            }
        }


        static string Build_2_Tsql_CreateTables()
        {
                    return @"
            DROP TABLE IF EXISTS tabEmployee;
            DROP TABLE IF EXISTS tabDepartment;  -- Drop parent table last.

            ";
        }
    
    }
}
