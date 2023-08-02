using System;
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

namespace DjProgram1.Services
{
    class MusicService
    {
        private DispatcherTimer timer;
        private int currentPosition; // Aktualna pozycja w próbkach audio

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

        


    }
        
}
//TODO
// nie sprawdzac bpm
// dodaj przycisk do synchronizacji
// dodac analogowy suwak do synchronizacji
// dodac geenrowanie prostego wave formu 
// poprawic gui by bylo nowoczesne
// bardzo wazne - zrobic tak zeby dzialalo dla wave i dla mp3

