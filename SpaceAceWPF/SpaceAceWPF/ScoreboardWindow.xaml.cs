using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for ScoreboardWindow.xaml
    /// </summary>
    public partial class ScoreboardWindow : Window
    {
        enum opt { returnStart, clearScores, confirmNo, confirmYes };
        opt curOpt = opt.returnStart;

        public class ScoreboardContext
        {   
            private string[] _names;
            private string[] _scores;
            private string path;

            public string[] names
            {
                get { return _names; }
            }

            public string[] scores
            {
                get { return _scores; }
            }

            public ScoreboardContext()
            {
                _names = new string[10];
                _scores = new string[10];
                path = String.Format("{0}..\\..\\Assets\\scores.txt", System.AppDomain.CurrentDomain.BaseDirectory);
                readInScoreboard();
            }

            private void readInScoreboard()
            {
                string defaultEntry = "-";
                for (int i = 0; i < 10; ++i)
                {
                    _names[i] = defaultEntry;
                    _scores[i] = defaultEntry;
                }

                if (!System.IO.File.Exists(path))
                {
                    string defaultText = "";
                    System.IO.File.WriteAllText(path, defaultText);
                }
                else
                {
                    string readText = System.IO.File.ReadAllText(path);

                    string delimStr = "\r\n\t";
                    char[] delimiter = delimStr.ToCharArray();
                    int maxSubstr = 20;
                    string[] split = readText.Split(delimiter, maxSubstr);

                    for (int i = 0; i < split.Length / 2; ++i)
                    {
                        _names[i] = Regex.Replace(split[2 * i], @"\t|\n|\r", "");
                        _scores[i] = Regex.Replace(split[(2 * i) + 1], @"\t|\n|\r", "");
                    }
                }
            }

            public void resetBoard()
            {
                string defaultText = "";
                System.IO.File.WriteAllText(path, defaultText);
            }
        }

        ScoreboardContext scoreboardContext = new ScoreboardContext();

        public ScoreboardWindow()
        {
            this.Cursor = Cursors.None;
            InitializeComponent();
            App.checkForJoy();
            App.joyDown += new EventHandler<JoyDownArgs>(score_joyDown);
            updateFont(opt.returnStart);
            DataContext = this.scoreboardContext;
        }

        private void updateFont(opt nextOpt)
        {
            switch (curOpt)
            {
                case opt.returnStart:
                    this.returnStart.Foreground = Brushes.White;
                    break;
                case opt.clearScores:
                    this.clearScores.Foreground = Brushes.White;
                    break;
                case opt.confirmNo:
                    this.confirmNo.Foreground = Brushes.White;
                    break;
                case opt.confirmYes:
                    this.confirmYes.Foreground = Brushes.White;
                    break;
            }

            switch (nextOpt)
            {
                case opt.returnStart:
                    this.returnStart.Foreground = Brushes.Yellow;
                    break;
                case opt.clearScores:
                    this.clearScores.Foreground = Brushes.Yellow;
                    break;
                case opt.confirmNo:
                    this.confirmNo.Foreground = Brushes.Yellow;
                    break;
                case opt.confirmYes:
                    this.confirmYes.Foreground = Brushes.Yellow;
                    break;
            }

            curOpt = nextOpt;
        }

        private void score_keyDown(object sender, KeyEventArgs e)
        {
            scoreboard_inputEvent(InputType.wasd, e.Key);
        }

        private void score_joyDown(object sender, JoyDownArgs e)
        {
            scoreboard_inputEvent(InputType.joy, e.Key);
        }

        private void scoreboard_inputEvent(InputType inType, Key key)
        {
            if (App.menuDelay != 0)
                return;

            switch (key)
            {
                case Key.Left:
                case Key.A:
                case Key.Right:
                case Key.D:
                    switch (curOpt)
                    {
                        case opt.returnStart:
                            updateFont(opt.clearScores);
                            break;
                        case opt.clearScores:
                            updateFont(opt.returnStart);
                            break;
                        case opt.confirmNo:
                            updateFont(opt.confirmYes);
                            break;
                        case opt.confirmYes:
                            updateFont(opt.confirmNo);
                            break;
                    }
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Space:
                case Key.Enter:
                    selectOpt();
                    break;
            }
        }

        private void selectOpt()
        {
            switch (curOpt)
            {
                case opt.returnStart:
                    StartWindow start = new StartWindow();
                    App.Current.MainWindow = start;
                    start.Show();
                    this.Close();
                    break;
                case opt.clearScores:
                    this.confirmNo.Visibility = Visibility.Visible;
                    this.confirmYes.Visibility = Visibility.Visible;
                    this.confirmAsk.Visibility = Visibility.Visible;
                    this.returnStart.Visibility = Visibility.Collapsed;
                    this.clearScores.Visibility = Visibility.Collapsed;
                    updateFont(opt.confirmNo);
                    break;
                case opt.confirmYes:
                    scoreboardContext.resetBoard();
                    ScoreboardWindow score = new ScoreboardWindow();
                    App.Current.MainWindow = score;
                    score.Show();
                    this.Close();
                    break;
                case opt.confirmNo:
                    this.confirmNo.Visibility = Visibility.Collapsed;
                    this.confirmYes.Visibility = Visibility.Collapsed;
                    this.confirmAsk.Visibility = Visibility.Collapsed;
                    this.returnStart.Visibility = Visibility.Visible;
                    this.clearScores.Visibility = Visibility.Visible;
                    updateFont(opt.clearScores);
                    break;
            }
        }
    }
}
