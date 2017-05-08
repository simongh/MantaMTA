using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Builders.Create.Table;

namespace OpenManta.Data.Migrations
{
	public static class Extensions
	{
		public static ICreateTableColumnOptionOrWithColumnSyntax WithIdColumn(this ICreateTableWithColumnSyntax tableWithColumnSyntax, string name)
		{
			return tableWithColumnSyntax
				.WithColumn(name)
				.AsInt32()
				.NotNullable()
				.PrimaryKey()
				.Identity();
		}
	}
}