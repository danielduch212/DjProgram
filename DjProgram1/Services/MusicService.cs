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

namespace DjProgram1.Services
{
    public class MusicService
    {
        private DispatcherTimer timer;
        private int currentPosition; // Aktualna pozycja w próbkach audio
        List<DjProgram1.Data.AudioFile> audioFiles;
        public Rectangle progressIndicator1;
        public Rectangle progressIndicator2;
        private AudioFileReader reader1;
        private AudioFileReader reader2;
        private TextBlock actualTime1;
        private TextBlock actualTime2;
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

                int sampleInterval = reader.WaveFormat.SampleRate / 2; // Dla 44.1kHz, to będzie 22050 próbek

                int totalSamples = (int)(totalSeconds * reader.WaveFormat.SampleRate / sampleInterval);

                float[] buffer = new float[sampleInterval];
                int samplesRead;

                for (int i = 0; i < totalSamples; i++)
                {
                    samplesRead = reader.Read(buffer, 0, sampleInterval);
                    if (samplesRead == 0) break;

                    // Oblicz maksymalną wartość próbki w buforze
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
                int beatDisplayInterval = 4; // Co ile uderzeń wyświetlać linię

                // Rysuj próbki audio jako czarne linie/prostokąty
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

                    // Sprawdź, czy w tym miejscu jest time stamp
                    if (timeStamps.Any(ts => Math.Abs(ts - i * sampleDuration) < sampleDuration))
                    {
                        beatCounter++;
                        if (beatCounter % beatDisplayInterval == 0) // Rysuj linię tylko co 'beatDisplayInterval' uderzeń
                        {
                            var line = new Line
                            {
                                X1 = i * barWidth,
                                X2 = i * barWidth,
                                Y1 = 0,
                                Y2 = canvasHeight,
                                Stroke = Brushes.Green,
                                StrokeThickness = 2
                            };
                            waveformCanvas.Children.Add(line);
                        }
                    }
                    ProcessBeats(timeStamps);
                }
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

            Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";

            PythonEngine.Initialize();

            

            PythonEngine.Initialize();
            
            
            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(@"C:\Users\Janusz\source\repos\DjProgram1\DjProgram1\Services");
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


            string newFilePath="";

            Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";

            PythonEngine.Initialize();


            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(@"C:\Users\Janusz\source\repos\DjProgram1\DjProgram1\Services");
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
            Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";
            PythonEngine.Initialize();
            List<double> timeStamps = new List<double>();

            using (Py.GIL())
            {
                try
                {
                    dynamic sys = Py.Import("sys");
                    sys.path.append(@"C:\Users\Janusz\source\repos\DjProgram1\DjProgram1\Services");
                    dynamic pythonScript = Py.Import("pythonApp");
                    var beatTimes = pythonScript.return_time_stamps(filePath);
                    timeStamps = new List<double>(beatTimes.As<double[]>());
                }
                catch (Exception ex)
                {
                    // Obsługa błędu
                }
            }
            PythonEngine.Shutdown();
            return timeStamps;
        }


        public void ProcessBeats(List<double> timeStamps)
        {
            // Upewnij się, że mamy przynajmniej dwa znaczniki czasowe do obliczenia interwału
            if (timeStamps.Count >= 2)
            {
                firstBeatTimeStamp = timeStamps[0];
                beatInterval = timeStamps[3] - timeStamps[0];
            }
        }

        public double GetCurrentPosition(NAudio.Wave.AudioFileReader reader)
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


        public void UpdateWaveformAndIndicator(double currentPosition, double totalDuration, Canvas waveformCanvas, List<double> audioSamples, List<double> timeStamps, int whichOne)
        {
            int samplesToDisplay = 30 * 2; // Liczba próbek do wyświetlenia na raz (dla 30 sekund)
            double canvasWidth = waveformCanvas.ActualWidth;
            double canvasHeight = waveformCanvas.ActualHeight - 30;
            double halfCanvasWidth = canvasWidth * 0.5;
            double barWidth = canvasWidth / samplesToDisplay;
            double adjustedDuration = Math.Min(30, totalDuration); // Używaj 30 sekund lub mniej, jeśli utwór jest krótszy
            double sampleRate = audioSamples.Count / totalDuration; // Samples per second

            // Aktualizacja wskaźnika postępu
            double indicatorPosition = (currentPosition / adjustedDuration) * canvasWidth;
            bool shouldMove = indicatorPosition >= halfCanvasWidth;
            if(indicatorPosition >= halfCanvasWidth)
            {
                indicatorPosition = halfCanvasWidth;
            }
            // Rysowanie próbek audio
            waveformCanvas.Children.Clear(); // Wyczyść canvas przed aktualizacją
            int startSampleIndex = shouldMove ? (int)(currentPosition * sampleRate) - (samplesToDisplay / 2) : 0;
            if (startSampleIndex < 0) startSampleIndex = 0;
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


                double nextBeat = firstBeatTimeStamp;
                while (nextBeat < currentPosition)
                {
                    nextBeat += beatInterval; // Znajdź najbliższe uderzenie po bieżącej pozycji
                }

                while (nextBeat < currentPosition + adjustedDuration)
                {
                    double linePosition = shouldMove ? ((nextBeat - currentPosition) / adjustedDuration) * canvasWidth : nextBeat * sampleRate * barWidth;
                    if (linePosition >= 0 && linePosition <= canvasWidth)
                    {
                        Line line = new Line
                        {
                            X1 = linePosition,
                            X2 = linePosition,
                            Y1 = 0,
                            Y2 = canvasHeight,
                            Stroke = Brushes.Green,
                            StrokeThickness = 1
                        };
                        waveformCanvas.Children.Add(line);
                    }
                    nextBeat += beatInterval;
                }

            }
            Rectangle progressIndicator1 = new Rectangle
            {
                Width = 2,
                Height = canvasHeight,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(progressIndicator1, indicatorPosition);
            Canvas.SetTop(progressIndicator1, 0);
            waveformCanvas.Children.Add(progressIndicator1);

        }




        private void UpdateIndicatorPosition(Canvas waveformCanvas, double indicatorPosition, int whichOne)
        {
            Rectangle progressIndicator;

            if (whichOne == 1)
            {
                if (progressIndicator1 == null)
                {
                    progressIndicator1 = new Rectangle
                    {
                        Width = 2,
                        Height = waveformCanvas.ActualHeight - 30,
                        Fill = Brushes.Red
                    };
                    waveformCanvas.Children.Add(progressIndicator1);
                }
                progressIndicator = progressIndicator1;
            }
            else
            {
                if (progressIndicator2 == null)
                {
                    progressIndicator2 = new Rectangle
                    {
                        Width = 2,
                        Height = waveformCanvas.ActualHeight - 30,
                        Fill = Brushes.Red
                    };
                    waveformCanvas.Children.Add(progressIndicator2);
                }
                progressIndicator = progressIndicator2;
            }

            // Uaktualnij pozycję wskaźnika na canvasie
            Canvas.SetLeft(progressIndicator, indicatorPosition);
        }


        private void DisplayWaveformSegment(Canvas waveformCanvas, List<double> audioSamples, List<double> timeStamps, double currentPosition, double totalDuration, int samplesToDisplay, int whichOne)
        {
            Rectangle existingIndicator = whichOne == 1 ? progressIndicator1 : progressIndicator2;

            waveformCanvas.Children.Clear(); // Clear the canvas

            double canvasWidth = waveformCanvas.ActualWidth;
            double canvasHeight = waveformCanvas.ActualHeight - 30;
            double barWidth = canvasWidth / samplesToDisplay;
            double sampleRate = audioSamples.Count / totalDuration; // Samples per second
            int startSampleIndex = (int)(currentPosition * sampleRate); // Starting sample index based on current position

            // Rysowanie próbek audio
            for (int i = 0; i < samplesToDisplay && (startSampleIndex + i) < audioSamples.Count; i++)
            {
                double sampleHeight = audioSamples[startSampleIndex + i] * canvasHeight;
                Rectangle rect = new Rectangle
                {
                    Width = Math.Max(barWidth - 1, 1),
                    Height = sampleHeight,
                    Fill = Brushes.Black
                };

                Canvas.SetLeft(rect, i * barWidth);
                Canvas.SetTop(rect, (canvasHeight - sampleHeight) / 2);
                waveformCanvas.Children.Add(rect);
            }

            // Rysowanie zielonych linii (uderzeń)
            double nextBeat = firstBeatTimeStamp;
            // Znajdź najbliższe uderzenie po bieżącej pozycji
            while (nextBeat < currentPosition)
            {
                nextBeat += beatInterval;
            }

            // Rysuj linie od najbliższego uderzenia do końca okna wyświetlania
            while (nextBeat < currentPosition + (samplesToDisplay / sampleRate))
            {
                double linePosition = (nextBeat - currentPosition) * sampleRate * barWidth;
                if (linePosition >= 0 && linePosition <= canvasWidth) // Rysuj tylko linie w obrębie canvas
                {
                    Line line = new Line
                    {
                        X1 = linePosition,
                        X2 = linePosition,
                        Y1 = 0,
                        Y2 = canvasHeight,
                        Stroke = Brushes.Green,
                        StrokeThickness = 2
                    };
                    waveformCanvas.Children.Add(line);
                }
                nextBeat += beatInterval;
            }

            // Dodanie wskaźnika czerwonego
            if (existingIndicator != null)
            {
                Canvas.SetLeft(existingIndicator, (currentPosition / totalDuration) * canvasWidth - existingIndicator.Width / 2);
                waveformCanvas.Children.Add(existingIndicator);
            }
            else
            {
                existingIndicator = new Rectangle
                {
                    Width = 2,
                    Height = canvasHeight,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(existingIndicator, (currentPosition / totalDuration) * canvasWidth - existingIndicator.Width / 2);
                waveformCanvas.Children.Add(existingIndicator);
                // Aktualizacja referencji wskaźnika
                if (whichOne == 1)
                    progressIndicator1 = existingIndicator;
                else
                    progressIndicator2 = existingIndicator;
            }
        }

        public void InitTimer(System.Windows.Forms.Timer timer, TextBlock textBLock,AudioFileReader reader, int whichOne)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                
                timer.Interval = 250;
                if (whichOne == 1)
                {
                    reader1 = reader;
                    actualTime1 = textBLock;
                    timer.Tick += new EventHandler(UpdateLabel1);
                }
                else
                {
                    reader2 = reader;

                    actualTime2 = textBLock;
                    timer.Tick += new EventHandler(UpdateLabel2);

                }
            });
            



        }

        private void UpdateLabel1(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (reader1 != null)
                {
                    TimeSpan currentTime = reader1.CurrentTime;
                    actualTime1.Text = currentTime.ToString(@"mm\:ss");

                }
            });
            
        }
        private void UpdateLabel2(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (reader2 != null)
                {
                    TimeSpan currentTime = reader2.CurrentTime;
                    actualTime2.Text = currentTime.ToString(@"mm\:ss");

                }
            });
            
        }
    }




}









