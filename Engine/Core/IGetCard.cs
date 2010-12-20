using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Collections;

namespace Spider.Engine
{
    public interface IGetCard
    {
        Card GetCard(int column);
    }
}
