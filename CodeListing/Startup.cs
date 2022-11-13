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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace NoughtsCrossesWPF
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup() //Temp Window for correct startup
        {
            InitializeComponent();
            GameInfo Info;

            //string Folder = "MusicSounds"; //creates sound assets //GOD I WISH I COULD FIGURE OUT HOW TO JUST LOOP THIS GOD DAMNIT //nvm i figured it out //So sad... what a waste
            //CreateFolder(Folder);
            //CreateAsset(Folder, "Click"); //basic sounds
            //CreateAsset(Folder, "Hover");
            //CreateAsset(Folder, "Begin");
            //CreateAsset(Folder, "TilePress");
            //CreateAsset(Folder, "Fail");

            //for (int i = 1; i < 8; i++) //music
            //{
            //    CreateAsset(Folder, "Main" + i);
            //    CreateAsset(Folder, "Battle" + i);
            //}

            Info.PlayerIcons = new string[] { "Cross", "Nought" };
            Info.Back = 0; //chooses random background
            Info.FormSize = 0;
            Info.ColourBytes = new Byte[,] { { 0, 0, 200 }, { 200, 0, 0 } }; //P1 = blue, P2 = Red
            Info.SoundPlayers = new MediaPlayer[] { new MediaPlayer(), new MediaPlayer(), new MediaPlayer(), new MediaPlayer(), new MediaPlayer() }; //0 = music, 1 = click, 2 = hover, 3 = misc1, 4 = misc2
            Info.GameSettings = new int[] { 7, 4, 15, 20, 20, 6}; //Colum Number, win con, Timer, ai 1 randomness, ai2 randomness, Row number
            Info.GameRules = new bool[] { false, false, false, false, false }; //Inverse, Memory, Random, Player 1 AI, Player 2 AI
            foreach (MediaPlayer Player in Info.SoundPlayers) //sets base volume of soundplayers
            {
                Player.Volume = 0.1;
            }
            Info.SoundPlayers[0].MediaEnded += LoopMusic; //allows music to loop
            Info.Music = 0; //Main, Battle, and Victory themes //-1 = random
            Utils.PlaySound("Main", Info.SoundPlayers[0], 0); //plays main theme

            Window main = new MainWindow(Info);
            Utils.NewWindow(this, main, Info);
        }

        //CREATE ASSET CODE REMOVED TO SAVE SPACE //KEEPING HERE FOR FUTURE REFERENCE

        //private void CreateFolder(string Folder) //makes sure folder 
        //{
        //    if (!System.IO.Directory.Exists(Directory.GetCurrentDirectory() + "/" + Folder)) //checks if folder exists
        //    {
        //        System.IO.Directory.CreateDirectory(Directory.GetCurrentDirectory()+ "/" + Folder);
        //    }
        //}

        //private void CreateAsset(string Folder, string Name)
        //{
        //    if (!System.IO.Directory.Exists(Directory.GetCurrentDirectory() + "/" + Folder + "/" + Name + ".mp3")) //checks if file exists,
        //    {
        //        BinaryFormatter bf = new BinaryFormatter(); //gets resource required
        //        MemoryStream ms = new MemoryStream();
        //        object obj;
        //        Byte[] Res;
        //        obj = Properties.Resources.ResourceManager.GetObject(Name);
        //        bf.Serialize(ms, obj);
        //        Res = ms.ToArray();

        //        File.WriteAllBytes(Directory.GetCurrentDirectory() + "/" + Folder + "/" + Name + ".mp3", Res); //copies from resources if not
        //    }
        //}

        private void LoopMusic(object sender, EventArgs e) //for Looping music
        {
            MediaPlayer Player = sender as MediaPlayer;
            Player.Position = TimeSpan.Zero;
            Player.Play();
        }
    }
}