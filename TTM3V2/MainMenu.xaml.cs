using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TTM3V2
{
    public partial class MainMenu : Window
    {
        public MainMenu()
        {
            InitializeComponent();

            this.PlayButton.IsEnabled = false;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(1000)
            };

            animation.Completed += delegate
            {
                this.PlayButton.IsEnabled = true;
            };

            this.PlayButton.BeginAnimation(OpacityProperty, animation);

            this.PlayButton.Click += delegate
            {
                GlobalEvents.OpenGame();
                this.Close();
            };
        }
    }
}
