using System;
using System.Collections.Generic;
using Value;

namespace DefiningExports
{
	public class ExportDefinitionId : ValueType<ExportDefinitionId>
	{
		public readonly Guid Id;
		public ExportDefinitionId()
		{
			Id = Guid.NewGuid();
		}
		public ExportDefinitionId(Guid id)
		{
			Id = id;
		}
		protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
		{
			return new List<object> { Id };
		}
		public override string ToString()
		{
			return Id.ToString();
		}
	}
}