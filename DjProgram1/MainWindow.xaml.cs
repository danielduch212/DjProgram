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
        DjProgram1.Services.MusicService musicService1 = new DjProgram1.Services.MusicService();
        DjProgram1.Services.MusicService musicService2 = new DjProgram1.Services.MusicService();
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

        //przygotowania MVService
        ViewModelService ViewModelService1;
        ViewModelService ViewModelService2;


        //watki
        ThreadsService threadsService1;
        ThreadsService threadsService2;
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

        public bool uploadTrackFinished1 = true;
        public bool uploadTrackFinished2 = true;

        public Synchronizer synchronizer = new Synchronizer();

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


        private Task refreshListBoxTask;
        
        public MainWindow()
        {

            InitializeComponent();
            this.fileService = new FIleService();
            threadsService1 = new ThreadsService(musicService1, fileService);
            threadsService2 = new ThreadsService(musicService2, fileService);
            volumeSlider1.Value = 100;
            volumeSlider2.Value = 100;
            imageLoading2.Visibility = Visibility.Hidden;
            imageLoading1.Visibility = Visibility.Hidden;

            knob1.bpmTextBox = bpmTextBox1;
            knob2.bpmTextBox = bpmTextBox2;
            knob1.LockKnobRotation();
            knob2.LockKnobRotation();

            knobToCut1.LockKnobRotation();
            knobToCut2.LockKnobRotation();
            knobToCut1.Initialize(canvas1, musicService1.progressIndicator, actualTime1);
            knobToCut2.Initialize(canvas2, musicService2.progressIndicator, actualTime2);
            this.Closing += MainWindow_Closing;

        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(audioFiles.Count > 0)
            {
                fileService.UpdateDataBase(audioFiles.ToArray());

            }
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

            ViewModelService2.UploadTrack(selectedIndex);
            


        }
        private async void buttonUpload1_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = songList.SelectedIndex;
            ViewModelService1.UploadTrack(selectedIndex);
            
        }
        private async void buttonUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath; 

                audioFiles = fileService.uploadSongs(audioFiles, folderPath, songList);
                this.ViewModelService1 = new ViewModelService(audioFiles, fileService,songList, canvas1, knobToCut1, knob1, bpmTextBox1, songOnDeck1, actualTime1, durationTime1, rotateTransformLoading1, rotateTransformCD1,imageLoading1, readyText1, volumeSlider1, synchronizer);
                this.ViewModelService2 = new ViewModelService(audioFiles, fileService, songList, canvas2, knobToCut2, knob2, bpmTextBox2, songOnDeck2, actualTime2, durationTime2, rotateTransformLoading2, rotateTransformCD2, imageLoading2, readyText2, volumeSlider2, synchronizer);

            }
        }

        private void prepare_track_click(object sender, RoutedEventArgs e)
        {

        }

        private async void reset_Data_Base_click(object sender, RoutedEventArgs e)
        {
            if (audioFiles.Count == 0 || audioFiles == null)
            {
                fileService.clearDataBase();
                MessageBoxResult result = MessageBox.Show(
                    "Zresetowano baze danych",
                    "Potwierdzenie",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "Bazę danych można usunąć tylko przy starcie programu.",
                    "Potwierdzenie",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void playButton1_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService1.PlayTrack();
            
        }

        private async void playButton2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService2.PlayTrack();
            
        }

        private void pauseButton1_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService1.PauseTrack();
            
        }

        private void pauseButton2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService2.PauseTrack();

        }

        private async void stopButton1_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService1.StopTrack();

        }
        private async void stopButton2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService2.StopTrack();

        }

        private void volumeSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(ViewModelService1 != null)
            {
                ViewModelService1.VolumeSliderChanged();

            }
        }

        private void volumeSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModelService2 != null)
            {
                ViewModelService2.VolumeSliderChanged();

            }
        }

        private async void synchroButton1_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService1.SynchronizeTrack(bpmTextBox2);
        }
        private async void synchroButton2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService2.SynchronizeTrack(bpmTextBox1);
        }

        private async void changeBPM1_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService1.ChangeBPM();
        }

        private async void changeBPM2_Click(object sender, RoutedEventArgs e)
        {
            ViewModelService2.ChangeBPM();

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




    }
}
