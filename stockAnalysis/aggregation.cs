using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace stockAnalysis
{
    public class aggregation
    {
        public static DataTable Aggregate(EnumerableRowCollection<DataRow> resu, Criteria currentCriteria)
        {
            IEnumerable<string> columnsToGroupBy = currentCriteria.agKey.Split(',');
            IEnumerable<string> sumsToSelect = currentCriteria.agSum.Split(',');

            var keys = currentCriteria.agKey.Split(',');

            var groupList = resu.GroupBy(x => new NTuple<object>(from column in columnsToGroupBy select x[column])); //.Select(val => new { nK=val.FirstOrDefault().Field<string>(keys[0])+"~"+ val.FirstOrDefault().Field<string>(keys[1]), total=val.Sum(c=>Convert.ToDecimal(c.Field<string>(sumsToSelect.FirstOrDefault()))).ToString()});//new NTuple<object>(from sum in sumsToSelect select val[sum])

            if (resu.Count() == 0)
            {
                return new DataTable();
            }

            DataTable aggregatedTable = resu.FirstOrDefault().Table.Clone();
            aggregatedTable.Columns.Add("AggregatedKey", typeof(string));
            /*foreach (DataColumn col in resu.ElementAtOrDefault(0).Table.Columns)
            {
                table.Columns.Add(col.ColumnName, col.DataType);
            }*/

            //DataRow toAdd = aggregatedTable.NewRow();
            //toAdd.Table.Columns.Add("AggregatedKey", typeof(string));
            //toAdd["AggregatedKey"] = aggregatedKey;



            foreach (var group in groupList)
            {
                string aggregatedKey = "";
                DataRow toAdd = group.FirstOrDefault();
                if (!(toAdd.Table.Columns.Contains("AggregatedKey"))) toAdd.Table.Columns.Add("AggregatedKey", typeof(string));
                for (int i = 1; i < group.Count(); i++)
                { //each row in the group (except first)
                    for (int j = 0; j < group.ElementAt(i).Table.Columns.Count; j++) //each column in row
                    {
                        var colName = group.ElementAt(i).Table.Columns[j].ColumnName;
                        if (sumsToSelect.Contains(colName))
                        {
                            toAdd[colName] = Convert.ToDouble(toAdd[colName].ToString()) + Convert.ToDouble(group.ElementAt(i)[colName].ToString());
                        }
                        else if (toAdd[colName].ToString() != group.ElementAt(i)[colName].ToString())
                        {
                            toAdd[colName] = null;//group.ElementAt(i)[colName].ToString();
                        }
                    }

                }
                for (int i = 0; i < columnsToGroupBy.Count(); i++)
                {
                    if (i == 0) aggregatedKey = toAdd[columnsToGroupBy.ElementAt(i)].ToString();
                    else aggregatedKey += "~" + toAdd[columnsToGroupBy.ElementAt(i)].ToString();
                }
                toAdd["AggregatedKey"] = aggregatedKey;
                aggregatedTable.Rows.Add(toAdd.ItemArray);
            }

            return aggregatedTable;
        }
    }
}
