using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace stockAnalysis
{
    public class printDataTable
    {
        public static void PrintTable(DataTable table)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            System.IO.File.WriteAllText("C:\\Users\\Cjoew\\Google Drive\\documents\\college\\7th Year\\CIS 625\\stockAnalysis\\test" + Thread.CurrentThread.ManagedThreadId + ".csv", sb.ToString());
        }
        
    }
}
