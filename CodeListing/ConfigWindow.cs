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

namespace NoughtsCrossesWPF
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        GameInfo Info;

        public ConfigWindow(GameInfo InfoT)
        {
            Info = InfoT;
            InitializeComponent();
        }

        //---------------------------------------------------------------------------------Util Stuff
        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            Window main = new MainWindow(Info);
            Utils.NewWindow(this, main, Info);
        }
        private void ButtonHover(object sender, MouseEventArgs e) //button highlighting
        {
            Rectangle button = sender as Rectangle;
            Utils.ButtonHover(button, Info.SoundPlayers[2], "i");
        }
        private void ButtonLeave(object sender, MouseEventArgs e)
        {
            Rectangle button = sender as Rectangle;
            Utils.ButtonHover(button, Info.SoundPlayers[2], "");
        }
        private void FullScreenClick(object sender, MouseButtonEventArgs e)
        {
            Utils.FullScreen(NCFullScreen, this, Info.SoundPlayers[1]);
        }
        private void CheckState(object sender, DependencyPropertyChangedEventArgs e)
        {
            Utils.CheckState(NCFullScreen, this); //ensures fullscreen button is correct
        }

        //------------------------------------------------------------------------------Other Stuff

        private void FormLoad(object sender, EventArgs e)
        {
            updwnColumn.Value = Info.GameSettings[0]; //sets number boxes
            updwnRow.Value = Info.GameSettings[5];
            updwnWin.Value = Info.GameSettings[1];
            updwnTimer.Value = Info.GameSettings[2];

            Rectangle[] CheckBoxes = new Rectangle[] { NCUnCheckBox0, NCUnCheckBox1, NCUnCheckBox2, NCUnCheckBox3, NCUnCheckBox4 }; //sets checkboxes
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/NCCheckbox.png")));
            for (int i = 0; i < Info.GameRules.Length; i++)
            {
                if (Info.GameRules[i] == true)
                {
                    CheckBoxes[i].Name = "NCCheckBox" + i;
                    CheckBoxes[i].Fill = b;
                    if(i == 3) //ai options visible if ai
                    {
                        AiSlider1.Visibility = Visibility.Visible;
                        rectAI1Randomness.Visibility = Visibility.Visible;
                    }
                    if (i == 4)
                    {
                        AiSlider2.Visibility = Visibility.Visible;
                        rectAI2Randomness.Visibility = Visibility.Visible;
                    }
                }
            }

            AiSlider1.Value = Info.GameSettings[3]; //sets ai slider
            AiSlider2.Value = Info.GameSettings[4];
        }

        private void CheckBoxHover(object sender, MouseEventArgs e)
        {
            Rectangle button = sender as Rectangle;
            Utils.ButtonHover(button, Info.SoundPlayers[2], "i", true);
        }

        private void CheckBoxLeave(object sender, MouseEventArgs e)
        {
            Rectangle button = sender as Rectangle;
            Utils.ButtonHover(button, Info.SoundPlayers[2], "", true);
        }

        private void GridColumnChange(object sender, RoutedPropertyChangedEventArgs<object> e) //number boxes changed
        {
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
            Info.GameSettings[0] = Convert.ToInt16(updwnColumn.Value);
            if (updwnColumn.Value < updwnRow.Value) updwnWin.Maximum = updwnColumn.Value;
            if (updwnWin.Value > updwnColumn.Value) updwnWin.Value = updwnColumn.Value;
        }

        private void GridRowChange(object sender, RoutedPropertyChangedEventArgs<object> e) //number boxes changed
        {
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
            Info.GameSettings[5] = Convert.ToInt16(updwnRow.Value);
            if (updwnRow.Value < updwnColumn.Value) updwnWin.Maximum = updwnRow.Value;
            if (updwnWin.Value > updwnRow.Value) updwnWin.Value = updwnRow.Value;
        }

        private void WinValueChange(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
            Info.GameSettings[1] = Convert.ToInt16(updwnWin.Value);
        }
        private void TimerValueChange(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
            Info.GameSettings[2] = Convert.ToInt16(updwnTimer.Value);
        }

        private void BoxCheck(object sender, MouseButtonEventArgs e) //for checkboxes
        {
            Rectangle Box = sender as Rectangle;
            char BoxNum = Box.Name[Box.Name.Length - 1];
            Info.GameRules[BoxNum-'0'] = !Info.GameRules[BoxNum-'0']; //char to int
            if (Info.GameRules[BoxNum-'0'] == true) Box.Name = "NCCheckbox" + BoxNum;
            else Box.Name = "NCUnCheckbox" + BoxNum;
            Utils.ButtonHover(Box, Info.SoundPlayers[2], "", true);
            Utils.PlaySound("Click", Info.SoundPlayers[1]);

            if (Info.GameRules[3])
            {
                AiSlider1.Visibility = Visibility.Visible;
                rectAI1Randomness.Visibility = Visibility.Visible;
            }
            else
            {
                AiSlider1.Visibility = Visibility.Hidden;
                rectAI1Randomness.Visibility = Visibility.Hidden;
            }

            if (Info.GameRules[4])
            {
                AiSlider2.Visibility = Visibility.Visible;
                rectAI2Randomness.Visibility = Visibility.Visible;
            }
            else
            {
                AiSlider2.Visibility = Visibility.Hidden;
                rectAI2Randomness.Visibility = Visibility.Hidden;
            }
        }

        private void ResizeText(object sender, SizeChangedEventArgs e) //resizes text
        {
            double Multiplier = this.ActualWidth / 640;
            updwnColumn.FontSize = 18 * Multiplier;
            updwnRow.FontSize = 18 * Multiplier;
            updwnWin.FontSize = 18 * Multiplier;
            updwnTimer.FontSize = 18 * Multiplier;
        }

        private void Slider1Change(object sender, RoutedPropertyChangedEventArgs<double> e) //affects a1 diff
        {
            Info.GameSettings[3] = Convert.ToInt16(AiSlider1.Value);
        }

        private void Slider2Change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Info.GameSettings[4] = Convert.ToInt16(AiSlider2.Value);
        }
    }
}