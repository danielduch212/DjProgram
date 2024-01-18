using DjProgram1.Model.Services;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DjProgram1.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy knobMix.xaml
    /// </summary>
    public partial class KnobToCut : UserControl
    {
        private const int NumberOfDots = 10;
        private double centerX = 35;
        private double centerY = 35;
        public AudioFileReader reader;
        private double totalDuration;
        private double currentPosition = 0;


        private bool isKnobLocked = false;
        private bool isDragging = false;
        private Point lastMousePosition;
        private MusicService musicService = new MusicService();
        private Canvas waveFormCanvas;
        private Rectangle progressIndicator;

        private TimeSpan currentTime;

        List<double> audioSamples;
        List<double> timeStamps;
        List<double> beatIntervals;
        int whichOne;


        TextBlock textBLock;
        double beatThreshold = 0.5;



        public KnobToCut()
        {
            InitializeComponent();

            DrawDots();
        }

        public void Initialize(Canvas waveFormCanvas, Rectangle progressIndicator, TextBlock textBlock)
        {
            this.progressIndicator = progressIndicator;
            this.waveFormCanvas = waveFormCanvas;
            this.textBLock = textBlock;
        }

        public void addAtributes(AudioFileReader reader, List<double> audioSamples, List<double> timeStamps)
        {
            this.reader = reader;
            totalDuration = reader.TotalTime.TotalSeconds;
            this.audioSamples = audioSamples;
            this.timeStamps = timeStamps;
            beatIntervals = GenerateBeatIntervals(timeStamps, totalDuration);


        }
        public List<double> GenerateBeatIntervals(List<double> timeStamps, double totalDuration)
        {
            List<double> beatIntervals = new List<double>();
            double interval = musicService.GetInterval(timeStamps);
            if (interval > 0)
            {
                for (double beat = timeStamps[0]; beat <= totalDuration; beat += interval)
                {
                    beatIntervals.Add(beat);
                }
            }
            return beatIntervals;
        }

        private void UpdateWaveformPosition(double angleDifference)
        {
            double positionChange = (angleDifference / 500.0) * totalDuration;
            currentPosition += positionChange;
            currentPosition = Math.Clamp(currentPosition, 0, totalDuration);

            currentPosition = SnapToNearestBeat(currentPosition, beatIntervals, beatThreshold);

            double currentTimeInSeconds = currentPosition / totalDuration * reader.TotalTime.TotalSeconds;
            SetCurrentPosition(reader, currentTimeInSeconds);

            TimeSpan time = TimeSpan.FromSeconds(currentTimeInSeconds);
            textBLock.Text = time.ToString(@"mm\:ss");

            musicService.UpdateWaveformByKnob(currentPosition / totalDuration, totalDuration, waveFormCanvas, audioSamples, beatIntervals);
        }


        private double SnapToNearestBeat(double currentPosition, List<double> timeStamps, double threshold)
        {
            double nearestBeat = beatIntervals
                .OrderBy(beat => Math.Abs(currentPosition - beat))
                .FirstOrDefault();

            if (Math.Abs(currentPosition - nearestBeat) <= threshold)
            {
                return nearestBeat;
            }

            return currentPosition;
        }

        public void SetCurrentPosition(AudioFileReader reader, double currentTime)
        {
            if (reader != null && currentTime >= 0 && currentTime <= reader.TotalTime.TotalSeconds)
            {
                reader.CurrentTime = TimeSpan.FromSeconds(currentTime);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || isKnobLocked) return;

            Point currentMousePosition = e.GetPosition(this);
            double angleDifference = CalculateAngleDifference(lastMousePosition, currentMousePosition);

            if (angleDifference != 0)
            {
                UpdateWaveformPosition(angleDifference);
                RotateKnob(angleDifference);
                lastMousePosition = currentMousePosition;
            }
        }


        private void DrawDots()
        {
            double radius = 25;
            double centerX = 35;
            double centerY = 35;
            double angleStep = 360.0 / NumberOfDots;
            double lineLength = 7;
            double lineWidth = 2;

            DotsCanvas.Children.Clear();

            for (int i = 0; i < NumberOfDots; i++)
            {
                double angle = angleStep * i;
                double angleRad = (Math.PI / 180) * angle;


                double lineStartX = centerX + (radius - lineLength / 2) * Math.Cos(angleRad);
                double lineStartY = centerY + (radius - lineLength / 2) * Math.Sin(angleRad);
                double lineEndX = centerX + (radius + lineLength / 2) * Math.Cos(angleRad);
                double lineEndY = centerY + (radius + lineLength / 2) * Math.Sin(angleRad);

                Line line = new Line
                {
                    X1 = lineStartX,
                    Y1 = lineStartY,
                    X2 = lineEndX,
                    Y2 = lineEndY,
                    Stroke = Brushes.Red,
                    StrokeThickness = lineWidth
                };

                DotsCanvas.Children.Add(line);
            }
        }

        public double CalculateAngleDifference(Point previous, Point current)
        {
            Vector previousVector = new Vector(previous.X - centerX, previous.Y - centerY);
            Vector currentVector = new Vector(current.X - centerX, current.Y - centerY);

            double angleBetween = Vector.AngleBetween(previousVector, currentVector);

            //Console.WriteLine($"Angle difference: {angleBetween}"); 
            return angleBetween;
        }

        private void RotateKnob(double angle)
        {
            var transform = DotsCanvas.RenderTransform as RotateTransform;
            if (transform == null)
            {
                transform = new RotateTransform(0, centerX, centerY);
                DotsCanvas.RenderTransform = transform;
            }
            transform.Angle += angle;
        }



        public void LockKnobRotation()
        {
            isKnobLocked = true;
        }

        public void UnlockKnobRotation()
        {
            isKnobLocked = false;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isKnobLocked) return;

            isDragging = true;
            lastMousePosition = e.GetPosition(this);
            Mouse.Capture(DotsCanvas);
        }



        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isKnobLocked) return;

            isDragging = false;
            Mouse.Capture(null);
        }

        public void SetProgressIndicatorToStart()
        {
            currentPosition = 0;
        }
    }
}


