using System;

namespace DefiningExports.Events
{
	public class ExportRowAddedToExportDefinition
	{
		public Guid ExportRowId { get; }
		public string Name { get; }

		public ExportRowAddedToExportDefinition(Guid exportRowId, string name)
		{
			ExportRowId = exportRowId;
			Name = name;
		}
	}
}