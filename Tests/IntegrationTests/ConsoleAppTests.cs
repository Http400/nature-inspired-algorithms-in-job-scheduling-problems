using ConsoleApp;
using NUnit.Framework;

namespace Tests.IntegrationTests.ConsoleAppTests
{
    [TestFixture]
    public class ConsoleAppTests
    {
        [Test]
        public void test_downloading_test_instances()
        {
            // Arrange
            var runner = new Runner();

            // Act
            runner.DownloadTestInstances();
        }
    }
}