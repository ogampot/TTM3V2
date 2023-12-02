﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static int boardSize = 8;
        private static int boardTileSize = 55;

        private static int tileSize = 50;
        private static int tileBorderThickness = 2;

        private Tile[,] boardTiles = new Tile[boardSize, boardSize];

        private Tile selectedTile;

        private List<Tile> tilesToClear = new List<Tile>();

        private FigureType[] figureTypes =
        {
            new FigureType(0, new BitmapImage(new Uri("pack://application:,,,/Images/circle.png"))),
            new FigureType(1, new BitmapImage(new Uri("pack://application:,,,/Images/triangle.png"))),
            new FigureType(2, new BitmapImage(new Uri("pack://application:,,,/Images/square.png"))),
            new FigureType(3, new BitmapImage(new Uri("pack://application:,,,/Images/pentagon.png"))),
            new FigureType(4, new BitmapImage(new Uri("pack://application:,,,/Images/hexagon.png")))
        };

        private ResourceDictionary resourceDictionary = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary.xaml", UriKind.Relative));

        private Style defaultTileButtonStyle;
        private Style selectedTileButtonStyle;

        private int dropAnimationInProcess = 0;
        private int successAnimationInProcess = 0;
        private int failAnimationInProcess = 0;
        private int clearAnimationInProcess = 0;

        public MainWindow()
        {
            InitializeComponent();

            GlobalEvents.OnTileButtonClicked += SelectTile;

            defaultTileButtonStyle = (Style)resourceDictionary["DefaultTileButton"];
            selectedTileButtonStyle = (Style)resourceDictionary["SelectedTileButton"];

            Start();
        }

        private void Start()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            this.Width = boardSize * boardTileSize + 50;
            this.Height = boardSize * boardTileSize + 100;

            MainCanvas.Width = boardSize * boardTileSize;
            MainCanvas.Height = boardSize * boardTileSize;

            FillBoard(1000);
        }

        private void FillBoard(int dropDuration)
        {
            if (dropAnimationInProcess > 0) return;

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

        private void InitializeFigures(int dropDuration)
        {
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

                        boardTiles[i, j].SetFigureType(randomType);

                        DropAnimation(boardTiles[i, j], dropDuration);
                    }
                }
            }

            CheckMatchWithClear();
        }

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

                ResetDropAnimations(duration);
            };

            tile.Figure.BeginAnimation(Canvas.TopProperty, animation);
        }

        private async void ResetDropAnimations(int delay)
        {
            await Task.Delay(delay);
            dropAnimationInProcess = 0;
        }

        private void SelectTile(Tile tile)
        {
            //Debug.WriteLine(selectedTile == null);
            //Debug.WriteLine("F: " + failAnimationInProcess + " | S: " + successAnimationInProcess + " | D: " + dropAnimationInProcess + " | C: " + clearAnimationInProcess);

            //Debug.WriteLine(tile.Position.X + ":" + tile.Position.Y);

            //Debug.WriteLine(tile.Figure.Style == defaultTileButtonStyle);

            if (dropAnimationInProcess + successAnimationInProcess + failAnimationInProcess + clearAnimationInProcess > 0) return;

            if (selectedTile != null)
            {
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
            SwapFiguresWithoutImage(tile1, tile2);

            tilesToClear = CheckMatch();

            SwapFiguresWithoutImage(tile1, tile2);

            if(tilesToClear.Count > 0)
            {
                SuccessAnimation(tile1, tile2, duration);
            }
            else
            {
                FailAnimation(tile1, tile2, duration);
            }
        }

        private void SwapFigures(Tile tile1, Tile tile2)
        {
            FigureType type1 = tile1.Figure.FigureType;
            tile1.SetFigureType(tile2.Figure.FigureType);
            tile2.SetFigureType(type1);
        }

        private void SwapFiguresWithoutImage(Tile tile1, Tile tile2)
        {
            FigureType type1 = tile1.Figure.FigureType;
            tile1.SetFigureTypeWithoutImage(tile2.Figure.FigureType);
            tile2.SetFigureTypeWithoutImage(type1);
        }

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

        private List<Tile> CheckMatch()
        {
            List<Tile> tiles = new List<Tile>();
            List<Tile> tilesMatched = new List<Tile>();

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

            return tilesMatched;
        }

        private void CheckMatchWithClear()
        {
            tilesToClear = CheckMatch();

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

                MainCanvas.Children.Remove(boardTiles[tile.Position.X, tile.Position.Y].Figure);
                boardTiles[tile.Position.X, tile.Position.Y] = null;

                PrepareTilesToShift();
            };

            tile.Figure.BeginAnimation(OpacityProperty, animation);
        }

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
                        ShiftAnimation(boardTiles[x, y], 200);
                    }
                }
            }
        }

        public void ShiftAnimation(Tile tile, int duration)
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

                ResetDropAnimations(duration);
            };

            tile.Figure.BeginAnimation(Canvas.TopProperty, animation);
        }
    }
}
