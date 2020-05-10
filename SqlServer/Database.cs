using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SqlServer
{
    /// <summary>
    /// A database service.
    /// </summary>
    public class Database
    {
        private string connectionString;

        /// <summary>
        /// Creates a new instance of the <see cref="Database"/> service.
        /// </summary>
        /// <param name="connectionString">The Sql Server connection string.</param>
        public Database(string connectionString)
        {
            this.connectionString = string.IsNullOrWhiteSpace(connectionString) ? throw new ArgumentNullException(nameof(connectionString)) : connectionString;
        }

        /// <summary>
        /// Gets a collection of <see cref="Table"/> objects for the database.
        /// </summary>
        /// <returns>A task representing the asyncronous operation.
        /// The task contains a collection of <see cref="Table"/> objects.</returns>
        public async Task<IEnumerable<Table>> GetTablesAsync()
        {
            List<Table> tables = new List<Table>();
            string tableSql = $"{GET_TABLES_SQL} WHERE TABLE_NAME <> 'sysdiagrams' AND TABLE_TYPE = 'BASE TABLE' ORDER BY [TABLE_SCHEMA], [TABLE_NAME]";

            using var connection = new SqlConnection(connectionString);

            var tableNames = await connection.QueryAsync<(string schema, string name)>(tableSql);

            foreach (var table in tableNames)
            {
                string columnSql = $"{GET_COLUMNS_SQL} WHERE TABLE_SCHEMA = '{table.schema}' AND TABLE_NAME = '{table.name}' ORDER BY ORDINAL_POSITION";

                var columns = await connection.QueryAsync<Column>(columnSql);

                tables.Add(new Table(table.schema, table.name, columns));
            }
            return tables;
        }

        /// <summary>
        /// Gets a specific <see cref="Table"/> from the database.
        /// </summary>
        /// <param name="schema">The schema of the table.</param>
        /// <param name="name">The name of the table.</param>
        /// <returns>A task representing the asyncronous operation.
        /// The task contains a <see cref="Table"/> object.</returns>
        public async Task<Table> GetTableAsync(string schema, string name)
        {
            using var connection = new SqlConnection(connectionString);

            string columnSql = $"{GET_COLUMNS_SQL} WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{name}' ORDER BY ORDINAL_POSITION";

            var columns = await connection.QueryAsync<Column>(columnSql);

            if (columns.Any())
            {
                return new Table(schema, name, columns);
            }

            return null;
        }

        /// <summary>
        /// Gets a collection of <see cref="ForeignKey"/> objects from the database.
        /// </summary>
        /// <returns>A task representing the asyncronous operation.
        /// The task contains a collection of <see cref="ForeignKey"/> objects.</returns>
        public async Task<IEnumerable<ForeignKey>> GetForeignKeysAsync()
        {
            List<ForeignKey> foreignKeys = new List<ForeignKey>();

            string fkSql = $"{GET_FOREIGN_KEYS_SQL} order by [Schema], TableName";

            using var connection = new SqlConnection(connectionString);

            var fks = await connection.QueryAsync<ForeignKeyDto>(fkSql);

            foreach (var fk in fks)
            {
                var table = await GetTableAsync(fk.Schema, fk.TableName);
                var refTable = await GetTableAsync(fk.ReferenceSchema, fk.ReferenceTableName);
                foreignKeys.Add(new ForeignKey(table.Schema, fk.Name, refTable, fk.ReferenceColumnName, table, fk.ReferenceColumnName));
            }

            return foreignKeys;
        }

        /// <summary>
        /// Get a collection of <see cref="View"/> objects from the database.
        /// </summary>
        /// <returns>A task representing the asyncronous operation.
        /// The task contains a collection of <see cref="View"/> objects.</returns>
        public async Task<IEnumerable<View>> GetViewsAsync()
        {
            string viewSql = $"{GET_VIEWS_SQL} ORDER BY TABLE_SCHEMA, TABLE_NAME";

            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<View>(viewSql);
        }

        /// <summary>
        /// Gets a collection of <see cref="Routine"/> objects from the database.
        /// </summary>
        /// <returns>A task representing the asyncronous operation.
        /// The task contains a collection of <see cref="Routine"/> objects.</returns>
        public async Task<IEnumerable<Routine>> GetRoutinesAsync()
        {
            string routineSql = $"{GET_ROUTINES_SQL} ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Routine>(routineSql);
        }

        private const string GET_TABLES_SQL = @"
SELECT
[TABLE_SCHEMA] AS [SCHEMA],
[TABLE_NAME] AS [NAME]
FROM [INFORMATION_SCHEMA].[TABLES]
";

        private const string GET_COLUMNS_SQL = @"
SELECT
TABLE_SCHEMA AS [SCHEMA],
COLUMN_NAME AS [NAME],
ORDINAL_POSITION AS ORDINALPOSITION,
COLUMN_DEFAULT AS COLUMNDEFAULT,
CAST(CASE IS_NULLABLE
WHEN 'YES' THEN 1
ELSE 0
END AS BIT) AS ISNULLABLE,
DATA_TYPE AS DATATYPE,
CHARACTER_MAXIMUM_LENGTH AS MAXLENGTH,
NUMERIC_PRECISION AS NUMERICPRECISION
FROM INFORMATION_SCHEMA.COLUMNS
";

        public const string GET_FOREIGN_KEYS_SQL = @"
SELECT
f.name AS [NAME],
SCHEMA_NAME(f.SCHEMA_ID) AS [SCHEMA],
OBJECT_NAME(f.parent_object_id) AS TableName,
COL_NAME(fc.parent_object_id,fc.parent_column_id) AS ColumnName,
SCHEMA_NAME(o.SCHEMA_ID) AS ReferenceSchema,
OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName,
COL_NAME(fc.referenced_object_id,fc.referenced_column_id) AS ReferenceColumnName
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id
INNER JOIN sys.objects AS o ON o.OBJECT_ID = fc.referenced_object_id
";

        public const string GET_VIEWS_SQL = @"
SELECT
TABLE_SCHEMA AS [SCHEMA],
TABLE_NAME AS [NAME],
VIEW_DEFINITION AS [DEFINITION]
FROM INFORMATION_SCHEMA.VIEWS
";

        public const string GET_ROUTINES_SQL = @"
SELECT
[ROUTINE_SCHEMA] AS [SCHEMA],
[ROUTINE_NAME] AS [NAME],
[ROUTINE_DEFINITION] AS [DEFINITION],
[ROUTINE_TYPE] AS [ROUTINETYPE]
FROM [INFORMATION_SCHEMA].[ROUTINES]";
    }
}
