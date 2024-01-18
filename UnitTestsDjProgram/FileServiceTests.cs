using DjProgram1.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UnitTestsDjProgram
{

    [TestClass]
    internal class FileServiceTests
    {


        [TestMethod]
        public void FindGoodPathToControls()
        {
            FileService fileService = new FileService();
            var path = fileService.findControlsFilePath("loading");
            var estimatedPath = "C:\\Users\\Janusz\\source\\repos\\DjProgram1\\DjProgram1\\controlsImages\\loading.png";
            Assert.AreEqual(path, estimatedPath);

        }

        [TestMethod]
        public void ClearSongCopies_DeletesAllFilesInSongCopiesFolder()
        {
            FileService fileService = new FileService();

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent.Parent;
            string songCopiesPath = Path.Combine(projectDirectoryInfo.FullName, "DjProgram1", "songCopies");

            if (!Directory.Exists(songCopiesPath))
            {
                Directory.CreateDirectory(songCopiesPath);
            }

            string testFile1 = Path.Combine(songCopiesPath, "testFile1.mp3");
            File.WriteAllText(testFile1, "Dummy content");
            string testFile2 = Path.Combine(songCopiesPath, "testFile2.wav");
            File.WriteAllText(testFile2, "Dummy content");

            fileService.DeleteAllCopies();

            var filesAfterDeletion = Directory.GetFiles(songCopiesPath);
            Assert.AreEqual(0, filesAfterDeletion.Length, "Folder songCopies nie jest pusty po usunięciu wszystkich plików.");
        }

        [TestMethod]
        public void CheckIfSongExists_ReturnGoodFilePath()
        {
            FileService fileService = new FileService();
            var path = fileService.CheckIfSongExists("jersey club v1.wav");

            var estimatedPath = "C:\\Users\\Janusz\\source\\repos\\DjProgram1\\UnitTestsDjProgram\\testSongBase\\jersey club v1.wav";

            Assert.AreEqual(path, estimatedPath);

        }

    }
}
