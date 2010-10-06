using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    [Flags]
    public enum MoveFlags
    {
        Empty = 0x0000,
        CreatesEmptyPile = 0x0001,
        TurnsOverCard = 0x0002,
        UsesEmptyPile = 0x0004,
        Holding = 0x0008,
        UndoHolding = 0x0010,
        Discards = 0x0020,
        Flagged = 0x8000,
    }

    public static class MoveFlagsExensions
    {
        public static int ChangeInEmptyPiles(this MoveFlags flags)
        {
            if ((flags & MoveFlags.CreatesEmptyPile) == MoveFlags.CreatesEmptyPile)
            {
                return 1;
            }
            if ((flags & MoveFlags.UsesEmptyPile) == MoveFlags.UsesEmptyPile)
            {
                return -1;
            }
            return 0;
        }

        public static bool CreatesEmptyPile(this MoveFlags flags)
        {
            return (flags & MoveFlags.CreatesEmptyPile) == MoveFlags.CreatesEmptyPile;
        }

        public static bool PreservesEmptyPiles(this MoveFlags flags)
        {
            return (flags & (MoveFlags.CreatesEmptyPile | MoveFlags.UsesEmptyPile)) == MoveFlags.Empty;
        }

        public static bool UsesEmptyPile(this MoveFlags flags)
        {
            return (flags & MoveFlags.UsesEmptyPile) == MoveFlags.UsesEmptyPile;
        }

        public static bool Discards(this MoveFlags flags)
        {
            return (flags & MoveFlags.Discards) == MoveFlags.Discards;
        }

        public static bool TurnsOverCard(this MoveFlags flags)
        {
            return (flags & MoveFlags.TurnsOverCard) == MoveFlags.TurnsOverCard;
        }

        public static bool Holding(this MoveFlags flags)
        {
            return (flags & MoveFlags.Holding) == MoveFlags.Holding;
        }

        public static bool UndoHolding(this MoveFlags flags)
        {
            return (flags & MoveFlags.UndoHolding) == MoveFlags.UndoHolding;
        }
    }
}
