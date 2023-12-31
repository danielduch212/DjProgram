using DjProgram1.Services;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DjProgram1.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy knobMix.xaml
    /// </summary>
    public partial class knobToCut : UserControl
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

        public knobToCut()
        {
            InitializeComponent();

            DrawDots();
        }

        public void Initialize(Canvas waveFormCanvas, Rectangle progressIndicator)
        {
            this.progressIndicator = progressIndicator;
            this.waveFormCanvas = waveFormCanvas;
        }

        public void InitializeReader(AudioFileReader reader)
        {
            this.reader = reader;
            totalDuration = reader.TotalTime.TotalSeconds;
        }
        private void UpdateWaveformPosition(double angleDifference)
        {
            double secondsPerDot = 1;
            double timeChange = angleDifference / 360.0 * secondsPerDot;
            currentPosition += timeChange;

            currentPosition = Math.Clamp(currentPosition, 0, totalDuration);
            waveFormCanvas.Children.Clear();

            progressIndicator = new Rectangle
            {
                Width = 2,
                Height = waveFormCanvas.Height,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(progressIndicator, currentPosition);
            Canvas.SetTop(progressIndicator, 0);
            waveFormCanvas.Children.Add(progressIndicator);


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

        private double RoundToNearestHalf(double number)
        {
            return Math.Round(number * 2, MidpointRounding.AwayFromZero) / 2;
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

        
    }
}


