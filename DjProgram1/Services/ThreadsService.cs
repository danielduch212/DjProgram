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
        
        public async Task GenerateWaveForm(string filePath, Canvas waveformCanvas)
        {
            try
            {
                await Task.Run(() =>
                {

                    musicService.GenerateWaveform(filePath, waveformCanvas);
                });

                //to dodalem nizej






            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task MovePositionLine(Canvas waveformCanvas, string audioFilePath, TextBlock actualTime, TextBlock durationTime, CancellationToken cancellationToken, double positionOfLine)
        {
            try
            {
                var audioPlayer = new AudioFileReader(audioFilePath);
                double audioDuration = audioPlayer.TotalTime.TotalSeconds;
                durationTime.Text = audioDuration.ToString(@"mm\:ss");
                await Task.Run(() =>
                {
                    double canvasWidth = waveformCanvas.ActualWidth;
                    int numSteps = 1000;

                    Line positionLine = null;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        positionLine = waveformCanvas.Children.OfType<Line>().FirstOrDefault(); // Find an existing line on the canvas
                    });

                    double xPosition = positionOfLine > 0 ? positionOfLine : 0;

                    if (positionLine == null) // If line does not exist, create a new one
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            positionLine = new Line();
                            positionLine.Stroke = Brushes.Red;
                            positionLine.X1 = xPosition;
                            positionLine.X2 = xPosition;
                            positionLine.Y1 = 0;
                            positionLine.Y2 = waveformCanvas.ActualHeight;
                            waveformCanvas.Children.Add(positionLine);
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            xPosition = positionLine.X1;

                        });
                        
                    }

                    double remainingDuration = audioDuration - (xPosition / canvasWidth) * audioDuration;
                    double xStep = canvasWidth / numSteps;
                    double delay = (remainingDuration * 1000) / (numSteps - (xPosition / xStep));

                    int stepsToPerform = numSteps - (int)(xPosition / xStep);

                    for (int step = 0; step < stepsToPerform; step++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        if(cancellationToken.IsCancellationRequested == false)
                        {
                            xPosition += xStep;
                        }
                        
                        Thread.Sleep((int)delay);

                        if(cancellationToken.IsCancellationRequested == false)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                positionLine.X1 = xPosition;
                                positionLine.X2 = xPosition;
                            });
                        }
                        
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
            }
        }

    }
}
