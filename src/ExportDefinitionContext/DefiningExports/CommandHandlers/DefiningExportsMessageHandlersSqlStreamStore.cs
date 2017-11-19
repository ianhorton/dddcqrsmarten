using System;
using System.Linq;
using System.Threading.Tasks;
using DefiningExports.Commands;
using InMemoryBus;
using Newtonsoft.Json;
using SqlStreamStore.Streams;
using SSS;

namespace DefiningExports.CommandHandlers
{
	public class DefiningExportsMessageHandlersSqlStreamStore :
		IHandle<CreateExportDefinition>,
		IHandle<AddExportRowToExportDefinition>
	{
		private readonly Repository<ExportDefinition> _repository;

		public DefiningExportsMessageHandlersSqlStreamStore(Repository<ExportDefinition> repository)
		{
			_repository = repository;
		}

		public async Task HandleAsync(CreateExportDefinition command)
		{
			var exportDefinition = ExportDefinition.CreateNew(command.ExportDefinitionId, command.Name);
			_repository.Add(exportDefinition.ExportDefinitionId.ToString(), exportDefinition);

			foreach (var aggregate in _repository.UnitOfWork.GetChanges())
			{
				await _repository.EventStore.AppendToStream(
						$@"definingExports-{aggregate.Identifier}",
						aggregate.ExpectedVersion,
						aggregate.Root.GetChanges().Select(
								@event => new NewStreamMessage(
									Guid.NewGuid(),
									@event.GetType().AssemblyQualifiedName,
									JsonConvert.SerializeObject(@event),
									JsonConvert.SerializeObject(new { timeStamp = DateTime.Now })))
							.ToArray())
					.ConfigureAwait(false);
			}


		}

		public Task HandleAsync(AddExportRowToExportDefinition message)
		{
			throw new NotImplementedException();
		}
	}
}