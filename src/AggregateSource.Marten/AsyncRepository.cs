using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;

namespace AggregateSource.Marten
{
	public class AsyncRepository<TAggregateRoot> : IAsyncRepository<TAggregateRoot>
		where TAggregateRoot : class, IAggregateRootEntity,  new()
	{
		private string _stream;
		private int _expectedVersion = -1;
		private readonly Dictionary<int, object> _changeCache = new Dictionary<int, object>();

		public AsyncRepository(Func<TAggregateRoot> rootFactory, UnitOfTwerk unitOfWork)
		{
			if (rootFactory == null) throw new ArgumentNullException("rootFactory");
			if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

			RootFactory = rootFactory;
			UnitOfWork = unitOfWork;
			unitOfWork.PushToStream = PushToStream;
		}

		public Func<TAggregateRoot> RootFactory { get; }
		public UnitOfTwerk UnitOfWork { get; }

		public async Task<TAggregateRoot> GetAsync(string identifier)
		{
			var result = await GetOptionalAsync(identifier);
			if (!result.HasValue)
				throw new AggregateNotFoundException(identifier, typeof(TAggregateRoot));
			return result.Value;
		}

		public async Task<Optional<TAggregateRoot>> GetOptionalAsync(string identifier)
		{
			Aggregate aggregate;
			if (UnitOfWork.TryGet(identifier, out aggregate))
			{
				return new Optional<TAggregateRoot>((TAggregateRoot) aggregate.Root);
			}

			var events = await UnitOfWork.Session.Events.FetchStreamAsync(identifier);
			if (events == null || events.IsEmpty()) return Optional<TAggregateRoot>.Empty;

			var root = RootFactory();
			var eventObjects = events.Select(x => x.Data).ToList();
			var lastVersion = eventObjects.Any() ? events.Max(x => x.Version) : 0;

			root.Initialize(eventObjects);
			UnitOfWork.Attach(new Aggregate(identifier, lastVersion, root));
			return new Optional<TAggregateRoot>(root);
		}

		public void Add(string identifier, TAggregateRoot root)
		{
			UnitOfWork.Session.Events.StartStream<TAggregateRoot>(identifier);
			UnitOfWork.Attach(new Aggregate(identifier, 0, root));
		}

		public void AppendToStream(string stream, int expectedVersion, params object[] changes)
		{
			if (_stream == null) _stream = stream;
			else if( _stream != stream) throw new InvalidOperationException();

			if (_expectedVersion == -1) _expectedVersion = expectedVersion;
			else if(_expectedVersion != expectedVersion) throw new InvalidOperationException();

			foreach (var change in changes)
			{
				var hash = change.GetHashCode();
				if(_changeCache.ContainsKey(hash)) continue;

				_changeCache.Add(hash, change);
			}

		}

		private void PushToStream()
		{
			UnitOfWork.Session.Events.Append(_stream, _expectedVersion + _changeCache.Count, _changeCache.Values.ToArray());
		}

		// http://jasperfx.github.io/marten/documentation/events/appending/
			// events are inserted into the event stream if and only if the maximum event id 


			//public void AppendToStream(string stream, int expectedVersion, params object[] changes)
			//{
			//	int version;

			//	if (_session.Events.FetchStream(stream, 1).IsEmpty())
			//	{
			//		_session.Events.StartStream(stream);

			//		// new stream
			//		version = (expectedVersion + changes.Length);
			//		_session.Events.Append(stream, version, changes);
			//		return;
			//	}

			//	if (_session.PendingChanges.Streams().Any(s => s.Key == stream) == false)
			//	{
			//		// existing stream - no event
			//		version = (expectedVersion + changes.Length);
			//		_session.Events.Append(stream, version, changes);
			//		return;
			//	}
			//	else
			//	{
			//		// existing stream - pending events
			//	}





			//	if (_session.PendingChanges.Streams().Any() == false)
			//	{
			//		version = (expectedVersion + changes.Length);
			//		_session.Events.Append(stream, version, changes);
			//		return;
			//	}

			//	var pendingEvents = _session.PendingChanges.Streams().First(s => s.Key == stream).Events.ToList();
			//	var pendingEventData = pendingEvents.Select(e => e.Data);

			//	var notAppended = changes.Except(pendingEventData).ToList();

			//	version = expectedVersion + pendingEvents.Count + notAppended.Count;
			//	_session.Events.Append(stream, version, notAppended.ToArray());

			//}

			//public void _AppendToStream(string stream, int expectedVersion, params object[] changes)
			//	{

			//		// new stream
			//		// existing stream 



			//		int version;

			//		if (_session.PendingChanges.Streams().Any() == false)
			//		{
			//			version = (expectedVersion + changes.Length);
			//			_session.Events.Append(stream, version, changes);
			//			return;
			//		}

			//		var pendingEvents = _session.PendingChanges.Streams().First(s => s.Key == stream).Events.ToList();
			//		var pendingEventData = pendingEvents.Select(e => e.Data);

			//		var notAppended = changes.Except(pendingEventData).ToList();

			//		version = expectedVersion + pendingEvents.Count + notAppended.Count;
			//		_session.Events.Append(stream, version, notAppended.ToArray());

			//	}
		}
}


