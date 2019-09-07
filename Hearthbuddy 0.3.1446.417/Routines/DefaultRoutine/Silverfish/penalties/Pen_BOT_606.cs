using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Pen_BOT_606 : PenTemplate //lepergnome
    {

        // f¨¹gt dem feindlichen helden 4 schaden zu.
        public override int getPlayPenalty(Playfield p, Minion m, Minion target, int choice, bool isLethal)
        {
            return 0;
        }

    }
}