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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Drawing.Imaging;
using System.IO;

namespace NoughtsCrossesWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameInfo Info;

        public MainWindow(GameInfo InfoT)
        {
            Info = InfoT;
            InitializeComponent();
        }

        //----------------------------------------------------------------------------------------Util Stuff
        private void ButtonHover(object sender, MouseEventArgs e)
        {
            Rectangle button = sender as Rectangle;
            Utils.ButtonHover(button, Info.SoundPlayers[2], "i");
        }
        private void ButtonLeave(object sender, MouseEventArgs e)
        {
            Rectangle button = sender as Rectangle;
            Utils.ButtonHover(button, Info.SoundPlayers[2]);
        }

        private void CheckState(object sender, DependencyPropertyChangedEventArgs e)
        {
            Utils.CheckState(NCFullScreen, this); //ensures fullscreen button is correct
        }

        private void MenuLoad(object sender, EventArgs e) //sets up background
        {
            TitleColors();

            Rectangle[] HoverButtons = new Rectangle[] { NCConfigure, NCPlay, NCCustomise };
            Utils.ButtonAnimation(HoverButtons, this);
        }

        private void PlayClick(object sender, MouseButtonEventArgs e) //Opening Windows
        {
            Window GameWindow = new GameWindow(Info);
            Utils.NewWindow(this,GameWindow,Info);
        }
        private void ConfigClick(object sender, MouseButtonEventArgs e)
        {
            Window ConfigWindow = new ConfigWindow(Info);
            Utils.NewWindow(this, ConfigWindow, Info);
        }
        private void CustomClick(object sender, MouseButtonEventArgs e)
        {
            Window CustomWindow = new CustomWindow(Info);
            Utils.NewWindow(this, CustomWindow, Info);
        }

        private void FullScreenClick(object sender, MouseButtonEventArgs e)
        {
            Utils.FullScreen(NCFullScreen, this, Info.SoundPlayers[1]);
        }


        //--------------------------------------------------------------------------------------------------Other Stuff
        private async void TitleColors() //for filling "SUPER" in title
        {
            SolidColorBrush[] Brushes = new SolidColorBrush[5]; //creates brush for each letter
            for (int i = 0; i < 5; i++)
            {
                Brushes[i] = new SolidColorBrush();
            }
            Random r = new Random();
            Rectangle[] TitleFills = new Rectangle[] { TitleFill1, TitleFill2, TitleFill3, TitleFill4, TitleFill5 };
            byte[] CValues = new byte[3];
            while (true)
            {
                for (int Index = 0; Index < 5; Index++) //loops for each letter
                {
                    for (int i = 0; i < 3; i++) //generates random rgb color
                    {
                        CValues[i] = Convert.ToByte(r.Next(0, 255));
                    }
                    Brushes[Index].Color = Color.FromRgb(CValues[0], CValues[1], CValues[2]);
                    TitleFills[Index].Fill = Brushes[Index];
                }
                await Task.Delay(1000);
            }
        }
    }
}