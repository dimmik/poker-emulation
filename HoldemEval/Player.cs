namespace HoldemEval
{
    using System.Collections.Generic;

    public class Player
    {
        public string Name { get; set; }
        public List<Card> Hand { get; set; }

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
        }
    }
}

// Пример использования:
// var game = new PokerGame();
// game.DealSpecificCardsToPlayer(0, new Card { Value = 14, Suit = "Hearts" }, new Card { Value = 14, Suit = "Spades" });
// game.SetupGame();
// game.DisplayGameState();
