using System;
using System.Collections.Generic;
using Value;

namespace DefiningExports
{
	public class ExportRowId : ValueType<ExportRowId>
	{
		public readonly Guid Id;
		public ExportRowId()
		{
			Id = Guid.NewGuid();
		}
		public ExportRowId(Guid id)
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