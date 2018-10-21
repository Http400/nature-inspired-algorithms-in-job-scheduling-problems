using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Utils
{
    public static class FileHelper
    {
        private static string _url = "http://mistic.heig-vd.ch/taillard/problemes.dir/ordonnancement.dir/ordonnancement.html";
        private static string _urlBase = "http://mistic.heig-vd.ch/taillard/problemes.dir/ordonnancement.dir/";


        public static void DownloadTaillardTestInstances(string pathToSave)
        {
            if (Directory.Exists(pathToSave))
            {
                var directory = new DirectoryInfo(pathToSave);
                directory.Delete(true);
            }

            // Act
            var pageSource = FileHelper.GetTaillardTestingInstancesPageSource().Result;
            var urls = FileHelper.GetTxtFilesUrlsFromPageSource(pageSource);

            foreach (var fileUrl in urls)
            {
                var (problemType, fileName) = FileHelper.GetSchedulingProblemTypeAndFilenameFromUrl(fileUrl);
                FileHelper.DownloadFile(fileUrl, pathToSave + "/" + problemType, fileName);

                if (fileName.Contains("best"))
                    continue;

                FileHelper.SplitFile(pathToSave + "/" + problemType, fileName, new string[] { "number of jobs", "Nb of jobs" });
            }
        }

        public static async Task<string> GetTaillardTestingInstancesPageSource()
        {
            var httpClient = new HttpClient();

            using (var response = await httpClient.GetAsync(_url))
            {
                using (var content = response.Content)
                {
                    var stringResult = await content.ReadAsStringAsync();
                    //Console.Write(stringResult);
                    return stringResult;
                }
            }
        }

        public static string[] GetTxtFilesUrlsFromPageSource(string pageSource)
        {
            return Regex.Matches(pageSource, "\"([^\"]*).txt\"")
                .OfType<Match>()
                .Select(m => m.Groups[0].Value.Trim('"'))
                .ToArray();
        }

        public static (string problemType, string fileName) GetSchedulingProblemTypeAndFilenameFromUrl(string fileUrl)
        {
            var split = fileUrl.Split("/");
            var schedulingProblemType = split[0].Split(".")[0];
            var fileName = split[1];
            
            return (schedulingProblemType, fileName);
        }

        public static void DownloadFile(string fileUrl, string path, string fileName)
        {
            CreateFolderIfNotExists(path);
            string filePath = path + "/" + fileName;
            
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(_urlBase + fileUrl, filePath);
            }
        }

        public static void ParseFile(string path, string fileName)
        {
            var lines = File.ReadAllLines(path + "/" + fileName);
            foreach (var line in lines)
            {
                System.Console.WriteLine(line);
            }
        }

        public static void SplitFile(string path, string fileName, string[] splittingStrings)
        {
            var lines = File.ReadAllLines(path + "/" + fileName);
            string newFolder = fileName.Split(".")[0];
            string newFile = "";
            int fileCounter = 0;

            for (int i = 0; i < lines.Count(); i++)
            {
                if (newFile != "") { newFile += "\n"; }

                newFile += lines[i];

                if (i == lines.Count()-1 || splittingStrings.Any(s => lines[i+1].StartsWith(s)))
                {
                    CreateFolderIfNotExists(path + "/" + newFolder);
                    File.WriteAllText(path + "/" + newFolder + "/" + fileCounter + ".txt", newFile);
                    newFile = "";
                    fileCounter++;
                }
            }

            File.Delete(path + "/" + fileName);
        }

        public static string ReadFile(string path, string fileName)
        {
            using (StreamReader reader = new StreamReader(path + "/" + fileName))
            {
                return reader.ReadToEnd();
            }
        }

        public static void WriteToFile(string path, string fileName, string content)
        {
            CreateFolderIfNotExists(path);
            File.AppendAllText(path + "/" + fileName, content + Environment.NewLine);
        }

        public static void CreateFile(string path, string fileName, string content)
        {
            CreateFolderIfNotExists(path);
            File.WriteAllText(path + "/" + fileName, content + Environment.NewLine);
        }

        private static void CreateFolderIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}