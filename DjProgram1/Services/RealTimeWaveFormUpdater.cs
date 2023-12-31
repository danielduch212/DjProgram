using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DjProgram1.Services
{
    

    public class RealtimeWaveformUpdater
    {
        private Canvas waveformCanvas;
        private double[] audioSamples;
        private int currentPosition;


        public RealtimeWaveformUpdater(Canvas canvas, double[] samples)
        {
            waveformCanvas = canvas;
            audioSamples = samples;
            currentPosition = 0;
        }

        public void Start()
        {
            waveformCanvas.Children.Clear();
            currentPosition = 0;

            double canvasWidth = waveformCanvas.ActualWidth;
            double canvasHeight = waveformCanvas.ActualHeight;

            int numSamples = audioSamples.Length;
            int stepSize = (int)(numSamples / canvasWidth);

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = canvasWidth;
            animation.Duration = TimeSpan.FromSeconds(numSamples / (double)stepSize);
            animation.Completed += Animation_Completed;

            Line line = new Line();
            line.X1 = 0;
            line.Y1 = canvasHeight / 2;
            line.X2 = 0;
            line.Y2 = canvasHeight / 2;
            line.Stroke = Brushes.Blue;
            line.StrokeThickness = 1;
            line.BeginAnimation(Line.X2Property, animation);

            waveformCanvas.Children.Add(line);
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            waveformCanvas.Children.Clear();
        }
    }
}
