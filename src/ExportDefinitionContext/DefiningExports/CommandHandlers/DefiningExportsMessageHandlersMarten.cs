using System;
using System.Linq;
using System.Threading.Tasks;
using AggregateSource;
using AggregateSource.Marten;
using DefiningExports.Commands;
using InMemoryBus;
using Marten;

namespace DefiningExports.CommandHandlers
{

	public class DefiningExportsMessageHandlersMarten :
		IHandle<CreateExportDefinition>,
		IHandle<AddExportRowToExportDefinition>
	{
		private readonly AsyncRepository<ExportDefinition> _repository;

		public DefiningExportsMessageHandlersMarten(AsyncRepository<ExportDefinition> repository)
		{
			_repository = repository;
		}

		public async Task HandleAsync(CreateExportDefinition command)
		{
			var exportDefinition = ExportDefinition.CreateNew(command.ExportDefinitionId, command.Name);
			_repository.Add($@"ed-{exportDefinition.ExportDefinitionId}", exportDefinition);

			foreach (var aggregate in _repository.UnitOfWork.GetChanges())
			{
				var aggregateIdentifier = aggregate.Identifier;
				var aggregateExpectedVersion = aggregate.ExpectedVersion;
				var changes = aggregate.Root.GetChanges().ToArray();

				_repository.AppendToStream(
					aggregateIdentifier,
					aggregateExpectedVersion,
					changes);
			}
		}

		public async Task HandleAsync(AddExportRowToExportDefinition message)
		{
			var exportDefinition = await _repository
				.GetAsync($@"ed-{message.ExportDefinitionId}")
				.ConfigureAwait(false);

			var exprtRowId = Guid.NewGuid();
			exportDefinition.AddRowToRowsCollection(exprtRowId, message.Name);

			var aggregates = _repository.UnitOfWork.GetChanges();
			foreach (var aggregate in aggregates)
			{
				var aggregateIdentifier = aggregate.Identifier;
				var aggregateExpectedVersion = aggregate.ExpectedVersion;
				var changes = aggregate.Root.GetChanges().ToArray();

				_repository.AppendToStream(
					aggregateIdentifier,
					aggregateExpectedVersion,
					changes);
			}
		}
	}
}