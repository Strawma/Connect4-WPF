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
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Media.Animation;
using System.Net.Sockets;

namespace NoughtsCrossesWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        GameInfo Info;
        Color[] PColours;
        bool MusicRestart = false; //restarts main theme if needed
        Random r = new Random();
        double Time;

        int GameTurn;
        bool Start = false;
        Rectangle[,] Tiles; //tile list
        Rectangle[,] TileColors; //tiles under tiles for color
        Rectangle[,] PTiles; //Placeholder tiles
        int[,] GameScores;
        Line Winline; //for winchecking
        double[,] LinePos;
        int Spaces;

        int CNum;
        int RNum;
        int[] BottomPos; //keeps a track of where you can place on each column

        public GameWindow(GameInfo InfoT)
        {
            Info = InfoT;
            CNum = Info.GameSettings[0]; //sets colum + row number
            RNum = Info.GameSettings[5];

            BottomPos = new int[CNum]; //sets base playable points in each column
            Tiles = new Rectangle[CNum, RNum]; //array for tiles
            TileColors = new Rectangle[CNum, RNum]; //array for tileColors
            PTiles = new Rectangle[CNum, RNum]; //array for placeholder tiles
            PColours = new Color[] { Color.FromRgb(Info.ColourBytes[0, 0], Info.ColourBytes[0, 1], Info.ColourBytes[0, 2]), Color.FromRgb(Info.ColourBytes[1, 0], Info.ColourBytes[1, 1], Info.ColourBytes[1, 2]) }; //Converts RGB Bytes to Actual Colours
            InitializeComponent();
        }

        //---------------------------------------------------------------------------------Util Stuff
        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            Start = false; //prevents weird stuff
            if (MusicRestart == true) Utils.PlaySound("Main", Info.SoundPlayers[0], Info.Music);
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


        //-----------------------------------------------------------------------------------------Other Stuff
        private void FormLoad(object sender, EventArgs e)
        {
            Time = Info.GameSettings[2];
            if(!Info.GameRules[0]) lblTimer.Content = "Timer: " + Time.ToString("n2");
            else lblTimer.Content = "Inverse Mode";

            SolidColorBrush b = new SolidColorBrush(PColours[0]); //colours battlebar
            SolidColorBrush b2 = new SolidColorBrush(PColours[1]);
            BattleBar.Foreground = b;
            BattleBar.Background = b2;

            Winline = new Line(); //creates winline
            Grid.SetColumnSpan(Winline, CNum); //allows line to span columns
            Grid.SetRowSpan(Winline, RNum);
            Winline.Stroke = System.Windows.Media.Brushes.Black;

            Rectangle fill = rectP1Colour; //setting display icons
            Rectangle shape = rectP1Icon;
            for (int i = 0; i < 2; i++)
            {
                IconPaint(shape, fill, i);
                fill = rectP2Colour;
                shape = rectP2Icon;
            }

            //GridPlay.ShowGridLines = true; //debugging DONT LEAVE IN CODE

            for (int i = 0; i < CNum; i++) //creates grid
            {
                ColumnDefinition GridColumn = new ColumnDefinition();
                GridPlay.ColumnDefinitions.Add(GridColumn);
            }
            for (int i = 0; i < RNum; i++) //creates grid
            {
                RowDefinition GridRow = new RowDefinition();
                GridPlay.RowDefinitions.Add(GridRow);
            }

            for (int c = 0; c < CNum; c++) //grid column
            {
                for (int r = 0; r < RNum; r++) //grid row
                {
                    PTiles[c, r] = new Rectangle(); //placeholder tiles keep outline
                    Grid.SetColumn(PTiles[c, r], c);
                    Grid.SetRow(PTiles[c, r], r);
                    PTiles[c, r].Stroke = System.Windows.Media.Brushes.Black;

                    GridPlay.Children.Add(PTiles[c, r]);

                    TileColors[c, r] = new Rectangle();
                    Grid.SetColumn(TileColors[c, r], c);
                    Grid.SetRow(TileColors[c, r], r);
                    //TileColors[c, r].MouseLeave += this.TileLeave;
                    TileColors[c, r].MinHeight = c; //this is awful, but it works for tying values to these grids
                    TileColors[c, r].MinWidth = r;

                    GridPlay.Children.Add(TileColors[c, r]);

                    Tiles[c, r] = new Rectangle(); //creates tiles in each row
                    Tiles[c, r].Name = "N";
                    Grid.SetColumn(Tiles[c, r], c);
                    Grid.SetRow(Tiles[c, r], r);
                    Tiles[c, r].Fill = Brushes.Transparent;
                    Tiles[c, r].Stroke = System.Windows.Media.Brushes.Black; //outline
                    Tiles[c, r].HorizontalAlignment = HorizontalAlignment.Stretch; //tile fills grid
                    Tiles[c, r].VerticalAlignment = VerticalAlignment.Stretch;
                    Tiles[c, r].MouseDown += this.TileClick; //adds a click event to Tile
                    Tiles[c, r].MouseEnter += this.TileHover; //for highlighting tile
                    Tiles[c, r].MouseLeave += this.TileLeave;
                    Tiles[c, r].MinHeight = c; //awful x2
                    Tiles[c, r].MinWidth = r;

                    GridPlay.Children.Add(Tiles[c, r]);
                }
            }
        }

        private void BeginClick(object sender, MouseButtonEventArgs e) //begin button pressed
        {
            GameLabel.Content = " Battle! ";
            NCBegin.MouseEnter -= ButtonHover; //removes begin button events
            NCBegin.MouseLeave -= ButtonLeave;
            NCBegin.MouseDown -= BeginClick;
            NCBegin.Cursor = Cursors.Arrow;

            TextBox box = tbxP1Name; // makes it so player names are locked in
            for (int i = 0; i < 2; i++)
            {
                box.IsReadOnly = true;
                box.Cursor = Cursors.Arrow;
                box.BorderBrush = Brushes.Black;
                box.Foreground = Brushes.Black;
                box.Background = Brushes.White;
                box = tbxP2Name;
            }

            Reset();
        }

        private void TileClick(Object sender, MouseButtonEventArgs e) //when Tile is clicked
        {
            if (Info.GameRules[GameTurn+3] == false) //checks for ai turn
            {
                Rectangle Tile = sender as Rectangle;
                TileClickEvent(Tile);

                int Column = Convert.ToInt32(Tile.MinHeight); //CODE FOR HIGHLIGHTING TILE ABOVE
                int Row = BottomPos[Column];
                if (Row >= 0)
                {
                    Rectangle HighTile = Tiles[Column, Row];
                    SolidColorBrush brush = new SolidColorBrush();
                    brush.Color = Color.FromArgb(125, 0, 0, 0);
                    HighTile.Fill = brush;
                }
            }
        }

        private void TileClickEvent(Rectangle CTile) //inputs clicked tile
        {
            if (Start == true) //only Plays if game is begun
            {
                int Column = Convert.ToInt32(CTile.MinHeight); //gets clicked Column and Row
                int Row = BottomPos[Column];
                Rectangle Tile = Tiles[Column, Row]; //gets new tile after considering Gravity

                int Score = GameTurn + 1;

                Tile.Name = "Y"; //signifies tile being pressed
                Utils.PlaySound("TilePress", Info.SoundPlayers[4]); //plays tilepress sound
                Tile.MouseDown -= this.TileClick; //tile cannot be clicked again
                Tile.MouseEnter -= this.TileHover;
                //Tile.MouseLeave -= this.TileLeave;
                Tile.Cursor = Cursors.None;

                if (Info.GameRules[1]) //if memory mode, fills with question mark
                {
                    ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/MemoryTile.png")));
                    TileColors[Column, Row].Fill = Brushes.Black;
                    Tile.Fill = b;
                }
                else IconPaint(Tile, TileColors[Column, Row], GameTurn); //fills tile

                TileFall(Tile, TileColors[Column, Row]);

                GameScores[Column, Row] = Score; //adds player score to relevant grid position (for wincheck)
                BottomPos[Column] -= 1; //adjusts playable value for that column

                int Win = WinCheck(Column, Row, Score);
                if (Win == 10) //checks for win
                {
                    int WinNum = GameTurn;
                    if (Info.GameRules[0]) WinNum = 1 - GameTurn; //reverses if inverse mode

                    if (Info.GameRules[1]) //makes line not black for memory mode
                    {
                        SolidColorBrush lb = new SolidColorBrush(PColours[GameTurn]);
                        Winline.Stroke = lb;
                    }

                    double TileWidth = GridPlay.ActualWidth / CNum; //WHY IS IT NOT 300X300?????? //dont worry past me it doesn't matter :)
                    double TileHeight = GridPlay.ActualHeight / RNum;
                    Winline.StrokeThickness = TileWidth / 3; //sets pen options
                    Winline.X1 = (LinePos[0, 0] + 0.5) * TileWidth; //sets line positions
                    Winline.X2 = (LinePos[1, 0] + 0.5) * TileWidth;
                    Winline.Y1 = (LinePos[0, 1] + 0.5) * TileHeight;
                    Winline.Y2 = (LinePos[1, 1] + 0.5) * TileHeight;
                    Winline.StrokeStartLineCap = PenLineCap.Triangle; //sets end of lines
                    Winline.StrokeEndLineCap = PenLineCap.Triangle;
                    GridPlay.Children.Add(Winline);
                    GridPlay.SizeChanged += ResizeLine; //adds resize event

                    TextBox[] textBoxes = new TextBox[] { tbxP1Name, tbxP2Name }; //displays winner text
                    Label[] PlayerWins = new Label[] { P1Wins, P2Wins };
                    GameLabel.Content = textBoxes[WinNum].Text + " Wins!";
                    SolidColorBrush b = new SolidColorBrush(PColours[WinNum]);
                    GameLabel.Foreground = b;
                    GameEnd(); //end game message

                    PlayerWins[WinNum].Content = Convert.ToInt32(PlayerWins[WinNum].Content) + 1; //adds win and changes bar
                    AdjustBar();

                    GridPlay.Cursor = Cursors.None;
                }
                else if (Win == 0) //checks for draw
                {
                    GameLabel.Content = "It's a Draw!";
                    GameEnd();
                }

                Spaces -= 1; //-1 to possible spaces

                NextTurn();
            }
        }

        private void TileHover(Object sender, EventArgs e) //highlights tile when hovered over
        {
            if (Start == true && Info.GameRules[GameTurn + 3] == false) //prevents hovering on AI turn
            {
                Rectangle HovTile = sender as Rectangle;
                int Column = Convert.ToInt32(HovTile.MinHeight); //gets column num from minheight
                Rectangle Tile = Tiles[Column, BottomPos[Column]];

                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = Color.FromArgb(125, 0, 0, 0);
                Tile.Fill = brush;
            }
        }
        private void TileLeave(Object sender, EventArgs e) //unhighlights tile when mouse leaves
        {
            Rectangle HovTile = sender as Rectangle;
            int Column = Convert.ToInt32(HovTile.MinHeight); //gets column num from minheight
            int Row = BottomPos[Column];

            if (Row >= 0)
            {
                Rectangle Tile = Tiles[Column, Row];
                Tile.Fill = Brushes.Transparent;
            }
        }

        private int WinCheck(int Column, int Row, int Score)
        {
            int Streak;
            LinePos = new double[2, 2]; //2d array for Line Positions
            int[] TestPos = new int[2];
            int DirectionY;
            int DirectionX;
            bool Stop;

            for (int Vindex = -1; Vindex < 3; Vindex++) //verttical offset checked
            {
                if (Vindex == 2) //special case for checking vertically
                {
                    DirectionY = 1;
                    DirectionX = 0;
                }
                else
                {
                    DirectionY = Vindex; //moves Vertically
                    DirectionX = -1; //moves right
                }
                Streak = 1; //sets streak
                LinePos[0, 0] = Column;
                LinePos[0, 1] = Row; //sets starting point for line as current place
                LinePos[1, 0] = Column;
                LinePos[1, 1] = Row; //also sets second line point

                for (int i = 0; i < 2; i++) //checks leftwards then rightwards
                {
                    Stop = false;
                    TestPos[0] = Column + DirectionX; //sets testposition
                    TestPos[1] = Row + DirectionY;
                    while (Stop == false && TestPos[0] >= 0 && TestPos[0] < CNum && TestPos[1] >= 0 && TestPos[1] < RNum) //checks position is within bounds and streak is still ongoing
                    {
                        if (GameScores[TestPos[0], TestPos[1]] == Score)
                        {
                            Streak += 1;
                            LinePos[i, 0] = TestPos[0]; //sets new line point if streak found, offset for style
                            LinePos[i, 1] = TestPos[1];
                            if (Streak == Info.GameSettings[1]) //if a streak of win condition is met
                            {
                                return 10; //winscore
                            }
                        }
                        else Stop = true;

                        TestPos[0] += DirectionX; //moves testposition
                        TestPos[1] += DirectionY;
                    }
                    DirectionX = -DirectionX; //swaps direction to rightwards
                    DirectionY = -DirectionY;
                }
            }

            foreach (int Space in GameScores)
            {
                if (Space == 0) return -1; //spaces left == not draw
            }

            return 0; //draw
        }

        private void RematchClick(object sender, MouseButtonEventArgs e) //resets game
        {
            GameLabel.Foreground = Brushes.Black; //resets win text colour
            NCRematch.Visibility = Visibility.Hidden; //hides rematch button
            foreach (Rectangle Tile in Tiles) //clears grid
            {
                Tile.Fill = Brushes.Transparent;
                Tile.Cursor = null;
                if (Tile.Name == "Y") //resets events if need be
                {
                    Tile.Name = "N";
                    Tile.MouseDown += this.TileClick;
                    Tile.MouseEnter += this.TileHover;
                    //Tile.MouseLeave += this.TileLeave;
                }
            }
            foreach (Rectangle Color in TileColors) Color.Fill = Brushes.Transparent; //clears grid colors
            GridPlay.SizeChanged -= ResizeLine; //removes line
            GridPlay.Children.Remove(Winline);
            Reset();
        }

        private void Reset() //resets variables for a match/rematch
        {
            Spaces = RNum * CNum; //gets all spaces
            GameScores = new int[CNum, RNum]; //sets score board
            MusicRestart = true; //restarts maint theme when form closes

            for (int Index = 0; Index < BottomPos.Length; Index++)
            {
                BottomPos[Index] = RNum - 1;
            }

            GameLabel.Content = " Battle! "; //changes gamelabel
            Utils.PlaySound("Begin", Info.SoundPlayers[3]); //plays gong sound
            Utils.PlaySound("Battle", Info.SoundPlayers[0], Info.Music); //plays battle theme
            GridPlay.Cursor = Cursors.Cross; //cursor set to Player with current turn
            rectTurnIcon.Visibility = Visibility.Visible; //shows turn image
            rectTurnColour.Visibility = Visibility.Visible;
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/NCTurn.png"))); //changes begin to turn
            NCBegin.Fill = b;

            Start = true;
            GameTurn = r.Next(2); //sets turn to random player
            NextTurn(); //icon paint + ai turn
        }

        private async void AdjustBar() //adjusts battlebar when win
        {
            double P1Num = Convert.ToInt32(P1Wins.Content);
            double P2Num = Convert.ToInt32(P2Wins.Content);
            double Total = P1Num + P2Num;
            double ratio = P1Num / Total; //finds a battlebar percentage
            ratio = 100 * ratio;
            int position = Convert.ToInt32(ratio);
            int offset;
            if (position >= BattleBar.Value) offset = 1;
            else offset = -1;
            while (BattleBar.Value != position) //delays for effect!
            {
                BattleBar.Value += offset;
                await Task.Delay(20);
            }
        }

        private async void GameEnd() //when game ends
        {
            MusicRestart = false;
            Start = false;
            Info.SoundPlayers[0].Stop(); //ends music, plays gong again and main theme
            Utils.PlaySound("Begin", Info.SoundPlayers[3]);
            Utils.PlaySound("Main", Info.SoundPlayers[0], Info.Music);
            NCRematch.Visibility = Visibility.Visible; //allows rematch
            SpeechSynthesizer voice = new SpeechSynthesizer(); //speaks when game over
            int VoiceVolume = Convert.ToInt32(100 * Info.SoundPlayers[0].Volume * 4); //sets volume of voice
            if (VoiceVolume > 100) VoiceVolume = 100;
            voice.Volume = VoiceVolume;
            voice.SpeakAsync(GameLabel.Content.ToString());
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/NCGameOver.png"))); //changes turn to Game over
            NCBegin.Fill = b;
            rectTurnIcon.Visibility = Visibility.Hidden; //removes turn image
            rectTurnColour.Visibility = Visibility.Hidden;

            Explosion.Visibility = Visibility.Visible; //displays awesome explosion
            Utils.PlaySound("explosion", Info.SoundPlayers[3]);
            await Task.Delay(1800);
            Explosion.Visibility = Visibility.Hidden;
        }

        private void IconPaint(Rectangle IconTile, Rectangle ColourTile, int Player) //for.. painting tiles
        {
            SolidColorBrush c = new SolidColorBrush(); //paints tile
            c.Color = PColours[Player];
            ColourTile.Fill = c;
            ImageBrush b = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/" + Info.PlayerIcons[Player] + ".png"))); //changes tile icon
            IconTile.Fill = b;
        }

        private void ResizeLine(object sender, SizeChangedEventArgs e) //Resizes Line when grid resized
        {
            double TileWidth = GridPlay.ActualWidth / CNum;
            double TileHeight = GridPlay.ActualHeight / RNum;
            Winline.StrokeThickness = TileWidth / 3;
            Winline.X1 = (LinePos[0, 0] + 0.5) * TileWidth;
            Winline.X2 = (LinePos[1, 0] + 0.5) * TileWidth;
            Winline.Y1 = (LinePos[0, 1] + 0.5) * TileHeight;
            Winline.Y2 = (LinePos[1, 1] + 0.5) * TileHeight;
        }

        private void TextResize(object sender, SizeChangedEventArgs e) //resizes text when form resized
        {
            double Multiplier = this.ActualWidth / 640; //uses multiplier based on minimum width

            if (CNum < RNum) GridPlay.Width = 281 * Multiplier * CNum / RNum; //Scales Grid to prevent stretching if needed
            else if (CNum > RNum) GridPlay.Height = 281 * Multiplier * RNum / CNum;

            tbxP1Name.FontSize = 24 * Multiplier;
            tbxP2Name.FontSize = 24 * Multiplier;
            P1Wins.FontSize = 12 * Multiplier;
            P2Wins.FontSize = 12 * Multiplier;
            GameLabel.FontSize = 24 * Multiplier;
            lblTimer.FontSize = 18 * Multiplier;
        }

        private async void AIMove() //call when ai turn
        {
            await Task.Delay(700); //time to click

            List<Rectangle> FreeTiles = new List<Rectangle>();
            int Turn = GameTurn;
            int BestScore = int.MinValue;
            int Score;
            int[] BestMove = new int[2];
            int Depth = 1764 / (RNum*RNum*CNum); //depth calculation //gets further as game progresses
            Depth += RNum - Spaces / RNum; //might help boost smartness???

            for (int Col = 0; Col < CNum; Col++) //loops columns like NoughtsNCrosses
            {
                int Row = BottomPos[Col]; //gets playable row

                if (Row >= 0)
                {
                    FreeTiles.Add(Tiles[Col, Row]); //adds spot to free spaces

                    GameScores[Col, Row] = Turn + 1; //adds score of AI turn
                    BottomPos[Col] -= 1; //adjusts playable Row

                    int AIVal = 1;
                    if (Info.GameRules[0]) AIVal = -1;
                    Score = Minimax(AIVal, Turn, Col, Row, Depth); //passes in inverse of whether inverse mode is on //adjusted to give 7x6 board a depth of 8 moves

                    GameScores[Col, Row] = 0; //board change only temporary
                    BottomPos[Col] += 1; //also resets playable value

                    if (Score > BestScore) //takes best move
                    {
                        BestScore = Score;
                        BestMove[0] = Col;
                        BestMove[1] = Row;
                    }

                    if (Score == BestScore) //takes best move
                    {
                        int vary = r.Next(1, 420/(CNum*RNum)); //adds a bit o spice to things
                        if (vary == 1)
                        {
                            BestScore = Score;
                            BestMove[0] = Col;
                            BestMove[1] = Row;
                        }
                    }
                }
            }

            if (Start == true) //helps prevent weirdness
            {
                int RandomChoose = r.Next(1, 101); //chance of making random move
                if (RandomChoose < Info.GameSettings[3 + Turn]) TileClickEvent(FreeTiles[r.Next(FreeTiles.Count)]); //based on random slider
                else TileClickEvent(Tiles[BestMove[0], BestMove[1]]); //chooses best move tile
            }
        }

        private int Minimax(int AI, int Turn, int CheckColumn, int CheckRow, int depth, int alpha = int.MinValue, int beta = int.MaxValue) //tree of outcomes
        {
            int Win = WinCheck(CheckColumn,CheckRow,Turn+1);
            if (Win == 10)
            {
                return AI*(100*depth);
            }

            if (Win == 0) return -depth; //draw, further better for draw

            int Score;
            int NewTurn = 1 - Turn; //swaps turn

            if (depth >= 0) //something of a limit //I HATE MINIMAX I HATE MINIMAX I HATE MINIMAX
            {
                if (AI == -1) //if maximising
                {
                    for (int Col = 0; Col < Info.GameSettings[0]; Col++)
                    {
                        int Row = BottomPos[Col]; //gets playable row
                        if (Row >= 0) //checks spot is free
                        {
                            BottomPos[Col] -= 1;
                            GameScores[Col, Row] = NewTurn + 1; //adds score of next player
                            Score = Minimax(1, NewTurn, Col, Row, depth - 1, alpha, beta); //calls func again
                            GameScores[Col, Row] = 0; //board change only temporary
                            BottomPos[Col] += 1;

                            alpha = Math.Max(alpha, Score);
                            if (alpha >= beta) break; //pruning
                        }
                        
                    }
                    return alpha;
                }
                else //if minimising
                {
                    for (int Col = 0; Col < Info.GameSettings[0]; Col++)
                    {
                        int Row = BottomPos[Col]; //gets playable row
                        if (Row < 0) Row = 0;
                        if (GameScores[Col, Row] == 0) //checks spot is free
                        {
                            BottomPos[Col] -= 1;
                            GameScores[Col, Row] = NewTurn + 1; //adds score of AI turn
                            Score = Minimax(-1, NewTurn, Col, Row, depth - 1, alpha, beta);
                            GameScores[Col, Row] = 0; //board change only temporary
                            BottomPos[Col] += 1;

                            beta = Math.Min(beta, Score);
                            if (beta <= alpha) break; //pruning
                        }
                    }
                    return beta;
                }
            }
            return 0;
        }

        //private int Negamax(int AI, int Turn, int CheckColumn, int CheckRow, int depth, int alpha = -10000, int beta = 10000) //tree of outcomes //FAILED NEGAMAX ALGORITHM :(
        //{
        //    int Win = WinCheck(CheckColumn, CheckRow, Turn + 1);
        //    if (Win == 10)
        //    {
        //        return AI*(100 + depth); //-10 for min win, further is better for for loss //10 for max win, closer better for win
        //    }

        //    if (Win == 0) return 0; //draw,

        //    int Score = -10000;
        //    int NewTurn = 1 - Turn; //swaps turn

        //    if (depth >= 0) //something of a limit //WARNING AI GETS VERY DUMB FOR NON 3x3
        //    {
        //        int Max = int.MinValue;
        //        for (int Col = 0; Col < Info.GameSettings[0]; Col++)
        //        {
        //            int Row = BottomPos[Col]; //gets playable row
        //            if (Row < 0) Row = 0;
        //            if (GameScores[Col, Row] == 0) //checks spot is free
        //            {
        //                BottomPos[Col] -= 1;
        //                GameScores[Col, Row] = NewTurn + 1; //adds score of next player
        //                Score = -Negamax(-AI, NewTurn, Col, Row, depth - 1, -beta, -alpha); //calls func again, - AI and swapped - alpha beta
        //                GameScores[Col, Row] = 0; //board change only temporary
        //                BottomPos[Col] += 1;

        //                Max = Math.Max(Max, Score);
        //                alpha = Math.Max(alpha, Max);
        //                if (alpha >= beta) return alpha;
        //            }
        //        }
        //        return Score;
        //    }
        //    return -1;
        //}

        private async void Timer(int Turn) //timer for turn
        {
            Time = Info.GameSettings[2] + 0.01;
            while (Turn == GameTurn && Start == true)
            {
                await Task.Delay(10);
                Time -= 0.01;
                lblTimer.Content = "Time: " + Time.ToString("n2");
                if (Time <= 0)
                {
                    Utils.PlaySound("Fail", Info.SoundPlayers[3]);
                    Time = Info.GameSettings[2];

                    NextTurn();
                }
            }
        }

        private void NextTurn()
        {
            if (Start == true)
            {
                if (Info.GameRules[2]) GameTurn = r.Next(2); //swaps turn depending on setting
                else GameTurn = 1 - GameTurn;

                if(!Info.GameRules[0]) Timer(GameTurn); //timer if not inverse
                IconPaint(rectTurnIcon, rectTurnColour, GameTurn); //paints next turn tile

                if (Info.GameRules[4] == true && Start == true && GameTurn == 1) AIMove(); //does ai turn;
                else if (Info.GameRules[3] == true && Start == true && GameTurn == 0) AIMove(); //does ai turn;
            }
        }

        private void TileFall(Rectangle Tile, Rectangle ColourTile) //animation for connect 4 falling
        {
            Storyboard FallBoard = new Storyboard(); //storyboard will contain 2 animations for both tiles (image + colour)
            ThicknessAnimation[] animations = new ThicknessAnimation[2];
            Rectangle[] Tiles = new Rectangle[] { Tile, ColourTile };

            double Fall = Tile.ActualHeight * (Tile.MinWidth+1);

            for (int i = 0; i < 2; i++) //uses a for loop to create 2 animations
            {
                animations[i] = new ThicknessAnimation();
                animations[i].From = new Thickness(0, -Fall, 0, Fall);
                animations[i].To = new Thickness(0, 0, 0, 0);
                animations[i].AccelerationRatio = 0.6;
                animations[i].Duration = TimeSpan.FromSeconds(Math.Sqrt(Tile.MinWidth/20)); //calculates fall time needed
                Storyboard.SetTargetProperty(animations[i], new PropertyPath(MarginProperty));
                Storyboard.SetTarget(animations[i], Tiles[i]);
                FallBoard.Children.Add(animations[i]);
            }
            FallBoard.Begin(); //does animation WOW!!!
        }
    }
}