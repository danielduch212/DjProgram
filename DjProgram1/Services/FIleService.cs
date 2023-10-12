using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if(!File.Exists(filePath))
                {
                    StreamWriter writer = File.CreateText(filePath);
                }
                // Utwórz nowy plik tekstowy
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void writeBPMData(string filename)
        {

        }
        
    }
}
