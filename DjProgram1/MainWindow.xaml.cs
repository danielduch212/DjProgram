﻿using System;
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
using DjProgram1.Services;
using System.Threading;
using System.Threading.Tasks;

namespace DjProgram1
{
    
    public partial class MainWindow : Window
    {
        DjProgram1.Services.MusicService musicService = new DjProgram1.Services.MusicService(); 
        List<DjProgram1.Data.AudioFile> audioFiles = new List<DjProgram1.Data.AudioFile>();
        DjProgram1.Data.AudioFile currentAudioFile1 = new DjProgram1.Data.AudioFile();
        DjProgram1.Data.AudioFile currentAudioFile2 = new DjProgram1.Data.AudioFile();
        DjProgram1.Services.RealtimeWaveformUpdater realtimeWaveformUpdater1;
        DjProgram1.Services.RealtimeWaveformUpdater realtimeWaveformUpdater2;
        AudioPlayerService audioPlayer = new AudioPlayerService();
        FIleService FIleService = new FIleService();
        private double lastAngle = 0;


        private NAudio.Wave.AudioFileReader audioFileReader1;
        private NAudio.Wave.AudioFileReader audioFileReader2;
        private NAudio.Wave.WaveOut waveOut1;
        private NAudio.Wave.WaveOut waveOut2;
        bool waveOut1Playing = false;
        bool waveOut2Playing = false;
        private int lastDeck = 2;

        //watki
        ThreadsService threadsService;
        private Task audioLoadingTask1;
        private CancellationTokenSource ctsAudioLoadingTask1;
        private Task audioLoadingTask2;
        private CancellationTokenSource ctsAudioLoadingTask2;

        //private Task audioPlayingTask1;
        //private CancellationTokenSource ctsAudioPlayingTask1;

        //private Task audioPlayingTask2;
        //private CancellationTokenSource ctsAudioPlayingTask2;




        int selectedIndex;
        //DjProgram1.Services.BpmCalculator bpmCalculator = new DjProgram1.Services.BpmCalculator();
        public MainWindow()
        {

            InitializeComponent();
            threadsService = new ThreadsService(musicService);
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

        private async void buttonUpload2_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = songList.SelectedIndex;
            if (songList.SelectedItem != null)
            {
                if (selectedIndex >= 0 && selectedIndex < audioFiles.Count)
                {

                    if (currentAudioFile2 == audioFiles[selectedIndex])
                    {
                        return;
                    }
                    if (audioLoadingTask2 != null && !audioLoadingTask2.IsCompleted)
                    {
                        ctsAudioLoadingTask2.Cancel(); 
                        waveOut2.Stop();
                        await audioLoadingTask2; 
                    }
                    if (waveOut2Playing == true)
                    {
                        waveOut2.Stop();
                        waveOut2Playing = false;
                    }
                    if(waveOut2 != null) 
                        waveOut2.Stop();
                    currentAudioFile2 = audioFiles[selectedIndex];
                    songOnDeck2.Text = currentAudioFile2.FileName;
                    //double[] audioSamples = musicService.LoadAudioSamples(currentAudioFile2.FilePath);
                    //musicService.GenerateWaveform(audioSamples, canvas2);
                    bpmTextBox2.Text = "BPM: ";
                    //lastDeck = 2;
                    ctsAudioLoadingTask2 = new CancellationTokenSource();
                    audioLoadingTask2 = threadsService.LoadAudioAsync(currentAudioFile2.FilePath, ctsAudioLoadingTask2.Token);

                    await audioLoadingTask2;
                }
            }


        }
        private async void buttonUpload1_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = songList.SelectedIndex;
            if (songList.SelectedItem != null)
            {
                
                if (selectedIndex >= 0 && selectedIndex < audioFiles.Count)
                {
                    
                    
                    if(currentAudioFile1 == audioFiles[selectedIndex])
                    {
                        return;
                    }
                    if (audioLoadingTask1 != null && !audioLoadingTask1.IsCompleted)
                    {
                        ctsAudioLoadingTask1.Cancel();
                        waveOut1.Stop();
                        await audioLoadingTask1; // Poczekaj na zakończenie
                    }
                    if (waveOut1Playing == true)
                    {
                        
                        waveOut1.Stop();
                        waveOut1Playing = false;
                    }
                    
                    if(waveOut1 != null)
                        waveOut1.Stop();
                    currentAudioFile1 = audioFiles[selectedIndex];
                    songOnDeck1.Text = currentAudioFile1.FileName;
                    //double[] audioSamples = musicService.LoadAudioSamples(currentAudioFile1.FilePath);
                    //musicService.GenerateWaveform(audioSamples, canvas1);
                    bpmTextBox1.Text = "BPM: ";
                    //lastDeck = 1;
                    ctsAudioLoadingTask1 = new CancellationTokenSource();
                    audioLoadingTask1 = threadsService.LoadAudioAsync(currentAudioFile1.FilePath, ctsAudioLoadingTask1.Token);

                    await audioLoadingTask1;


                }
            }
        }
        private void buttonUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            ListBox listBox = songList; 

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
                audioFileReader1 = new AudioFileReader(audioFile.FilePath);
               
            }
            
            foreach (var file in audioFiles)
            {
                listBox.Items.Add(file.FileName);
            }
            FIleService.createFile();
        }
        private void prepare_track_click(object sender, RoutedEventArgs e)
        {

        }

        private void reset_Data_Base_click(object sender, RoutedEventArgs e)
        {

        }
        private async void playButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 == null)
            {

                audioFileReader1 = new NAudio.Wave.AudioFileReader(currentAudioFile1.FilePath);
                waveOut1 = new NAudio.Wave.WaveOut();
                waveOut1.Init(audioFileReader1);

                waveOut1Playing = true;
                waveOut1.Play();
                //musicService.animatePhoto(rotateTransform1, lastAngle);
            }
            else if (waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                waveOut1.Play();
                
                waveOut1Playing = true;
            }
        }

        private async void playButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 == null)
            {
                audioFileReader2 = new NAudio.Wave.AudioFileReader(currentAudioFile2.FilePath);
                waveOut2 = new NAudio.Wave.WaveOut();
                waveOut2.Init(audioFileReader2);
                waveOut2.Play();

                //await audioPlayingTask2;

                waveOut2Playing = true;
            }
            else if (waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {

                waveOut2.Play();
                //await audioPlayingTask2;
                waveOut2Playing = true;
            }
        }

        private void pauseButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {

                waveOut1.Pause();
                //musicService.StopRotation(rotateTransform1, lastAngle);
                waveOut1Playing = false;
            }
        }

        private void pauseButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {

                waveOut2.Pause();
                //musicService.StopRotation(rotateTransform2, lastAngle);
                waveOut2Playing = false;
            }
        }

        private void stopButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                
                waveOut1.Stop();
                
                waveOut1Playing = false;

            }
        }
        private void stopButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                
                waveOut2.Stop();
                
                waveOut2Playing = false;

            }
        }

        private void synchroButton1_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void synchroButton2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        

        // pobierac bpm z metadanych - lub ustawiac recznie
        // dodac dwa pokretla - jedno do ustawiania bpm drugie do przyspieszania - chyba to nie ma sensu? xd
        // poprawić wyglad
        // 03.08 zrobic zeby mozna bylo odpalac mp3 i wave bez przeszkod
        // Konsultacja: 04.08 10:00
        // zrobic taka mala baze danych w pliku i pobierac te nuty z metadanych co sa i liczyc na watku dla innych
        // zrobic na watkach 

        // PO PRZERWIE:
        // nowy pomysl: dodanie przycisku zeby dj mogl se przygotowac kilka plikow podczas odtwarzania czegos : 
        // zaznacza kilka plikow i daje policz przygotuj - wtedy watek sie odpala i liczy te bpm 
        // te metadane tez mozna zrobic wiadomo
    }
}
