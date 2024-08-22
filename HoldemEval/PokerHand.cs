using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoldemEval
{
    public class PokerHand
    {
        public static Hand EvaluateHand(IEnumerable<Card> hand)
        {
            if (hand.Count() != 7)
                throw new ArgumentException("Hand must contain exactly 7 cards");

            var groupedCards = hand.GroupBy(card => card.Value)
                                   .OrderByDescending(g => g.Count())
                                   .ThenByDescending(g => g.Key)
                                   .ToList();
            var flush = hand.GroupBy(card => card.Suit).Any(g => g.Count() >= 5);
            var straight = IsStraight(hand);

            var fss = hand.GroupBy(card => card.Suit)
                    .Where(g => g.Count() >= 5)
                    .Select(g => g.Key);
            Suit? flushSuit = fss.Any() ? fss.First() : null;

            if (flushSuit != null)
            {
                var flushCards = hand.Where(card => card.Suit == flushSuit).OrderByDescending(card => card.Value).ToList();
                flushCards.Add(flushCards[0]);
                // Проверяем все возможные 5-карточные последовательности
                for (int i = 0; i <= flushCards.Count - 5; i++)
                {
                    var potentialStraight = flushCards.Skip(i).Take(5).ToList();
                    if (IsStraight(potentialStraight))
                    {
                        if (potentialStraight[0].Value == 14 && potentialStraight[1].Value == 13)
                            return new Hand(HandRank.RoyalFlush, new List<int>());
                        else
                            return new Hand(HandRank.StraightFlush, new List<int> { potentialStraight[0].Value });
                    }
                }

            }
            if (groupedCards.Any(g => g.Count() == 4))
                return new Hand(HandRank.FourOfAKind, new List<int> { groupedCards.First(g => g.Count() == 4).Key, groupedCards.First(g => g.Count() != 4).Key });
            if (groupedCards.Any(g => g.Count() == 3) && groupedCards.Any(g => g.Count() == 2))
                return new Hand(HandRank.FullHouse, new List<int> { groupedCards.First(g => g.Count() == 3).Key, groupedCards.First(g => g.Count() == 2).Key });
            if (flush)
                return new Hand(HandRank.Flush, hand.Where(c => c.Suit == hand.GroupBy(x => x.Suit).OrderByDescending(g => g.Count()).First().Key).OrderByDescending(c => c.Value).Take(5).Select(c => c.Value).ToList());
            if (straight)
                return new Hand(HandRank.Straight, new List<int> { GetHighestStraightCard(hand) });
            if (groupedCards.Any(g => g.Count() == 3))
                return new Hand(HandRank.ThreeOfAKind, new List<int> { groupedCards.First(g => g.Count() == 3).Key }.Concat(groupedCards.Where(g => g.Count() == 1).Take(2).Select(g => g.Key)).ToList());
            if (groupedCards.Count(g => g.Count() == 2) == 2)
                return new Hand(HandRank.TwoPair, groupedCards.Where(g => g.Count() == 2).Take(2).Select(g => g.Key).Concat(new[] { groupedCards.First(g => g.Count() == 1).Key }).ToList());
            if (groupedCards.Any(g => g.Count() == 2))
                return new Hand(HandRank.OnePair, new List<int> { groupedCards.First(g => g.Count() == 2).Key }.Concat(groupedCards.Where(g => g.Count() == 1).Take(3).Select(g => g.Key)).ToList());

            return new Hand(HandRank.HighCard, groupedCards.Take(5).Select(g => g.Key).ToList());
        }

        private static bool IsStraight(IEnumerable<Card> hand)
        {
            var orderedValues = hand.Select(card => card.Value).Distinct().OrderBy(v => v).ToList();
            if (orderedValues.Count < 5)
                return false;

            for (int i = 0; i <= orderedValues.Count - 5; i++)
            {
                if (orderedValues[i + 4] - orderedValues[i] == 4)
                    return true;
            }

            // Check for Ace-low straight
            if (orderedValues.Contains(14) && orderedValues.Contains(2) && orderedValues.Contains(3) && orderedValues.Contains(4) && orderedValues.Contains(5))
                return true;

            return false;
        }

        private static int GetHighestStraightCard(IEnumerable<Card> hand)
        {
            var orderedValues = hand.Select(card => card.Value).Distinct().OrderBy(v => v).ToList();
            for (int i = orderedValues.Count - 1; i >= 4; i--)
            {
                if (orderedValues[i] - orderedValues[i - 4] == 4)
                    return orderedValues[i];
            }
            // Check for Ace-low straight
            if (orderedValues.Contains(14) && orderedValues.Contains(2) && orderedValues.Contains(3) && orderedValues.Contains(4) && orderedValues.Contains(5))
                return 5;
            throw new InvalidOperationException("No straight found");
        }
    }


    public class Hand : IComparable<Hand>
    {
        public HandRank Rank { get; set; }
        public List<int> TieBreakers { get; set; }
        
        public Hand(HandRank rank, List<int> tieBreakers)
        {
            Rank = rank;
            TieBreakers = tieBreakers;
        }

        public int CompareTo(Hand other)
        {
            if (other == null) return 1;

            if (this.Rank != other.Rank)
                return this.Rank.CompareTo(other.Rank);

            for (int i = 0; i < Math.Min(this.TieBreakers.Count, other.TieBreakers.Count); i++)
            {
                if (this.TieBreakers[i] != other.TieBreakers[i])
                    return this.TieBreakers[i].CompareTo(other.TieBreakers[i]);
            }

            return this.TieBreakers.Count.CompareTo(other.TieBreakers.Count);
        }

        public static bool operator >(Hand hand1, Hand hand2)
        {
            return hand1.CompareTo(hand2) > 0;
        }

        public static bool operator <(Hand hand1, Hand hand2)
        {
            return hand1.CompareTo(hand2) < 0;
        }

        public static bool operator >=(Hand hand1, Hand hand2)
        {
            return hand1.CompareTo(hand2) >= 0;
        }

        public static bool operator <=(Hand hand1, Hand hand2)
        {
            return hand1.CompareTo(hand2) <= 0;
        }

        public static bool operator ==(Hand hand1, Hand hand2)
        {
            if (ReferenceEquals(hand1, hand2))
                return true;
            if (hand1 is null || hand2 is null)
                return false;
            return hand1.CompareTo(hand2) == 0;
        }

        public static bool operator !=(Hand hand1, Hand hand2)
        {
            return !(hand1 == hand2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Hand other)
                return this.CompareTo(other) == 0;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Rank.GetHashCode();
                foreach (var tieBreaker in TieBreakers)
                {
                    hash = hash * 23 + tieBreaker.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Rank} - {string.Join(", ", TieBreakers.Select(t => t.CardName()))}";
        }
    }
    public enum HandRank
    {
        HighCard, OnePair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush
    }

}
