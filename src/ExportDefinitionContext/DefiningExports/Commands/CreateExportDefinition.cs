using System;
using InMemoryBus;

namespace DefiningExports.Commands
{
	public class CreateExportDefinition : Message
	{
		public readonly Guid ExportDefinitionId;
		public readonly string Name;

		public CreateExportDefinition(Guid exportDefinitionId, string name)
		{
			ExportDefinitionId = exportDefinitionId;
			Name = name;
		}
	}


}