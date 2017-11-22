using System.Collections.Generic;

namespace AggregateSource.Marten
{
	public class StreamCache
	{
		private readonly int _expectedVersion;

		public StreamCache(int expectedVersion, string stream)
		{
			_expectedVersion = expectedVersion;
			Stream = stream;
			ChangeCache = new Dictionary<int, object>();
		}

		public string Stream { get; }
		public Dictionary<int, object> ChangeCache { get; }
		public int Version => _expectedVersion + ChangeCache.Count;
	}
}