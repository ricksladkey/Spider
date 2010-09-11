using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class ComplexMove
    {
        public Move ScoreMove { get; set; }
        public MoveList SupplementaryMoves { get; set; }
        public List<HoldingInfo> HoldingList { get; set; }

        public ComplexMove(int index, MoveList moves, MoveList supplementaryMoves, List<HoldingInfo> holdingList)
        {
            Move scoreMove = moves[index];
            ScoreMove = scoreMove;
            SupplementaryMoves = new MoveList();
            for (int next = scoreMove.Next; next != -1; next = supplementaryMoves[next].Next)
            {
                SupplementaryMoves.Add(supplementaryMoves[next]);
            }
            HoldingList = new List<HoldingInfo>();
            for (int next = scoreMove.HoldingNext; next != -1; next = holdingList[next].Next)
            {
                HoldingList.Add(holdingList[next]);
            }
        }
    }
}
