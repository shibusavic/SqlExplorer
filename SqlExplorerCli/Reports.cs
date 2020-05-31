using SqlServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlExplorerCli
{
    public class Reports
    {
        private readonly string directoryName;
        private readonly bool overwriteFiles = false;

        private readonly Database database;

        public Reports(Database database, string directoryName, bool overwriteFiles = false)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.directoryName = string.IsNullOrWhiteSpace(directoryName) ? throw new ArgumentNullException(nameof(directoryName)) : directoryName;
            this.overwriteFiles = overwriteFiles;
        }

        public async Task CreateTextReportAsync()
        {
            var fileName = $"{directoryName}\\{CleanupDbName(database.Name)}.txt";
            CheckExistingFile(fileName);

            var sortedTables = SortTablesByDependency();

            using Stream stream = File.Create(fileName);

            byte[] buffer;
            foreach (var table in sortedTables)
            {
                StringBuilder tableInfo = new StringBuilder();
                tableInfo.AppendLine($"{table.Schema}.{table.Name}");

                var tableDependencies = database.GetChildForeignKeysForTable(table);
                var viewDependencies = database.GetViewsReferencingTable(table);
                var routineDependencies = database.GetRoutinesReferencingTable(table);

                if (tableDependencies.Any())
                {
                    tableInfo.AppendLine("\tTable Dependencies");
                    foreach (var child in tableDependencies.Select(t => t.ChildTable))
                    {
                        tableInfo.AppendLine($"\t\t{child.FullName}");
                    }
                }

                if (viewDependencies.Any())
                {
                    tableInfo.AppendLine("\tView Dependencies");
                    foreach (var child in viewDependencies)
                    {
                        tableInfo.AppendLine($"\t\t{child.FullName}");
                    }
                }

                if (routineDependencies.Any())
                {
                    tableInfo.AppendLine("\tRoutine Dependencies");
                    foreach (var child in routineDependencies)
                    {
                        tableInfo.AppendLine($"\t\t{child.FullName}");
                    }
                }

                buffer = Encoding.UTF8.GetBytes(tableInfo.ToString());
                await stream.WriteAsync(buffer);
            }

            await stream.FlushAsync();
            stream.Close();
        }

        private IEnumerable<Table> SortTablesByDependency()
        {
            LinkedList<Table> tableList = new LinkedList<Table>(database.Tables);

            foreach (var table in database.Tables)
            {
                int parentPosition = tableList.ToList().IndexOf(table);
                var childrenFk = database.ForeignKeys.Where(f => f.ParentTable.Schema == table.Schema
                    && f.ParentTable.Name == table.Name);

                bool move = false;
                do
                {
                    move = false;
                    childrenFk.ToList().ForEach(c =>
                    {
                        if (c.ChildTable.FullName != table.FullName)
                        {
                            var childTableNode = tableList.Find(tableList.FirstOrDefault(t => t.FullName == c.ChildTable.FullName));
                            int childPosition = tableList.ToList().IndexOf(childTableNode.Value);
                            if (childPosition < parentPosition)
                            {
                                var parentTableNode = tableList.Find(table);
                                tableList.Remove(childTableNode);
                                tableList.AddAfter(parentTableNode, childTableNode);
                                move = true;
                            }
                        }
                    });
                }
                while (move == true);
            }

            return tableList;
        }

        private void CheckExistingFile(string filename)
        {
            if (File.Exists(filename) && !overwriteFiles)
            {
                throw new Exception($"File '{filename}' already exists; use -o to overwrite.");
            }
        }

        private string CleanupDbName(string databaseName)
        {
            return databaseName.Replace(" ", "_");
        }
    }
}
