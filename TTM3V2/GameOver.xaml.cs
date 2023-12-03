using System.Windows;

namespace TTM3V2
{
    public partial class GameOver : Window
    {
        public GameOver()
        {
            InitializeComponent();

            this.MainMenuButton.Click += delegate
            {
                GlobalEvents.OpenGame();
                this.Close();
            };
        }
    }
}
