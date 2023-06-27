using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VisioForge.Libs.NAudio.Wave;
using VisioForge.Libs.NAudio;
using VisioForge.Libs.TagLib.Mpeg;
using DjProgram1.Data;
using NAudio.Wave;


using Path = System.IO.Path;





namespace DjProgram1
{
    
    public partial class MainWindow : Window
    {
        
        List<DjProgram1.Data.AudioFile> audioFiles = new List<DjProgram1.Data.AudioFile>();
        DjProgram1.Data.AudioFile currentAudioFile1 = new DjProgram1.Data.AudioFile();
        DjProgram1.Data.AudioFile currentAudioFile2 = new DjProgram1.Data.AudioFile();


        public MainWindow()
        {
            InitializeComponent();
            LoadView();
        }

        private void LoadView()
        {
            
        }

        private void ListBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            if (songList.SelectedItem != null)
            {
                int selectedIndex = songList.SelectedIndex;
                if (selectedIndex >= 0 && selectedIndex < audioFiles.Count)
                {
                    currentAudioFile1 = audioFiles[selectedIndex];
                    //tutaj robic sprawdzenie czy cos jest odpalone itd

                    
                }
            }
        }
        private void buttonUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            ListBox listBox = songList; // Podmień 'yourListBoxName' na nazwę swojego ListBox

            string folderPath = @"C:\Users\Janusz\Desktop\BazaPiosenek";
            string[] files = Directory.GetFiles(folderPath)
                .Where(file => file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            foreach (string file in files)
            {
                DjProgram1.Data.AudioFile audioFile = new DjProgram1.Data.AudioFile();
                audioFile.FileName = Path.GetFileName(file); // Poprawne przypisanie wartości do właściwości FileName
                audioFile.FilePath = file;
                audioFiles.Add(audioFile);

                
            }
            
            foreach(var file in audioFiles)
            {
                listBox.Items.Add(file.FileName);
            }
        }

        private void createWaveform(Canvas canvas)
        {
            {
                // Wczytaj piosenkę z pliku audio
                string filePath = "ścieżka_do_pliku_audio";
                NAudio.Wave.WaveStream waveStream = new NAudio.Wave.WaveFileReader(filePath);

                // Przetwórz dane audio i uzyskaj wygląd waveform
                int sampleRate = waveStream.WaveFormat.SampleRate;
                byte[] audioData = new byte[waveStream.Length];
                waveStream.Read(audioData, 0, (int)waveStream.Length);
                float[] audioSamples = new float[audioData.Length / 2];
                for (int i = 0; i < audioSamples.Length; i++)
                {
                    short sampleValue = BitConverter.ToInt16(audioData, i * 2);
                    audioSamples[i] = sampleValue / 32768f;
                }

                // Wygeneruj wizualizację waveform
                double canvasWidth = canvas.ActualWidth;
                double canvasHeight = canvas.ActualHeight;
                canvas.Children.Clear();
                for (int i = 0; i < audioSamples.Length; i++)
                {
                    double x = canvasWidth * i / audioSamples.Length;
                    double y = canvasHeight * (1 - (audioSamples[i] + 1) / 2);
                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = canvasHeight / 2,
                        X2 = x,
                        Y2 = y,
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(line);
                }

                // Zamknij strumień audio
                waveStream.Close();
            }
        }
        private void playButton1_Click(object sender, RoutedEventArgs e)
        {
            var waveOut = new NAudio.Wave.WaveOut();
            var audioFileReader = new NAudio.Wave.AudioFileReader(currentAudioFile1.FilePath);
            waveOut.Init(audioFileReader);
            waveOut.Play();
        }

        private void playButton2_Click(object sender, RoutedEventArgs e)
        {
            var waveOut = new NAudio.Wave.WaveOut();
            var audioFileReader = new NAudio.Wave.AudioFileReader(currentAudioFile2.FilePath);
            waveOut.Init(audioFileReader);
            waveOut.Play();
        }

        private void pauseButton1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pauseButton2_Click(object sender, RoutedEventArgs e)
        {

        }

        //przeorganizowac kod - zastanowić się jak najlepiej ma on byc
        // 
    }
}
