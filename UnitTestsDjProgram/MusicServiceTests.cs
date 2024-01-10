using DjProgram1.Model.Services;

namespace UnitTestsDjProgram
{
    [TestClass]
    public class MusicServiceTests
    {
        [TestMethod]
        public void GenerateWaveformData_GenerateWaveformData_ReturnsCorrectSampleCount_ForValidFile()
        {


            MusicService musicService = new MusicService();            
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "testFiles");

            var testFilePath = @"œcie¿ka\do\pliku\poprawnego.wav";

            var result = musicService.GenerateWaveformData(testFilePath);

            // SprawdŸ oczekiwane wyniki, np. czy liczba próbek jest poprawna
            Assert.AreEqual(expectedSampleCount, result.SampleCount);
        }

        // Testowanie z pustym plikiem
        [TestMethod]
        public void GenerateWaveformData_ReturnsZero_ForEmptyFile()
        {
            var musicService = new MusicService();
            var emptyFilePath = @"œcie¿ka\do\pustego\pliku.wav";

            var result = musicService.GenerateWaveformData(emptyFilePath);

            // SprawdŸ, czy liczba próbek jest równa zero dla pustego pliku
            Assert.AreEqual(0, result.SampleCount);
        }

        // Testowanie wszystkich plików w folderze
        [TestMethod]
        public void GenerateWaveformData_ReturnsValidSampleCount_ForAllFilesInFolder()
        {
            var musicService = new MusicService();
            var testFilesPath = @"œcie¿ka\do\folderu\z\plikami\testowymi";
            var testFiles = Directory.GetFiles(testFilesPath, "*.wav");

            foreach (var filePath in testFiles)
            {
                var result = musicService.GenerateWaveformData(filePath);

                // SprawdŸ oczekiwane wyniki dla ka¿dego pliku
                // Mo¿esz chcieæ u¿yæ Assert.IsTrue z warunkiem, który definiuje "poprawnoœæ" próbki
                // na przyk³ad, ¿e liczba próbek powinna byæ wiêksza ni¿ 0 dla niepustych plików
                Assert.IsTrue(result.SampleCount > 0);
            }
        }

        //w katalogu projektu zrobic sobie plik do testow lub dwa ogolnie zrobic folder testing czy cos
        //a i ogolnie wywalic te polskie nazwy plikow raczej
        //plan na jutro - z kilka testow skonczyc - pozniej troche interfejsu z 2 godziny ulepszania - pozniej caly dzien pisanie pracy

    }
}