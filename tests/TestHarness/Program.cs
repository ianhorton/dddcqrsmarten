using System;
using System.Linq;
using AggregateSource;
using AggregateSource.Marten;
using Baseline;
using DefiningExports;
using DefiningExports.CommandHandlers;
using DefiningExports.Commands;
using Marten;
using Marten.Events;

namespace TestHarness
{
	class Program
	{
		static void Main()
		{
			var store = DocumentStore.For(_ =>
			{
				_.Connection("host=localhost; database=event_store; password=postgres; username=postgres");
				_.Events.StreamIdentity = StreamIdentity.AsString;
				//.._.Events.AsyncProjections.AggregateStreamsWith<ExportDefinition>();
			});



			//using (var session = store.OpenSession())
			//{
			//	// questId is the id of the stream
			//	var party = session.Events.AggregateStream<ExportDefinition>("5307eb1f-ed92-44dd-b7d4-1cca35da3fc5");
			//	Console.WriteLine(party);

			//	//var party_at_version_3 = session.Events
			//	//	.AggregateStream<QuestParty>(questId, 3);


			//	//var party_yesterday = session.Events
			//	//	.AggregateStream<QuestParty>(questId, timestamp: DateTime.UtcNow.AddDays(-1));
			//}



			//var id = new Guid("5307eb1f-ed92-44dd-b7d4-1cca35da3fc5");
			var id = Guid.NewGuid();
			var aggId = $@"ed-{id}";

			using (var unitOfWork = new UnitOfTwerk(store.OpenSession()))
			{
				var bus = new InMemoryBus.InMemoryBus("bus");

				var exportDefinitionRepo =
					new AsyncRepository<ExportDefinition>(ExportDefinition.Factory, unitOfWork);

				var definingExportsMessageHandlersMartenHandler = new DefiningExportsMessageHandlersMarten(exportDefinitionRepo);
				bus.Subscribe<CreateExportDefinition>(definingExportsMessageHandlersMartenHandler);
				bus.Subscribe<AddExportRowToExportDefinition>(definingExportsMessageHandlersMartenHandler);

				bus.PublishAsync(new CreateExportDefinition(id, "newest ed")).Wait();
				bus.PublishAsync(new AddExportRowToExportDefinition(id, "11 row")).Wait();
				bus.PublishAsync(new AddExportRowToExportDefinition(id, "again row")).Wait();

				unitOfWork.SaveChanges();
			}

			using (var session = store.OpenSession())
			{
				// events are an array of little IEvent objects
				// that contain both the actual event object captured
				// previously and metadata like the Id and stream version
				//var events = session.Events.FetchStream("99ca96f2-a277-43c2-8af5-308189ef69fa");
				var events = session.Events.FetchStream(aggId);
				events.Each(evt =>
				{
					Console.WriteLine($"{evt.Version}.) {evt.Data}");
				});


				var party_at_version_3 = session.Events
					.AggregateStream<ExportDefinition>(aggId, 3);
			}

			Console.ReadLine();
		}
	}
}
