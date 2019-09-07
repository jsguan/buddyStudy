using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_BOT_606 : SimTemplate //Kaboom Bot
    {

        // feindlichen helden 4 schaden zu.
        public override void onDeathrattle(Playfield p, Minion m)
        {
            p.minionGetDamageOrHeal(m.own ? p.enemyHero : p.ownHero, 4);
        }

    }
}