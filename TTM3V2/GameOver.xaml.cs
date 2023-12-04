using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TTM3V2
{
    public partial class GameOver : Window
    {
        public GameOver()
        {
            InitializeComponent();

            this.MainMenuButton.IsEnabled = false;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(1000)
            };

            animation.Completed += delegate
            {
                this.MainMenuButton.IsEnabled = true;
            };

            this.MainMenuButton.BeginAnimation(OpacityProperty, animation);

            this.MainMenuButton.Click += delegate
            {
                GlobalEvents.OpenMainMenu();
                this.Close();
            };
        }
    }
}
