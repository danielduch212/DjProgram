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
using VisioForge.Libs.WindowsMediaLib;

namespace DjProgram1
{
    
    public partial class MainWindow : Window
    {
        DjProgram1.Services.MusicService musicService = new DjProgram1.Services.MusicService(); 
        List<DjProgram1.Data.AudioFile> audioFiles = new List<DjProgram1.Data.AudioFile>();
        DjProgram1.Data.AudioFile currentAudioFile1 = new DjProgram1.Data.AudioFile();
        DjProgram1.Data.AudioFile currentAudioFile2 = new DjProgram1.Data.AudioFile();
        
        private NAudio.Wave.AudioFileReader audioFileReader1;
        private NAudio.Wave.AudioFileReader audioFileReader2;
        private NAudio.Wave.WaveOut waveOut1;
        private NAudio.Wave.WaveOut waveOut2;
        private int lastDeck = 2;

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
            int selectedIndex = songList.SelectedIndex;
            if (songList.SelectedItem != null)
            {
                
                if (selectedIndex >= 0 && selectedIndex < audioFiles.Count)
                {
                    if(lastDeck == 2)
                    {
                        currentAudioFile1 = audioFiles[selectedIndex];
                        songOnDeck1.Text = currentAudioFile1.FileName;
                        bpmTextBox1.Text = "BPM: " + musicService.GetBpm(currentAudioFile1.FilePath).ToString();
                        lastDeck = 1;
                    }
                    else
                    {
                        currentAudioFile2 = audioFiles[selectedIndex];
                        songOnDeck2.Text = currentAudioFile2.FileName;
                        bpmTextBox2.Text = "BPM: " + musicService.GetBpm(currentAudioFile2.FilePath).ToString();
                        lastDeck = 2;
                    }
                    
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

        
        private void playButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 == null)
            {
                //musicService.createWaveform(canvas2, currentAudioFile2.FilePath);

                audioFileReader1 = new NAudio.Wave.AudioFileReader(currentAudioFile1.FilePath);
                waveOut1 = new NAudio.Wave.WaveOut();
                waveOut1.Init(audioFileReader1);
                waveOut1.Play();
            }
            else if (waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                waveOut1.Play();
            }
        }

        private void playButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 == null)
            {
                //musicService.createWaveform(canvas2, currentAudioFile2.FilePath);

                audioFileReader2 = new NAudio.Wave.AudioFileReader(currentAudioFile2.FilePath);
                waveOut2 = new NAudio.Wave.WaveOut();
                waveOut2.Init(audioFileReader2);
                waveOut2.Play();
            }
            else if (waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                waveOut2.Play();
            }
        }

        private void pauseButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut1.Pause();
            }
        }

        private void pauseButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut2.Stop();
                waveOut2.Dispose();
                
            }
        }

        

        //przeorganizowac kod - zastanowić się jak najlepiej ma on byc
        // 
    }
}
