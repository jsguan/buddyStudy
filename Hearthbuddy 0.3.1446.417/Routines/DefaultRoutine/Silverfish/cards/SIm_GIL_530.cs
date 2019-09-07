using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_GIL_530 : SimTemplate //* North Sea Kraken
    {
        //Battlecry: Deal 2 damage.

        public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
        {
            int dmg = 2;
            p.minionGetDamageOrHeal(target, dmg);
            p.evaluatePenality -= 10;
        }
    }
}