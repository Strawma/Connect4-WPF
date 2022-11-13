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
using System.Windows.Resources;
using System.Windows.Media.Animation;
using System.Media;
using System.IO;

public struct GameInfo //struct for storing game info
{
    public string[] PlayerIcons;
    public int Back;
    public Byte[,] ColourBytes; //colour stored as rgb values for slider
    public MediaPlayer[] SoundPlayers;
    public int Music;
    public int FormSize; //index for size sign
    public int[] GameSettings;
    public bool[] GameRules;
};

namespace NoughtsCrossesWPF
{
    public class Utils //Holds methods to be used across forms
    {
        static Random r = new Random(); //random outside to give differing results

        public static void NewWindow(Window Current, Window New, GameInfo Info) //Switches Windows
        {
            PlaySound("Click", Info.SoundPlayers[1]);
            New.Height = Current.Height; //ensures form sizes are the same
            New.Width = Current.Width;
            New.Left = Current.Left;
            New.Top = Current.Top;
            New.MinHeight = Current.MinHeight;
            New.MinWidth = Current.MinWidth;
            New.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (Current.WindowState == WindowState.Maximized) New.WindowState = WindowState.Maximized; //ensures form state is the same
            else New.WindowState = WindowState.Normal;

            BackgroundCheck(Info, New); //sets background

            New.Show();
            Current.Close();
        }

        public static void ButtonHover(Rectangle Button, MediaPlayer HPlayer, string Invert = "", bool Numbered = false) //highlights or unhighlights button
        {
            string ImageName = Button.Name;
            if (Numbered == true) ImageName = ImageName.Remove(ImageName.Length - 1); //removes last number
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + ImageName + Invert + ".png")));
            Button.Fill = b;
            if(Invert == "i")
            {
                PlaySound("Hover", HPlayer);
            }
        }

        private static async void GradientBackground (Window window, Color[] PColours) //colour changing gradient
        {
            double Offset = 0.2; //decides how far gradient moves
            int GradientTime = 2; //decides speed of gradient
            Color[] Colours = PColours; //copies PColours for manual swappage 
            NameScope.SetNameScope(window, new NameScope());
            LinearGradientBrush BackBrush = new LinearGradientBrush();
            GradientStop[] Stops = new GradientStop[2];
            DoubleAnimation[] GradientAnimation = new DoubleAnimation[2];
            string Name;
            Storyboard GradientStoryBoard = new Storyboard();
            double[,] GradientPos = new double[,] { { -Offset, 1-Offset }, { Offset, Offset+1} };
            int Reverse = 0;
            for (int i = 0; i < 2; i++) //adds 2 stops to gradient for each player color
            {
                Name = "GradientStop" + i;
                Stops[i] = new GradientStop(PColours[i], GradientPos[0,i]);
                window.RegisterName(Name, Stops[i]);
                BackBrush.GradientStops.Add(Stops[i]);

                GradientAnimation[i] = new DoubleAnimation(); //animation changes offsets for gradient points
                GradientAnimation[i].From = i-Offset; //reverses animation, auto reverse doesnt allow wait smh
                GradientAnimation[i].To =  Offset+i;
                GradientAnimation[i].Duration = TimeSpan.FromSeconds(GradientTime);
                GradientAnimation[i].BeginTime = TimeSpan.FromSeconds(GradientTime);
                Storyboard.SetTargetName(GradientAnimation[i], Name); //Storyboard handles animation
                Storyboard.SetTargetProperty(GradientAnimation[i], new PropertyPath(GradientStop.OffsetProperty));
                GradientStoryBoard.Children.Add(GradientAnimation[i]);
            }

            window.Background = BackBrush;
            while (true)
            {
                GradientStoryBoard.Begin(window);
                Reverse = 1 - Reverse;
                switch (Reverse)
                {
                    case 1:
                        for (int i = 0; i < 2; i++)
                        {
                            GradientAnimation[i].From = Offset+i; //reverses animation, auto reverse doesnt allow wait smh
                            GradientAnimation[i].To = i-Offset;
                        }
                        break;
                    case 0:
                        for (int i = 0; i < 2; i++)
                        {
                            GradientAnimation[i].From = i-Offset;
                            GradientAnimation[i].To = Offset+i;
                        }
                        break;
                }
                await Task.Delay(2000 * GradientTime);
            }
        }

        public static async void FullScreen(Rectangle Button, Window Current, MediaPlayer CPlayer) //Custom Fullscreen to prevent resizing
        {
            if (Button.Name == "NCFullScreen") //Fullscreen when clicked
            {
                Current.WindowState = WindowState.Maximized;
                await Task.Delay(50); //delay otherwise sound does not play
                PlaySound("Click", CPlayer);
            }
            else //unfullscreen when clicked
            {
                Current.Width = Current.MinWidth; //stops sizing freakouts
                Current.Height = Current.MinHeight;
                Current.WindowState = WindowState.Normal;
                PlaySound("Click", CPlayer);
            }
            CheckState(Button, Current);
        }
        public static void CheckState(Rectangle Button, Window Current)
        {
            WindowState windowState = Current.WindowState;
            if (windowState == WindowState.Normal) Button.Name = "NCFullScreen";
            else Button.Name = "NCUnFullScreen";
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Button.Name + ".png")));
            Button.Fill = b;
        }

        public static void PlaySound(string Sound, MediaPlayer Player, int Choice = -1) //for playing sounds
        {
            string ChoiceString = "";
            switch (Choice) //choice allows for music picking
            {
                case 0: //random
                    ChoiceString = r.Next(1, 8).ToString();
                    break;
                case -1: //no choice
                    break;
                default: //chosen music
                    ChoiceString = Choice.ToString();
                    break;
            }
            Player.Open(new Uri(Directory.GetCurrentDirectory() + "/MusicSounds/" + Sound + ChoiceString + ".mp3"));
            Player.Play();
        }

        public static void BackgroundCheck(GameInfo Info, Window Window) //for checking background
        {
            int Back = Info.Back;
            Color[] PColours = new Color[] { Color.FromRgb(Info.ColourBytes[0, 0], Info.ColourBytes[0, 1], Info.ColourBytes[0, 2]), Color.FromRgb(Info.ColourBytes[1, 0], Info.ColourBytes[1, 1], Info.ColourBytes[1, 2]) }; //Converts RGB Bytes to Actual Colours
            if (Info.Back == 0) Back = r.Next(1,10); //random
            switch (Back)
            {
                case 1:
                    GradientBackground(Window,PColours);
                    break;
                default:
                    ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/Wallpaper" + (Back-1) + ".jpg")));
                    Window.Background = b;
                    break;
            }
        }

        public static void ButtonAnimation(Rectangle[] Buttons, Window window)
        {
            Storyboard FallBoard = new Storyboard(); //storyboard will contain 2 animations for both tiles (image + colour)
            ThicknessAnimation[] animations = new ThicknessAnimation[Buttons.Length];

            double Hover = 16 * window.Width / 640;
            Random r = new Random();

            for (int i = 0; i < Buttons.Length; i++) //uses a for loop to create 2 animations
            {
                animations[i] = new ThicknessAnimation();
                animations[i].From = new Thickness(0, 0, 0, 0);
                animations[i].To = new Thickness(0, -Hover, 0, Hover);
                animations[i].AutoReverse = true;
                animations[i].RepeatBehavior = RepeatBehavior.Forever;
                animations[i].AccelerationRatio = 0.5;
                animations[i].Duration = TimeSpan.FromSeconds(2);
                animations[i].BeginTime = TimeSpan.FromMilliseconds(100*i);
                Storyboard.SetTargetProperty(animations[i], new PropertyPath(FrameworkElement.MarginProperty));
                Storyboard.SetTarget(animations[i], Buttons[i]);
                FallBoard.Children.Add(animations[i]);
            }
            FallBoard.Begin();

        }
    }
}