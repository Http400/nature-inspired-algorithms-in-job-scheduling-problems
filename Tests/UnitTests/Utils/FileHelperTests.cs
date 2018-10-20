using System.IO;
using Core.Utils;
using NUnit.Framework;

namespace Tests.UnitTests.Utils
{
    [TestFixture]
    public class FileHelperTests
    {
        [Test]
        public void simple_test()
        {
            // Act
            var result = FileHelper.GetTaillardTestingInstancesPageSource().Result;
            
            // Assert
            Assert.True(result.Length > 0);
        }

        [Test]
        public void test_getting_txt_files_names_from_page_source()
        {
            // Arrange
            var testString = @"<TITLE>Éric Taillard's page </TITLE>
                <H2> Scheduling instances  </H2>
                <H3>Published in E. Taillard, ""Benchmarks for basic scheduling problems"", EJOR 64(2):278-285, 1993. 
                <a href=""http://mistic.heig-vd.ch/taillard/articles.dir/Taillard1993EJOR.pdf"">Technical report </a></H3>

                <LI> Flow shop sequencing
                <UL>
                <LI><a href=""flowshop.dir/best_lb_up.txt""> Summary</a> of best known lower and upper bounds of Taillard's instances
                <LI><a href=""flowshop.dir/tai20_5.txt""> Taillard,  20 jobs  5 machines</a>
                </UL>";

            // Act
            var result = FileHelper.GetTxtFilesUrlsFromPageSource(testString);
            
            // Assert
            Assert.AreEqual(2, result.Length);
            foreach (var txt in result)
            {
                Assert.True(txt.EndsWith(".txt"));
            }
        }

        [Test]
        public void test_getting_scheduling_problem_type_and_filename_from_file_url()
        {
            // Arrange
            var testUrl = "flowshop.dir/tai20_5.txt";

            // Act
            var result = FileHelper.GetSchedulingProblemTypeAndFilenameFromUrl(testUrl);

            // Assert
            Assert.AreEqual("flowshop", result.problemType);
            Assert.AreEqual("tai20_5.txt", result.fileName);
        }

        [Test]
        public void test_downloading_files()
        {
            // Arrange
            var fileUrl = "http://mistic.heig-vd.ch/taillard/problemes.dir/ordonnancement.dir/flowshop.dir/tai20_5.txt";
            var fileName = "tai20_5.txt";
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "UnitTests/Utils";

            // Act
            FileHelper.DownloadFile(fileUrl, path, fileName);

            // Assert
            Assert.IsTrue( File.Exists(path + "/" + fileName) );
            
            // Clean
            File.Delete(path + "/" + fileName);
        }

        [Test]
        public void test_parsing_data_from_file()
        {
            // Arrange
            var fileName = "tai20_5.txt";
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/test";

            // Act
            FileHelper.ParseFile(path, fileName);
        }

        [Test]
        public void test_splitting_instaces_file_into_multiple_files()
        {
            // Arrange
            var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "/Utils";
            var fileName = "tai20_5.txt";

            // Act
            FileHelper.SplitFile(path, fileName, new string[] {"number of jobs"});
        }
    }
}