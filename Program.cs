// See https://aka.ms/new-console-template for more information

using Business;
using Business.Events;
using Business.Exceptions;
using Business.Models;
using Business.Services;
using DataAccess;

class Program
    {
        private static MemoryGameService _game;
        private static IHighScoreRepository _highScoreRepository;
        private static ConsoleColor[] _cardColors = new[]
        {
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Blue,
            ConsoleColor.Yellow,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan
        };

        static void Main(string[] args)
        {
            Console.Title = "Memory Game";
            
            // Initialize high score repository
            string highScoreFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MemoryGame",
                "highscores.json");
            _highScoreRepository = new JsonHighScoreRepository(highScoreFilePath);

            bool playAgain = true;
            while (playAgain)
            {
                Console.Clear();
                DrawTitle();
                
                // Ask player name
                Console.Write("Enter your name: ");
                string playerName = Console.ReadLine() ?? "Player";
                Player player = new Player(playerName);

                // Ask for number of pairs
                int pairCount;
                do
                {
                    Console.Write("Enter number of pairs (5-15): ");
                } while (!int.TryParse(Console.ReadLine(), out pairCount) || pairCount < 5 || pairCount > 15);

                // Start game
                _game = new MemoryGameService(player, pairCount, _highScoreRepository);
                
                // Subscribe to events
                _game.CardFlipped += Game_CardFlipped;
                _game.MatchFound += Game_MatchFound;
                _game.MatchFailed += Game_MatchFailed;
                _game.GameCompleted += Game_GameCompleted;

                PlayGame();

                // Ask to play again
                Console.WriteLine("\nWould you like to play again? (y/n)");
                playAgain = Console.ReadKey(true).Key == ConsoleKey.Y;
            }
        }

        private static void PlayGame()
        {
            bool gameRunning = true;

            while (gameRunning && _game.RemainingPairs > 0)
            {
                Console.Clear();
                DrawGameInfo();
                DrawCards();

                Console.WriteLine("\nSelect first card (or press 'Q' to quit):");
                int firstCardIndex = GetCardSelection();
                if (firstCardIndex == -1)
                {
                    gameRunning = false;
                    break;
                }

                try
                {
                    _game.FlipCard(firstCardIndex);
                    
                    Console.Clear();
                    DrawGameInfo();
                    DrawCards();

                    // If game is over, exit the loop
                    if (_game.RemainingPairs == 0)
                        break;

                    Console.WriteLine("\nSelect second card (or press 'Q' to quit):");
                    int secondCardIndex = GetCardSelection();
                    if (secondCardIndex == -1)
                    {
                        gameRunning = false;
                        break;
                    }

                    _game.FlipCard(secondCardIndex);
                    
                    Console.Clear();
                    DrawGameInfo();
                    DrawCards();

                    // Wait for a moment to show the second card
                    Thread.Sleep(1500);

                    // Flip unmatched cards back
                    for (int i = 0; i < _game.Cards.Count; i++)
                    {
                        var card = _game.Cards[i];
                        if (card.IsFaceUp && !card.IsMatched)
                        {
                            _game.FlipCard(i);
                        }
                    }
                }
                catch (InvalidGameOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                }
            }

            if (!gameRunning)
            {
                Console.WriteLine("Game aborted.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        private static int GetCardSelection()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Q)
                    return -1;

                if (char.IsDigit(keyInfo.KeyChar))
                {
                    int index = int.Parse(keyInfo.KeyChar.ToString());
                    
                    // Adjust for 0-based indexing
                    index--;
                    
                    if (index >= 0 && index < _game.Cards.Count)
                    {
                        var card = _game.Cards[index];
                        if (!card.IsFaceUp && !card.IsMatched)
                            return index;
                    }
                }
            }
        }

        private static void DrawTitle()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  __  __                                  _____                      
 |  \/  |                                / ____|                     
 | \  / | ___ _ __ ___   ___  _ __ _   _| |  __  __ _ _ __ ___   ___ 
 | |\/| |/ _ \ '_ ` _ \ / _ \| '__| | | | | |_ |/ _` | '_ ` _ \ / _ \
 | |  | |  __/ | | | | | (_) | |  | |_| | |__| | (_| | | | | | |  __/
 |_|  |_|\___|_| |_| |_|\___/|_|   \__, |\_____|\_\__,_|_| |_| |_|\___|
                                     __/ |                           
                                    |___/                            
");
            Console.ResetColor();
        }

        private static void DrawGameInfo()
        {
            Console.WriteLine($"Pairs remaining: {_game.RemainingPairs}");
            Console.WriteLine($"Attempts: {_game.Attempts}");
            Console.WriteLine($"Time: {_game.Duration.TotalSeconds:F1} seconds");
            Console.WriteLine();
        }

        private static void DrawCards()
        {
            int cardCount = _game.Cards.Count;
            int columns = (int)Math.Ceiling(Math.Sqrt(cardCount));
            int rows = (int)Math.Ceiling((double)cardCount / columns);

            for (int row = 0; row < rows; row++)
            {
                // Draw card tops
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < cardCount)
                    {
                        Console.Write("+-----+ ");
                    }
                }
                Console.WriteLine();

                // Draw card content
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < cardCount)
                    {
                        Card card = _game.Cards[index];
                        
                        if (card.IsFaceUp)
                        {
                            int symbolIndex = int.Parse(card.Symbol) - 1;
                            Console.ForegroundColor = _cardColors[symbolIndex % _cardColors.Length];
                            Console.Write($"|  {card.Symbol}  | ");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write("|     | ");
                        }
                    }
                }
                Console.WriteLine();

                // Draw card numbers
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < cardCount)
                    {
                        Console.Write($"|  {index + 1}  | ");
                    }
                }
                Console.WriteLine();

                // Draw card bottoms
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < cardCount)
                    {
                        Console.Write("+-----+ ");
                    }
                }
                Console.WriteLine();
            }
        }

        private static void ShowHighScores()
        {
            Console.Clear();
            Console.WriteLine("===== HIGH SCORES =====");
            
            var highScores = _highScoreRepository.GetTopHighScores(10);
            
            if (highScores.Count == 0)
            {
                Console.WriteLine("No high scores yet.");
            }
            else
            {
                Console.WriteLine($"{"Rank",-4} {"Player",-15} {"Score",-7} {"Cards",-5} {"Attempts",-8} {"Time",-10} {"Date",-10}");
                Console.WriteLine(new string('-', 70));
                
                for (int i = 0; i < highScores.Count; i++)
                {
                    var score = highScores[i];
                    Console.WriteLine($"{i+1,-4} {score.PlayerName,-15} {score.Score,-7} {score.CardCount,-5} {score.Attempts,-8} {score.Duration.TotalSeconds:F1}s{"",-5} {score.Date.ToShortDateString(),-10}");
                }
            }
            
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        #region Event Handlers

        private static void Game_CardFlipped(object sender, CardFlippedEventArgs e)
        {
            // Nothing to do here, as we're redrawing the entire game board
        }

        private static void Game_MatchFound(object sender, MatchFoundEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nMatch found!");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        private static void Game_MatchFailed(object sender, MatchFailedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nNo match. Try again!");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        private static void Game_GameCompleted(object sender, GameCompletedEventArgs e)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Congratulations! You've completed the game!");
            Console.ResetColor();
            
            Console.WriteLine($"\nPlayer: {e.Player.Name}");
            Console.WriteLine($"Score: {e.Score}");
            Console.WriteLine($"Cards: {e.CardCount}");
            Console.WriteLine($"Attempts: {e.Attempts}");
            Console.WriteLine($"Time: {e.Duration.TotalSeconds:F1} seconds");
            
            if (e.IsHighScore)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nNew High Score!");
                Console.ResetColor();
            }
            
            Console.WriteLine("\nPress any key to see high scores...");
            Console.ReadKey(true);
            
            ShowHighScores();
        }

        #endregion
    }