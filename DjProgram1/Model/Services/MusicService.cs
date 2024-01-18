using DjProgram1.Model.Data;
using NAudio.Wave;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        private string filePathPythonDDL;
        FileService fileService;
        //= @"C:\Program Files\Python311\python311.dll";

        public MusicService(string filePath, FileService fileService)
        {
            this.filePathPythonDDL = @filePath;
            this.fileService = fileService;
        }
        public MusicService()
        {
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
                double canvasHeight = waveformCanvas.ActualHeight - 20;
                int samplesToDisplay = displaySeconds * 2;
                double barWidth = canvasWidth / samplesToDisplay;
                double sampleDuration = displaySeconds / (double)samplesToDisplay;


                for (int i = 0; i < samplesToDisplay && i < audioSamples.Count; i++)
                {
                    double sampleHeight = audioSamples[i] * canvasHeight;

                    Rectangle rect = new Rectangle
                    {
                        Width = Math.Max(barWidth - 1, 1),
                        Height = sampleHeight,
                        Fill = Brushes.Black
                    };

                    Canvas.SetLeft(rect, i * barWidth);
                    Canvas.SetTop(rect, ((canvasHeight - sampleHeight) / 2)+10);
                    waveformCanvas.Children.Add(rect);
                }

                List<double> beatIntervals = new List<double>();
                double interval = GetInterval(timeStamps);
                if (interval > 0)
                {
                    double lastTimeStamp = timeStamps.Last();
                    for (double beat = timeStamps[0]; beat <= lastTimeStamp; beat += interval)
                    {
                        beatIntervals.Add(beat);
                    }
                }
                List<double> foundTimeStamps = beatIntervals
                .Where(ts => ts >= 0 && ts <= 30)
                .ToList();

                foreach (double timeStamp in foundTimeStamps)
                {
                    double linePosition = timeStamp * (canvasWidth / 30);



                    Rectangle timeMarkerRect = new Rectangle
                    {
                        Width = 2,
                        Height = waveformCanvas.ActualHeight,
                        Fill = Brushes.DarkGray,
                    };
                    Canvas.SetLeft(timeMarkerRect, linePosition);
                    Canvas.SetTop(timeMarkerRect, 0);
                    waveformCanvas.Children.Add(timeMarkerRect);

                }

            });
        }

        public double[] LoadAudioSamples(string filePath)
        {
            List<double> samples = new List<double>();
            try
            {
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
            catch(Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBoxResult result1 = MessageBox.Show(
                    "Nieprawidlowy plik",
                    "Potwierdzenie",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                    return samples.ToArray();
                });
            }
            return samples.ToArray();
        }

        public void AnimatePhoto(RotateTransform rotateTransform)
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

        public string CalculateBPMPython(string filePath)
        {
            string result = "";

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "Model", "Services");

            Runtime.PythonDLL = @filePathPythonDDL;

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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxResult result1 = MessageBox.Show(
                        "Nieprawidlowa biblioteka ddl error: " + ex,
                        "Potwierdzenie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                        fileService.ClearFilePythonDDl();
                        
                        return;
                    });
                }
            }
            PythonEngine.Shutdown();
            return result;
        }


        public string ChangeBPM(string filePath, double oldBPM, double newBPM)
        {

            string newFilePath = "";

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "Model", "Services");

            Runtime.PythonDLL = @filePathPythonDDL;

            PythonEngine.Initialize();


            //sciezka do folderu z plikami
            string baseDirectory1 = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo projectDirectoryInfo1 = Directory.GetParent(baseDirectory1).Parent.Parent.Parent;
            string servicesPath1 = Path.Combine(projectDirectoryInfo.FullName, "songCopies");

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(servicesPath);
                    dynamic pythonScript = Py.Import("pythonApp");
                    newFilePath = pythonScript.change_bpm(filePath, oldBPM, newBPM, servicesPath1);
                }
                catch (Exception ex)
                {
                    newFilePath = "An error occurred: " + ex.Message;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxResult result1 = MessageBox.Show(
                        "Nieprawidlowa biblioteka ddl error: " + ex,
                        "Potwierdzenie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                        fileService.ClearFilePythonDDl();
                        return;
                    });
                }
            }
            PythonEngine.Shutdown();
            return newFilePath;
        }

        public List<double> ReturnTimeStampsPYTHON(string filePath)
        {

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            DirectoryInfo projectDirectoryInfo = Directory.GetParent(baseDirectory).Parent.Parent.Parent;

            string servicesPath = Path.Combine(projectDirectoryInfo.FullName, "Model", "Services");

            Runtime.PythonDLL = @filePathPythonDDL;
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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxResult result1 = MessageBox.Show(
                        "Nieprawidlowa biblioteka ddl error: " + ex,
                        "Potwierdzenie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                        fileService.ClearFilePythonDDl();
                        return;
                    });
                }
            }
            PythonEngine.Shutdown();
            return timeStamps;
        }
        public double GetInterval(List<double> timeStamps)
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
            catch(Exception ex)
            {

            }
            return 0;
        }


        public void UpdateWaveformAndIndicator(double currentPosition, double totalDuration, Canvas waveformCanvas, List<double> audioSamples, List<double> timeStamps)
        {
            int samplesToDisplay = 30 * 2;
            double canvasWidth = waveformCanvas.ActualWidth;
            double canvasHeight = waveformCanvas.ActualHeight-20;
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
                Canvas.SetTop(rect, ((canvasHeight - sampleHeight) / 2) + 10);
                waveformCanvas.Children.Add(rect);


            }
            Rectangle progressIndicator = new Rectangle
            {
                Width = 2,
                Height = waveformCanvas.ActualHeight,
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
            double canvasHeight = waveformCanvas.ActualHeight-20;
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
                Canvas.SetTop(rect, ((canvasHeight - sampleHeight) / 2) + 10);
                waveformCanvas.Children.Add(rect);
            }

            List<double> foundTimeStamps = timeStamps
                .Where(ts => ts >= (startSampleIndex / 2) && ts <= ((startSampleIndex + samplesToDisplay) / 2))
                .ToList();

            foreach (double timeStamp in foundTimeStamps)
            {
                double adjustedTimeStamp = timeStamp - (startSampleIndex / 2);
                double linePosition = adjustedTimeStamp * (canvasWidth / adjustedDuration);

                Rectangle timeMarkerRect = new Rectangle
                {
                    Width = 2,
                    Height = waveformCanvas.ActualHeight,
                    Fill = Brushes.DarkGray,
                };
                Canvas.SetLeft(timeMarkerRect, linePosition);
                Canvas.SetTop(timeMarkerRect, 0);
                waveformCanvas.Children.Add(timeMarkerRect);

            }

            Rectangle progressIndicator = new Rectangle
            {
                Width = 2,
                Height = waveformCanvas.ActualHeight,
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









