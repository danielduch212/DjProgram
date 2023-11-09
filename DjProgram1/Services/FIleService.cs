using DjProgram1.Data;
using GLib;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using TagLib;

namespace DjProgram1.Services
{
    public class FIleService
    {

        private MusicService musicService = new MusicService();

        public void createFile()
        {
            string fileName = "baza_Danych_BPM";

            try
            {
                // Pobierz ścieżkę do bieżącego katalogu aplikacji
                string currentCatalog = AppDomain.CurrentDomain.BaseDirectory;

                // Utwórz pełną ścieżkę do nowego pliku
                string filePath = Path.Combine(currentCatalog, fileName);
                if(!System.IO.File.Exists(filePath))
                {
                    StreamWriter writer = System.IO.File.CreateText(filePath);
                }
                // Utwórz nowy plik tekstowy
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }



        public void writeBPMData(AudioFile[] audioFiles)
        {
            string fileName = "baza_Danych_BPM";
            string currentCatalog = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(currentCatalog, fileName);

            try
            {
                var existingLines = new HashSet<string>(System.IO.File.ReadAllLines(filePath));

                using (StreamWriter writer = new StreamWriter(filePath, append: true))
                {
                    foreach (var audioFile in audioFiles)
                    {
                        string lineToWrite = $"{audioFile.FileName}   BPM: {audioFile.BPM}";

                        // Sprawdź, czy linia już istnieje w pliku
                        if (!existingLines.Contains(lineToWrite))
                        {
                            writer.WriteLine(lineToWrite);
                            existingLines.Add(lineToWrite); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public string checkFileExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".wav")
            {
                return "wav";
            }
            else if (extension == ".mp3")
            {
                return "mp3";
            }

            return "unknown";
        }
        
        public double checkMetaDataBPM(string filePath)
        {
            
            var file = TagLib.File.Create(filePath);

            if (file.Tag.BeatsPerMinute > 0)
            {
                return file.Tag.BeatsPerMinute;
            }
            else
            {
                return 0;
            }
        }

        public void checkSongsMetaData(AudioFile[] audioFiles)
        {
            double bpm = 0;
            
            foreach(var audioFile in audioFiles)
            {
                bpm = checkMetaDataBPM(audioFile.FilePath);
                audioFile.BPM = bpm;

            }
            writeBPMData(audioFiles);

        }

        public List<DjProgram1.Data.AudioFile> uploadSongs(List<DjProgram1.Data.AudioFile> audioFiles, System.Windows.Controls.ListBox listBox)
        {
            string folderPath = @"C:\Users\Janusz\Desktop\BazaPiosenek";
            string[] files = Directory.GetFiles(folderPath)
                .Where(file => file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string file in files)
            {
                DjProgram1.Data.AudioFile audioFile = new DjProgram1.Data.AudioFile();
                audioFile.FileName = Path.GetFileName(file);
                audioFile.FilePath = file;
                audioFiles.Add(audioFile);
                var audioFileReader = new AudioFileReader(audioFile.FilePath);

            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

                foreach (var file in audioFiles)
                {
                    listBox.Items.Add(file.FileName);

                }
            });

            return audioFiles; 
        }
    }
}
