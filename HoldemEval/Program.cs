using System.Security.Cryptography;

namespace HoldemEval
{
    internal class Program
    {
        private static object llock = new object();
        
        static void Main(string[] args)
        {
            List<(int, int, bool, double)> results = [];
            List<Thread> tasks = [];
            /*for (var cardV1 = 14; cardV1 > 1; cardV1--)
            {
                for (var cardV2 = cardV1; cardV2 > 1; cardV2--)
                {
                    var c1 = cardV1;
                    var c2 = cardV2;
                    OneExecution(results, c1, c2, suited: false);
                    OneExecution(results, c1, c2, suited: true);
                }
            }*/
            var percc = OneExecutionRandom(results);
            foreach (var task in tasks)
            {
                task.Join();
            }
            Console.WriteLine($"win: {percc}%");
            return;
            var strs = new List<string>();
            foreach (var r in results.OrderByDescending(rr => rr.Item4))
            {
                var (c1, c2, suited, perc) = r;
                var str = $"{c1.CardName()}\t{c2.CardName()}\t{suited}\t{perc * 100:0.0}%";
                strs.Add(str);
                Console.WriteLine(str);
            }
            File.WriteAllLines($@"c:\tmp\poker-hands.tsv", strs);
        }
        private static int GetSecureRandomNumber(int minValue, int maxValue)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumber = new byte[4];
                rng.GetBytes(randomNumber);
                long value = BitConverter.ToInt64([.. randomNumber, .. new byte[] { 0, 0, 0, 0 }], 0);
                return (int)((Math.Abs(value) % (maxValue - minValue) + minValue));
            }
        }
        private static double OneExecutionRandom(List<(int, int, bool, double)> results, int iterations = 1000000)
        {
            for (int i = 0; i < iterations; i++)
            {
                if (i % 50000 == 0)
                {
                    Console.WriteLine($"{i:0000000} / {iterations}");
                }
                int c1 = GetSecureRandomNumber(2, 14);
                int c2 = GetSecureRandomNumber(2, 14);
                bool suited = Random.Shared.Next() % 2 == 0;
                OneExecution(results, c1, c2, suited, 1);
            }
            return results.Select(rr => rr.Item4).Sum() * 1.0 / results.Count();
        }
        private static void OneExecution(List<(int, int, bool, double)> results, int cardV1, int cardV2, bool suited, int iterations = 100000)
        {
            int p1Wins = 0;
            Hand? bestHand = null;
            for (int i = 0; i < iterations; i++)
            {
                var (p, hand) = RunGame(
                    new Card { Value = cardV1, Suit = Suit.Diamonds },
                    new Card { Value = cardV2, Suit = cardV2 == cardV1 ? Suit.Spades : (suited ? Suit.Diamonds : Suit.Hearts) },
                    2
                    );
                if (p.Name == "Player 1")
                {
                    p1Wins++;
                    if (bestHand == null || hand > bestHand)
                    {
                        bestHand = hand;
                    }
                }
                if (i % 5000 == 1)
                {
                    //Console.WriteLine($"{cardV1.CardName()} {cardV2.CardName()} {suited} ({i} / {iterations})");
                    //Console.WriteLine($"Best Hand: {bestHand}");
                    bestHand = null;
                }
            }
            //Console.WriteLine($"{cardV1.CardName()} {cardV2.CardName()} Won {p1Wins} out of {iterations} {p1Wins * 1.0 / iterations * 100:0.00}");
            //Console.WriteLine($"Best Hand: {bestHand}");
            lock (llock)
                results.Add((cardV1, cardV2, suited, p1Wins));
        }

        private static (Player, Hand) RunGame(Card c1, Card c2, int numPlayers = 2)
        {
            var game = new PokerGame();
            game.SetupGame(numPlayers);
            game.DealSpecificCardsToPlayer(0, c1, c2);
            game.DealCards();
            //game.DisplayGameState();
            var (p, hand) = game.Winner();
            //Console.WriteLine($"Winner: {p.Name} with {hand}");
            return (p, hand);
        }
    }
}
