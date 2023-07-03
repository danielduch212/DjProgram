using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisioForge.MediaFramework.NAudio.VisioForge;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundTouch;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using NAudio.SoundTouch;


namespace DjProgram1.Services
{
    class MusicService
    {
        public double GetBpm(string filePath)
        {
            using (var audioFileReader = new NAudio.Wave.AudioFileReader(filePath))
            {
                var soundTouch = new SoundTouch.BpmDetect(audioFileReader.WaveFormat.SampleRate, audioFileReader.WaveFormat.Channels);

                int blockSize = 8192;
                var buffer = new float[blockSize];
                int samplesRead;

                do
                {
                    samplesRead = audioFileReader.Read(buffer, 0, blockSize);
                    soundTouch.InputSamples(buffer, samplesRead);
                    
                } while (samplesRead > 0);

                
                double bpm = soundTouch.GetBpm();

                return bpm;
            }
        }

        public void createWaveform(Canvas canvas, string filePath)
        {
            NAudio.Wave.WaveStream waveStream = new NAudio.Wave.WaveFileReader(filePath);
            int sampleRate = waveStream.WaveFormat.SampleRate;

            const int blockSize = 512; // Rozmiar bloku przetwarzania
            int blockCount = (int)Math.Ceiling((double)waveStream.Length / blockSize);

            // Inicjalizacja tablic przechowujących dane audio i wizualizację waveform
            byte[] audioData = new byte[blockSize * blockCount];
            float[] audioSamples = new float[audioData.Length / 2];
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            canvas.Children.Clear();

            // Wczytanie i przetwarzanie danych audio blok po bloku
            for (int blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                int bytesRead = waveStream.Read(audioData, blockIndex * blockSize, blockSize);
                int sampleCount = bytesRead / 2;

                for (int i = 0; i < sampleCount; i++)
                {
                    short sampleValue = BitConverter.ToInt16(audioData, (blockIndex * blockSize) + (i * 2));
                    audioSamples[(blockIndex * sampleCount) + i] = sampleValue / 32768f;
                }

                // Generowanie wizualizacji waveform
                for (int i = 0; i < audioSamples.Length; i++)
                {
                    double x = canvasWidth * i / audioSamples.Length;
                    double y = canvasHeight * (1 - (audioSamples[i] + 1) / 2);
                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = canvasHeight / 2,
                        X2 = x,
                        Y2 = y,
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(line);
                }
            }

            // Zamknięcie strumienia audio
            waveStream.Close();
        }
    }
}

