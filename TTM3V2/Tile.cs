using System.Windows.Controls;
using System.Windows.Media.Animation;
using System;
using System.Windows.Media;

namespace TTM3V2
{
    public class Tile
    {
        public enum TileStatus { Full, Empty, NeedToBeClean };


        private TileStatus status = TileStatus.Empty;
        public TileStatus Status { get { return status; } }


        private Vector2 position;
        public Vector2 Position { get { return position; } }


        private Figure figure;
        public Figure Figure { get { return figure; } }


        private int movedByPlayerCount = 0;
        public int MovedByPlayerCount { get { return movedByPlayerCount; } }


        public Tile(Vector2 position, int size, int borderThickness)
        {
            this.position = position;
            this.figure = new Figure(size, borderThickness, this);
        }


        public void SetNewPosition(Vector2 position)
        {
            this.position = position;
        }

        public void SetFigureType(FigureType figureType, ImageSource imageSource, Action<Vector2> action)
        {
            if (figureType == null) ChangeStatus(TileStatus.Empty);
            else ChangeStatus(TileStatus.Full);

            figure.ApplyFigureType(figureType, imageSource, action);
        }

        public void ChangeStatus(TileStatus status)
        {
            this.status = status;
        }

        // When a tile has been marked as "need to be clean"
        public void Clean()
        {
            ChangeStatus(TileStatus.Empty);
            figure.ApplyFigureType(null, null, null);
        }

        public void AddMoveCount()
        {
            movedByPlayerCount++;
        }

        public void ResetMoveCount()
        {
            movedByPlayerCount = 0;
        }
    }
}
