using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Marten;
using Marten.Events.Projections.Async;
using DefiningExports.Events;
using Marten.Events.Projections;
using Marten.Storage;
using Npgsql;

namespace DefiningExports.ProjectionRunner
{
	public partial class Program
	{
		public class ExportDefinitionProjection : IProjection
		{
			private readonly string _connectionString;

			public Guid Id { get; set; }

			public Type[] Consumes { get; } = {typeof(ExportDefinitionCreated), typeof(ExportRowAddedToExportDefinition)};

			public AsyncOptions AsyncOptions { get; } = new AsyncOptions();

			public ExportDefinitionProjection(string connectionString)
			{
				_connectionString = connectionString;
			}

			public void Apply(IDocumentSession session, EventPage page)
			{
			}

			public async Task ApplyAsync(IDocumentSession session, EventPage page, CancellationToken token)
			{
				using (var c = new NpgsqlConnection(_connectionString))
				{
					await c.OpenAsync(token).ConfigureAwait(false);
					using (var t = c.BeginTransaction())
					{
						try
						{
							var creationEvents = page.Events.OrderBy(s => s.Sequence).Select(s => s.Data)
								.OfType<ExportDefinitionCreated>();
							foreach (var e in creationEvents)
							{
								await t.Connection.ExecuteAsync(@"
										INSERT into export_definition (id, title)
										VALUES (@ExportDefinitionId, @Title);", e);
							}

							var rowAddedEvents = page.Events.OrderBy(s => s.Sequence).Select(s => s.Data)
								.OfType<ExportRowAddedToExportDefinition>();
							foreach (var e in rowAddedEvents)
							{
								await t.Connection.ExecuteAsync(@"
										INSERT into export_row (id, export_definition_id, name)
										VALUES (@ExportRowId, @ExportDefinitionId, @Name);", e);
							}

							await t.CommitAsync(token).ConfigureAwait(false);

						}
						catch
						{
							await t.RollbackAsync(token).ConfigureAwait(false);
							throw;
						}
					}
				}
			}

			public void EnsureStorageExists(ITenant tenant)
			{

			}
		}

	}
}
