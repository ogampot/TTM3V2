using System;
using System.Windows;

namespace TTM3V2
{
    public static class GlobalEvents
    {
        public static Action<Tile> OnTileButtonClicked;
        public static Action<int> OnTileClean;

        public static void SendTileButtonClicked(Tile tile)
        {
            OnTileButtonClicked?.Invoke(tile);
        }

        public static void SendTileClean(int n)
        {
            OnTileClean?.Invoke(n);
        }

        // Windows switch
        public static void OpenGame()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        public static void OpenGameOver()
        {
            GameOver gameOver = new GameOver();
            gameOver.Show();
        }

        public static void OpenMainMenu()
        {
            MainMenu mainMenu = new MainMenu();
            mainMenu.Show();
        }
    }
}
