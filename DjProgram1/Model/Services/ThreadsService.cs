using DjProgram1.Model.Data;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DjProgram1.Model.Services
{
    public class ThreadsService
    {
        public MusicService musicService;
        public FileService fileService;
        private DispatcherTimer waveformUpdateTimer;

        private double bpm;

        public ThreadsService(MusicService musicService, FileService fileService)
        {
            this.musicService = musicService;
            this.fileService = fileService;


        }



        public async Task LoadAudioAsync(string filePath, CancellationToken cancellationToken)
        {
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
            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task GenerateInitialWaveForm(List<double> data, List<double> timeStamps, Canvas waveformCanvas)
        {
            try
            {
                await Task.Run(() =>
                {
                    musicService.DisplayInitialWaveform(data, timeStamps, waveformCanvas);

                });

            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task<List<double>> GenerateWaveFormData(string filePath)
        {
            var data = new List<double>();

            await Task.Run(() =>
            {

                data = musicService.GenerateWaveformData(filePath);

            });
            return data;

        }

        public async Task<List<double>> GenerateTimeStamps(string filePath)
        {

            var dataTimeStamps = new List<double>();

            await Task.Run(() =>
            {

                dataTimeStamps = musicService.ReturnTimeStampsPYTHON(filePath);

            });
            return dataTimeStamps;

        }

        public async Task UpdateWaveformAsync(Canvas waveformCanvas, AudioFileReader reader, List<double> audioSamples, List<double> timeStamps, double totalDuration, CancellationToken cancellationToken)
        {
            double sampleDisplayInterval = totalDuration / audioSamples.Count;
            while (true)
            {
                await Task.Delay(50);

                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                double currentPosition = musicService.GetCurrentPosition(reader);
                int currentSampleIndex = (int)(currentPosition / sampleDisplayInterval);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    musicService.UpdateWaveformAndIndicator(currentPosition, totalDuration, waveformCanvas, audioSamples, timeStamps);
                });
            }
        }




        public async Task CountBPM(string filePath, System.Windows.Controls.TextBlock textBox, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Run(() =>
                {
                    var result = musicService.CalculateBPMPython(filePath);
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

        public async Task<string> ChangeBPM(string filePath, double originalBPM, double newBPM, CancellationToken cancellationToken)
        {
            string newFilePath = "";
            try
            {

                newFilePath = await Task.Run(() =>
                 {
                     return musicService.ChangeBPM(filePath, originalBPM, newBPM);
                 }, cancellationToken);


            }
            catch (OperationCanceledException)
            {
                return newFilePath = null;
            }

            return newFilePath;
        }

        public async Task DeleteCopiedTrack(string name)
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
        public async Task DeleteCopies()
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

        public async Task RefreshList(ListBox lisbox, List<AudioFile> audioFiles)
        {
            try
            {
                await Task.Run(() =>
                {
                    fileService.RefreshListBox(lisbox, audioFiles);
                });
            }
            catch
            {

            }
        }







    }
}
