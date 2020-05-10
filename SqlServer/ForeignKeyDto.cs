using System;
using System.Collections.Generic;

namespace SqlServer
{
    /// <summary>
    /// Represents a DTO for foreign keys.
    /// </summary>
    internal class ForeignKeyDto : IEquatable<ForeignKeyDto>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ForeignKeyDto"/> class.
        /// </summary>
        /// <param name="name">The name of the foreign key.</param>
        /// <param name="schema">The schema of the child table.</param>
        /// <param name="tableName">The name of the child table name.</param>
        /// <param name="columnName">The name of the child table's column.</param>
        /// <param name="referenceSchema">The schema of the parent table.</param>
        /// <param name="referenceTableName">The name of the parent table name.</param>
        /// <param name="referenceColumnName">The name of the parent table's column name.</param>
        public ForeignKeyDto(string name,
            string schema,
            string tableName,
            string columnName,
            string referenceSchema,
            string referenceTableName,
            string referenceColumnName)
        {
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
            Schema = string.IsNullOrWhiteSpace(schema) ? throw new ArgumentNullException(nameof(schema)) : schema;
            TableName = string.IsNullOrWhiteSpace(tableName) ? throw new ArgumentNullException(nameof(tableName)) : tableName;
            ColumnName = string.IsNullOrWhiteSpace(columnName) ? throw new ArgumentNullException(nameof(columnName)) : columnName;
            ReferenceSchema = string.IsNullOrWhiteSpace(referenceSchema) ? throw new ArgumentNullException(nameof(referenceSchema)) : referenceSchema;
            ReferenceTableName = string.IsNullOrWhiteSpace(referenceTableName) ? throw new ArgumentNullException(nameof(referenceTableName)) : referenceTableName;
            ReferenceColumnName = string.IsNullOrWhiteSpace(referenceColumnName) ? throw new ArgumentNullException(nameof(referenceColumnName)) : referenceColumnName;
        }

        /// <summary>
        /// Gets the schema of the child table.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// Gets the name of the foreign key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the schema of the parent table.
        /// </summary>
        public string ReferenceSchema { get; }

        /// <summary>
        /// Gets the name of the parent table.
        /// </summary>
        public string ReferenceTableName { get; }

        /// <summary>
        /// Gets the name of the column in the parent table.
        /// </summary>
        public string ReferenceColumnName { get; }

        /// <summary>
        /// Gets the name of the child table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets the name of the child table's column.
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ForeignKeyDto);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
        public bool Equals(ForeignKeyDto other)
        {
            return other != null &&
                   Schema == other.Schema &&
                   Name == other.Name &&
                   ReferenceSchema == other.ReferenceSchema &&
                   ReferenceTableName == other.ReferenceTableName &&
                   ReferenceColumnName == other.ReferenceColumnName &&
                   TableName == other.TableName &&
                   ColumnName == other.ColumnName;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            int hashCode = 671857233;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Schema);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceSchema);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceTableName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReferenceColumnName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TableName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnName);
            return hashCode;
        }
    }
}
