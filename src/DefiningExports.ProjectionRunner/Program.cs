using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Dapper;
using Marten;
using Marten.Events;
using Marten.Events.Projections.Async;
using DefiningExports.Events;
using Marten.Events.Projections;
using Marten.Storage;
using Npgsql;

namespace DefiningExports.ProjectionRunner
{
	public class Program
	{
		static void Main()
		{
			var store = DocumentStore.For(_ =>
			{
				_.Connection("host=localhost; database=event_store; password=postgres; username=postgres");
				_.Events.StreamIdentity = StreamIdentity.AsString;
				_.Events.AsyncProjections.Add(new ExportDefinitionProjection("host=localhost; database=read_model; password=postgres; username=postgres"));

			});

			using (var daemon = store.BuildProjectionDaemon(logger: new ConsoleDaemonLogger(),
				settings: new DaemonSettings {LeadingEdgeBuffer = 1.Seconds()}))
			{
				daemon.StartAll();
				Console.ReadLine();
			}
		}

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
					await c.OpenAsync().ConfigureAwait(false);
					using (var t = c.BeginTransaction())
					{
						try
						{
							var creationEvents = page.Events.OrderBy(s => s.Sequence).Select(s => s.Data).OfType<ExportDefinitionCreated>();
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

							await t.CommitAsync().ConfigureAwait(false);

						}
						catch
						{
							await t.RollbackAsync().ConfigureAwait(false);
						}
					}
				}
			}

			public int ProjectCount { get; set; }

			public void Apply(ExportDefinitionCreated @event)
			{
				//var model = new ProjectCountProjection();
				//model.ProjectCount++;
				//_session.Store(model);
			}

			public void Apply(ExportRowAddedToExportDefinition @event)
			{
				//var model = new ProjectCountProjection();
				//model.ProjectCount++;
				//_session.Store(model);
			}

			public void EnsureStorageExists(ITenant tenant)
			{

			}
		}

	}
}
