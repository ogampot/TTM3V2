using System;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TTM3V2
{
    public partial class Figure : UserControl
    {
        private FigureType figureType;
        public FigureType FigureType { get { return figureType; } }

        private Tile tileParent;

        public Figure(int size, int borderThickness, Tile tileParent)
        {
            InitializeComponent();

            this.tileParent = tileParent;

            CreateTileShape(size, borderThickness);
        }

        private void CreateTileShape(int size, int borderThickness)
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

            this.TileImage.Width = size - 15;
            this.TileImage.Height = size - 15;
        }

        public void ApplyFigureType(FigureType figureType)
        {
            this.figureType = figureType;

            if (figureType == null) this.TileImage.Source = null;
            else this.TileImage.Source = this.figureType.Image;
        }

        public void ApplyFigureTypeWithoutImage(FigureType figureType)
        {
            this.figureType = figureType;
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            GlobalEvents.SendTileButtonClicked(tileParent);
        }

        public void SetButtonStyle(Style style)
        {
            this.TileButton.Style = style;
        }
    }
}
