using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Engine.Core
{
    public interface IRunFinder
    {
        int GetRunUpAnySuit(int column);
        int GetRunUp(int column, int row);
        int GetRunDown(int column, int row);
        int CountSuits(int column);
        int CountSuits(int column, int row);
        int CountSuits(int column, int startRow, int endRow);
    }
}
