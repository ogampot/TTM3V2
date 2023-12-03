using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TTM3V2
{
    public class Timer
    {
        private Label timeLabel;

        private TimeSpan timeLeft = TimeSpan.Zero;

        private MainWindow gameWindow;

        public Timer(MainWindow window, Label label, TimeSpan timeLeft)
        {
            this.gameWindow = window;
            this.timeLabel = label;
            this.timeLeft = timeLeft;

            TimerUpdate();
        }

        private async void TimerUpdate()
        {
            await Task.Delay(1000);

            timeLabel.Content = "Time left: " + string.Format("{0:D2}:{1:D2}", timeLeft.Minutes, timeLeft.Seconds);

            if (timeLeft.Minutes > 0 || timeLeft.Seconds > 0)
            {
                timeLeft = timeLeft.Subtract(TimeSpan.FromSeconds(1));
                TimerUpdate();
            }
            else
            {
                OnGameOver();
            }
        }

        private void OnGameOver()
        {
            GlobalEvents.OpenGameOver();
            gameWindow.Close();
        }
    }
}
