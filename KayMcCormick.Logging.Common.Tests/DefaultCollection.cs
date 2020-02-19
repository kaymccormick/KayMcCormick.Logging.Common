using System;
using System.Collections.Generic;
using System.Text;
using KayMcCormick.Test.Common.Fixtures ;
using Xunit ;

namespace KayMcCormick.Logging.Common.Tests
{
    [CollectionDefinition("Default")]
    class DefaultCollection : ICollectionFixture <GlobalLoggingFixture>
    {
    }
}
