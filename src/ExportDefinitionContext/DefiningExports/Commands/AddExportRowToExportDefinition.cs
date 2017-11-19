using System;
using InMemoryBus;

namespace DefiningExports.Commands
{
	public class AddExportRowToExportDefinition : Message
	{
		public readonly Guid ExportDefinitionId;
		public readonly string Name;

		public AddExportRowToExportDefinition(Guid exportDefinitionId, string name)
		{
			ExportDefinitionId = exportDefinitionId;
			Name = name;
		}
	}
}