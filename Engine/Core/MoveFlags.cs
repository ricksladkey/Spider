using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    [Flags]
    public enum MoveFlags
    {
        Empty = 0x0000,
        CreatesSpace = 0x0001,
        TurnsOverCard = 0x0002,
        UsesSpace = 0x0004,
        Holding = 0x0008,
        UndoHolding = 0x0010,
        Discards = 0x0020,
        Flagged = 0x8000,
    }

    public static class MoveFlagsExtensions
    {
        public static int ChangeInSpaces(this MoveFlags flags)
        {
            if ((flags & MoveFlags.CreatesSpace) == MoveFlags.CreatesSpace)
            {
                return 1;
            }
            if ((flags & MoveFlags.UsesSpace) == MoveFlags.UsesSpace)
            {
                return -1;
            }
            return 0;
        }

        public static bool CreatesSpace(this MoveFlags flags)
        {
            return (flags & MoveFlags.CreatesSpace) == MoveFlags.CreatesSpace;
        }

        public static bool PreservesSpace(this MoveFlags flags)
        {
            return (flags & (MoveFlags.CreatesSpace | MoveFlags.UsesSpace)) == MoveFlags.Empty;
        }

        public static bool UsesSpace(this MoveFlags flags)
        {
            return (flags & MoveFlags.UsesSpace) == MoveFlags.UsesSpace;
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
