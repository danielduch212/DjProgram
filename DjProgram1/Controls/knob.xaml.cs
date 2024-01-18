using DjProgram1.Model.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DjProgram1.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy knob.xaml
    /// </summary>
    public partial class Knob : UserControl
    {
        private const int NumberOfDots = 10;
        private bool isDragging = false;
        private Point lastMousePosition;
        private double centerX = 35;
        private double centerY = 35;
        private bool isKnobLocked = false;

        public TextBlock bpmTextBox { get; set; }

        private MusicService musicService = new MusicService();
        double bpmValue = 0;

        public Knob()
        {
            InitializeComponent();

            DrawDots();
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isKnobLocked) return;

            isDragging = true;
            lastMousePosition = e.GetPosition(this);
            Mouse.Capture(DotsCanvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || isKnobLocked) return;

            Point currentMousePosition = e.GetPosition(this);
            double angleDifference = CalculateAngleDifference(lastMousePosition, currentMousePosition);

            if (angleDifference != 0)
            {
                UpdateBPM(angleDifference);
                lastMousePosition = currentMousePosition;
                RotateKnob(angleDifference);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isKnobLocked) return;

            isDragging = false;
            Mouse.Capture(null);
        }

        private void UpdateBPM(double angleDifference)
        {
            string textBPM = bpmTextBox.Text;
            textBPM = textBPM.Replace("BPM: ", "").Trim();
            if (!double.TryParse(textBPM, out double currentBPM))
            {
                return;
            }

            currentBPM = RoundToNearestHalf(currentBPM);

            if (angleDifference > 0)
            {
                currentBPM += 0.5;
            }
            else if (angleDifference < 0)
            {
                currentBPM -= 0.5;
            }

            currentBPM = Math.Clamp(currentBPM, 40, 200);

            bpmTextBox.Text = "BPM: " + currentBPM.ToString("N2");
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

    }

}
