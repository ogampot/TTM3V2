using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TTM3V2
{
    public class ScoreCounter
    {
        private Label scoreLabel;

        private int score = 0;
        public int Score { get { return score; } }

        public ScoreCounter(Label label)
        {
            GlobalEvents.OnTileClean += AddScore;

            scoreLabel = label;
            UpdateLabelText();
        }

        private void AddScore(int n)
        {
            score += n;
            UpdateLabelText();
        }

        private void UpdateLabelText()
        {
            scoreLabel.Content = "Score: " + score.ToString();
        }
    }
}
