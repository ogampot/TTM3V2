using System.Windows;

namespace TTM3V2
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();

            this.PlayButton.Click += delegate
            {
                GlobalEvents.OpenGame();
                this.Close();
            };
        }
    }
}
