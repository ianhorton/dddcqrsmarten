using System;
using AggregateSource.Testing;
using DefiningExports;
using DefiningExports.Events;
using NUnit.Framework;

namespace Definingexports.Tests
{
	public class ExportDefinitionTestFixture
    {
		[Test]
	    public void CreateNew_CalledWithValidParams_GeneratesExportDefinitionCreatedEvent()
	    {
			var exportDefinitionId = Guid.NewGuid();
		    new ConstructorScenarioFor<ExportDefinition>(() => ExportDefinition.CreateNew(exportDefinitionId, "foo"))
			    .Then(new ExportDefinitionCreated(exportDefinitionId, "foo"))
			    .Assert();
	    }
    }
}
