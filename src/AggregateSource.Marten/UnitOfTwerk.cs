using System;
using Marten;

namespace AggregateSource.Marten
{
	public class UnitOfTwerk : UnitOfWork, IDisposable
	{
		public IDocumentSession Session { get; }
		public Action PushToStream;

		public UnitOfTwerk(IDocumentSession session)
		{
			Session = session;
		}

		public void SaveChanges()
		{
			PushToStream();
			Session.SaveChanges();
		}

		public void Dispose()
		{
			Session?.Dispose();
		}
	}
}