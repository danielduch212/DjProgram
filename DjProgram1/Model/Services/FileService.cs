using DjProgram1.Controls;
using DjProgram1.Model.Data;
using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using ListBox = System.Windows.Controls.ListBox;

namespace DjProgram1.Model.Services
{
    public class FileService
    {

        private MusicService musicService = new MusicService();
        

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

            foreach (var audioFile in audioFiles)
            {
                bpm = checkMetaDataBPM(audioFile.FilePath);
                audioFile.BPM = bpm;

            }
            CheckSongFileData(audioFiles);
            writeBPMData(audioFiles);

        }

        public void CheckSongFileData(AudioFile[] audioFiles)
        {
            string fileName = "baza_Danych_BPM";
            string currentCatalog = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(currentCatalog, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine("Plik bazy danych BPM nie istnieje.");
                return;
            }

            string[] lines = System.IO.File.ReadAllLines(filePath);

            foreach (var audioFile in audioFiles)
            {
                string matchingLine = lines.FirstOrDefault(line => line.Contains(audioFile.FileName));
                if (!string.IsNullOrEmpty(matchingLine))
                {
                    string[] parts = matchingLine.Split(new string[] { "BPM:" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        if (double.TryParse(parts[1].Trim(), out double bpm) && bpm != 0)
                        {
                            audioFile.BPM = bpm;
                        }
                    }
                }
            }
        }

        public void UpdateDataBase(AudioFile[] audioFiles)
        {
            string fileName = "baza_Danych_BPM";
            string currentCatalog = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(currentCatalog, fileName);

            using (StreamWriter writer = new StreamWriter(filePath, append: false))
            {
                foreach (var audioFile in audioFiles)
                {
                    double bpmValue = audioFile.BPM ?? 0;
                    string lineToWrite = $"{audioFile.FileName}   BPM: {bpmValue}";
                    writer.WriteLine(lineToWrite);
                }
            }
        }

        public void clearDataBase()
        {
            string fileName = "baza_Danych_BPM";
            string currentCatalog = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(currentCatalog, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine("Plik bazy danych BPM nie istnieje.");
                return;
            }

            System.IO.File.WriteAllText(filePath, string.Empty);
        }

        public List<AudioFile> uploadSongs(List<AudioFile> audioFiles, string folderPath, ListBox listBox)
        {
            string databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "baza_Danych_BPM");
            Dictionary<string, double?> bpmData = new Dictionary<string, double?>();

            if (System.IO.File.Exists(databasePath))
            {
                string[] bpmLines = System.IO.File.ReadAllLines(databasePath);
                foreach (var line in bpmLines)
                {
                    var parts = line.Split(new[] { "BPM:" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out double bpm))
                    {
                        bpmData[parts[0].Trim()] = bpm;
                    }
                }
            }

            string[] files = Directory.GetFiles(folderPath)
                .Where(file => file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            System.Windows.Application.Current.Dispatcher.Invoke(() => listBox.Items.Clear());

            foreach (string file in files)
            {
                AudioFile audioFile = new AudioFile
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                };

                if (bpmData.TryGetValue(audioFile.FileName, out double? bpmValue))
                {
                    audioFile.BPM = bpmValue;
                }

                audioFiles.Add(audioFile);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var listBoxItem = new ListBoxItem
                    {
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch
                    };

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var fileNameTextBlock = new TextBlock
                    {
                        Text = audioFile.FileName,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap
                    };
                    Grid.SetColumn(fileNameTextBlock, 0);

                    var bpmTextBlock = new TextBlock
                    {
                        Text = audioFile.BPM.HasValue && audioFile.BPM > 0 ? $"BPM: {audioFile.BPM.Value}" : "BPM: 0",
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    Grid.SetColumn(bpmTextBlock, 1);

                    grid.Children.Add(fileNameTextBlock);
                    grid.Children.Add(bpmTextBlock);

                    listBoxItem.Content = grid;
                    listBoxItem.Background = audioFile.BPM.HasValue && audioFile.BPM > 0 ? Brushes.White : Brushes.LightGray;

                    listBox.Items.Add(listBoxItem);
                });
            }

            return audioFiles;
        }

        public string CheckIfSongExists(string name)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "songCopies");
            string notFound = "";
            if (!Directory.Exists(folderPath))
            {

                return notFound;
            }

            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (file.Contains(name))
                {
                    return file;
                }
            }
            return notFound;
        }

        public void deleteSong(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        public void DeleteAllCopies()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent.Parent;

            string folderPath = Path.Combine(projectDirectoryInfo.FullName, "DjProgram1","songCopies");
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            foreach (System.IO.FileInfo file in directoryInfo.GetFiles())
            {
                try
                {
                    file.Delete();

                }
                catch (Exception e)
                {

                }
            }
        }
        public void DeleteCopies()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent.Parent;

            string folderPath = Path.Combine(projectDirectoryInfo.FullName, "DjProgram1", "songCopies");
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            System.IO.FileInfo[] files = new DirectoryInfo(folderPath).GetFiles();

            if (files.Length > 3)
            {
                System.IO.FileInfo oldestFile = files.OrderBy(f => f.CreationTime).First();

                oldestFile.Delete();
            }
        }

        public void RefreshListBox(ListBox listBox, List<AudioFile> audioFiles)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                listBox.Items.Clear();

                foreach (var audioFile in audioFiles)
                {
                    listBox.Items.Add(CreateListBoxItem(audioFile));
                }
            });

        }

        private ListBoxItem CreateListBoxItem(AudioFile audioFile)
        {
            return System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var listBoxItem = new ListBoxItem
                {
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch,
                    Background = audioFile.BPM.HasValue && audioFile.BPM > 0 ? Brushes.White : Brushes.LightGray
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var fileNameTextBlock = new TextBlock
                {
                    Text = audioFile.FileName,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.NoWrap
                };
                Grid.SetColumn(fileNameTextBlock, 0);

                var bpmTextBlock = new TextBlock
                {
                    Text = audioFile.BPM.HasValue && audioFile.BPM > 0 ? $"BPM: {audioFile.BPM.Value}" : "BPM: 0",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(bpmTextBlock, 1);

                grid.Children.Add(fileNameTextBlock);
                grid.Children.Add(bpmTextBlock);

                listBoxItem.Content = grid;

                return listBoxItem;
            }) as ListBoxItem;
        }
        public string findControlsFilePath(string name)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "DjProgram1", "controlsImages");

            DirectoryInfo dirInfo = new DirectoryInfo(servicesPath);
            FileInfo[] files = dirInfo.GetFiles(name + ".*", SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                return files[0].FullName;
            }
            else
            {
                return null;
            }

        }

        public void InitializeControls(Image playButtonImage1, Image pauseButtonImage1, Image stopButtonImage1, Image image1, Image imageLoading1, Image playButtonImage2, Image pauseButtonImage2, Image stopButtonImage2, Image image2, Image imageLoading2)
        {
            string filepath = findControlsFilePath("play");
            BitmapImage bitmapImage = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            playButtonImage1.Source = bitmapImage;
            filepath = findControlsFilePath("pause");
            BitmapImage bitmapImage1 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            pauseButtonImage1.Source = bitmapImage1;
            filepath = findControlsFilePath("stop");
            BitmapImage bitmapImage2 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            stopButtonImage1.Source = bitmapImage2;
            filepath = findControlsFilePath("cd");
            BitmapImage bitmapImage3 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            image1.Source = bitmapImage3;
            filepath = findControlsFilePath("loading");
            BitmapImage bitmapImage4 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            imageLoading1.Source = bitmapImage4;

            filepath = findControlsFilePath("play");
            BitmapImage bitmapImage5 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            playButtonImage2.Source = bitmapImage5;
            filepath = findControlsFilePath("pause");
            BitmapImage bitmapImage6 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            pauseButtonImage2.Source = bitmapImage6;
            filepath = findControlsFilePath("stop");
            BitmapImage bitmapImage7 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            stopButtonImage2.Source = bitmapImage7;
            filepath = findControlsFilePath("cd");
            BitmapImage bitmapImage8 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            image2.Source = bitmapImage8;
            filepath = findControlsFilePath("loading");
            BitmapImage bitmapImage9 = new BitmapImage(new Uri(filepath, UriKind.Absolute));
            imageLoading2.Source = bitmapImage9;


        }
        public string FindPythonDDLFilePath()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            FileInfo[] files = projectDirectoryInfo.GetFiles("pythonDLLfilePath.txt", SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                string filePath = files[0].FullName;
                string fileContent = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(fileContent))
                {
                    return fileContent;
                }
            }

            while (true)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBoxResult result1 = MessageBox.Show(
                        "Podaj folder zawierający bibliotekę pythonDLL 311",
                        "Informacja",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });

                string selectedFilePath = null;
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;
                    files = new DirectoryInfo(folderPath).GetFiles("python311.dll", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        WritePythonDDLFilePath(files[0].FullName);
                        return files[0].FullName;
                    }
                    else
                    {
                        MessageBox.Show("Nie znaleziono biblioteki python311.dll w podanym folderze. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }




            }
        }

        private void WritePythonDDLFilePath(string filePath)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            if (projectDirectoryInfo != null && projectDirectoryInfo.Exists)
            {
                string filePathToWrite = Path.Combine(projectDirectoryInfo.FullName, "pythonDLLfilePath.txt");

                if (File.Exists(filePathToWrite))
                {
                    File.WriteAllText(filePathToWrite, filePath);
                }
                else
                {
                    using (var fileStream = File.Create(filePathToWrite))
                    {
                        fileStream.Close();
                    }

                    File.WriteAllText(filePathToWrite, filePath);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Nie można znaleźć katalogu projektu.");
            }
        }
        

        public void ClearFilePythonDDl()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            if (projectDirectoryInfo != null && projectDirectoryInfo.Exists)
            {
                string fileToClearPath = Path.Combine(projectDirectoryInfo.FullName, "pythonDLLfilePath.txt");

                if (File.Exists(fileToClearPath))
                {
                    File.WriteAllText(fileToClearPath, string.Empty);
                }
                
            }
            
        }

    }
}



