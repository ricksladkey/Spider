using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public class ComplexMove
    {
        public ComplexMove(int index, MoveList moves, MoveList supplementaryList)
        {
            Move scoreMove = moves[index];
            ScoreMove = scoreMove;
            SupplementaryMoves = new MoveList();
            for (int next = scoreMove.Next; next != -1; next = supplementaryList[next].Next)
            {
                SupplementaryMoves.Add(supplementaryList[next]);
            }
            HoldingList = new MoveList();
            for (int next = scoreMove.HoldingNext; next != -1; next = supplementaryList[next].Next)
            {
                HoldingList.Add(supplementaryList[next]);
            }
        }

        public Move ScoreMove { get; set; }
        public MoveList SupplementaryMoves { get; set; }
        public MoveList HoldingList { get; set; }
    }
}
