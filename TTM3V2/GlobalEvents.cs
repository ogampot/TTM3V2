using System;

namespace TTM3V2
{
    public static class GlobalEvents
    {
        public static Action<Tile> OnTileButtonClicked;

        public static void SendTileButtonClicked(Tile tile)
        {
            OnTileButtonClicked?.Invoke(tile);
        }
    }
}
