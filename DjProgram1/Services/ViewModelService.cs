using DjProgram1.Controls;
using DjProgram1.Data;
using NAudio.Gui;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using VisioForge.Libs.ZXing;

namespace DjProgram1.Services
{
    internal class ViewModelService
    {

        DjProgram1.Services.MusicService musicService = new DjProgram1.Services.MusicService();
        List<DjProgram1.Data.AudioFile> audioFiles;


        DjProgram1.Data.AudioFile currentAudioFile = new DjProgram1.Data.AudioFile();

        AudioPlayerService audioPlayer = new AudioPlayerService();
        FIleService fileService;



        private NAudio.Wave.AudioFileReader audioFileReader;

        private NAudio.Wave.WaveOut waveOut;

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


        //sprawdzenie zakonczenia procesu

        //public bool uploadTrackFinished;
        //public bool uploadTrackFinished2;

        //animacja CD
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

        //Python
        //private bool pythonEngineisWorking = false;

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
        private TextBox bpmTextBox;
        private TextBox songOnDeck;
        private TextBlock actualTime;
        private TextBlock durationTime;
        RotateTransform rotateTransformLoading;
        Image imageLoading;
        TextBlock readyText;
        Slider volumeSlider;

        RotateTransform rotateTransformCD;

        public ViewModelService(List<DjProgram1.Data.AudioFile> audioFiles,FIleService fIleService,ListBox songList, Canvas canvas, KnobToCut knobToCut, Knob knob, TextBox bpmTextBox, TextBox songOnDeck, TextBlock actualTime, TextBlock durationTime, RotateTransform rotateTransformLoading, RotateTransform rotateTransformCD,Image imageLoading, TextBlock readyText, Slider volumeSlider, Synchronizer synchronizer)
        {
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
            this.audioFiles = audioFiles;
            this.rotateTransformCD = rotateTransformCD; 
            this.volumeSlider = volumeSlider;


            this.synchronizer = synchronizer;
         

            this.fileService = fIleService;
            this.threadsService = new ThreadsService(musicService, fileService);
            
        }
        public async void UploadTrack(int selectedIndex)
        {
            selectedIndex = songList.SelectedIndex;
            if (songList.SelectedItem != null)
            {
                if (selectedIndex >= 0 && selectedIndex < audioFiles.Count)
                {

                    if (synchronizer.uploadTrackFinished==false)
                    {
                        return;
                    }
                    synchronizer.uploadTrackFinished = false;

                    if (currentAudioFile == audioFiles[selectedIndex])
                    {
                        return;
                    }
                    if (audioLoadingTask != null && !audioLoadingTask.IsCompleted)
                    {
                        ctsAudioLoadingTask.Cancel();
                        waveOut.Stop();
                        await audioLoadingTask;
                    }
                    if (waveOutPlaying == true)
                    {
                        waveOut.Stop();
                        waveOutPlaying = false;
                    }
                    isWaveformGenerated = false;
                    if (waveOut != null)
                        waveOut.Stop();

                    if (ctsMoveLineWaveForm != null)
                        ctsMoveLineWaveForm.Cancel();

                    imageLoading.Visibility = Visibility.Visible;
                    musicService.animatePhoto(rotateTransformLoading);

                    synchronizer.uploadTrackFinished = false;
                    readyText.Text = "NOT READY";
                    readyText.Foreground = Brushes.Red;
                    ready = false;

                    positionOfLine = 0;
                    currentAudioFile = audioFiles[selectedIndex];
                    songOnDeck.Text = currentAudioFile.FileName;


                    bpmTextBox.IsReadOnly = false;

                    bpmTextBox.Text = "BPM: ";

                    ctsAudioLoadingTask = new CancellationTokenSource();
                    audioLoadingTask = threadsService.LoadAudioAsync(currentAudioFile.FilePath, ctsAudioLoadingTask.Token);
                    await audioLoadingTask;

                    audioFileReader = new NAudio.Wave.AudioFileReader(currentAudioFile.FilePath);


                    ctsCountBPM = new CancellationTokenSource();
                    synchronizer.pythonEngineIsRunning = true;

                    if (currentAudioFile.BPM == 0 || currentAudioFile.BPM == null)
                    {

                        countBPM = threadsService.CountBPM(currentAudioFile.FilePath, bpmTextBox, ctsCountBPM.Token);
                        await countBPM;
                        string textBPM = bpmTextBox.Text;
                        textBPM = textBPM.Replace("BPM: ", "").Trim();
                        double.TryParse(textBPM, out double countedBPM);
                        currentAudioFile.BPM = countedBPM;
                        refreshListBoxTask = threadsService.refreshList(songList, audioFiles);

                    }
                    else
                    {
                        bpmTextBox.Text = "BPM: " + currentAudioFile.BPM.ToString();
                    }
                    synchronizer.pythonEngineIsRunning = false;

                    knob.UnlockKnobRotation();

                    

                    waveFormData = await threadsService.generateWaveFormData(currentAudioFile.FilePath);
                    timeStampsData = await threadsService.generateTimeStamps(currentAudioFile.FilePath);

                    generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                    await generateWaveformTask;

                    isWaveformGenerated = true;

                    var audioPlayer = new AudioFileReader(currentAudioFile.FilePath);
                    changedSongFilePath = currentAudioFile.FilePath;


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

                    synchronizer.uploadTrackFinished = true;
                }
            }
        }

        public async void PlayTrack()
        {
            if (waveOut == null && isWaveformGenerated)
            {
                if ((changedSongFilePath != currentAudioFile.FilePath) && ready)
                {
                    knobToCut.LockKnobRotation();

                    ctsMoveLineWaveForm = new CancellationTokenSource();
                    audioFileReader = new NAudio.Wave.AudioFileReader(changedSongFilePath);

                    waveOut = new NAudio.Wave.WaveOut();

                    waveOut.Init(audioFileReader);

                    waveOut.PlaybackStopped += OnPlaybackStopped;

                    waveOut.Volume = 1.0f;

                    waveOutPlaying = true;

                    waveOut.Play();
                    musicService.InitTimer(timer, actualTime, audioFileReader);
                    timer.Start();

                    moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                    musicService.animatePhoto(rotateTransformCD);
                }
                else if ((changedSongFilePath == currentAudioFile.FilePath) && ready)
                {
                    knob.LockKnobRotation();
                    knobToCut.LockKnobRotation();

                    bpmTextBox.IsReadOnly = true;
                    string text = bpmTextBox.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double newBPM = double.Parse(text);
                    if (newBPM != currentAudioFile.BPM)
                    {
                        bpmTextBox.Text = "BPM: " + currentAudioFile.BPM.ToString();
                    }

                    ctsMoveLineWaveForm = new CancellationTokenSource();
                    waveOut = new NAudio.Wave.WaveOut();

                    waveOut.Init(audioFileReader);
                    waveOut.PlaybackStopped += OnPlaybackStopped;

                    waveOut.Volume = 1.0f;

                    waveOutPlaying = true;

                    musicService.InitTimer(timer, actualTime, audioFileReader);
                    timer.Start();

                    waveOut.Play();
                    moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                    musicService.animatePhoto(rotateTransformCD);
                }
            }
            else if (waveOut != null && waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                timer.Start();
                waveOut.Play();
                ctsMoveLineWaveForm = new CancellationTokenSource();
                moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                musicService.animatePhoto(rotateTransformCD);
                waveOutPlaying = true;
            }
            else if (waveOut != null && waveOut.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                waveOut.Dispose();
                audioFileReader.Dispose();
                audioFileReader = new NAudio.Wave.AudioFileReader(currentAudioFile.FilePath);
                waveOut = new NAudio.Wave.WaveOut();
                waveOut.Init(audioFileReader);

                waveOut.PlaybackStopped += OnPlaybackStopped;

                musicService.InitTimer(timer, actualTime, audioFileReader);
                timer.Start();
                waveOut.Play();

                ctsMoveLineWaveForm = new CancellationTokenSource();
                moveLineWaveForm = threadsService.UpdateWaveformAsync(canvas, audioFileReader, waveFormData, timeStampsData, audioFileReader.TotalTime.TotalSeconds, ctsMoveLineWaveForm.Token);

                musicService.animatePhoto(rotateTransformCD);
                waveOutPlaying = true;
            }
        }

        public async void PauseTrack()
        {
            if (waveOut != null && waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing)
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
            if (waveOut != null && (waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing || waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused))
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
            }

        }

        public async void SynchronizeTrack(TextBox oppositeBpmTextBox)
        {
            if (ready && oppositeBpmTextBox.Text != "BPM: " && bpmTextBox.IsReadOnly == false && !synchronizer.pythonEngineIsRunning)
            {
                knobToCut.LockKnobRotation();

                bpmTextBox.Text = oppositeBpmTextBox.Text;
                string text = bpmTextBox.Text;
                text = text.Replace("BPM: ", "").Trim();
                double newBPM = double.Parse(text);
                if (newBPM != currentAudioFile.BPM)
                {
                    ready = false;
                    readyText.Foreground = Brushes.Red;
                    deleteTrack = threadsService.deleteCopiedTrack(currentAudioFile.FileName);
                    await deleteTrack;
                    deleteOldCopies = threadsService.deleteCopies();

                    ctsChangeBPM = new CancellationTokenSource();

                    synchronizer.pythonEngineIsRunning = true;
                    changedSongFilePath = await threadsService.changeBPM(currentAudioFile.FilePath, (double)currentAudioFile.BPM, newBPM, ctsChangeBPM.Token);

                    waveFormData = await threadsService.generateWaveFormData(changedSongFilePath);
                    timeStampsData = await threadsService.generateTimeStamps(changedSongFilePath);

                    generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                    await generateWaveformTask;

                    audioFileReader = new AudioFileReader(changedSongFilePath);
                    knobToCut.addAtributes(audioFileReader, waveFormData, timeStampsData);

                    synchronizer.pythonEngineIsRunning = false;

                    readyText.Foreground = Brushes.Green;
                    knob.LockKnobRotation();
                    bpmTextBox.IsReadOnly = true;
                    ready = true;
                }
                knobToCut.UnlockKnobRotation();
            }

        }

        public async void ChangeBPM()
        {
            if (bpmTextBox.IsReadOnly == true)
            {
                return;
            }
            knobToCut.LockKnobRotation();

            string text = bpmTextBox.Text;
            text = text.Replace("BPM: ", "").Trim();
            double newBPM = double.Parse(text);
            if (newBPM != currentAudioFile.BPM && synchronizer.pythonEngineIsRunning == false)
            {
                ready = false;
                readyText.Foreground = Brushes.Red;
                deleteTrack = threadsService.deleteCopiedTrack(currentAudioFile.FileName);
                await deleteTrack;
                deleteOldCopies = threadsService.deleteCopies();

                ctsChangeBPM = new CancellationTokenSource();

                synchronizer.pythonEngineIsRunning = true;
                changedSongFilePath = await threadsService.changeBPM(currentAudioFile.FilePath, (double)currentAudioFile.BPM, newBPM, ctsChangeBPM.Token);

                waveFormData = await threadsService.generateWaveFormData(changedSongFilePath);
                timeStampsData = await threadsService.generateTimeStamps(changedSongFilePath);

                generateWaveformTask = threadsService.GenerateInitialWaveForm(waveFormData, timeStampsData, canvas);
                await generateWaveformTask;

                audioFileReader = new AudioFileReader(changedSongFilePath);
                knobToCut.addAtributes(audioFileReader, waveFormData, timeStampsData);

                synchronizer.pythonEngineIsRunning = false;

                readyText.Foreground = Brushes.Green;
                knob.LockKnobRotation();
                bpmTextBox.IsReadOnly = true;
                ready = true;
            }
            knobToCut.UnlockKnobRotation();

        }


        public void VolumeSliderChanged()
        {
            if (waveOut != null)
            {
                waveOut.Volume = (float)volumeSlider.Value / 100;
            }
        }

        private void OnPlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs args)
        {
            ctsMoveLineWaveForm.Cancel();
        }

    }
}
