using System;

namespace DefiningExports.Events
{
	public class ExportDefinitionCreated
	{
		public ExportDefinitionCreated(Guid exportDefinitionId, string title)
		{
			ExportDefinitionId = exportDefinitionId;
			Title = title;
		}

		public Guid ExportDefinitionId { get; }
		public string Title { get; }

	}
}