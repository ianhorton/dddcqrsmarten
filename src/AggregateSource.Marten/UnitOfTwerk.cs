using System;
using System.Collections.Generic;
using System.Linq;
using Marten;

namespace AggregateSource.Marten
{
	public class UnitOfTwerk : UnitOfWork, IDisposable
	{
		public IDocumentSession Session { get; }
		private readonly Dictionary<string, StreamCache> _streamChangesCache;

		public UnitOfTwerk(IDocumentSession session)
		{
			Session = session;
			_streamChangesCache = new Dictionary<string, StreamCache>();
		}

		// http://jasperfx.github.io/marten/documentation/events/appending/
		// events are inserted into the event stream if and only if the maximum event id 
		// we need to be able to gather up all the changes and push them to the stream 
		public void StoreChanges(string stream, int expectedVersion, params object[] changes)
		{
			if (_streamChangesCache.TryGetValue(stream, out var streamCache) == false)
			{
				streamCache = new StreamCache(expectedVersion, stream);
				_streamChangesCache.Add(stream, streamCache);
			}

			foreach (var change in changes)
			{
				var hash = change.GetHashCode();
				if (streamCache.ChangeCache.ContainsKey(hash)) continue;

				streamCache.ChangeCache.Add(hash, change);
			}
		}

		public void CommitChanges()
		{
			foreach (var streamCache in _streamChangesCache.Values)
				Session.Events.Append(streamCache.Stream, streamCache.Version, streamCache.ChangeCache.Values.ToArray());

			Session.SaveChanges();
		}

		public void Dispose()
		{
			Session?.Dispose();
		}
	}
}