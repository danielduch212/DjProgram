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
using DjProgram1.Services;
using System.Threading;
using System.Threading.Tasks;
using Python.Runtime;
using VisioForge.Libs.ZXing;
using DjProgram1.Controls;
using System.Windows.Threading;
using System.Diagnostics;

namespace DjProgram1
{

    public partial class MainWindow : Window
    {
        DjProgram1.Services.MusicService musicService = new DjProgram1.Services.MusicService();
        List<DjProgram1.Data.AudioFile> audioFiles = new List<DjProgram1.Data.AudioFile>();
        DjProgram1.Data.AudioFile currentAudioFile1 = new DjProgram1.Data.AudioFile();
        DjProgram1.Data.AudioFile currentAudioFile2 = new DjProgram1.Data.AudioFile();
        
        AudioPlayerService audioPlayer = new AudioPlayerService();
        FIleService fileService;
        


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

        private Task generateWaveformTask1;
        private Task generateWaveformTask2;

        private bool isWaveformGenerated1 = false;
        private bool isWaveformGenerated2 = false;

        private Task moveLineWaveForm1;
        private Task moveLineWaveForm2;
        private CancellationTokenSource ctsMoveLineWaveForm1;
        private CancellationTokenSource ctsMoveLineWaveForm2;


        //monitorowanie lini na waveform
        double positionOfLine1 = 0;
        double positionOfLine2 = 0;

        //gotowosc
        private bool ready1 = false;
        private bool ready2 = false;

        //bpm
        private Task countBPM1;
        private Task countBPM2;
        private CancellationTokenSource ctsCountBPM1;
        private CancellationTokenSource ctsCountBPM2;

        private Task createDataBase;
        private CancellationTokenSource ctsCreateDataBase;

        int selectedIndex;


        //sprawdzenie zakonczenia procesu

        private bool uploadTrackFinished1 = true;
        private bool uploadTrackFinished2 = true;

        //animacja CD
        private Task animateCD1;
        private Task animateCD2;
        private CancellationTokenSource ctsAnimateCD1;
        private CancellationTokenSource ctsAnimateCD2;

        //pokretla
        private double lastAngleKnob1;
        private double lastAngleKnob2;
        private Point lastMousePosition1;
        private Point lastMousePosition2;

        //change BPM
        private CancellationTokenSource ctsChangeBPM1;
        private CancellationTokenSource ctsChangeBPM2;
        private Task changeBPM1;
        private Task changeBPM2;
        public string changedSongFilePath1;
        public string changedSongFilePath2;

        //kopie plikow piosenek
        private Task deleteTrack;
        private Task deleteOldCopies;

        //Python
        private bool pythonEngineisWorking = false;

        //timery
        System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();

        //listy do przechowywania danych waveform
        private List<double> waveFormData1 = new List<double>();
        private List<double> waveFormData2 = new List<double>();
        private List<double> timeStampsData1 = new List<double>();
        private List<double> timeStampsData2 = new List<double>();

        private Task generateWaveFormData1;
        private Task generateWaveFormData2;
        private Task generateTimeStamps1;
        private Task generateTimeStamps2;

        
        public MainWindow()
        {

            InitializeComponent();
            this.fileService = new FIleService();
            threadsService = new ThreadsService(musicService, fileService);
            volumeSlider1.Value = 100;
            volumeSlider2.Value = 100;
            imageLoading2.Visibility = Visibility.Hidden;
            imageLoading1.Visibility = Visibility.Hidden;

            knob1.bpmTextBox = bpmTextBox1;
            knob2.bpmTextBox = bpmTextBox2;
            knob1.LockKnobRotation();
            knob2.LockKnobRotation();
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

                    if (uploadTrackFinished1 == false || uploadTrackFinished2 == false)
                    {
                        return;
                    }

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
                    isWaveformGenerated2 = false;
                    if (waveOut2 != null)
                        waveOut2.Stop();

                    if (ctsMoveLineWaveForm2 != null)
                        ctsMoveLineWaveForm2.Cancel();

                    imageLoading2.Visibility = Visibility.Visible;
                    musicService.animatePhoto(rotateTransformLoading2);

                    uploadTrackFinished2 = false;
                    readyText2.Foreground = Brushes.Red;
                    ready2 = false;

                    positionOfLine2 = 0;
                    currentAudioFile2 = audioFiles[selectedIndex];
                    songOnDeck2.Text = currentAudioFile2.FileName;

                    
                    bpmTextBox2.IsReadOnly = false;

                    bpmTextBox2.Text = "BPM: ";
                    
                    ctsAudioLoadingTask2 = new CancellationTokenSource();
                    audioLoadingTask2 = threadsService.LoadAudioAsync(currentAudioFile2.FilePath, ctsAudioLoadingTask2.Token);
                    await audioLoadingTask2;
                    


                    ctsCountBPM2 = new CancellationTokenSource();
                    pythonEngineisWorking = true;
                    countBPM2 = threadsService.CountBPM(currentAudioFile2.FilePath, bpmTextBox2, ctsCountBPM2.Token);
                    await countBPM2;
                    pythonEngineisWorking = false;

                    knob2.UnlockKnobRotation();

                    uploadTrackFinished2 = true;

                    //generowanie initial waveform
                    waveFormData2 = await threadsService.generateWaveFormData(currentAudioFile2.FilePath);
                    timeStampsData2 = await threadsService.generateTimeStamps(currentAudioFile2.FilePath);

                    generateWaveformTask2 = threadsService.GenerateInitialWaveForm(waveFormData2,timeStampsData2, canvas2);
                    await generateWaveformTask2;

                    isWaveformGenerated2 = true;

                    var audioPlayer = new AudioFileReader(currentAudioFile2.FilePath);
                    changedSongFilePath2 = currentAudioFile2.FilePath;


                    double audioDuration = audioPlayer.TotalTime.TotalSeconds;

                    TimeSpan audioDurationTimeSpan = TimeSpan.FromSeconds(audioDuration);
                    durationTime2.Text = audioDurationTimeSpan.ToString(@"mm\:ss");
                    actualTime2.Text = "00:00";
                    readyText2.Foreground = Brushes.Green;
                    
                    ready2 = true;


                    //bpmTextBox2.Text = fileService.checkMetaDataBPM(currentAudioFile2.FilePath).ToString();
                    //ctsCountBPM2 = new CancellationTokenSource();
                    //countBPM2 = threadsService.CountBPM(currentAudioFile2.FilePath, bpmTextBox2, ctsCountBPM2.Token); 
                    //await countBPM2;
                    string text = bpmTextBox2.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double.TryParse(text, out double bpmValue);
                    currentAudioFile2.BPM = bpmValue;

                    setRectangleOnWaveForm(currentAudioFile2.FilePath, canvas2);

                    musicService.StopRotation(rotateTransformLoading2);
                    imageLoading2.Visibility = Visibility.Hidden;

                    
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


                    
                    if (uploadTrackFinished1 == false || uploadTrackFinished2 == false)
                    {
                        return;
                    }
                    if (currentAudioFile1 == audioFiles[selectedIndex])
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
                    positionOfLine1 = 0;
                    isWaveformGenerated1 = false;
                    if (ctsMoveLineWaveForm1 != null)
                        ctsMoveLineWaveForm1.Cancel();
                    if (waveOut1 != null)
                        waveOut1.Stop();
                    imageLoading1.Visibility = Visibility.Visible;
                    musicService.animatePhoto(rotateTransformLoading1);
                    readyText1.Foreground = Brushes.Red;
                    ready1 = false;


                    uploadTrackFinished1 = false;

                    currentAudioFile1 = audioFiles[selectedIndex];

                    songOnDeck1.Text = currentAudioFile1.FileName;
                    
                    bpmTextBox1.Text = "BPM: ";

                    bpmTextBox1.IsReadOnly = false;

                    ctsAudioLoadingTask1 = new CancellationTokenSource();
                    audioLoadingTask1 = threadsService.LoadAudioAsync(currentAudioFile1.FilePath, ctsAudioLoadingTask1.Token);
                    await audioLoadingTask1;


                    ctsCountBPM1 = new CancellationTokenSource();
                    
                    pythonEngineisWorking = true;
                    countBPM1 = threadsService.CountBPM(currentAudioFile1.FilePath, bpmTextBox1, ctsCountBPM1.Token);
                    await countBPM1;
                    pythonEngineisWorking = false;

                    knob1.UnlockKnobRotation();



                    uploadTrackFinished1 = true;
                    waveFormData1 = await threadsService.generateWaveFormData(currentAudioFile1.FilePath);
                    timeStampsData1 = await threadsService.generateTimeStamps(currentAudioFile1.FilePath);


                    generateWaveformTask1 = threadsService.GenerateInitialWaveForm(waveFormData1,timeStampsData1, canvas1);
                    await generateWaveformTask1;

                    isWaveformGenerated1 = true;

                    var audioPlayer = new AudioFileReader(currentAudioFile1.FilePath);
                    changedSongFilePath1 = currentAudioFile1.FilePath;

                    double audioDuration = audioPlayer.TotalTime.TotalSeconds;

                    TimeSpan audioDurationTimeSpan = TimeSpan.FromSeconds(audioDuration);
                    durationTime1.Text = audioDurationTimeSpan.ToString(@"mm\:ss");
                    actualTime1.Text = "00:00";
                    readyText1.Foreground = Brushes.Green;

                    ready1 = true;

                    //setRectangleOnWaveForm(currentAudioFile1.FilePath, canvas1);


                    musicService.StopRotation(rotateTransformLoading1);
                    imageLoading1.Visibility = Visibility.Hidden;
                    string text = bpmTextBox1.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double.TryParse(text, out double bpmValue);
                    currentAudioFile1.BPM = bpmValue;

                }
            }
        }
        private async void buttonUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            
            ListBox listBox = songList;

            audioFiles = fileService.uploadSongs(audioFiles, listBox);

            fileService.createFile();

            ctsCreateDataBase = new CancellationTokenSource();  
            createDataBase = threadsService.GenerateDataBase(audioFiles.ToArray(), ctsCreateDataBase.Token);
            await createDataBase;
        }
        private void prepare_track_click(object sender, RoutedEventArgs e)
        {

        }

        private async void reset_Data_Base_click(object sender, RoutedEventArgs e)
        {
            if(ctsCreateDataBase != null)
            {
                ctsCreateDataBase.Cancel();
            }
            if(waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut1.Stop();
                ctsMoveLineWaveForm1.Cancel();
                waveOut1.Dispose();
            }
            if (waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                waveOut1.Stop();
                waveOut1.Dispose();
            }
            if (waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                waveOut1.Dispose();
            }



            if (waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                waveOut2.Stop();
                ctsMoveLineWaveForm2.Cancel();
                waveOut2.Dispose();
            }
            if (waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                waveOut2.Stop();
                waveOut2.Dispose();
            }
            if (waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                waveOut2.Dispose();
            }

            ListBox listBox = songList;

            audioFiles = fileService.uploadSongs(audioFiles, listBox);

            fileService.createFile();

            ctsCreateDataBase = new CancellationTokenSource();
            createDataBase = threadsService.GenerateDataBase(audioFiles.ToArray(), ctsCreateDataBase.Token);
            await createDataBase;
        }

        private async void playButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 == null && isWaveformGenerated1 == true)
            {
                
                
                if ((changedSongFilePath1 != currentAudioFile1.FilePath) && ready1== true)
                {
                    

                    ctsMoveLineWaveForm1 = new CancellationTokenSource();
                    audioFileReader1 = new NAudio.Wave.AudioFileReader(changedSongFilePath1);

                    waveOut1 = new NAudio.Wave.WaveOut();
                    waveOut1.Init(audioFileReader1);
                    waveOut1.Volume = 1.0f;


                    waveOut1Playing = true;

                    musicService.InitTimer(timer1, actualTime1, audioFileReader1,1);
                    timer1.Start();
                    waveOut1.Play();
                    //moveLineWaveForm1 = threadsService.MovePositionLine(canvas1, currentAudioFile1.FilePath, actualTime1, durationTime1, ctsMoveLineWaveForm1.Token, positionOfLine1, timer1);
                    //await moveLineWaveForm1;

                    moveLineWaveForm1 = threadsService.UpdateWaveformAsync(canvas1, audioFileReader1, waveFormData1, timeStampsData1, audioFileReader1.TotalTime.TotalSeconds,1);
                    
                    musicService.animatePhoto(rotateTransformCD1);
                }
                else if ((changedSongFilePath1 == currentAudioFile1.FilePath) && ready1 == true)
                {
                    knob1.LockKnobRotation();
                    bpmTextBox1.IsReadOnly = true;
                    string text = bpmTextBox1.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double newBPM = double.Parse(text);
                    if (newBPM != currentAudioFile1.BPM)
                    {
                        bpmTextBox1.Text = "BPM: " + currentAudioFile1.BPM.ToString();
                    }

                    ctsMoveLineWaveForm1 = new CancellationTokenSource();
                    audioFileReader1 = new NAudio.Wave.AudioFileReader(currentAudioFile1.FilePath);
                    waveOut1 = new NAudio.Wave.WaveOut();
                    waveOut1.Init(audioFileReader1);
                    waveOut1.Volume = 1.0f;

                    waveOut1Playing = true;

                    musicService.InitTimer(timer1, actualTime1, audioFileReader1, 1);
                    timer1.Start();

                    waveOut1.Play();

                    //moveLineWaveForm1 = threadsService.MovePositionLine(canvas1, currentAudioFile1.FilePath, actualTime1, durationTime1, ctsMoveLineWaveForm1.Token, positionOfLine1, timer1);

                    moveLineWaveForm1 = threadsService.UpdateWaveformAsync(canvas1, audioFileReader1, waveFormData1, timeStampsData1, audioFileReader1.TotalTime.TotalSeconds, 1);


                    musicService.animatePhoto(rotateTransformCD1);
                }
                    
                
                

                
                
            }

            else if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                timer1.Start();
                waveOut1.Play();
                ctsMoveLineWaveForm1 = new CancellationTokenSource();
                //moveLineWaveForm1 = threadsService.MovePositionLine(canvas1, currentAudioFile1.FilePath, actualTime1, durationTime1, ctsMoveLineWaveForm1.Token, positionOfLine1, timer1);
                moveLineWaveForm1 = threadsService.UpdateWaveformAsync(canvas1, audioFileReader1, waveFormData1, timeStampsData1, audioFileReader1.TotalTime.TotalSeconds, 1);


                musicService.animatePhoto(rotateTransformCD1);
                waveOut1Playing = true;
            }
            else if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                waveOut1.Dispose(); 
                audioFileReader1.Dispose(); 
                audioFileReader1 = new NAudio.Wave.AudioFileReader(currentAudioFile1.FilePath);
                waveOut1 = new NAudio.Wave.WaveOut();
                waveOut1.Init(audioFileReader1);

                timer1.Start();
                waveOut1.Play();

                ctsMoveLineWaveForm1 = new CancellationTokenSource();
                //moveLineWaveForm1 = threadsService.MovePositionLine(canvas1, currentAudioFile1.FilePath, actualTime1, durationTime1, ctsMoveLineWaveForm1.Token, positionOfLine1, timer1);
                moveLineWaveForm1 = threadsService.UpdateWaveformAsync(canvas1, audioFileReader1, waveFormData1, timeStampsData1, audioFileReader1.TotalTime.TotalSeconds, 1);


                musicService.animatePhoto(rotateTransformCD1);
                waveOut1Playing = true;
            }
        }

        private async void playButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 == null && isWaveformGenerated2 == true)
            {
                
                if((changedSongFilePath2 != currentAudioFile2.FilePath)&&ready2 == true)
                {
                    
                    ctsMoveLineWaveForm2 = new CancellationTokenSource();
                    audioFileReader2 = new NAudio.Wave.AudioFileReader(changedSongFilePath2);
                    waveOut2 = new NAudio.Wave.WaveOut();
                    waveOut2.Init(audioFileReader2);
                    waveOut2.Volume = 1.0f;

                    musicService.InitTimer(timer2, actualTime2, audioFileReader2, 2);
                    timer2.Start();
                    waveOut2.Play();
                    //moveLineWaveForm2 = threadsService.MovePositionLine(canvas2, currentAudioFile2.FilePath, actualTime2, durationTime2, ctsMoveLineWaveForm2.Token, positionOfLine2, timer2);
                    moveLineWaveForm2 = threadsService.UpdateWaveformAsync(canvas2, audioFileReader2, waveFormData2, timeStampsData2, audioFileReader2.TotalTime.TotalSeconds,2);

                    waveOut2Playing = true;

                    musicService.animatePhoto(rotateTransformCD2);
                }
                else if((changedSongFilePath2 == currentAudioFile2.FilePath) && ready2 == true)
                {
                    
                    knob2.LockKnobRotation();
                    bpmTextBox2.IsReadOnly = true;
                    string text = bpmTextBox2.Text;
                    text = text.Replace("BPM: ", "").Trim();
                    double newBPM = double.Parse(text);
                    if(newBPM != currentAudioFile2.BPM)
                    {
                        bpmTextBox2.Text = "BPM: " + currentAudioFile2.BPM.ToString();
                    }

                    ctsMoveLineWaveForm2 = new CancellationTokenSource();
                    audioFileReader2 = new NAudio.Wave.AudioFileReader(currentAudioFile2.FilePath);
                    waveOut2 = new NAudio.Wave.WaveOut();
                    waveOut2.Init(audioFileReader2);
                    waveOut2.Volume = 1.0f;

                    musicService.InitTimer(timer2, actualTime2, audioFileReader2, 2);

                    timer2.Start();
                    waveOut2.Play();
                    //moveLineWaveForm2 = threadsService.MovePositionLine(canvas2, currentAudioFile2.FilePath, actualTime2, durationTime2, ctsMoveLineWaveForm2.Token, positionOfLine2, timer2);
                    moveLineWaveForm2 = threadsService.UpdateWaveformAsync(canvas2, audioFileReader2, waveFormData2, timeStampsData2, audioFileReader2.TotalTime.TotalSeconds,2);


                    waveOut2Playing = true;
                    
                    musicService.animatePhoto(rotateTransformCD2);
                }
                
                
            }
            else if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                timer2.Start();
                waveOut2.Play();
                ctsMoveLineWaveForm2 = new CancellationTokenSource();

                moveLineWaveForm2 = threadsService.UpdateWaveformAsync(canvas2, audioFileReader2, waveFormData2, timeStampsData2, audioFileReader2.TotalTime.TotalSeconds,2);

                //await moveLineWaveForm2;
                musicService.animatePhoto(rotateTransformCD2);

                waveOut2Playing = true;
            }
            else if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            {
                waveOut2.Dispose(); // Zwolnij zasoby
                audioFileReader2.Dispose(); // Zwolnij zasoby
                audioFileReader2 = new NAudio.Wave.AudioFileReader(currentAudioFile2.FilePath);
                waveOut2 = new NAudio.Wave.WaveOut();
                waveOut2.Init(audioFileReader2);
                timer2.Start();
                waveOut2.Play();
                ctsMoveLineWaveForm2 = new CancellationTokenSource();
                moveLineWaveForm2 = threadsService.UpdateWaveformAsync(canvas2, audioFileReader2, waveFormData2, timeStampsData2, audioFileReader2.TotalTime.TotalSeconds,2);

                //await moveLineWaveForm2;

                musicService.animatePhoto(rotateTransformCD2);
                waveOut2Playing = true;
            }
        }

        private void pauseButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                if (ctsMoveLineWaveForm1 != null)
                    ctsMoveLineWaveForm1.Cancel();
                timer1.Stop();
                waveOut1.Pause();
                musicService.StopRotation(rotateTransformCD1);
                waveOut1Playing = false;
            }
        }

        private void pauseButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                if (ctsMoveLineWaveForm2 != null)
                    ctsMoveLineWaveForm2.Cancel();
                
                timer2.Stop();
                waveOut2.Pause();
                musicService.StopRotation(rotateTransformCD2);
                waveOut2Playing = false;
            }
        }

        private void stopButton1_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut1 != null && waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Playing || waveOut1.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {

                actualTime1.Text = "00:00";

                timer1.Stop();
                waveOut1.Pause();
                waveOut1.Stop();
                if (ctsMoveLineWaveForm1 != null)
                    ctsMoveLineWaveForm1.Cancel();

                musicService.StopRotation(rotateTransformCD1);

                positionOfLine1 = 0;
                waveOut1Playing = false;

                // Move the position line back to the start
                Line positionLine = canvas1.Children.OfType<Line>().FirstOrDefault();
                if (positionLine != null)
                {
                    positionLine.X1 = 0;
                    positionLine.X2 = 0;
                }
                // Reset actualTime to 00:00
                actualTime1.Text = "00:00";
            }
        }
        private void stopButton2_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut2 != null && waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Playing || waveOut2.PlaybackState == NAudio.Wave.PlaybackState.Paused)
            {
                actualTime2.Text = "00:00";

                timer2.Stop();
                
                waveOut2.Pause();

               waveOut2.Stop();
               if (ctsMoveLineWaveForm2 != null)
                  ctsMoveLineWaveForm2.Cancel();

               musicService.StopRotation(rotateTransformCD2);

               positionOfLine2 = 0;
               waveOut2Playing = false;
               Line positionLine = canvas2.Children.OfType<Line>().FirstOrDefault();
               if (positionLine != null)
               {
                   positionLine.X1 = 0;
                   positionLine.X2 = 0;
               }
                // Reset actualTime to 00:00
                actualTime2.Text = "00:00";
            }
        }

        private void volumeSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (waveOut1 != null)
            {
                // Ustaw głośność na procent wartości suwaka
                waveOut1.Volume = (float)volumeSlider1.Value / 100;
            }
        }

        private void volumeSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (waveOut2 != null)
            {
                // Ustaw głośność na procent wartości suwaka
                waveOut2.Volume = (float)volumeSlider2.Value / 100;
            }
        }

        private async void synchroButton1_Click(object sender, RoutedEventArgs e)
        {
            if((ready1 = true && bpmTextBox2.Text!="BPM: ")&&bpmTextBox1.IsReadOnly == false && pythonEngineisWorking == false)
            {

                bpmTextBox1.Text = bpmTextBox2.Text;
                string text = bpmTextBox1.Text;
                text = text.Replace("BPM: ", "").Trim();
                double newBPM = double.Parse(text);
                if (newBPM != currentAudioFile1.BPM)
                {
                    ready1 = false;
                    readyText1.Foreground = Brushes.Red;
                    deleteTrack = threadsService.deleteCopiedTrack(currentAudioFile1.FileName);
                    await deleteTrack;
                    deleteOldCopies = threadsService.deleteCopies();

                    ctsChangeBPM1 = new CancellationTokenSource();

                    pythonEngineisWorking = true;
                    changedSongFilePath1 = await threadsService.changeBPM(currentAudioFile1.FilePath, (double)currentAudioFile1.BPM, newBPM, ctsChangeBPM1.Token);
                    pythonEngineisWorking = false;

                    readyText1.Foreground = Brushes.Green;
                    knob1.LockKnobRotation();
                    bpmTextBox1.IsReadOnly = true;
                    ready1 = true;
                }

            }
        }
        private async void synchroButton2_Click(object sender, RoutedEventArgs e)
        {
            
            
            if (ready2 = true && bpmTextBox1.Text != "BPM: "&& pythonEngineisWorking == false) 
            {

                bpmTextBox2.Text = bpmTextBox1.Text;
                string text = bpmTextBox2.Text;
                text = text.Replace("BPM: ", "").Trim();
                double newBPM = double.Parse(text);
                if (newBPM != currentAudioFile2.BPM)
                {
                    ready2 = false;
                    readyText2.Foreground = Brushes.Red;
                    deleteTrack = threadsService.deleteCopiedTrack(currentAudioFile2.FileName);
                    await deleteTrack;
                    deleteOldCopies = threadsService.deleteCopies();
                    ctsChangeBPM2 = new CancellationTokenSource();

                    pythonEngineisWorking = true;
                    changedSongFilePath2 = await threadsService.changeBPM(currentAudioFile2.FilePath, (double)currentAudioFile2.BPM, newBPM, ctsChangeBPM2.Token);
                    pythonEngineisWorking = false;

                    readyText2.Foreground = Brushes.Green;
                    knob2.LockKnobRotation();
                    bpmTextBox2.IsReadOnly = true;
                    ready2 = true;
                }

            }
        }

        private async void changeBPM1_Click(object sender, RoutedEventArgs e)
        {
            
            if(bpmTextBox1.IsReadOnly == true)
            {
                return;
            }
            
            string text = bpmTextBox1.Text;
            text = text.Replace("BPM: ", "").Trim();
            double newBPM = double.Parse(text);
            if (newBPM != currentAudioFile1.BPM && pythonEngineisWorking==false)
            {
                ready1 = false;
                readyText1.Foreground = Brushes.Red;
                deleteTrack = threadsService.deleteCopiedTrack(currentAudioFile1.FileName);
                await deleteTrack;
                deleteOldCopies = threadsService.deleteCopies();

                ctsChangeBPM1 = new CancellationTokenSource();

                pythonEngineisWorking = true;
                changedSongFilePath1 = await threadsService.changeBPM(currentAudioFile1.FilePath, (double)currentAudioFile1.BPM, newBPM, ctsChangeBPM1.Token);
                pythonEngineisWorking = false;
                
                readyText1.Foreground = Brushes.Green;
                knob1.LockKnobRotation();
                bpmTextBox1.IsReadOnly = true;
                ready1 = true;
            }
            

        }

        private async void changeBPM2_Click(object sender, RoutedEventArgs e)
        {

            
            string text = bpmTextBox2.Text;
            text = text.Replace("BPM: ", "").Trim();
            double newBPM = double.Parse(text);
            if (newBPM != currentAudioFile2.BPM && pythonEngineisWorking == false)
            {
                ready2 = false;
                readyText2.Foreground = Brushes.Red;
                deleteTrack = threadsService.deleteCopiedTrack(currentAudioFile2.FileName);
                await deleteTrack;
                deleteOldCopies = threadsService.deleteCopies();
                ctsChangeBPM2 = new CancellationTokenSource();

                pythonEngineisWorking = true;
                changedSongFilePath2 = await threadsService.changeBPM(currentAudioFile2.FilePath, (double)currentAudioFile2.BPM, newBPM, ctsChangeBPM2.Token);
                pythonEngineisWorking = false;
                
                readyText2.Foreground = Brushes.Green;
                knob2.LockKnobRotation();
                bpmTextBox2.IsReadOnly = true;
                ready2 = true;
            }


        }

        private void songOnDeck1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void textChangedBPMTextBox1(object sender, RoutedEventArgs e)
        {
            string newBpm = bpmTextBox1.Text;

            if (!newBpm.StartsWith("BPM: "))
            {
                bpmTextBox1.Text = "BPM: " + newBpm;
            }
        }

        private void textChangedBPMTextBox2(object sender, RoutedEventArgs e)
        {
            string newBpm = bpmTextBox2.Text;

            if (!newBpm.StartsWith("BPM: "))
            {
                bpmTextBox2.Text = "BPM: " + newBpm;
            }
        }


        /////// ustawienie na razie na preview WaveFormu
        private void setRectangleOnWaveForm(string filePath, Canvas previewCanvas)
        {
            using (var reader = new AudioFileReader(filePath))
            {
                double trackLengthInSeconds = reader.TotalTime.TotalSeconds;
                double selectionWidth = (30.0 / trackLengthInSeconds) * previewCanvas.ActualWidth;

                Rectangle selectionRect = new Rectangle
                {
                    Width = selectionWidth,
                    Height = previewCanvas.ActualHeight,
                    Fill = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)) // Półprzezroczysty
                };

                // Ustawienie prostokąta selektora na początku podglądu
                Canvas.SetLeft(selectionRect, 0);
                Canvas.SetTop(selectionRect, 0);
                previewCanvas.Children.Add(selectionRect);

                // Tworzenie zachowania przeciągania dla prostokąta
                new DraggableBehavior(selectionRect, previewCanvas);
            }

        }

        


        // pobierac bpm z metadanych - lub ustawiac recznie
        // dodac dwa pokretla - jedno do ustawiania bpm drugie do przyspieszania - chyba to nie ma sensu? xd
        // poprawić wyglad
        // 03.08 zrobic zeby mozna bylo odpalac mp3 i wave bez przeszkod
        // Konsultacja: 04.08 10:00
        // zrobic taka mala baze danych w pliku i pobierac te nuty z metadanych co sa i liczyc na watku dla innych
        // zrobic na watkach

        // 18.10 NOWY POMYSL
        // zrobic dwa outputy - jezeli DJ chce dopasowac kawalek to wycisza sobie cos tam i patrzy na sluchawkach to przesuniecie
        // czyli moze zmienia sie kolor tego waveformu ktory juz leci (blokuje sie) - pokazuje apka ze tryb dopasowania
        // i dj moze na spokojnie se przesunac track
        // dodatkowo zmienic ten waveform na bardziej kwadratowe linie - nie jak teraz takie dziwne
    }
}
