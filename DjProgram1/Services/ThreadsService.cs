using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DjProgram1.Services
{
    public class ThreadsService
    {
        public MusicService musicService;
        public ThreadsService(MusicService musicService)
        {
            this.musicService = musicService;
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

        public async Task PlayMusic(NAudio.Wave.WaveOut waveOut, CancellationToken cancellationToken)
        {
            // Wykonaj operację ładowania audio w tle
            try
            {


                await Task.Run(() =>
                {
                    waveOut.Play();

                    
                    //animatePhoto(rotateTransform1);
                });

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Tutaj możesz dodać logikę, która monitoruje stan odtwarzania
                    // i przerywa pętlę, jeśli odtwarzanie zostanie zatrzymane lub anulowane.
                    if (waveOut.PlaybackState != NAudio.Wave.PlaybackState.Playing)
                    {
                        break;
                    }
                }

                //&& waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing

            }
            catch (OperationCanceledException)
            {
                
            }
        }
    }
}
