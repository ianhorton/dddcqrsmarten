using System;
using System.Configuration;
using System.Data.SqlClient;
using Baseline.Dates;
using DefiningExports.Projections;
using Marten;
using Marten.Events;
using Marten.Events.Projections.Async;
using Projac.Connector;

namespace DefiningExports.ProjectionRunner
{
    public class Program
    {

	    static void Main()
	    {
			var store = DocumentStore.For(_ =>
			{
				_.Connection("host=localhost; database=event_store; password=postgres; username=postgres");
			});

		    using (var daemon = store.BuildProjectionDaemon(logger:new ConsoleDaemonLogger(), settings: new DaemonSettings {LeadingEdgeBuffer = 1.Seconds()}))
		    {
				// Start all of the configured async projections
				//daemon.StartAll();
				//daemon.TrackFor<ExportDefinition>();
				//daemon.WaitForNonStaleResults().Wait();

				//var daemon = store.BuildProjectionDaemon(logger: new ConsoleDaemonLogger());
			    daemon.StartAll();
			   // input.WriteLine(ConsoleColor.Green, "Daemon started. Press enter to stop.");
			    Console.ReadLine();


			}

		    // Compare the actual data in the ActiveProject documents with 
		    // the expectation
		    //_fixture.CompareActiveProjects(theStore);

		    Console.ReadLine();
	    }

	

	    static void __Main()
	    {
			var projectionsConnectionString = ConfigurationManager.ConnectionStrings["Projections"].ConnectionString;
		    var eventStoreConnectionString = "host=localhost; database=event_store; password=postgres; username=postgres";

		    //   using (var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(eventStoreConnectionString)))
		    //   using (var connection = new SqlConnection(projectionsConnectionString))
		    //   {
		    //	var projector = new ConnectedProjector<SqlConnection>(
		    //	    Resolve.WhenEqualToHandlerMessageType<SqlConnection>(new ExportDefinitionProjections()));

		    //    //SetupProjectionsDb(projector, connection).Wait();

		    //    //var deserializer = new DefaultEventDeserializer();
		    //    //using (streamStore.SubscribeToAll(null, async (_, rawMessage) =>
		    //    //{
		    //	   // var @event = await deserializer.DeserializeAsync(rawMessage);
		    //	   // await projector.ProjectAsync(connection, @event);
		    //    //}))
		    //    //{
		    //	   // Console.WriteLine("Enter to exit...");
		    //	   // Console.ReadLine();
		    //    //}
		    //}
	    }
    }
}
