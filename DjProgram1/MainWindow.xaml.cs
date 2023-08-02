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
using NAudio.Gui;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using VisioForge.Libs.NAudio.VisioForge;

namespace DjProgram1
{
    
    public partial class MainWindow : Window
    {
        DjProgram1.Services.MusicService musicService = new DjProgram1.Services.MusicService(); 
        DjProgram1.Services.TempoAnalyzer tempoAnalyzer = new DjProgram1.Services.TempoAnalyzer();
        List<DjProgram1.Data.AudioFile> audioFiles = new List<DjProgram1.Data.AudioFile>();
        DjProgram1.Data.AudioFile currentAudioFile1 = new DjProgram1.Data.AudioFile();
        DjProgram1.Data.AudioFile currentAudioFile2 = new DjProgram1.Data.AudioFile();
        DjProgram1.Services.RealtimeWaveformUpdater realtimeWaveformUpdater1;
        DjProgram1.Services.RealtimeWaveformUpdater realtimeWaveformUpdater2;
        AudioPlayer audioPlayer = new AudioPlayer();
        private double lastAngle = 0;


        private NAudio.Wave.AudioFileReader audioFileReader1;
        private NAudio.Wave.AudioFileReader audioFileReader2;
        private NAudio.Wave.WaveOut waveOut1;
        private NAudio.Wave.WaveOut waveOut2;
        private int lastDeck = 2;
        //DjProgram1.Services.BpmCalculator bpmCalculator = new DjProgram1.Services.BpmCalculator();
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
                        double[] audioSamples = musicService.LoadAudioSamples(currentAudioFile1.FilePath);
                        musicService.GenerateWaveform(audioSamples, canvas1);
                        bpmTextBox1.Text = "BPM: " ;
                        lastDeck = 1;
                    }
                    else
                    {
                        currentAudioFile2 = audioFiles[selectedIndex];
                        songOnDeck2.Text = currentAudioFile2.FileName;
                        bpmTextBox2.Text = "BPM: " ;
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

                if (audioFile.FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    // Dodaj plik WAV do pojemnika odtwarzacza
                    NAudio.Wave.WaveStream waveStream = new NAudio.Wave.WaveFileReader(audioFile.FilePath);
                    audioPlayer.AddWaveStream(waveStream);
                }
                else if (audioFile.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    // Dodaj plik MP3 do pojemnika odtwarzacza
                    Mp3FileReader mp3File = new Mp3FileReader(audioFile.FilePath);
                    audioPlayer.AddMp3File(mp3File);
                }
            }

            foreach (var file in audioFiles)
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
                animatePhoto(rotateTransform1);
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
                StopRotation(rotateTransform1);
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

        private void stopButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut1.Stop();
                waveOut1.Play();
                waveOut1.Stop();

            }
        }
        private void stopButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut2.Stop();
                waveOut2.Play();
                waveOut2.Stop();

            }
        }

        private void synchroButton1_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void synchroButton2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void animatePhoto(RotateTransform rotateTransform)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = lastAngle;
            animation.To = lastAngle + 360;
            animation.Duration = TimeSpan.FromSeconds(5);
            animation.RepeatBehavior = RepeatBehavior.Forever;

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void StopRotation(RotateTransform rotateTransform)
        {
            lastAngle = rotateTransform.Angle;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        // pobierac bpm z metadanych - lub ustawiac recznie
        // zrobic tak zeby mozna bylo wgrac i mp3 i wava
        // dodac dwa pokretla - jedno do ustawiania bpm drugie do przyspieszania - chyba to nie ma sensu? xd
        // poprawić wyglad
    }
}
