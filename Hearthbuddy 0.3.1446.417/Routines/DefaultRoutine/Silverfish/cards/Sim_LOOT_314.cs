using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
    class Sim_LOOT_314 : SimTemplate //* Grizzled Guardian
    {
        //Taunt Deathrattle: Recruit 2_minions that cost(4)_or_less.

        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.LOOT_048);
        public override void onDeathrattle(Playfield p, Minion m)
        {
            int pos = (m.own) ? p.ownMinions.Count : p.enemyMinions.Count;
            p.callKid(kid, pos, m.own, false);
            p.callKid(kid, pos, m.own);
        }
    }
}