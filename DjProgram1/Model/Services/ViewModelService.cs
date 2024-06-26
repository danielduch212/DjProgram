﻿using DjProgram1.Controls;
using DjProgram1.Model.Data;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VisioForge.Libs.ZXing;

namespace DjProgram1.Model.Services
{
    internal class ViewModelService
    {


        MusicService musicService;
        AudioFile currentAudioFile = new AudioFile();

        FileService fileService;



        private AudioFileReader audioFileReader;

        private WaveOut waveOut;

        bool waveOutPlaying = false;



        //watki
        ThreadsService threadsService;

        private Task audioLoadingTask;
        private CancellationTokenSource ctsAudioLoadingTask;

        private Task generateWaveformTask;

        private bool isWaveformGenerated = false;

        private Task moveLineWaveForm;

        private CancellationTokenSource ctsMoveLineWaveForm;


        double positionOfLine = 0;

        //gotowosc
        private bool ready = false;

        //bpm
        private Task countBPM;

        private CancellationTokenSource ctsCountBPM;

        private Task createDataBase;
        private CancellationTokenSource ctsCreateDataBase;

        int selectedIndex;

        private Task animateCD1;

        private CancellationTokenSource ctsAnimateCD;

        //pokretla
        private double lastAngleKnob1;

        private Point lastMousePosition;

        //change BPM
        private CancellationTokenSource ctsChangeBPM;

        private Task changeBPM;

        public string changedSongFilePath;

        //kopie plikow piosenek
        private Task deleteTrack;
        private Task deleteOldCopies;

        //timery
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        //listy do przechowywania danych waveform
        private List<double> waveFormData = new List<double>();
        private List<double> timeStampsData = new List<double>();

        private Task generateWaveFormData;
        private Task generateTimeStamps;


        private Task refreshListBoxTask;

        Model model;

        Synchronizer synchronizer;

        //controls
        private ListBox songList;
        private Canvas canvas;
        private KnobToCut knobToCut;
        private Knob knob;
        private TextBlock bpmTextBox;
        private TextBlock songOnDeck;
        private TextBlock actualTime;
        private TextBlock durationTime;
        RotateTransform rotateTransformLoading;
        Image imageLoading;
        TextBlock readyText;
        Slider volumeSlider;

        RotateTransform rotateTransformCD;

        public ViewModelService(DjProgram1.Model.Model model, FileService fileService, MusicService musicService,ListBox songList, Canvas canvas, KnobToCut knobToCut, Knob knob, TextBlock bpmTextBox, TextBlock songOnDeck, TextBlock actualTime, TextBlock durationTime, RotateTransform rotateTransformLoading, RotateTransform rotateTransformCD, Image imageLoading, TextBlock readyText, Slider volumeSlider, Synchronizer synchronizer)
        {
            this.musicService = musicService;

            this.songList = songList;
            this.canvas = canvas;
            this.knobToCut = knobToCut;
            this.knob = knob;
            this.bpmTextBox = bpmTextBox;
            this.songOnDeck = songOnDeck;
            this.actualTime = actualTime;
            this.durationTime = durationTime;
            this.rotateTransformLoading = rotateTransformLoading;
            this.imageLoading = imageLoading;
            this.readyText = readyText;

            this.rotateTransformCD = rotateTransformCD;
            this.volumeSlider = volumeSlider;

            this.model = model;

            this.synchronizer = synchronizer;

            this.fileService = fileService;
            threadsService = new ThreadsService(musicService, fileService);

        }
        public async void UploadTrack(int selectedIndex)
        {
            selectedIndex = songList.SelectedIndex;
            if (songList.SelectedItem != null)
            {
                if (selectedIndex >= 0 && selectedIndex < model.GetAudioFilesCount())
                {

                    if (synchronizer.uploadTrackFinished == false || synchronizer.pythonEngineIsRunning == true)
                    {
                        return;
                    }

                    if (currentAudioFile == model.GetAudioFile(selectedIndex))
                    {
                        return;
                    }
                    if (waveOut != null)
                    {

                        waveOut.Dispose();
                        waveOut = null;
                        audioFileReader.Dispose();
                        timer.Stop();
                        musicService.StopRotation(rotateTransformCD);

                    }
                    if (audioLoadingTask != null && !audioLoadingTask.IsCompleted)
                    {
                        ctsAudioLoadingTask.Cancel();
                        waveOut.Stop();
                        await audioLoadingTask;
                    }
                    if (waveOutPlaying == true)
                    {

                    }
                    isWaveformGenerated = false;


                    if (ctsMoveLineWaveForm != null)
                        ctsMoveLineWaveForm.Cancel();

                    synchronizer.uploadTrackFinished = false;

                    synchronizer.pythonEngineIsRunning = true;

                    imageLoading.Visibility = Visibility.Visible;
                    musicService.AnimatePhoto(rotateTransformLoading);

                    readyText.Text = "NOT READY";
                    readyText.Foreground = Brushes.Red;
                    ready = false;

                    positionOfLine = 0;
                    currentAudioFile = model.GetAudioFile(selectedIndex);
                    songOnDeck.Text = currentAudioFile.FileName;



                    bpmTextBox.Text = "BPM: ";

                    ctsAudioLoadingTask = new CancellationTokenSource();
                    audioLoadingTask = threadsService.LoadAudioAsync(currentAudioFile.FilePath, ctsAudioLoadingTask.Token);
                    await audioLoadingTask;

                    try
                    {
                        audioFileReader = new AudioFileReader(currentAudioFile.FilePath);

                    }
                    catch (Exception e)
                    {
                        MessageBoxResult result1 = MessageBox.Show(
                            "Nieprawidlowy plik",
                            "Potwierdzenie",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        musicService.StopRotation(rotateTransformLoading);

                        return;
                    }
                    if(audioFileReader.TotalTime.TotalSeconds == 0 ||audioFileReader.TotalTime.TotalSeconds<1|| audioFileReader.TotalTime.TotalSeconds == null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxResult result1 = MessageBox.Show(
                            "Nieprawidlowy plik",
                            "Potwierdzenie",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                            musicService.StopRotation(rotateTransformLoading);

                            return;
                        });
                        
                    }

                    ctsCountBPM = new CancellationTokenSource();

                    if (currentAudioFile.BPM == 0 || currentAudioFile.BPM == null)
                    {

                        countBPM = threadsService.CountBPM(currentAudioFile.FilePath, bpmTextBox, ctsCountBPM.Token);
                        await countBPM;
                        string textBPM = bpmTextBox.Text;
                        textBPM = textBPM.Replace("BPM: ", "").Trim();
                        double.TryParse(textBPM, out double countedBPM);
                        currentAudioFile.BPM = countedBPM;
                        refreshListBoxTask = threadsService.RefreshList(songList, model.GetAudioFiles());

                    }
                    else
                    {
                        bpmTextBox.Text = "BPM: " + currentAudioFile.BPM.ToString();
                    }




                    waveFormData = await threadsService.GenerateWaveFormData(currentAudioFile.FilePath);
                    timeStampsData = await threadsService.GenerateTimeStamps(currentAudioFile.FilePath);

                    generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                    await generateWaveformTask;

                    isWaveformGenerated = true;

                    var audioPlayer = new AudioFileReader(currentAudioFile.FilePath);
                    changedSongFilePath = currentAudioFile.FilePath;

                    synchronizer.pythonEngineIsRunning = false;

                    double audioDuration = audioPlayer.TotalTime.TotalSeconds;



                    TimeSpan audioDurationTimeSpan = TimeSpan.FromSeconds(audioDuration);
                    durationTime.Text = audioDurationTimeSpan.ToString(@"mm\:ss");
                    actualTime.Text = "00:00";

                    readyText.Text = "READY";
                    readyText.Foreground = Brushes.Green;
                    ready = true;
                    knobToCut.addAtributes(audioFileReader, waveFormData, timeStampsData);

                    string text = bpmTextBox.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double.TryParse(text, out double bpmValue);
                    currentAudioFile.BPM = bpmValue;


                    musicService.StopRotation(rotateTransformLoading);
                    imageLoading.Visibility = Visibility.Hidden;
                    knobToCut.UnlockKnobRotation();
                    knob.UnlockKnobRotation();


                    synchronizer.uploadTrackFinished = true;
                }
            }
        }

        public async void PlayTrack()
        {
            if (waveOut == null && isWaveformGenerated)
            {
                if (changedSongFilePath != currentAudioFile.FilePath && ready)
                {
                    knob.LockKnobRotation();

                    ctsMoveLineWaveForm = new CancellationTokenSource();

                    waveOut = new WaveOut();

                    waveOut.Init(audioFileReader);

                    waveOut.PlaybackStopped += OnPlaybackStopped;

                    waveOut.Volume = 1.0f;

                    waveOutPlaying = true;

                    waveOut.Play();
                    musicService.InitTimer(timer, actualTime, audioFileReader);
                    timer.Start();

                    moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                    musicService.AnimatePhoto(rotateTransformCD);
                }
                else if (changedSongFilePath == currentAudioFile.FilePath && ready)
                {
                    knob.LockKnobRotation();

                    string text = bpmTextBox.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double newBPM = double.Parse(text);
                    if (newBPM != currentAudioFile.BPM)
                    {
                        bpmTextBox.Text = "BPM: " + currentAudioFile.BPM.ToString();
                    }

                    ctsMoveLineWaveForm = new CancellationTokenSource();
                    waveOut = new WaveOut();

                    waveOut.Init(audioFileReader);
                    waveOut.PlaybackStopped += OnPlaybackStopped;

                    waveOut.Volume = 1.0f;

                    waveOutPlaying = true;

                    musicService.InitTimer(timer, actualTime, audioFileReader);
                    timer.Start();

                    waveOut.Play();
                    moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                    musicService.AnimatePhoto(rotateTransformCD);
                }
            }
            else if (waveOut != null && waveOut.PlaybackState == PlaybackState.Paused)
            {
                timer.Start();
                waveOut.Play();
                ctsMoveLineWaveForm = new CancellationTokenSource();
                moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                musicService.AnimatePhoto(rotateTransformCD);
                waveOutPlaying = true;
            }
            else if (waveOut != null && waveOut.PlaybackState == PlaybackState.Stopped)
            {
                waveOut.PlaybackStopped += OnPlaybackStopped;

                musicService.InitTimer(timer, actualTime, audioFileReader);
                timer.Start();
                waveOut.Play();

                ctsMoveLineWaveForm = new CancellationTokenSource();
                moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                musicService.AnimatePhoto(rotateTransformCD);
                waveOutPlaying = true;
            }
        }

        public async void PauseTrack()
        {
            musicService.StopRotation(rotateTransformCD);

            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                if (ctsMoveLineWaveForm != null)
                    ctsMoveLineWaveForm.Cancel();
                timer.Stop();
                waveOut.Pause();
                musicService.StopRotation(rotateTransformCD);
                waveOutPlaying = false;
            }
        }

        public async void StopTrack()
        {
            musicService.StopRotation(rotateTransformCD);

            if (waveOut != null && (waveOut.PlaybackState == PlaybackState.Playing || waveOut.PlaybackState == PlaybackState.Paused))
            {
                actualTime.Text = "00:00";

                timer.Stop();
                waveOut.Pause();
                waveOut.Stop();
                if (ctsMoveLineWaveForm != null)
                    ctsMoveLineWaveForm.Cancel();

                musicService.StopRotation(rotateTransformCD);

                positionOfLine = 0;
                waveOutPlaying = false;

                generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                await generateWaveformTask;

                isWaveformGenerated = true;

                Line positionLine = canvas.Children.OfType<Line>().FirstOrDefault();
                if (positionLine != null)
                {
                    positionLine.X1 = 0;
                    positionLine.X2 = 0;
                }
                actualTime.Text = "00:00";


                waveOut.Dispose();
                audioFileReader.Dispose();
                if (changedSongFilePath != currentAudioFile.FilePath)
                {
                    audioFileReader = new AudioFileReader(changedSongFilePath);

                }
                else
                {
                    audioFileReader = new AudioFileReader(currentAudioFile.FilePath);
                }

                waveOut = new WaveOut();
                waveOut.Init(audioFileReader);
                knobToCut.addAtributes(audioFileReader, waveFormData, timeStampsData);
                knobToCut.UnlockKnobRotation();
            }

        }

        public async void SynchronizeTrack(TextBlock oppositeBpmTextBox)
        {
            if (ready && oppositeBpmTextBox.Text != "BPM: " &&  !synchronizer.pythonEngineIsRunning)
            {
                knob.LockKnobRotation();
                knobToCut.LockKnobRotation();

                bpmTextBox.Text = oppositeBpmTextBox.Text;
                string text = bpmTextBox.Text;
                text = text.Replace("BPM: ", "").Trim();
                double newBPM = double.Parse(text);
                if (newBPM != currentAudioFile.BPM)
                {
                    ready = false;
                    readyText.Text = "NOT READY";
                    readyText.Foreground = Brushes.Red;
                    deleteTrack = threadsService.DeleteCopiedTrack(currentAudioFile.FileName);
                    await deleteTrack;
                    deleteOldCopies = threadsService.DeleteCopies();

                    ctsChangeBPM = new CancellationTokenSource();

                    synchronizer.pythonEngineIsRunning = true;
                    changedSongFilePath = "";
                    changedSongFilePath = await threadsService.ChangeBPM(currentAudioFile.FilePath, (double)currentAudioFile.BPM, newBPM, ctsChangeBPM.Token);

                    waveFormData = await threadsService.GenerateWaveFormData(changedSongFilePath);
                    timeStampsData = await threadsService.GenerateTimeStamps(changedSongFilePath);

                    generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                    await generateWaveformTask;

                    audioFileReader = new AudioFileReader(changedSongFilePath);
                    knobToCut.addAtributes(audioFileReader, waveFormData, timeStampsData);

                    synchronizer.pythonEngineIsRunning = false;

                    readyText.Foreground = Brushes.Green;
                    readyText.Text = "READY";

                    knobToCut.SetProgressIndicatorToStart();

                    double audioDuration = audioFileReader.TotalTime.TotalSeconds;
                    TimeSpan audioDurationTimeSpan = TimeSpan.FromSeconds(audioDuration);
                    durationTime.Text = audioDurationTimeSpan.ToString(@"mm\:ss");

                    ready = true;
                }
                knobToCut.UnlockKnobRotation();
            }

        }

        public async void ChangeBPM()
        {
            if (ready == false)
            {
                return;
            }
            
            ready = false;
            knobToCut.LockKnobRotation();
            knob.LockKnobRotation();

            string text = bpmTextBox.Text;
            text = text.Replace("BPM: ", "").Trim();
            double newBPM = double.Parse(text);
            if (newBPM != currentAudioFile.BPM && synchronizer.pythonEngineIsRunning == false)
            {
                readyText.Text = "NOT READY";
                readyText.Foreground = Brushes.Red;
                deleteTrack = threadsService.DeleteCopiedTrack(currentAudioFile.FileName);
                await deleteTrack;
                deleteOldCopies = threadsService.DeleteCopies();

                ctsChangeBPM = new CancellationTokenSource();

                synchronizer.pythonEngineIsRunning = true;
                changedSongFilePath = "";

                changedSongFilePath = await threadsService.ChangeBPM(currentAudioFile.FilePath, (double)currentAudioFile.BPM, newBPM, ctsChangeBPM.Token);

                waveFormData = await threadsService.GenerateWaveFormData(changedSongFilePath);
                timeStampsData = await threadsService.GenerateTimeStamps(changedSongFilePath);

                generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                await generateWaveformTask;

                audioFileReader = new AudioFileReader(changedSongFilePath);
                knobToCut.addAtributes(audioFileReader, waveFormData, timeStampsData);

                synchronizer.pythonEngineIsRunning = false;

                readyText.Foreground = Brushes.Green;
                readyText.Text = "READY";

                knobToCut.SetProgressIndicatorToStart();
                actualTime.Text = "00:00";

                double audioDuration = audioFileReader.TotalTime.TotalSeconds;
                TimeSpan audioDurationTimeSpan = TimeSpan.FromSeconds(audioDuration);
                durationTime.Text = audioDurationTimeSpan.ToString(@"mm\:ss");
                knobToCut.UnlockKnobRotation();
                ready = true;


            }
            else
            {
                knob.UnlockKnobRotation();
            }

        }


        public void VolumeSliderChanged()
        {
            if (waveOut != null)
            {
                waveOut.Volume = (float)volumeSlider.Value / 100;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            ctsMoveLineWaveForm.Cancel();
            musicService.StopRotation(rotateTransformCD);


        }



    }
}
