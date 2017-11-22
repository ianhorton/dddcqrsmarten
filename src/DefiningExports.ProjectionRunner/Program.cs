using System;
using Baseline.Dates;
using Marten;
using Marten.Events;
using Marten.Events.Projections.Async;

namespace DefiningExports.ProjectionRunner
{
	public partial class Program
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
				settings: new DaemonSettings {LeadingEdgeBuffer = 1.Seconds(),}))
			{
				daemon.StartAll();
				//daemon.RebuildAll();
				Console.ReadLine();
			}
		}

	}
}
