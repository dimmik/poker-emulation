namespace HoldemEval
{
    public class Card
    {
        public int Value { get; set; } // 2-14, where 14 is Ace
        public Suit Suit { get; set; } // "Hearts", "Diamonds", "Clubs", "Spades"
        public string ValueStr()
        {
            return Value.CardName();   
        }
        public override string ToString()
        {
            return $"{Value.CardName()} of {Suit}";
        }
    }

}
