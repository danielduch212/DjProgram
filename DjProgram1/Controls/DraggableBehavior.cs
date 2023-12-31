using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace DjProgram1.Controls
{
    internal class DraggableBehavior
    {
        private UIElement element;
        private Canvas canvas;
        private bool isDragging;
        private Point clickPosition;

        public DraggableBehavior(UIElement element, Canvas canvas)
        {
            this.element = element;
            this.canvas = canvas;
            AttachEvents();
        }

        private void AttachEvents()
        {
            element.MouseDown += OnMouseDown;
            element.MouseMove += OnMouseMove;
            element.MouseUp += OnMouseUp;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition(canvas);
            element.CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(canvas);
                var transform = element.RenderTransform as TranslateTransform ?? new TranslateTransform();

                // Ograniczenie ruchu tylko do osi poziomej poprzez aktualizację transformacji X
                transform.X += currentPosition.X - clickPosition.X;

                // Resetujemy pozycję Y, aby element nie przesuwał się pionowo
                transform.Y = 0;

                element.RenderTransform = transform;

                // Aktualizujemy pozycję kliknięcia, ale tylko współrzędną X
                clickPosition = new Point(currentPosition.X, clickPosition.Y);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            element.ReleaseMouseCapture();
        }



    }
}
