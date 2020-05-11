using SqlServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SqlExplorerCli
{
    public class Reports
    {
        private readonly string databaseName;
        private readonly string directoryName;
        private readonly bool overwriteFiles = false;

        public Reports(string databaseName, string directoryName, bool overwriteFiles = false)
        {
            this.databaseName = string.IsNullOrWhiteSpace(databaseName) ? throw new ArgumentNullException(nameof(databaseName)) : databaseName;
            this.directoryName = string.IsNullOrWhiteSpace(directoryName) ? throw new ArgumentNullException(nameof(directoryName)) : directoryName;
            this.overwriteFiles = overwriteFiles;
        }

        public void CreateTextReport(IEnumerable<Table> tables,
            IEnumerable<ForeignKey> foreignKeys,
            IEnumerable<Routine> routines,
            IEnumerable<View> views)
        {
            var fileName = $"{directoryName}\\{CleanupDbName(databaseName)}.txt";
            CheckExistingFile(fileName);
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
