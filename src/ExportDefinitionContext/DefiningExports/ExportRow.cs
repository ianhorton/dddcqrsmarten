using System;
using AggregateSource;
using DefiningExports.Events;

namespace DefiningExports
{
	public class ExportRow : Entity
	{
		public ExportRowId Id { get; set; }

		public string Name { get; set; }


		public ExportRow(Action<object> applier) : base(applier)
		{
			Register<ExportRowAddedToExportDefinition>(@event =>
			{
				Id = new ExportRowId(@event.ExportRowId);
				Name = @event.Name;
			});
		}

		public static ExportRowAddedToExportDefinition CreateNewRow(Guid exportDefinitionId, Guid exportRowId, string name)
		{
			if(exportDefinitionId == Guid.Empty) throw new ArgumentNullException(nameof(exportDefinitionId));
			if (exportRowId == Guid.Empty) throw new ArgumentNullException(nameof(exportRowId));
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

			return new ExportRowAddedToExportDefinition(exportDefinitionId, exportRowId, name);
		}

	}
}