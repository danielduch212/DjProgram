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

namespace DjProgram1.Services
{
    public class MusicService
    {
        private DispatcherTimer timer;
        private int currentPosition; // Aktualna pozycja w próbkach audio
        List<DjProgram1.Data.AudioFile> audioFiles;
        public void GenerateWaveform(double[] audioSamples, Canvas waveformCanvas)
        {
            waveformCanvas.Children.Clear();

            double canvasWidth = waveformCanvas.Width;
            double canvasHeight = waveformCanvas.Height;

            int numSamples = audioSamples.Length;
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


        
        //liczenie bpm
        public double GetTempo(double[] audioSamples, double sampleRate)
        {
            int windowSize = 1024; // Rozmiar okna analizy
            int hopSize = 512; // Przesunięcie okna

            double[] autocorrelation = ComputeAutocorrelation(audioSamples, windowSize, hopSize);

            int peakIndex = FindPeakIndex(autocorrelation, sampleRate);

            double tempo = 60.0 / (peakIndex / sampleRate);

            return tempo;
        }

        private double[] ComputeAutocorrelation(double[] samples, int windowSize, int hopSize)
        {
            int numWindows = (samples.Length - windowSize) / hopSize + 1;
            double[] autocorrelation = new double[numWindows];

            for (int i = 0; i < numWindows; i++)
            {
                double[] window = new double[windowSize];
                Array.Copy(samples, i * hopSize, window, 0, windowSize);

                autocorrelation[i] = ComputeAutocorrelationValue(window);
            }

            return autocorrelation;
        }

        private double ComputeAutocorrelationValue(double[] window)
        {
            int windowSize = window.Length;
            double sum = 0.0;

            for (int lag = 0; lag < windowSize; lag++)
            {
                for (int i = 0; i < windowSize - lag; i++)
                {
                    sum += window[i] * window[i + lag];
                }
            }

            return sum;
        }

        private int FindPeakIndex(double[] autocorrelation, double sampleRate)
        {
            double maxAutocorrelation = 0.0;
            int peakIndex = 0;

            for (int i = 0; i < autocorrelation.Length; i++)
            {
                if (autocorrelation[i] > maxAutocorrelation)
                {
                    maxAutocorrelation = autocorrelation[i];
                    peakIndex = i;
                }
            }

            return peakIndex;
        }
        public double GetTempoFromWavFile(string filePath)
        {
            // Otwieranie pliku WAV
            using (var waveStream = new WaveFileReader(filePath))
            {
                int sampleRate = waveStream.WaveFormat.SampleRate;
                int desiredDurationInSeconds = 30; // Pożądana długość próbki w sekundach

                // Obliczanie liczby próbek na podstawie pożądanej długości próbki
                int numSamples = sampleRate * desiredDurationInSeconds;

                // Przesunięcie początkowe w sekundach
                int startPositionInSeconds = (int)(waveStream.TotalTime.TotalSeconds / 2 - desiredDurationInSeconds / 2);

                // Przesunięcie początkowe w próbkach
                int startPosition = sampleRate * startPositionInSeconds;

                // Utworzenie bufora na próbki
                double[] audioSamples = new double[numSamples];

                // Przesunięcie odczytu do pozycji początkowej
                waveStream.Position = startPosition * 2;

                // Odczyt próbek audio
                var buffer = new byte[numSamples * 2];
                int bytesRead = waveStream.Read(buffer, 0, numSamples * 2);

                // Konwersja bajtów na wartości audio
                for (int i = 0; i < bytesRead; i += 2)
                {
                    short sample = (short)((buffer[i + 1] << 8) | buffer[i]);
                    audioSamples[i / 2] = sample / 32768.0; // Normalizacja wartości audio do zakresu [-1, 1]
                }

                // Obliczanie tempa
                return GetTempo(audioSamples, sampleRate);
            }
        }

        public double GetTempoFromMp3File(string filePath)
        {
            // Otwieranie pliku WAV
            using (var waveStream = new WaveFileReader(filePath))
            {
                int sampleRate = waveStream.WaveFormat.SampleRate;
                int desiredDurationInSeconds = 30; // Pożądana długość próbki w sekundach

                // Obliczanie liczby próbek na podstawie pożądanej długości próbki
                int numSamples = sampleRate * desiredDurationInSeconds;

                // Przesunięcie początkowe w sekundach
                int startPositionInSeconds = (int)(waveStream.TotalTime.TotalSeconds / 2 - desiredDurationInSeconds / 2);

                // Przesunięcie początkowe w próbkach
                int startPosition = sampleRate * startPositionInSeconds;

                // Utworzenie bufora na próbki
                double[] audioSamples = new double[numSamples];

                // Przesunięcie odczytu do pozycji początkowej
                waveStream.Position = startPosition * 2;

                // Odczyt próbek audio
                var buffer = new byte[numSamples * 2];
                int bytesRead = waveStream.Read(buffer, 0, numSamples * 2);

                // Konwersja bajtów na wartości audio
                for (int i = 0; i < bytesRead; i += 2)
                {
                    short sample = (short)((buffer[i + 1] << 8) | buffer[i]);
                    audioSamples[i / 2] = sample / 32768.0; // Normalizacja wartości audio do zakresu [-1, 1]
                }

                // Obliczanie tempa
                return GetTempo(audioSamples, sampleRate);
            }
        }

        public void animatePhoto(RotateTransform rotateTransform, double lastAngle)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = lastAngle;
            animation.To = lastAngle + 360;
            animation.Duration = TimeSpan.FromSeconds(5);
            animation.RepeatBehavior = RepeatBehavior.Forever;

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        public void StopRotation(RotateTransform rotateTransform, double lastAngle)
        {
            lastAngle = rotateTransform.Angle;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }
    }

    
        
}
//TODO
// nie sprawdzac bpm
// dodaj przycisk do synchronizacji
// dodac analogowy suwak do synchronizacji
// 12.09 NAJNOWSZE INFO:
// 1. na razie zrobic funkcjie do liczenia bpm i zrobic to na watkach zeby sprawdzic czy to by smigalo




