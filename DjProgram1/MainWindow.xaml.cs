using DjProgram1.Model.Data;
using DjProgram1.Model.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DjProgram1
{

    public partial class MainWindow : Window
    {
        
        List<Model.Data.AudioFile> audioFiles = new List<Model.Data.AudioFile>();

        FileService fileService;

        MusicService musicService1;
        MusicService musicService2;

        ViewModelService ViewModelService1;
        ViewModelService ViewModelService2;

        Model.Model model;

        private int selectedIndex;

        public Synchronizer synchronizer = new Synchronizer();

        public MainWindow()
        {

            InitializeComponent();

            volumeSlider1.Value = 100;
            volumeSlider2.Value = 100;
            imageLoading2.Visibility = Visibility.Hidden;
            imageLoading1.Visibility = Visibility.Hidden;

   
            this.fileService = new FileService();

            string pathDDL = fileService.FindPythonDDLFilePath();

            musicService1 = new DjProgram1.Model.Services.MusicService(pathDDL,fileService);
            musicService2 = new DjProgram1.Model.Services.MusicService(pathDDL, fileService);

            knob1.Initialize(musicService1);
            knob2.Initialize(musicService2);

            knob1.bpmTextBox = bpmTextBox1;
            knob2.bpmTextBox = bpmTextBox2;
            knob1.LockKnobRotation();
            knob2.LockKnobRotation();

            knobToCut1.Initialize(musicService1, canvas1, musicService1.progressIndicator, actualTime1);
            knobToCut2.Initialize( musicService2, canvas2, musicService2.progressIndicator, actualTime2);
            knobToCut1.LockKnobRotation();
            knobToCut2.LockKnobRotation();

            this.Closing += MainWindow_Closing;

            fileService.InitializeControls(playButtonImage1,pauseButtonImage1,stopButtonImage1,image1,imageLoading1,playButtonImage2,pauseButtonImage2,stopButtonImage2,image2,imageLoading2);
            
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (audioFiles.Count > 0)
            {
                fileService.UpdateDataBase(audioFiles.ToArray());
                fileService.DeleteAllCopies();
            }
        }
        

        private async void buttonUpload2_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = songList.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex != null)
                ViewModelService2.UploadTrack(selectedIndex);



        }
        private async void buttonUpload1_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = songList.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex != null)
                ViewModelService1.UploadTrack(selectedIndex);

        }
        private async void buttonUploadFiles_Click(object sender, RoutedEventArgs e)
        {

            if (audioFiles.Count > 0)
            {
                MessageBoxResult result1 = MessageBox.Show(
                    "Można załadować utwory tylko raz na działanie programu.",
                    "Potwierdzenie",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                return;
            }
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;

                audioFiles = fileService.uploadSongs(audioFiles, folderPath, songList);
                model = new Model.Model(audioFiles);
                this.ViewModelService1 = new ViewModelService(model, fileService, musicService1,songList, canvas1, knobToCut1, knob1, bpmTextBox1, songOnDeck1, actualTime1, durationTime1, rotateTransformLoading1, rotateTransformCD1, imageLoading1, readyText1, volumeSlider1, synchronizer);
                this.ViewModelService2 = new ViewModelService(model, fileService, musicService2, songList, canvas2, knobToCut2, knob2, bpmTextBox2, songOnDeck2, actualTime2, durationTime2, rotateTransformLoading2, rotateTransformCD2, imageLoading2, readyText2, volumeSlider2, synchronizer);

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
            if (ViewModelService1 != null)
                ViewModelService1.PlayTrack();

        }

        private async void playButton2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService2 != null)
                ViewModelService2.PlayTrack();

        }

        private void pauseButton1_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService1 != null)
                ViewModelService1.PauseTrack();

        }

        private void pauseButton2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService2 != null)
                ViewModelService2.PauseTrack();

        }

        private async void stopButton1_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService1 != null)
                ViewModelService1.StopTrack();

        }
        private async void stopButton2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService2 != null)
                ViewModelService2.StopTrack();

        }

        private void volumeSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModelService1 != null)
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
            if (ViewModelService1 != null)
                ViewModelService1.SynchronizeTrack(bpmTextBox2);
        }
        private async void synchroButton2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService2 != null)
                ViewModelService2.SynchronizeTrack(bpmTextBox1);
        }

        private async void changeBPM1_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService1 != null)
                ViewModelService1.ChangeBPM();
        }

        private async void changeBPM2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelService2 != null)
                ViewModelService2.ChangeBPM();


        }

        private void songOnDeck1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void textChangedBPMTextBox1(object sender, RoutedEventArgs e)
        {
            
        }

        private void textChangedBPMTextBox2(object sender, RoutedEventArgs e)
        {
            
        }
        




    }
}
