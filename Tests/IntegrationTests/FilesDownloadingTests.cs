using System.IO;
using Core.Utils;
using NUnit.Framework;

namespace Tests.IntegrationTests
{
    [TestFixture]
    public class FilesDownloadingTests
    {
        [Test]
        public void test_downloading_taillard_instances()
        {
            // Arrange
            var urlBase = "http://mistic.heig-vd.ch/taillard/problemes.dir/ordonnancement.dir/";
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/IntegrationTests/TestInstaces";

            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path);
                directory.Delete(true);
            }

            // Act
            var pageSource = FileHelper.GetTaillardTestingInstancesPageSource().Result;
            var urls = FileHelper.GetTxtFilesUrlsFromPageSource(pageSource);

            foreach (var fileUrl in urls)
            {
                var (problemType, fileName) = FileHelper.GetSchedulingProblemTypeAndFilenameFromUrl(fileUrl);
                FileHelper.DownloadFile(urlBase + fileUrl, path + "/" + problemType, fileName);

                if (fileName.Contains("best"))
                    continue;

                FileHelper.SplitFile(path + "/" + problemType, fileName, new string[] { "number of jobs", "Nb of jobs" });
            }

            // Assert
            Assert.True(Directory.Exists(path + "/" + "flowshop"));
            Assert.True(Directory.Exists(path + "/" + "jobshop"));
            Assert.True(Directory.Exists(path + "/" + "openshop"));
        }
    }
}