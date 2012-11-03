using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    public interface IGetCard
    {
        Card GetCard(int column);
    }
}
