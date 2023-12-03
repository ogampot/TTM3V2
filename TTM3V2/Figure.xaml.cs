using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TTM3V2
{
    public partial class Figure : UserControl
    {
        // Need to send which tile was clicked
        private Tile tileParent;


        private FigureType figureType;
        public FigureType FigureType { get { return figureType; } }


        private ImageSource specialImage = null;
        public ImageSource SpecialImage { get { return specialImage; } }

        private Action<Vector2> onClearEvent = null;
        public Action<Vector2> OnClearEvent { get { return onClearEvent; } }


        public Figure(int size, int borderThickness, Tile tileParent)
        {
            InitializeComponent();

            this.tileParent = tileParent;

            SetupFigureShape(size, borderThickness);
        }


        private void SetupFigureShape(int size, int borderThickness)
        {
            this.Width = size;
            this.Height = size;

            this.Tile.Height = size;
            this.Tile.Width = size;

            this.TileBody.Width = size;
            this.TileBody.Height = size;

            this.TileButton.Width = size;
            this.TileButton.Height = size;

            this.TileButton.BorderThickness = new System.Windows.Thickness(borderThickness);

            this.TileButton.Click += ButtonClick;

            this.TileImage.Width = size - 5;
            this.TileImage.Height = size - 5;
        }

        public void ApplyFigureType(FigureType figureType, ImageSource imageSource, Action<Vector2> action)
        {
            this.figureType = figureType;

            if(imageSource != null)
            {
                specialImage = imageSource;

                this.TileImage.Source = imageSource;
                this.TileBack.Fill = figureType.Color;
            }
            else
            {
                specialImage = null;

                this.TileImage.Source = this.figureType.Image;
                this.TileBack.Fill = Brushes.Transparent;
            }

            if (action != null) onClearEvent = action;
            else onClearEvent = null;

            if (figureType == null) this.TileImage.Source = null;
        }

        public void SetButtonStyle(Style style)
        {
            this.TileButton.Style = style;
        }


        // On tile click event
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            GlobalEvents.SendTileButtonClicked(tileParent);
        }
    }
}
