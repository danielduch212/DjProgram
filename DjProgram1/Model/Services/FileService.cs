﻿using DjProgram1.Model.Data;
using GLib;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using TagLib;
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

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string folderPath = Path.Combine(projectDirectoryInfo.FullName, "songCopies");
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(folderPath);
            foreach (System.IO.FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete(); 
            }
        }
        public void DeleteCopies()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string folderPath = Path.Combine(projectDirectoryInfo.FullName, "songCopies");
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




    }

}


