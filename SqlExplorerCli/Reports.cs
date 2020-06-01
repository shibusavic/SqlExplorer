using SqlExplorer.MsSqlServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlExplorerCli
{
    /// <summary>
    /// A service class for generating reports.
    /// </summary>
    public class Reports
    {
        private readonly string directoryName;
        private readonly bool overwriteFiles = false;
        private readonly Database database;

        /// <summary>
        /// Creates a new instance of the <see cref="Reports"/> class.
        /// </summary>
        /// <param name="database">The <see cref="Database"/> object.</param>
        /// <param name="directoryName">The directory in which to generate reports.</param>
        /// <param name="overwriteFiles">An indicator of whether to overwrite files when they exist.</param>
        public Reports(Database database, string directoryName, bool overwriteFiles = false)
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
            this.directoryName = string.IsNullOrWhiteSpace(directoryName) ? throw new ArgumentNullException(nameof(directoryName)) : directoryName;
            this.overwriteFiles = overwriteFiles;
        }

        /// <summary>
        /// Create the Tables CSV file.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public async Task CreateTableReportAsync()
        {
            var fileName = $"{directoryName}\\{CleanupDbName(database.Name)}_Tables.csv";
            CheckExistingFile(fileName);

            using Stream stream = File.Create(fileName);

            string line = $"Schema,Table,Position,Column,Data Type,Precision,Max Length,Is Nullable,Default{Environment.NewLine}";

            byte[] buffer = Encoding.UTF8.GetBytes(line);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            foreach (var table in database.Tables.OrderBy(t => t.FullName))
            {
                foreach (var column in table.Columns.OrderBy(c => c.Key))
                {
                    var col = column.Value;
                    line = $"{table.Schema},{table.Name},{col.OrdinalPosition},{col.Name},{col.DataType},{col.NumericPrecision},{col.MaxLength},{col.IsNullable},{col.ColumnDefault}{Environment.NewLine}";
                    buffer = Encoding.UTF8.GetBytes(line);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                }
            }

            await stream.FlushAsync();
            stream.Close();
        }

        /// <summary>
        /// Creates the View CSV file.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public async Task CreateViewReportAsync()
        {
            var fileName = $"{directoryName}\\{CleanupDbName(database.Name)}_Views.csv";
            CheckExistingFile(fileName);

            using Stream stream = File.Create(fileName);


            string line = $"Schema,View,Definition{Environment.NewLine}";

            byte[] buffer = Encoding.UTF8.GetBytes(line);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            foreach (var view in database.Views.OrderBy(v => v.FullName))
            {
                var def = view.Definition.Length < 50 ? view.Definition.Replace(Environment.NewLine, " ") : view.Definition.Substring(0, 50).Replace(Environment.NewLine, " ");
                line = $"{view.Schema},{view.Name},{def}{Environment.NewLine}";
                buffer = Encoding.UTF8.GetBytes(line);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }

            await stream.FlushAsync();
            stream.Close();
        }

        /// <summary>
        /// Creates the routines CSV file.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public async Task CreateRoutineReportAsync()
        {
            var fileName = $"{directoryName}\\{CleanupDbName(database.Name)}_Routines.csv";
            CheckExistingFile(fileName);

            using Stream stream = File.Create(fileName);


            string line = $"Schema,Routine,Definition{Environment.NewLine}";

            byte[] buffer = Encoding.UTF8.GetBytes(line);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            foreach (var routine in database.Routines.OrderBy(v => v.FullName))
            {
                var def = routine.Definition.Length < 50 ? routine.Definition.Replace(Environment.NewLine, " ") : routine.Definition.Substring(0, 50).Replace(Environment.NewLine, " ");
                line = $"{routine.Schema},{routine.Name},{def}{Environment.NewLine}";
                buffer = Encoding.UTF8.GetBytes(line);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }

            await stream.FlushAsync();
            stream.Close();
        }

        /// <summary>
        /// Creates the database object dependency report.
        /// </summary>
        /// <returns>A task that represents the underlying operation.</returns>
        public async Task CreateDependencyReportAsync()
        {
            var fileName = $"{directoryName}\\{CleanupDbName(database.Name)}_Dependency.txt";
            CheckExistingFile(fileName);

            var sortedTables = database.GetTablesSortedByDependency();

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
