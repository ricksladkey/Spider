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
        CreatesFreeCell = 0x0001,
        TurnsOverCard = 0x0002,
        UsesFreeCell = 0x0004,
        Holding = 0x0008,
        UndoHolding = 0x0010,
        Flagged = 0x0020,
    }

    public static class MoveFlagsExensions
    {
        public static int ChangeInFreeCells(this MoveFlags flags)
        {
            if ((flags & MoveFlags.CreatesFreeCell) == MoveFlags.CreatesFreeCell)
            {
                return 1;
            }
            if ((flags & MoveFlags.UsesFreeCell) == MoveFlags.UsesFreeCell)
            {
                return -1;
            }
            return 0;
        }

        public static bool CreatesFreeCell(this MoveFlags flags)
        {
            return (flags & MoveFlags.CreatesFreeCell) == MoveFlags.CreatesFreeCell;
        }

        public static bool PreservesFreeCells(this MoveFlags flags)
        {
            return (flags & (MoveFlags.CreatesFreeCell | MoveFlags.UsesFreeCell)) == MoveFlags.Empty;
        }

        public static bool UsesFreeCell(this MoveFlags flags)
        {
            return (flags & MoveFlags.UsesFreeCell) == MoveFlags.UsesFreeCell;
        }

        public static bool TurnsOverCard(this MoveFlags flags)
        {
            return (flags & MoveFlags.TurnsOverCard) == MoveFlags.TurnsOverCard;
        }

    }
}
