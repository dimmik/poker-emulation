namespace HoldemEval
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Numerics;
    using System.Security.Cryptography;


    public enum Suit
    {
        Hearts, Diamonds, Clubs, Spades
    }
    public class PokerGame
    {
        public List<Player> Players { get; private set; }
        public List<Card> CommunityCards { get; private set; }
        private List<Card> Deck { get; set; }

        public PokerGame()
        {
            InitializeDeck();
            Players = new List<Player>();
            CommunityCards = new List<Card>();
        }

        private void InitializeDeck()
        {
            Deck = new List<Card>();
            Suit[] suits = { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };
            for (int i = 2; i <= 14; i++)
            {
                foreach (var suit in suits)
                {
                    Deck.Add(new Card { Value = i, Suit = suit });
                }
            }
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = GetSecureRandomNumber(n + 1);
                Card value = Deck[k];
                Deck[k] = Deck[n];
                Deck[n] = value;
            }
        }

        private int GetSecureRandomNumber(int maxValue)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumber = new byte[4];
                rng.GetBytes(randomNumber);
                long value = BitConverter.ToInt64([.. randomNumber, .. new byte[] {0,0,0,0}], 0);
                return (int)(Math.Abs(value) % maxValue);
            }
        }


        public void DealSpecificCardsToPlayer(int playerIndex, Card card1, Card card2)
        {
            if (playerIndex < 0 || playerIndex >= Players.Count)
                throw new ArgumentException("Invalid player index");

            if (Players[playerIndex].Hand.Count > 0)
                throw new InvalidOperationException("Player already has cards");

            // Remove the specific cards from the deck if they're there
            int removed = Deck.RemoveAll(c => (c.Value == card1.Value && c.Suit == card1.Suit) ||
                                (c.Value == card2.Value && c.Suit == card2.Suit));
            if (removed != 2)
            {
                throw new ArgumentException("should be 2 different cards"); 
            }


            // Add the specific cards to the player's hand
            Players[playerIndex].Hand.Add(card1);
            Players[playerIndex].Hand.Add(card2);
        }

        public void SetupGame(int numberOfPlayers = 6)
        {
            if (numberOfPlayers < 2 || numberOfPlayers > 10)
                throw new ArgumentException("Number of players must be between 2 and 10");

            // Create players
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (i >= Players.Count)
                {
                    Players.Add(new Player($"Player {i + 1}"));
                }
            }

        }

        public void DealCards()
        {
            // Deal two cards to each player who doesn't have cards yet
            foreach (var player in Players)
            {
                if (player.Hand.Count == 0)
                {
                    player.Hand.Add(Deck[0]);
                    player.Hand.Add(Deck[1]);
                    Deck.RemoveRange(0, 2);
                }
            }

            // Deal five community cards
            CommunityCards.AddRange(Deck.Take(5));
            Deck.RemoveRange(0, 5);
        }

        public void DisplayGameState()
        {
            Console.WriteLine("Community Cards:");
            foreach (var card in CommunityCards)
            {
                Console.WriteLine($"{card.ValueStr()} of {card.Suit}");
            }

            Console.WriteLine("\nPlayers:");
            foreach (var player in Players)
            {
                Console.WriteLine($"{player.Name}:");
                foreach (var card in player.Hand)
                {
                    Console.WriteLine($"  {card.ValueStr()} of {card.Suit}");
                }
                Console.WriteLine($"rank: {PokerHand.EvaluateHand(CommunityCards.Concat(player.Hand))}");
            }
            var orderedPlayers = Players
                .Select(p => (p, hand: PokerHand.EvaluateHand(CommunityCards.Concat(p.Hand))))
                .OrderByDescending(ph => ph.hand)
                ;
            var winner = orderedPlayers.First();
            Console.WriteLine($"Winner: {winner.p.Name} with {winner.hand}");
        }
        public (Player, Hand) Winner()
        {
            var orderedPlayers = Players
                .OrderByDescending(p => Guid.NewGuid())
                .Select(p => (p, hand: PokerHand.EvaluateHand(CommunityCards.Concat(p.Hand))))
                .OrderByDescending(ph => ph.hand)
                ;
            var (winner, second) = (orderedPlayers.First(), orderedPlayers.Skip(1).First());
            /*if (winner.hand == second.hand && winner.p.Name == "Player 1")
            {
                //return second;
                return winner;
            }*/
            return winner;
        }
    }
}

// Пример использования:
// var game = new PokerGame();
// game.DealSpecificCardsToPlayer(0, new Card { Value = 14, Suit = "Hearts" }, new Card { Value = 14, Suit = "Spades" });
// game.SetupGame();
// game.DisplayGameState();
