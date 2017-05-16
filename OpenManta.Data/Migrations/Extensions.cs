using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;

namespace OpenManta.Data.Migrations
{
	public static class Extensions
	{
		public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax, string name)
		{
			return WithLookupId(tableWithColumnSyntax, name)
				.Identity();
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithLookupId(this ICreateTableWithColumnSyntax tableWithColumnSyntax, string name)
		{
			return tableWithColumnSyntax
				.WithColumn(name)
				.AsInt32()
				.NotNullable()
				.PrimaryKey();
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithDescriptionColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax)
		{
			return tableWithColumnSyntax
				.WithColumn("Description")
				.AsString()
				.Nullable();
		}

		public static ICreateTableColumnOptionOrWithColumnSyntax WithNameColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax)
		{
			return tableWithColumnSyntax
				.WithColumn("Name")
				.AsString(50)
				.NotNullable();
		}

		public static void QuickForeignKey(this ICreateExpressionRoot root, string schema, string fromTable, string toTable, string columnName)
		{
			root.ForeignKey("FK_" + fromTable + "_" + columnName)
				.FromTable(fromTable).InSchema(schema)
				.ForeignColumn(columnName)
				.ToTable(toTable).InSchema(schema)
				.PrimaryColumn(columnName);
		}
	}
}