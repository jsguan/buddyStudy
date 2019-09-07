using System;
using System.Collections.Generic;
using System.Text;
namespace HREngine.Bots
{
    class Sim_GIL_600 : SimTemplate //* Zap
    {
        //Deal 2 damage to a minion. Overload: (1)

        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            int dmg = (ownplay) ? p.getSpellDamageDamage(2) : p.getEnemySpellDamageDamage(2);
            p.minionGetDamageOrHeal(target, dmg);
            if (ownplay) p.ueberladung++;
        }
    }
}