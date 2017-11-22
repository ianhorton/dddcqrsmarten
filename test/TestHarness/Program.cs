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
			Run();
		}

		static void Run()
		{
			var store = DocumentStore.For(_ =>
			{
				_.Connection("host=localhost; database=event_store; password=postgres; username=postgres");
				_.Events.StreamIdentity = StreamIdentity.AsString;
			});


			Console.WriteLine("CQRS Test Harness. Type exit to quit.");

			while (true)
			{
				Console.WriteLine("Enter a code to add more lines to an existing or enter to create a new object:");

				var userInput = Console.ReadLine();

				if (userInput == "exit")
				{
					break;
				}

				var isCreate = false;
				if (Guid.TryParse(userInput, out var id) == false)
				{
					Console.WriteLine("No code input, creating new...");
					id = Guid.NewGuid();
					isCreate = true;
				}

				using (var unitOfWork = new UnitOfTwerk(store.OpenSession()))
				{
					var bus = new InMemoryBus.InMemoryBus("bus");

					var exportDefinitionRepo =
						new AsyncRepository<ExportDefinition>(ExportDefinition.Factory, unitOfWork);

					var definingExportsMessageHandlersMartenHandler = new DefiningExportsMessageHandlersMarten(exportDefinitionRepo);
					bus.Subscribe<CreateExportDefinition>(definingExportsMessageHandlersMartenHandler);
					bus.Subscribe<AddExportRowToExportDefinition>(definingExportsMessageHandlersMartenHandler);

					if (isCreate) bus.PublishAsync(new CreateExportDefinition(id, "newest ed")).Wait();
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
					var events = session.Events.FetchStream($@"ed-{id}");
					events.Each(evt =>
					{
						Console.WriteLine($"{evt.Version}.) {evt.Data}");
					});
				}

			}

			//using (var session = store.OpenSession())
			//{
			//	// events are an array of little IEvent objects
			//	// that contain both the actual event object captured
			//	// previously and metadata like the Id and stream version
			//	//var events = session.Events.FetchStream("99ca96f2-a277-43c2-8af5-308189ef69fa");
			//	var events = session.Events.FetchStream(aggId);
			//	events.Each(evt =>
			//	{
			//		Console.WriteLine($"{evt.Version}.) {evt.Data}");
			//	});
			//}

			//Console.ReadLine();
		}
	}
}
