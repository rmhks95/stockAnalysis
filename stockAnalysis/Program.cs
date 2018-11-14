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
            parseCriteria();
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            runDBConnect();

            
        }

        static void runDBConnect()
        {
            try
            {
                var cb = new SqlConnectionStringBuilder();
                cb.DataSource = "";
                cb.UserID = "";
                cb.Password = "";
                cb.InitialCatalog = "";

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
            Console.ReadKey();
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


            CREATE TABLE tabDepartment
            (
               DepartmentCode  nchar(4)          not null
                  PRIMARY KEY,
               DepartmentName  nvarchar(128)     not null
            );

            CREATE TABLE tabEmployee
            (
               EmployeeGuid    uniqueIdentifier  not null  default NewId()
                  PRIMARY KEY,
               EmployeeName    nvarchar(128)     not null,
               EmployeeLevel   int               not null,
               DepartmentCode  nchar(4)              null
                  REFERENCES tabDepartment (DepartmentCode)  -- (REFERENCES would be disallowed on temporary tables.)
            );
            ";
        }


        static void parseCriteria()
        {
            string line;
            int colCounter = 0;
            int CriteriaCount = -1;
            string agKey = "";
            string agSum = "";
            string [] preProcess = new string[10];
            string[] preAg = new string[10];
            string[,] preValues = new string[10, 15];
            string[] postProcess = new string[10];
            string[] postAg = new string[10];
            string[,] postValues = new string[10, 15];
            Criteria criteria = new Criteria();
            List<Criteria> list = new List<Criteria>();

            System.IO.StreamReader file = new System.IO.StreamReader(@"U:\\stockAnalysis\\stockAnalysis\\criteria sets.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("--"))
                {
                    if (line.StartsWith("!"))
                    {
                        criteria = new Criteria();
                        criteria.Name = line.Substring(1);
                        CriteriaCount = -1;
                        agKey = "";
                        preAg = new string[10];
                        postAg = new string[10];
                    }
                    else if (line.StartsWith("@"))
                    {
                        CriteriaCount++;
                        preAg[CriteriaCount] = line.Substring(1);
                        colCounter = 0;
                    }
                    else if (line.StartsWith("^"))
                    {
                        preProcess[CriteriaCount] = line.Substring(1);
                    }
                    else if (line.StartsWith("#"))
                    {
                        if (Regex.IsMatch(line, @"[A-Za-z]"))
                        {
                            preValues[CriteriaCount, colCounter] = line.Substring(1);
                            colCounter++;
                        }
                        if (Regex.IsMatch(line, @"\d"))
                        {
                            postValues[CriteriaCount, colCounter] = line.Substring(1);
                            colCounter++;
                        }
                    }
                    else if (line.StartsWith("*"))
                    {
                        agKey += line.Substring(1);
                        agKey += ",";
                    }
                    else if (line.StartsWith("+"))
                    {
                        agSum += line.Substring(1);
                        agSum += ",";
                        CriteriaCount = -1;
                    }
                    else if (line.StartsWith("$"))
                    {
                        CriteriaCount++;
                        postAg[CriteriaCount] = line.Substring(1);
                        colCounter = 0;
                    }
                    else if (line.StartsWith("&"))
                    {
                        postProcess[CriteriaCount] = line.Substring(1);
                    }

                    if (string.IsNullOrEmpty(line) && file.ReadLine().StartsWith("--") || file.EndOfStream)
                    {
                        if (criteria.Name != null)
                        {
                            criteria.PreAgCol = preAg;
                            criteria.PreProcess = preProcess;
                            criteria.PreValues = preValues;
                            criteria.agKey = agKey.Substring(0, agKey.Length - 1);
                            criteria.agSum = agSum.Substring(0, agSum.Length - 1);
                            criteria.PostAgCol = postAg;
                            criteria.PostProcess = postProcess;
                            criteria.PostValues = postValues;
                            list.Add(criteria);
                        }
                    }
                }

            }

        }


    }
}
