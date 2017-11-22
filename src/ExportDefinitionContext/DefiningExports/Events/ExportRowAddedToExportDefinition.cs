using System;

namespace DefiningExports.Events
{
	public class ExportRowAddedToExportDefinition
	{
		public Guid ExportDefinitionId { get; }
		public Guid ExportRowId { get; }
		public string Name { get; }

		public ExportRowAddedToExportDefinition(Guid exportDefinitionId, Guid exportRowId, string name)
		{
			ExportDefinitionId = exportDefinitionId;
			ExportRowId = exportRowId;
			Name = name;
		}
	}
}