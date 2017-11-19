using System;
using System.Collections.Generic;
using AggregateSource;
using DefiningExports.Events;

namespace DefiningExports
{
	public class ExportDefinition : AggregateRootEntity
	{
		public string Id { get; }
		public ExportDefinitionId ExportDefinitionId { get; private set; }
		public string Title { get; private set; }

		public List<ExportRow> ExportRowList { get; private set; }

		public ExportDefinition()
		{
			Register<ExportDefinitionCreated>(@event =>
			{
				ExportDefinitionId = new ExportDefinitionId(@event.ExportDefinitionId);
				Title = @event.Title;
				ExportRowList = new List<ExportRow>();
			});

			Register<ExportRowAddedToExportDefinition>(@event =>
			{
				var row = new ExportRow(ApplyChange);
				row.Route(@event);
				ExportRowList.Add(row);
			});

		}

		public static readonly Func<ExportDefinition> Factory = () => new ExportDefinition();

		public static ExportDefinition CreateNew(Guid exportDefinitionId, string title)
		{
			var ed = new ExportDefinition();
			ed.ApplyChange(new ExportDefinitionCreated(exportDefinitionId, title));
			return ed;
		}

		public void AddRowToRowsCollection(Guid exportRowId, string name)
		{
			// delgate invariant checks to the object that is interested in this stuff
			ApplyChange(ExportRow.CreateNewRow(exportRowId, name));
		}
	}
}