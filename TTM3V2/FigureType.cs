using System;
using System.Windows.Media;

namespace TTM3V2
{
    public class FigureType
    {
        private Brush color = Brushes.Black;
        private ImageSource image = null;

        public Brush Color { get { return color; } }
        public ImageSource Image { get { return image; } }

        public FigureType(Brush color, ImageSource image)
        {
            this.color = color;
            this.image = image;
        }
    }
}