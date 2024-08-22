namespace HoldemEval
{
    public static class CardHelper
    {
        public static string CardName(this int c)
        {
            if (c <= 10) return $"{c}";
            if (c == 11) return "Jack";
            if (c == 12) return "Queen";
            if (c == 13) return "King";
            return "Ace";
        }
    }

}
