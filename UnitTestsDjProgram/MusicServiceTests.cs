using DjProgram1.Model.Services;

namespace UnitTestsDjProgram
{
    [TestClass]
    public class MusicServiceTests
    {
        [TestMethod]
        public void GenerateWaveformData_ReturnsCorrectSampleCount()
        {


            MusicService musicService = new MusicService();            
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;
            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "testFiles");






        }


        //w katalogu projektu zrobic sobie plik do testow lub dwa ogolnie zrobic folder testing czy cos
        //a i ogolnie wywalic te polskie nazwy plikow raczej
        //plan na jutro - z kilka testow skonczyc - pozniej troche interfejsu z 2 godziny ulepszania - pozniej caly dzien pisanie pracy

    }
}