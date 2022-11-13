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
    /// Interaction logic for CustomWindow.xaml
    /// </summary>
    public partial class CustomWindow : Window
    {
        GameInfo Info;
        Random r = new Random();

        public CustomWindow(GameInfo InfoT)
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
            Utils.ButtonHover(button, Info.SoundPlayers[2]);
        }
        private void FullScreenClick(object sender, MouseButtonEventArgs e)
        {
            Utils.FullScreen(NCFullScreen, this, Info.SoundPlayers[1]);
        }
        private void CheckState(object sender, DependencyPropertyChangedEventArgs e)
        {
            Utils.CheckState(NCFullScreen, this); //ensures fullscreen button is correct
        }

        //--------------------------------------------------------------------------------------Other Stuff

        private void FormLoad(object sender, EventArgs e)
        {
            barVolume.Value = Info.SoundPlayers[1].Volume; //sets sound slider
            barVolume.ValueChanged += VolumeChanged;
            Slider[,] Sliders = new Slider[,] { { barP1R, barP1G, barP1B }, { barP2R, barP2G, barP2B } };
            for (int i = 0; i < 2; i++)
            {
                for (int RGBValue = 0; RGBValue < 3; RGBValue ++) //each color - sets color slider correctly
                {
                    Sliders[i, RGBValue].Value = Convert.ToInt16(Info.ColourBytes[i,RGBValue]);
                }
            }
            UpdateSquareFill(); //fills in colour squares
            UpdateIcon();

            cboxBackGround.SelectedIndex = Info.Back; //sets up comboboxes
            cboxMusicTheme.SelectedIndex = Info.Music;
            cboxScreenSize.SelectedIndex = Info.FormSize;
            cboxBackGround.SelectionChanged += BackgroundChanged;
            cboxMusicTheme.SelectionChanged += MusicChanged;
            cboxScreenSize.SelectionChanged += ScreenChanged;
            SetIconBoxes(); 
        }

        private void BarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) //when slider value changes
        { 
            Slider Bar = sender as Slider;
            int IndexA = Convert.ToInt16(Bar.MinWidth);
            int IndexB = Convert.ToInt16(Bar.MinHeight);
            Info.ColourBytes[IndexA, IndexB] = Convert.ToByte(Bar.Value);
            UpdateSquareFill();

            if (Info.Back == 1) //changes background if gradient
            {
                Utils.BackgroundCheck(Info, this);
            }
        }

        private void UpdateSquareFill() //fills in colourshowcase squares
        {
            SolidColorBrush b = new SolidColorBrush(Color.FromRgb(Info.ColourBytes[0, 0], Info.ColourBytes[0, 1], Info.ColourBytes[0, 2]));
            SolidColorBrush b2 = new SolidColorBrush(Color.FromRgb(Info.ColourBytes[1, 0], Info.ColourBytes[1, 1], Info.ColourBytes[1, 2]));
            rectP1ShowColour.Fill = b;
            rectP1IconColour.Fill = b;
            rectP2ShowColour.Fill = b2;
            rectP2IconColour.Fill = b2;
        }
        
        private void UpdateIcon() //changes displayicons
        {
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Info.PlayerIcons[0] + ".png")));
            rectP1ShowIcon.Fill = b;
            ImageBrush b2 = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Info.PlayerIcons[1] + ".png")));
            rectP2ShowIcon.Fill = b2;
        }
    

        private void IconChange(object sender, SelectionChangedEventArgs e) //for changing icon
        {
            ComboBox[] Boxes = new ComboBox[] { cboxP1Icon, cboxP2Icon };
            ComboBox Box = sender as ComboBox; //gets selected box
            ComboBox OtherBox = Boxes[1 - Convert.ToInt16(Box.MinWidth)]; //gets other box

            int IndexA = Convert.ToInt16(Box.MinWidth); //sets text to info icon array
            string SelectedName = Box.SelectedValue.ToString();
            Info.PlayerIcons[IndexA] = SelectedName;
            UpdateIcon();

            object OtherSelection = OtherBox.SelectedValue; //this all prevents players from having same icon 
            OtherBox.SelectionChanged -= IconChange; //temp disable event
            OtherBox.Items.Clear();
            IconLoop(SelectedName, OtherBox);
            OtherBox.SelectedValue = OtherSelection;
            OtherBox.SelectionChanged += IconChange;
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
        }

        private void TextResize(object sender, SizeChangedEventArgs e) //changes text size when form resizes
        {
            double Multiplier = this.ActualWidth / 640;
            foreach  (ComboBox Box in CustomWindowGrid.Children.OfType<ComboBox>())
            {
                Box.FontSize = 16 * Multiplier;
            }
        }

        private void BackgroundChanged(object sender, SelectionChangedEventArgs e) //when swapping Backgrounds
        {
            ComboBox Box = sender as ComboBox;
            int Value = Box.SelectedIndex;
            Info.Back = Value;
            Utils.BackgroundCheck(Info, this);
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
        }

        private void MusicChanged(object sender, SelectionChangedEventArgs e) //when swapping Backgrounds
        {
            ComboBox Box = sender as ComboBox;
            int Value = Box.SelectedIndex;
            Info.Music = Value;
            Utils.PlaySound("Main", Info.SoundPlayers[0], Value);
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
        }

        private void ScreenChanged(object sender, SelectionChangedEventArgs e) //when changing screen size
        {
            ComboBox Box = sender as ComboBox;
            Info.FormSize = Box.SelectedIndex;
            double Multiplier = 1 + Info.FormSize * 0.5;
            this.MinWidth = 640 * Multiplier; //changes form sizes
            this.MinHeight = 360 * Multiplier;
            this.Width = this.MinWidth;
            this.Height = this.MinHeight;
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
            Window Reopen = new CustomWindow(Info); //reopens window
            Utils.NewWindow(this, Reopen, Info);
        }

        private void SetIconBoxes() //ensures correct selection options for combo boxes
        {
            ComboBox[] Boxes = new ComboBox[] { cboxP1Icon, cboxP2Icon };
            for (int i = 0; i < 2; i++)
            {
                IconLoop(Info.PlayerIcons[1 - i], Boxes[i]);
                Boxes[i].Text = Info.PlayerIcons[i];
                Boxes[i].SelectionChanged += IconChange;
            }
        }

        private void IconLoop(string CheckName, ComboBox Box) //repeated code for setting icon comboboxes
        {
            ComboBoxItem item;
            string[] ComboItems = new string[] { "Nought", "Cross", "Square", "Triangle" };
            foreach (string Name in ComboItems)
            {
                if (Name != CheckName)
                {
                    item = new ComboBoxItem();
                    item.Content = Name;
                    Box.Items.Add(item);
                }
            }
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider bar = sender as Slider;
            foreach (MediaPlayer Player in Info.SoundPlayers)
            {
                Player.Volume = bar.Value;
            }
        }

        private void Randomise(object sender, MouseButtonEventArgs e) //randomises customisation options , not sound or screen
        {
            Utils.PlaySound("Click", Info.SoundPlayers[1]);
            Slider[,] Sliders = new Slider[,] { { barP1R, barP1G, barP1B }, { barP2R, barP2G, barP2B } }; //randomise  sliders
            for (int i = 0; i < 2; i++)
            {
                for (int RGBValue = 0; RGBValue < 3; RGBValue++) 
                {
                    Sliders[i, RGBValue].Value = r.Next(1, 201);
                }
            }
            ComboBox[] Boxes = new ComboBox[] { cboxBackGround, cboxMusicTheme, cboxP1Icon, cboxP2Icon };
            foreach (ComboBox Box in Boxes) Box.SelectedIndex = r.Next(Box.Items.Count); //randomises customise boxes
        }
    }
}