using DjProgram1.Model.Services;
using Python.Runtime;
using VisioForge.Libs.ZXing;

namespace UnitTestsDjProgram
{
    [TestClass]
    public class MusicServiceTests
    {
        string filePathDDL = @"C:\Program Files\Python311\python311.dll";

        [TestMethod]
        public void GenerateWaveformData_GenerateWaveformData_ReturnsCorrectSampleCount_ForValidFile()
        {

            MusicService musicService = new MusicService(filePathDDL);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string testFilesPath = Path.Combine(projectDirectoryInfo.FullName, "testFiles");

            string testFileName = "Sentino & Doda - ＂Lato＂ feat. Sanah (AMF BLEND).wav";

            var testFilePath = Path.Combine(testFilesPath, testFileName);

            var result = musicService.GenerateWaveformData(testFilePath);

            Assert.AreEqual(510, result.Count);
        }
        [TestMethod]
        public void GenerateWaveformData_ReturnsZero_ForEmptyFile()
        {
            MusicService musicService = new MusicService(filePathDDL);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string testFilesPath = Path.Combine(projectDirectoryInfo.FullName, "testFiles");

            string testFileName = "emptyWav.wav";

            var testFilePath = Path.Combine(testFilesPath, testFileName);

            var result = musicService.GenerateWaveformData(testFilePath);

            Assert.AreEqual(0, result.Count);
        }
        [TestMethod]
        public void GenerateWaveformData_ReturnsValidSampleCount_ForAllFilesInFolder()
        {
            MusicService musicService = new MusicService(filePathDDL);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string testFilesPath = Path.Combine(projectDirectoryInfo.FullName, "testSongBase");

            string[] testFiles = Directory.GetFiles(testFilesPath);

            foreach (var filePath in testFiles)
            {
                var result = musicService.GenerateWaveformData(filePath);

                Assert.IsTrue(result.Count > 0, $"Test failed for file: {filePath}");
            }
        }
        [TestMethod]
        public void CountBPM_returnsExpectedValue()
        {
            MusicService musicService = new MusicService(filePathDDL);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string testFilesPath = Path.Combine(projectDirectoryInfo.FullName, "testFiles");

            string testFileName = "MACKLEMORE & RYAN LEWIS - CAN'T HOLD US FEAT. RAY DALTON (OFFICIAL MUSIC VIDEO).wav";
            var testFilePath = Path.Combine(testFilesPath, testFileName);

            var bpmString = musicService.CalculateBPMPython(testFilePath);

            

            if (double.TryParse(bpmString, out double bpm))
            {
                Assert.IsTrue(bpm >= 143 && bpm <= 149, $"BPM {bpm} nie mieści się w oczekiwanym przedziale od 143 do 147.");
            }
            else
            {
                Assert.Fail("Nie udało się przekonwertować BPM na wartość liczbową.");
            }
        }
        [TestMethod]
        public void ChangeBPM_EffectivelyChangesBPM()
        {
            MusicService musicService = new MusicService(filePathDDL);
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string testFilesPath = Path.Combine(projectDirectoryInfo.FullName, "testSongBase");

            string testFileName = "Avicii - Waiting For Love.wav";
            var testFilePath = Path.Combine(testFilesPath, testFileName);

            var originalBpmString = musicService.CalculateBPMPython(testFilePath);
            double.TryParse(originalBpmString, out double bpm);


            var filePathNewBPM=musicService.ChangeBPM(testFilePath, bpm, 180.0);
            var calculatedNewBPM = musicService.CalculateBPMPython(filePathNewBPM);
            double.TryParse(calculatedNewBPM, out double newBpm);

            FileService fileService = new FileService();
            fileService.deleteSong(filePathNewBPM);
            Assert.AreEqual(180, newBpm);

        }
    }
}