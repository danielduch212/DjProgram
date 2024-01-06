using System;
using System.IO;

using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using NAudio.Wave;
using SoundTouch;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using TagLib;
using System.Windows.Media.Animation;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using System.Linq;
using Python.Runtime;
using System.Threading;
using VisioForge.Libs.ZXing;
using System.Security.Policy;
using DjProgram1.Model.Data;
using Path = System.IO.Path;

namespace DjProgram1.Model.Services
{
    public class MusicService
    {
        private DispatcherTimer timer;
        private int currentPosition; 
        List<AudioFile> audioFiles;
        public Rectangle progressIndicator;
        private AudioFileReader reader;
        private TextBlock actualTime;
        private double firstBeatTimeStamp;
        private double beatInterval;

        ~MusicService()
        {
            try
            {
                var directory = new DirectoryInfo(@"C:\Users\Janusz\source\repos\DjProgram1\DjProgram1\songCopies");

                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

            }
            catch (Exception ex)
            {

            }
        }

        public List<double> GenerateWaveformData(string filePath)
        {
            List<double> audioSamples = new List<double>();
            using (var reader = new AudioFileReader(filePath))
            {
                double totalSeconds = reader.TotalTime.TotalSeconds;

                int sampleInterval = reader.WaveFormat.SampleRate / 2; 

                int totalSamples = (int)(totalSeconds * reader.WaveFormat.SampleRate / sampleInterval);

                float[] buffer = new float[sampleInterval];
                int samplesRead;

                for (int i = 0; i < totalSamples; i++)
                {
                    samplesRead = reader.Read(buffer, 0, sampleInterval);
                    if (samplesRead == 0) break;

                    double maxSampleValue = buffer.Take(samplesRead).Max(x => Math.Abs(x));
                    audioSamples.Add(maxSampleValue);
                }
            }
            return audioSamples;
        }

        public void DisplayInitialWaveform(List<double> audioSamples, List<double> timeStamps, Canvas waveformCanvas, int displaySeconds = 30)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                waveformCanvas.Children.Clear();
                double canvasWidth = waveformCanvas.ActualWidth;
                double canvasHeight = waveformCanvas.ActualHeight - 30;
                int samplesToDisplay = displaySeconds * 2;
                double barWidth = canvasWidth / samplesToDisplay;
                double sampleDuration = displaySeconds / (double)samplesToDisplay;


                int beatCounter = 0;
                int beatDisplayInterval = 4; 

                for (int i = 0; i < samplesToDisplay && i < audioSamples.Count; i++)
                {
                    double sampleHeight = audioSamples[i] * canvasHeight;

                    // Czarna linia reprezentująca próbkę audio
                    Rectangle rect = new Rectangle
                    {
                        Width = Math.Max(barWidth - 1, 1),
                        Height = sampleHeight,
                        Fill = Brushes.Black
                    };

                    Canvas.SetLeft(rect, i * barWidth);
                    Canvas.SetTop(rect, (canvasHeight - sampleHeight) / 2);
                    waveformCanvas.Children.Add(rect);

                    if (timeStamps.Any(ts => Math.Abs(ts - i * sampleDuration) < sampleDuration))
                    {
                        beatCounter++;
                        if (beatCounter % beatDisplayInterval == 0)
                        {
                            var line = new Line
                            {
                                X1 = i * barWidth,
                                X2 = i * barWidth,
                                Y1 = 0,
                                Y2 = canvasHeight,
                                Stroke = Brushes.DarkGray,
                                StrokeThickness = 2
                            };
                            waveformCanvas.Children.Add(line);
                        }
                    }

                }
                ProcessBeats(timeStamps);
            });
        }

        public double[] LoadAudioSamples(string filePath)
        {
            List<double> samples = new List<double>();

            using (var reader = new AudioFileReader(filePath))
            {
                var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                int bytesRead;

                while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < bytesRead / reader.WaveFormat.Channels; i++)
                    {
                        for (int channel = 0; channel < reader.WaveFormat.Channels; channel++)
                        {
                            samples.Add(buffer[i * reader.WaveFormat.Channels + channel]);
                        }
                    }
                }
            }

            return samples.ToArray();
        }

        public void animatePhoto(RotateTransform rotateTransform)
        {


            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 360;
            animation.Duration = TimeSpan.FromSeconds(5);
            animation.RepeatBehavior = RepeatBehavior.Forever;


            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        public void StopRotation(RotateTransform rotateTransform)
        {
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            rotateTransform.Angle = 10;
        }

        public string calculateBPMPython(string filePath)
        {
            string result = "";

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "Model", "Services");

            Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";

            PythonEngine.Initialize();

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(servicesPath);
                    dynamic pythonScript = Py.Import("pythonApp");
                    dynamic bpm = pythonScript.calculate_bpm(filePath);

                    double bpmRounded = Math.Round((double)bpm, 2);
                    result = bpmRounded.ToString();
                }
                catch (Exception ex)
                {
                    result = "An error occurred: " + ex.Message;
                }
            }
            PythonEngine.Shutdown();
            return result;
        }


        public string changeBPM(string filePath, double oldBPM, double newBPM)
        {

            string newFilePath = "";

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "Model", "Services");

            Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";

            PythonEngine.Initialize();

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(servicesPath);
                    dynamic pythonScript = Py.Import("pythonApp");
                    newFilePath = pythonScript.change_bpm(filePath, oldBPM, newBPM);
                }
                catch (Exception ex)
                {
                    newFilePath = "An error occurred: " + ex.Message;
                }
            }
            PythonEngine.Shutdown();
            return newFilePath;
        }

        public List<double> returnTimeStampsPYTHON(string filePath)
        {

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "Model", "Services");

            Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";
            PythonEngine.Initialize();
            List<double> timeStamps = new List<double>();

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(servicesPath);
                    dynamic pythonScript = Py.Import("pythonApp");
                    var beatTimes = pythonScript.return_time_stamps(filePath);
                    timeStamps = new List<double>(beatTimes.As<double[]>());
                }
                catch (Exception ex)
                {
                }
            }
            PythonEngine.Shutdown();
            return timeStamps;
        }


        public void ProcessBeats(List<double> timeStamps)
        {

            if (timeStamps.Count >= 4)
            {
                firstBeatTimeStamp = timeStamps[0];
                beatInterval = timeStamps[3] - timeStamps[0];
            }
        }
        public double getInterval(List<double> timeStamps)
        {

            if (timeStamps.Count >= 4)
            {
                if (timeStamps[3] - timeStamps[0] < 1.5)
                {
                    return beatInterval = timeStamps[7] - timeStamps[0];

                }
                else
                {
                    return beatInterval = timeStamps[3] - timeStamps[0];

                }
            }
            else
            {
                return 0;
            }

        }

        public double GetCurrentPosition(AudioFileReader reader)
        {
            try
            {
                if (reader != null)
                {
                    return reader.CurrentTime.TotalSeconds;
                }
            }
            catch
            {

            }
            return 0;
        }


        public void UpdateWaveformAndIndicator(double currentPosition, double totalDuration, Canvas waveformCanvas, List<double> audioSamples, List<double> timeStamps)
        {
            int samplesToDisplay = 30 * 2; 
            double canvasWidth = waveformCanvas.ActualWidth;
            double canvasHeight = waveformCanvas.ActualHeight - 30;
            double halfCanvasWidth = canvasWidth * 0.5;
            double barWidth = canvasWidth / samplesToDisplay;
            double adjustedDuration = Math.Min(30, totalDuration); 
            double sampleRate = audioSamples.Count / totalDuration; 

            double indicatorPosition = currentPosition / adjustedDuration * canvasWidth;
            bool shouldMove = indicatorPosition >= halfCanvasWidth;
            if (indicatorPosition >= halfCanvasWidth)
            {
                indicatorPosition = halfCanvasWidth;
            }
            waveformCanvas.Children.Clear(); 
            int startSampleIndex = shouldMove ? (int)(currentPosition * sampleRate) - samplesToDisplay / 2 : 0;
            if (startSampleIndex < 0) startSampleIndex = 0;
            for (int i = 0; i < samplesToDisplay && startSampleIndex + i < audioSamples.Count; i++)
            {
                double sampleHeight = audioSamples[startSampleIndex + i] * canvasHeight;
                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(barWidth - 1, 1),
                    Height = sampleHeight,
                    Fill = Brushes.Black
                };

                double rectPosition = shouldMove ? i * barWidth : (startSampleIndex + i) * barWidth;
                Canvas.SetLeft(rect, rectPosition);
                Canvas.SetTop(rect, (canvasHeight - sampleHeight) / 2);
                waveformCanvas.Children.Add(rect);


            }
            Rectangle progressIndicator = new Rectangle
            {
                Width = 2,
                Height = canvasHeight,
                Fill = Brushes.MediumVioletRed
            };
            Canvas.SetLeft(progressIndicator, indicatorPosition);
            Canvas.SetTop(progressIndicator, 0);
            waveformCanvas.Children.Add(progressIndicator);

        }

        public void UpdateWaveformByKnob(double knobRotation, double totalDuration, Canvas waveformCanvas, List<double> audioSamples, List<double> timeStamps)
        {
            double currentPosition = knobRotation * totalDuration;

            int samplesToDisplay = 30 * 2; 
            double canvasWidth = waveformCanvas.ActualWidth;
            double canvasHeight = waveformCanvas.ActualHeight - 30;
            double halfCanvasWidth = canvasWidth * 0.5;
            double barWidth = canvasWidth / samplesToDisplay;
            double adjustedDuration = Math.Min(30, totalDuration); 
            double sampleRate = audioSamples.Count / totalDuration; 

            double indicatorPosition = (currentPosition / adjustedDuration) * canvasWidth;
            bool shouldMove = indicatorPosition >= halfCanvasWidth;
            if (shouldMove)
            {
                indicatorPosition = halfCanvasWidth;
            }

            waveformCanvas.Children.Clear();

            int startSampleIndex = shouldMove ? (int)(currentPosition * sampleRate) - (samplesToDisplay / 2) : 0;
            startSampleIndex = Math.Max(0, startSampleIndex);

            for (int i = 0; i < samplesToDisplay && (startSampleIndex + i) < audioSamples.Count; i++)
            {
                double sampleHeight = audioSamples[startSampleIndex + i] * canvasHeight;
                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(barWidth - 1, 1),
                    Height = sampleHeight,
                    Fill = Brushes.Black
                };

                double rectPosition = shouldMove ? (i * barWidth) : ((startSampleIndex + i) * barWidth);
                Canvas.SetLeft(rect, rectPosition);
                Canvas.SetTop(rect, (canvasHeight - sampleHeight) / 2);
                waveformCanvas.Children.Add(rect);
            }

            double firstValidBeat = timeStamps.FirstOrDefault(t => t > 0);
            double beatInterval = getInterval(timeStamps);
            double nextBeat = firstValidBeat;

            while (nextBeat <= currentPosition + adjustedDuration)
            {
                double linePosition = ((nextBeat - (shouldMove ? currentPosition : 0)) / adjustedDuration) * canvasWidth;
                if (linePosition >= 0 && linePosition <= canvasWidth)
                {
                    Line line = new Line
                    {
                        X1 = linePosition,
                        X2 = linePosition,
                        Y1 = 0,
                        Y2 = canvasHeight,
                        Stroke = Brushes.DarkGray,
                        StrokeThickness = 2
                    };
                    waveformCanvas.Children.Add(line);
                }
                nextBeat += beatInterval;
            }

            Rectangle progressIndicator = new Rectangle
            {
                Width = 2,
                Height = canvasHeight,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(progressIndicator, indicatorPosition);
            Canvas.SetTop(progressIndicator, 0);
            waveformCanvas.Children.Add(progressIndicator);
        }

        public void InitTimer(System.Windows.Forms.Timer timer, TextBlock textBLock, AudioFileReader reader)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                timer.Interval = 250;


                this.reader = reader;
                actualTime = textBLock;
                timer.Tick += new EventHandler(UpdateLabel);
            });

        }

        private void UpdateLabel(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (reader != null)
                {
                    TimeSpan currentTime = reader.CurrentTime;
                    actualTime.Text = currentTime.ToString(@"mm\:ss");

                }
            });

        }

    }




}









