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







namespace DjProgram1.Services
{
    public class MusicService
    {
        private DispatcherTimer timer;
        private int currentPosition; // Aktualna pozycja w próbkach audio
        List<DjProgram1.Data.AudioFile> audioFiles;

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
        public void GenerateWaveform(string filePath, Canvas waveformCanvas)
        {
            List<double> audioSamples = new List<double>();

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
                            audioSamples.Add(buffer[i * reader.WaveFormat.Channels + channel]);
                        }
                    }
                }
            }
            //audioSamples.ToArray();

            Application.Current.Dispatcher.Invoke(() =>
            {
                waveformCanvas.Children.Clear();

                double canvasWidth = waveformCanvas.ActualWidth; // Użyj ActualWidth i ActualHeight
                double canvasHeight = waveformCanvas.ActualHeight;

                int numSamples = audioSamples.Count;
                int stepSize = (int)(numSamples / canvasWidth);

                Polyline polyline = new Polyline();
                polyline.Stroke = Brushes.Blue;
                polyline.StrokeThickness = 1;

                for (int i = 0; i < numSamples; i += stepSize)
                {
                    double sample = audioSamples[i];
                    double x = i * (canvasWidth / numSamples);
                    double y = (sample + 1) * (canvasHeight / 2);

                    polyline.Points.Add(new Point(x, y));
                }

                waveformCanvas.Children.Add(polyline);
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

        //ulepszone liczenie bpm

        private const int MinBpm = 50;
        private const int MaxBpm = 200;

        public double GetTempoFromWavFile(string filePath)
        {
            using (var waveStream = new WaveFileReader(filePath))
            {
                int sampleRate = waveStream.WaveFormat.SampleRate;
                double[] audioSamples = ExtractAudioSamples(waveStream);

                double[] windowedSamples = ApplyHanningWindow(audioSamples);

                double[] autocorrelation = ComputeAutocorrelation(windowedSamples);

                List<int> peakIndices = FindPeakIndices(autocorrelation);

                double bpm = peakIndices.Select(index => 60.0 * sampleRate / index).Where(bpm => bpm >= MinBpm && bpm <= MaxBpm).FirstOrDefault();

                return bpm;
            }
        }

        private double[] ApplyHanningWindow(double[] samples)
        {
            int n = samples.Length;
            double[] windowedSamples = new double[n];

            for (int i = 0; i < n; i++)
            {
                windowedSamples[i] = samples[i] * (0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (n - 1)));
            }

            return windowedSamples;
        }


        private double[] ExtractAudioSamples(WaveStream waveStream)
        {
            int bytesPerSample = waveStream.WaveFormat.BitsPerSample / 8;
            int numChannels = waveStream.WaveFormat.Channels;

            // Liczba bajtów na komplet próbek (wszystkie kanały)
            int bytesPerCompleteSample = bytesPerSample * numChannels;

            int totalSamples = (int)(waveStream.Length / bytesPerCompleteSample);
            double[] audioSamples = new double[totalSamples];
            byte[] buffer = new byte[bytesPerCompleteSample];

            for (int i = 0; i < totalSamples; i++)
            {
                waveStream.Read(buffer, 0, bytesPerCompleteSample);

                audioSamples[i] = BitConverter.ToInt16(buffer, 0) / 32768.0;
            }

            return audioSamples;
        }

        private double[] ComputeAutocorrelation(double[] samples)
        {
            int n = samples.Length;
            double[] result = new double[n];

            for (int lag = 0; lag < n; lag++)
            {
                for (int i = 0; i < n - lag; i++)
                {
                    result[lag] += samples[i] * samples[i + lag];
                }
            }

            return result;
        }

        private List<int> FindPeakIndices(double[] autocorrelation)
        {
            List<int> peakIndices = new List<int>();
            for (int i = 1; i < autocorrelation.Length - 1; i++)
            {
                if (autocorrelation[i] > autocorrelation[i - 1] && autocorrelation[i] > autocorrelation[i + 1])
                {
                    // Interpolacja
                    double alpha = autocorrelation[i - 1];
                    double beta = autocorrelation[i];
                    double gamma = autocorrelation[i + 1];
                    double interpolatedIndex = i + 0.5 * (alpha - gamma) / (alpha - 2 * beta + gamma);
                    peakIndices.Add((int)Math.Round(interpolatedIndex));
                }
            }

            return peakIndices;
        }




        public string calculateBPMPython1(string filePath)
        {
            double bpm = 0;

            try
            {
                Runtime.PythonDLL = @"C:\Program Files\Python311\python311.dll";

                PythonEngine.Initialize();
                using (Py.GIL()) // acquire the Python GIL (Global Interpreter Lock)
                {
                    dynamic librosa = Py.Import("librosa");
                    dynamic beat = Py.Import("librosa.beat");

                    // Load the audio file
                    PyObject[] loadArgs = { new PyString(filePath) };
                    using (var loadKwargs = new PyDict())
                    {
                        loadKwargs["sr"] = new PyInt(22050); // sample rate
                        using (PyObject y_sr = librosa.InvokeMethod("load", loadArgs, loadKwargs))
                        {
                            PyObject y = y_sr[0];
                            PyObject sr = y_sr[1];

                            // Call the beat_track function
                            PyObject[] beatTrackArgs = new PyObject[] { y, sr };
                            PyObject tempo_beats = beat.GetAttr("beat_track").Invoke(beatTrackArgs);
                            PyObject tempo = tempo_beats[0];
                            bpm = tempo.As<double>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = "An error occurred: " + ex.Message;
                return message;
            }
            finally
            {
                PythonEngine.Shutdown();
            }

            return bpm.ToString();
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

        public double ChangeBPM(double angleDelta, string BPM)
        {
            int change = (int)Math.Round(angleDelta / 10); 
            double newBPM = double.Parse(BPM);

            if (angleDelta > 0)
            {
                newBPM += change;
            }
            else if (angleDelta < 0)
            {
                newBPM -= change;
                newBPM = Math.Max(0, newBPM); 
            }
            return newBPM;
        }

    }




}




//TODO
// nie sprawdzac bpm
// dodaj przycisk do synchronizacji
// dodac analogowy suwak do synchronizacji
// 12.09 NAJNOWSZE INFO:
// 1. na razie zrobic funkcjie do liczenia bpm i zrobic to na watkach zeby sprawdzic czy to by smigalo




