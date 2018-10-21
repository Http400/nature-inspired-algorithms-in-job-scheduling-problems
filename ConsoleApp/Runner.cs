using System.IO;
using System.Reflection;
using Core.Utils;

namespace ConsoleApp
{
    public class Runner
    {
        //public void PerformJobScheduling()

        public void DownloadTestInstances()
        {
            var p2 = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/../../../InputData/TestInstances";

            FileHelper.DownloadTaillardTestInstances(p2);
        }
    }
}