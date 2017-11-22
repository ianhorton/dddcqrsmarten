using System;
using System.Linq;
using System.Threading.Tasks;
using Marten;

namespace AggregateSource.Marten
{
	public class AsyncRepository<TAggregateRoot> : IAsyncRepository<TAggregateRoot>
		where TAggregateRoot : class, IAggregateRootEntity, new()
	{
		public AsyncRepository(Func<TAggregateRoot> rootFactory, UnitOfTwerk unitOfWork)
		{
			RootFactory = rootFactory ?? throw new ArgumentNullException(nameof(rootFactory));
			UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
				return new Optional<TAggregateRoot>((TAggregateRoot)aggregate.Root);
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
			UnitOfWork.StoreChanges(stream, expectedVersion, changes);
		}
	}
}