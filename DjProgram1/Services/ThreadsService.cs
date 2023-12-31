using System.Windows.Media;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using NAudio.Wave.SampleProviders;
using System.Windows.Media.Animation;
using TagLib;
using System.Windows;
using DjProgram1.Data;
using Python.Runtime;
using System.Windows.Threading;
using System.Diagnostics;
using Gst.Rtsp;

namespace DjProgram1.Services
{
    public class ThreadsService
    {
        public MusicService musicService;
        public FIleService fileService;
        private DispatcherTimer waveformUpdateTimer;

        private double bpm;

        public ThreadsService(MusicService musicService, FIleService fileService)
        {
            this.musicService = musicService;
            this.fileService = fileService;
            
            
        }
        


        public async Task LoadAudioAsync(string filePath, CancellationToken cancellationToken)
        {
            // Wykonaj operację ładowania audio w tle
            try
            {
                double[] audioSamples = await Task.Run(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    return musicService.LoadAudioSamples(filePath);
                }, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                
                //musicService.GenerateWaveform(audioSamples, canvas2);
                //bpmTextBox1.Text = "BPM: " + musicService.CalculateBPM(audioSamples); // Przykładowe obliczenie BPM

            }
            catch (OperationCanceledException)
            {
                
            }
        }
        
        public async Task GenerateInitialWaveForm(List<double> data,List<double> timeStamps, Canvas waveformCanvas)
        {
            try
            {
                await Task.Run(() =>
                {
                    musicService.DisplayInitialWaveform(data, timeStamps,waveformCanvas);

                });

            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task<List<double>> generateWaveFormData(string filePath)
        {
            var data = new List<double>();

            await Task.Run(() =>
            {

                data = musicService.GenerateWaveformData(filePath);
                
            });
            return data;

        }

        public async Task<List<double>> generateTimeStamps(string filePath)
        {
            
            var dataTimeStamps = new List<double>();

            await Task.Run(() =>
            {

                dataTimeStamps = musicService.returnTimeStampsPYTHON(filePath);

            });
            return dataTimeStamps;

        }

        public async Task UpdateWaveformAsync(Canvas waveformCanvas, AudioFileReader reader, List<double> audioSamples, List<double> timeStamps, double totalDuration, int whichOne)
        {
            
            double sampleDisplayInterval = totalDuration / audioSamples.Count; // Interwał czasowy dla jednej próbki
            while (true) 
            {
                await Task.Delay(10);

                double currentPosition = musicService.GetCurrentPosition(reader);
                int currentSampleIndex = (int)(currentPosition / sampleDisplayInterval);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    musicService.UpdateWaveformAndIndicator(currentPosition, totalDuration, waveformCanvas, audioSamples, timeStamps, whichOne);
                });

            }
        }

        


        public async Task CountBPM(string filePath,System.Windows.Controls.TextBox textBox, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {
                    var result = musicService.calculateBPMPython(filePath);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        textBox.Text = "BPM: " + result;
                    });
                    
                });


            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task GenerateDataBase(AudioFile[] audioFiles, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {

                    fileService.checkSongsMetaData(audioFiles);
                }, cancellationToken);

            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task<string> changeBPM(string filePath, double originalBPM, double newBPM, CancellationToken cancellationToken)
        {
            string newFilePath = "";
            try
            {

               newFilePath = await Task.Run(() =>
                {
                    // Zwracamy nową ścieżkę pliku z funkcji 'musicService.changeBPM'
                    return musicService.changeBPM(filePath, originalBPM, newBPM);
                }, cancellationToken);


            }
            catch (OperationCanceledException)
            {
                return newFilePath = null;
            }
            return newFilePath;
        }

        public async Task deleteCopiedTrack(string name)
        {
            string filePath = "";
            try
            {
                await Task.Run(() =>
                {
                    filePath = fileService.CheckIfSongExists(name);
                    if (filePath != "")
                    {
                        fileService.deleteSong(filePath);
                    }
                    else
                    {

                    }


                });
            }
            catch (OperationCanceledException)
            {

            }

        }
        public async Task deleteCopies()
        {
            try
            {
                await Task.Run(() =>
                {
                    fileService.DeleteCopies();
                });
            }
            catch
            {

            }
        }

        public async Task updateTimePassed()
        {



        }



        


    }
}
