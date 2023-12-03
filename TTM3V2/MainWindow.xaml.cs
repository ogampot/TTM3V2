using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TTM3V2
{
    public partial class MainWindow : Window
    {
        private static Random rand = new Random();


        // Size of game field/board
        private static int boardSize = 8;

        // Size of single cell of the game field/board
        private static int boardTileSize = 55;


        // Size of tile (which will be set in cells of the game field/board) 
        private static int tileSize = 50;
        private static int tileBorderThickness = 2;


        // Main array of tiles
        private Tile[,] boardTiles = new Tile[boardSize, boardSize];


        private Tile selectedTile;

        // Tiles which will be cleared
        private List<Tile> tilesToClear = new List<Tile>();


        // Types of figures
        private FigureType[] figureTypes =
        {
            new FigureType(Brushes.Red, new BitmapImage(new Uri("pack://application:,,,/Images/circle.png"))),
            new FigureType(Brushes.Yellow, new BitmapImage(new Uri("pack://application:,,,/Images/triangle.png"))),
            new FigureType(Brushes.CornflowerBlue, new BitmapImage(new Uri("pack://application:,,,/Images/square.png"))),
            new FigureType(Brushes.Magenta, new BitmapImage(new Uri("pack://application:,,,/Images/pentagon.png"))),
            new FigureType(Brushes.Lime, new BitmapImage(new Uri("pack://application:,,,/Images/hexagon.png")))
        };


        // Bomb setup
        private ImageSource bombImage = new BitmapImage(new Uri("pack://application:,,,/Images/bomb.png"));
        private int bombExplosionDelay = 250;
        private int matchesCountToSpawnBomb = 5;

        // Line setup
        private ImageSource lineImageV = new BitmapImage(new Uri("pack://application:,,,/Images/line.png"));
        private ImageSource lineImageH = new BitmapImage(new Uri("pack://application:,,,/Images/lineHorizontal.png"));
        private int lineActivationDelay = 50;
        private int matchesCountToSpawnLine = 4;


        // Dictionary of styles for tiles
        private ResourceDictionary resourceDictionary = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary.xaml", UriKind.Relative));

        // Styles for tiles
        private Style defaultTileButtonStyle;
        private Style selectedTileButtonStyle;


        // Counters of animations in process
        private int dropAnimationInProcess = 0;
        private int successAnimationInProcess = 0;
        private int failAnimationInProcess = 0;
        private int clearAnimationInProcess = 0;
        private int specialAnimationInProcess = 0;


        // Score counter
        private int scorePerTile = 10;
        private ScoreCounter scoreCounter;


        // Timer
        private TimeSpan timeLeft = TimeSpan.FromMinutes(1);
        private Timer timer;


        public MainWindow()
        {
            InitializeComponent();

            GlobalEvents.OnTileButtonClicked += SelectTile;

            scoreCounter = new ScoreCounter(this.ScoreLabel);
            timer = new Timer(this, this.TimeLabel, timeLeft);

            defaultTileButtonStyle = (Style)resourceDictionary["DefaultTileButton"];
            selectedTileButtonStyle = (Style)resourceDictionary["SelectedTileButton"];

            Start();
        }

        private void Start()
        {
            InitializeBoard();
        }

        // Setup of the game field/board (and of the main canvas)
        private void InitializeBoard()
        {
            this.Width = boardSize * boardTileSize + 50;
            this.Height = boardSize * boardTileSize + this.ScoreLabel.Height + this.TimeLabel.Height + 50;

            MainCanvas.Width = boardSize * boardTileSize;
            MainCanvas.Height = boardSize * boardTileSize;

            FillBoard(1000);
        }

        // Fill empty tiles on the game field/board
        private void FillBoard(int dropDuration)
        {
            if (dropAnimationInProcess + clearAnimationInProcess + specialAnimationInProcess > 0) return;

            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (boardTiles[x, y] == null)
                    {
                        Tile tile = new Tile(new Vector2(x, y), tileSize, tileBorderThickness);

                        Canvas.SetLeft(tile.Figure, x * boardTileSize);
                        Canvas.SetTop(tile.Figure, y * boardTileSize);

                        boardTiles[x, y] = tile;
                        MainCanvas.Children.Add(tile.Figure);
                    }
                }
            }

            InitializeFigures(dropDuration);
        }

        // Setup figures of every tile
        private void InitializeFigures(int dropDuration)
        {
            if(clearAnimationInProcess + dropAnimationInProcess > 0) return;

            FigureType[] previousTypes = new FigureType[] { GetRandomFigureType(figureTypes), GetRandomFigureType(figureTypes) };

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (boardTiles[i, j].Status == Tile.TileStatus.Empty)
                    {
                        FigureType randomType = GetRandomFigureType(figureTypes);

                        if (randomType == previousTypes[0] && randomType == previousTypes[1])
                            randomType = GetRandomFigureType(figureTypes, randomType);

                        previousTypes[1] = previousTypes[0];
                        previousTypes[0] = randomType;

                        if (i > 1)
                        {
                            randomType = GetRandomFigureType(figureTypes, previousTypes[1], boardTiles[i - 1, j].Figure.FigureType);

                            previousTypes[1] = previousTypes[0];
                            previousTypes[0] = randomType;
                        }

                        boardTiles[i, j].SetFigureType(randomType, null, null);

                        DropAnimation(boardTiles[i, j], dropDuration);
                    }
                }
            }

            int countOfNull = 0;

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (boardTiles[i, j] == null) countOfNull++;
                }
            }

            if(countOfNull > 0)
            {
                FillBoard(dropDuration);
                return;
            }

            CheckMatchWithClear();
        }


        // Take random figure
        private FigureType GetRandomFigureType(FigureType[] types)
        {
            FigureType result = types[rand.Next(figureTypes.Length)];
            return result;
        }

        private FigureType GetRandomFigureType(FigureType[] types, FigureType excluded)
        {
            List<FigureType> figureTypes = new List<FigureType>();

            foreach (FigureType type in types)
            {
                if (type == excluded) continue;
                figureTypes.Add(type);
            }

            FigureType result = types[rand.Next(types.Length)];

            if (figureTypes.Count > 0) result = figureTypes[rand.Next(figureTypes.Count)];

            return result;
        }

        private FigureType GetRandomFigureType(FigureType[] types, FigureType excluded1, FigureType excluded2)
        {
            List<FigureType> figureTypes = new List<FigureType>();

            foreach (FigureType type in types)
            {
                if (type == excluded1 || type == excluded2) continue;
                figureTypes.Add(type);
            }

            FigureType result = types[rand.Next(types.Length)];

            if (figureTypes.Count > 0) result = figureTypes[rand.Next(figureTypes.Count)];

            return result;
        }


        // Animation for tiles that are just filled
        public void DropAnimation(Tile tile, int duration)
        {
            dropAnimationInProcess++;

            double startPosition = Canvas.GetTop(tile.Figure);

            DoubleAnimation animation = new DoubleAnimation
            {
                From = -(boardSize * boardTileSize - startPosition),
                To = startPosition,
                Duration = TimeSpan.FromMilliseconds(duration),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += delegate
            {
                tile.Figure.BeginAnimation(Canvas.TopProperty, null);
                dropAnimationInProcess--;
            };

            tile.Figure.BeginAnimation(Canvas.TopProperty, animation);
        }


        // Logic of tile selection
        private void SelectTile(Tile tile)
        {
            //Debug.WriteLine("F: " + failAnimationInProcess + " | S: " + successAnimationInProcess + 
            //    " | D: " + dropAnimationInProcess + " | C: " + clearAnimationInProcess + 
            //    " | Sp: " + specialAnimationInProcess);

            if (dropAnimationInProcess + successAnimationInProcess + failAnimationInProcess + clearAnimationInProcess > 0) return;

            if (selectedTile != null)
            {
                if(tile == selectedTile) return;

                StopSelectionAnimation(selectedTile);

                SwapTiles(selectedTile, tile, 200);

                selectedTile = null;

                return;
            }

            selectedTile = tile;

            StartSelectionAnimation(tile);
        }

        private void StartSelectionAnimation(Tile tile)
        {
            tile.Figure.SetButtonStyle(selectedTileButtonStyle);

            double startWidth = tile.Figure.TileImage.Width;

            var animation = new DoubleAnimation
            {
                From = startWidth,
                To = startWidth - 10,
                Duration = TimeSpan.FromMilliseconds(300),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };

            tile.Figure.TileImage.BeginAnimation(WidthProperty, animation);
            tile.Figure.TileImage.BeginAnimation(HeightProperty, animation);
        }

        private void StopSelectionAnimation(Tile tile)
        {
            tile.Figure.SetButtonStyle(defaultTileButtonStyle);

            tile.Figure.TileImage.BeginAnimation(WidthProperty, null);
            tile.Figure.TileImage.BeginAnimation(HeightProperty, null);
        }


        // Tile swapping
        private void SwapTiles(Tile tile1, Tile tile2, int durationMilliseconds)
        {
            if (selectedTile == null) return;

            if (tile1.Position.X != tile2.Position.X && tile1.Position.Y != tile2.Position.Y) return;

            if (tile1.Position.X < tile2.Position.X - 1 || tile1.Position.X > tile2.Position.X + 1
                || tile1.Position.Y < tile2.Position.Y - 1 || tile1.Position.Y > tile2.Position.Y + 1) return;

            TryToSwapTiles(tile1, tile2, durationMilliseconds);
        }

        private void TryToSwapTiles(Tile tile1, Tile tile2, int duration)
        {
            SwapFigures(tile1, tile2);

            tilesToClear = CheckMatch(false);

            SwapFigures(tile1, tile2);

            if(tilesToClear.Count > 0)
            {
                tile1.AddMoveCount();
                tile2.AddMoveCount();

                SuccessAnimation(tile1, tile2, duration);
            }
            else
            {
                FailAnimation(tile1, tile2, duration);
            }
        }

        private void SwapFigures(Tile tile1, Tile tile2)
        {
            FigureType figureType = tile1.Figure.FigureType;
            ImageSource specialImage = tile1.Figure.SpecialImage;
            Action<Vector2> action = tile1.Figure.OnClearEvent;

            tile1.SetFigureType(tile2.Figure.FigureType, tile2.Figure.SpecialImage, tile2.Figure.OnClearEvent);
            tile2.SetFigureType(figureType, specialImage, action);
        }

        // Swap animation setup
        public void SwapAnimation(Tile tile1, Tile tile2, int durationMilliseconds, Action<Tile, Tile, int> onComplete)
        {
            double startPositionTile1X = Canvas.GetLeft(tile1.Figure);
            double startPositionTile1Y = Canvas.GetTop(tile1.Figure);

            double startPositionTile2X = Canvas.GetLeft(tile2.Figure);
            double startPositionTile2Y = Canvas.GetTop(tile2.Figure);

            DoubleAnimation animationForTile1 = new DoubleAnimation();
            animationForTile1.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);

            DoubleAnimation animationForTile2 = new DoubleAnimation();
            animationForTile2.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);

            animationForTile1.Completed += delegate
            {
                onComplete?.Invoke(tile1, tile2, durationMilliseconds);
            };

            animationForTile2.Completed += delegate
            {
                onComplete?.Invoke(tile1, tile2, durationMilliseconds);
            };

            if (tile1.Position.X != tile2.Position.X && tile1.Position.Y == tile2.Position.Y)
            {
                animationForTile1.From = startPositionTile1X;
                animationForTile1.To = startPositionTile2X;

                animationForTile2.From = startPositionTile2X;
                animationForTile2.To = startPositionTile1X;

                tile1.Figure.BeginAnimation(Canvas.LeftProperty, animationForTile1);
                tile2.Figure.BeginAnimation(Canvas.LeftProperty, animationForTile2);
            }
            else if (tile1.Position.X == tile2.Position.X && tile1.Position.Y != tile2.Position.Y)
            {
                animationForTile1.From = startPositionTile1Y;
                animationForTile1.To = startPositionTile2Y;

                animationForTile2.From = startPositionTile2Y;
                animationForTile2.To = startPositionTile1Y;

                tile1.Figure.BeginAnimation(Canvas.TopProperty, animationForTile1);
                tile2.Figure.BeginAnimation(Canvas.TopProperty, animationForTile2);
            }
            else
            {
                return;
            }
        }

        private void SuccessAnimation(Tile tile1, Tile tile2, int duration)
        {
            successAnimationInProcess += 2;

            SwapAnimation(tile1, tile2, duration, SuccessAnimationCompleted);
        }

        private void SuccessAnimationCompleted(Tile tile1, Tile tile2, int duration)
        {
            successAnimationInProcess--;

            if (successAnimationInProcess == 0)
            {
                tile1.Figure.BeginAnimation(Canvas.LeftProperty, null);
                tile2.Figure.BeginAnimation(Canvas.LeftProperty, null);

                tile1.Figure.BeginAnimation(Canvas.TopProperty, null);
                tile2.Figure.BeginAnimation(Canvas.TopProperty, null);

                SwapFigures(tile1, tile2);

                CheckMatchWithClear();
            }
        }

        private void FailAnimation(Tile tile1, Tile tile2, int duration)
        {
            failAnimationInProcess += 2;

            SwapAnimation(tile1, tile2, duration, FailAnimationCompleted);
        }

        private void FailAnimationCompleted(Tile tile1, Tile tile2, int duration)
        {
            failAnimationInProcess--;

            if (failAnimationInProcess != 0) return;

            failAnimationInProcess += 2;

            SwapAnimation(tile1, tile2, duration, delegate { failAnimationInProcess--; });
        }


        // Checking of matches
        private List<Tile> CheckMatch(bool needToMakeSpecials)
        {
            List<Tile> tiles = new List<Tile>();
            List<Tile> tilesMatched = new List<Tile>();

            // Dictionaries for spawning bonuses
            Dictionary<FigureType, List<Tile>> matchedTilesHorizontal = new Dictionary<FigureType, List<Tile>>();
            Dictionary<FigureType, List<Tile>> matchedTilesVertical = new Dictionary<FigureType, List<Tile>>();
            Dictionary<FigureType, List<Tile>> matchedTilesBoth = new Dictionary<FigureType, List<Tile>>();

            foreach(FigureType type in figureTypes)
            {
                matchedTilesHorizontal[type] = new List<Tile>();
                matchedTilesVertical[type] = new List<Tile>();
                matchedTilesBoth[type] = new List<Tile>();
            }

            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (boardTiles[x, y] == null) continue;

                    if (tiles.Count == 0) tiles.Add(boardTiles[x, y]);
                    else
                    {
                        if (tiles[tiles.Count - 1].Figure.FigureType == boardTiles[x, y].Figure.FigureType)
                        {
                            tiles.Add(boardTiles[x, y]);
                        }
                        else
                        { 
                            if (tiles.Count >= 3)
                            {
                                foreach (Tile tile in tiles)
                                {
                                    tile.ChangeStatus(Tile.TileStatus.NeedToBeClean);
                                    tilesMatched.Add(tile);

                                    if (matchedTilesHorizontal.ContainsKey(tile.Figure.FigureType))
                                    {
                                        matchedTilesHorizontal[tile.Figure.FigureType].Add(tile);
                                    }

                                    if (matchedTilesBoth.ContainsKey(tile.Figure.FigureType))
                                    {
                                        matchedTilesBoth[tile.Figure.FigureType].Add(tile);
                                    }
                                }
                            }

                            tiles.Clear();
                            tiles.Add(boardTiles[x, y]);
                        }
                    }
                }

                if (tiles.Count >= 3)
                {
                    foreach (Tile tile in tiles)
                    {
                        tile.ChangeStatus(Tile.TileStatus.NeedToBeClean);
                        tilesMatched.Add(tile);
                    }
                }

                tiles.Clear();
            }

            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    if (boardTiles[x, y] == null) continue;

                    if (tiles.Count == 0) tiles.Add(boardTiles[x, y]);
                    else
                    {
                        if (tiles[tiles.Count - 1].Figure.FigureType == boardTiles[x, y].Figure.FigureType)
                        {
                            tiles.Add(boardTiles[x, y]);
                        }
                        else
                        {
                            if (tiles.Count >= 3)
                            {
                                foreach (Tile tile in tiles)
                                {
                                    tile.ChangeStatus(Tile.TileStatus.NeedToBeClean);
                                    tilesMatched.Add(tile);

                                    if (matchedTilesVertical.ContainsKey(tile.Figure.FigureType))
                                    {
                                        matchedTilesVertical[tile.Figure.FigureType].Add(tile);
                                    }

                                    if (matchedTilesBoth.ContainsKey(tile.Figure.FigureType))
                                    {
                                        matchedTilesBoth[tile.Figure.FigureType].Add(tile);
                                    }
                                }
                            }

                            tiles.Clear();
                            tiles.Add(boardTiles[x, y]);
                        }
                    }
                }

                if (tiles.Count >= 3)
                { 
                    foreach (Tile tile in tiles)
                    {
                        tile.ChangeStatus(Tile.TileStatus.NeedToBeClean);
                        tilesMatched.Add(tile);
                    }
                }

                tiles.Clear();
            }

            if (needToMakeSpecials == true)
            {
                foreach (var mt in matchedTilesHorizontal)
                {
                    if (mt.Value.Count > 0)
                    {
                        if (mt.Value.Count == 4) CheckToSpawnLineVertical(mt.Value);
                    }
                }

                foreach (var mt in matchedTilesVertical)
                {
                    if (mt.Value.Count > 0)
                    {
                        if (mt.Value.Count == 4) CheckToSpawnLineHorizontal(mt.Value);
                    }
                }

                int check = 0;

                foreach (var mt in matchedTilesBoth)
                {
                    if (mt.Value.Count > 0)
                    {
                        if(mt.Value.Count >= 3) check++;
                        if (mt.Value.Count >= 5 || check > 1) CheckToSpawnBombs(mt.Value, check);
                    }
                }
            }

            return tilesMatched;
        }

        // Checking to add bonuses
        private void CheckToSpawnBombs(List<Tile> tiles, int check)
        {
            if (tiles.Count >= matchesCountToSpawnBomb || check > 1)
            {
                Tile tileWithBiggestMoveCount = tiles[0];

                foreach(var tile in tiles)
                {
                    if (tile.MovedByPlayerCount > tileWithBiggestMoveCount.MovedByPlayerCount)
                        tileWithBiggestMoveCount = tile;
                }

                Vector2 position = tileWithBiggestMoveCount.Position;

                boardTiles[position.X, position.Y].ResetMoveCount();
                boardTiles[position.X, position.Y].ChangeStatus(Tile.TileStatus.Full);
                boardTiles[position.X, position.Y].SetFigureType(boardTiles[position.X, position.Y].Figure.FigureType, bombImage, BombExplosion);
            }
        }

        private void CheckToSpawnLineHorizontal(List<Tile> tiles)
        {
            CheckToSpawnLine(tiles, LineActivationHorizontal, lineImageH);
        }

        private void CheckToSpawnLineVertical(List<Tile> tiles)
        {
            CheckToSpawnLine(tiles, LineActivationVertical, lineImageV);
        }

        private void CheckToSpawnLine(List<Tile> tiles, Action<Vector2> action, ImageSource image)
        {
            if (tiles.Count == matchesCountToSpawnLine)
            {
                Tile tileWithBiggestMoveCount = tiles[0];

                foreach (var tile in tiles)
                {
                    if (tile.MovedByPlayerCount > tileWithBiggestMoveCount.MovedByPlayerCount)
                        tileWithBiggestMoveCount = tile;
                }

                Vector2 position = tileWithBiggestMoveCount.Position;

                boardTiles[position.X, position.Y].ResetMoveCount();
                boardTiles[position.X, position.Y].ChangeStatus(Tile.TileStatus.Full);
                boardTiles[position.X, position.Y].SetFigureType(boardTiles[position.X, position.Y].Figure.FigureType, image, action);
            }
        }


        // Clear of matched tiles
        private void CheckMatchWithClear()
        {
            tilesToClear = CheckMatch(true);

            if (tilesToClear.Count > 0)
            {
                ClearMatches();
            }
        }

        private void ClearMatches()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (boardTiles[x, y] == null) continue;

                    if (boardTiles[x, y].Status == Tile.TileStatus.NeedToBeClean) 
                        ClearAnimation(boardTiles[x, y]);
                }
            }
        }

        // Clear animation setup
        private void ClearAnimation(Tile tile)
        {
            clearAnimationInProcess++;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(400)
            };

            animation.Completed += delegate
            {
                clearAnimationInProcess--;

                tile.Figure.OnClearEvent?.Invoke(tile.Position);

                MainCanvas.Children.Remove(tile.Figure);
                boardTiles[tile.Position.X, tile.Position.Y] = null;

                GlobalEvents.SendTileClean(scorePerTile);

                PrepareTilesToShift();
            };

            if(tile != null) tile.Figure.BeginAnimation(OpacityProperty, animation);
            else clearAnimationInProcess--;
        }

        // Tiles shifting
        private void PrepareTilesToShift()
        {
            if (clearAnimationInProcess > 0) return;

            for (int x = 0; x < boardSize; x++)
            {
                for (int y = boardSize - 1; y > 0; y--)
                {
                    if (boardTiles[x, y] == null)
                    {
                        if (boardTiles[x, y - 1] != null)
                        {
                            boardTiles[x, y] = boardTiles[x, y - 1];
                            boardTiles[x, y - 1] = null;

                            boardTiles[x, y].SetNewPosition(new Vector2(x, y));

                            PrepareTilesToShift();

                            return;
                        }
                    }
                }
            }

            ShiftTiles();
        }

        private void ShiftTiles()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    if (boardTiles[x, y] != null)
                    {
                        ShiftAnimation(boardTiles[x, y], 300);
                    }
                }
            }
        }

        // Tile shift animation setup
        private void ShiftAnimation(Tile tile, int duration)
        {
            dropAnimationInProcess++;

            DoubleAnimation animation = new DoubleAnimation
            {
                From = Canvas.GetTop(tile.Figure),
                To = tile.Position.Y * boardTileSize,
                Duration = TimeSpan.FromMilliseconds(duration),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += delegate
            {
                dropAnimationInProcess--;

                Canvas.SetTop(tile.Figure, tile.Position.Y * boardTileSize);
                tile.Figure.BeginAnimation(Canvas.TopProperty, null);

                FillBoard(400);
            };

            tile.Figure.BeginAnimation(Canvas.TopProperty, animation);
        }


        // Bonuses logic
        private async void BombExplosion(Vector2 position)
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (i >= position.X - 1 && i <= position.X + 1
                        && j >= position.Y - 1 && j <= position.Y + 1)
                    {
                        specialAnimationInProcess++;

                        Ellipse explosion = new Ellipse();

                        explosion.Width = boardTileSize;
                        explosion.Height = boardTileSize;

                        explosion.Fill = Brushes.DarkOrange;

                        Canvas.SetLeft(explosion, i * boardTileSize);
                        Canvas.SetTop(explosion, j * boardTileSize);

                        MainCanvas.Children.Add(explosion);

                        DoubleAnimation animation = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(bombExplosionDelay * 2)
                        };

                        animation.Completed += delegate
                        {
                            specialAnimationInProcess--;
                            MainCanvas.Children.Remove(explosion);
                        };

                        explosion.BeginAnimation(Ellipse.OpacityProperty, animation);
                    }
                }
            }

            await Task.Delay(bombExplosionDelay);

            for (int i = 0; i < boardSize; i++)
            {
                for(int j = 0; j < boardSize; j++)
                {
                    if (i >= position.X - 1 && i <= position.X + 1
                        && j >= position.Y - 1 && j <= position.Y + 1)
                    {
                        if (boardTiles[i, j] != null || boardTiles[i, j].Status != Tile.TileStatus.Empty)
                        {
                            ClearAnimation(boardTiles[i, j]);
                        }
                    }
                }
            }
        }

        private void LineActivationHorizontal(Vector2 position)
        {
            LineActivation(position, true);
        }

        private void LineActivationVertical(Vector2 position)
        {
            LineActivation(position, false);
        }

        private async void LineActivation(Vector2 position, bool isHorizontal)
        {
            await Task.Delay(lineActivationDelay);

            for (int i = 0; i < 2; i++)
            {
                specialAnimationInProcess++;

                Ellipse destroyer = new Ellipse();

                destroyer.Width = boardTileSize;
                destroyer.Height = boardTileSize;

                destroyer.Fill = Brushes.OrangeRed;

                Canvas.SetLeft(destroyer, position.X * boardTileSize);
                Canvas.SetTop(destroyer, position.Y * boardTileSize);

                MainCanvas.Children.Add(destroyer);

                int endPosition = -100;
                if (i == 1) endPosition = boardSize * boardTileSize + 100;

                DoubleAnimation animationDestroyer = new DoubleAnimation
                {
                    From = (isHorizontal == true ? position.Y : position.X) * boardTileSize,
                    To = endPosition,
                    Duration = TimeSpan.FromMilliseconds(lineActivationDelay * 8)
                };

                animationDestroyer.Completed += delegate
                {
                    specialAnimationInProcess--;
                    MainCanvas.Children.Remove(destroyer);
                };

                destroyer.BeginAnimation(isHorizontal == true ? Canvas.LeftProperty : Canvas.TopProperty, animationDestroyer);
            }

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if ((j == position.Y && isHorizontal == true) || (i == position.X && isHorizontal == false))
                    {
                        if (boardTiles[i, j] != null)
                        {
                            specialAnimationInProcess++;

                            Ellipse explosion = new Ellipse();

                            explosion.Width = boardTileSize;
                            explosion.Height = boardTileSize;

                            explosion.Fill = Brushes.DarkOrange;

                            Canvas.SetLeft(explosion, i * boardTileSize);
                            Canvas.SetTop(explosion, j * boardTileSize);

                            MainCanvas.Children.Add(explosion);

                            DoubleAnimation animation = new DoubleAnimation
                            {
                                From = 1.0,
                                To = 0,
                                Duration = TimeSpan.FromMilliseconds(lineActivationDelay * 4)
                            };

                            animation.Completed += delegate
                            {
                                specialAnimationInProcess--;
                                MainCanvas.Children.Remove(explosion);
                            };

                            ClearAnimation(boardTiles[i, j]);

                            explosion.BeginAnimation(Ellipse.OpacityProperty, animation);
                        }
                    }
                }
            }
        }
    }
}