using System.Data.SqlClient;
using Dapper;
using DefiningExports.Events;
using Projac.Connector;

namespace DefiningExports.Projections
{
	public class ExportDefinitionProjections : ConnectedProjection<SqlConnection>
	{
		public ExportDefinitionProjections()
		{
			When<ExportDefinitionCreated>((connection, @event) =>
			{
				connection.Execute(@"
									INSERT INTO ExportDefinition (id, title)
									VALUES (@Id, @Title)", new {@event.ExportDefinitionId, @event.Title});
			});

			When<ExportRowAddedToExportDefinition>((connection, @event) =>
			{

			});
		}
	}
}