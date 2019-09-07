using System;
using System.Collections.Generic;
using System.Text;


namespace HREngine.Bots
{
    class Sim_BOT_263 : SimTemplate //* Soul Infusion
    {
        // Give the left-most minion in your hand +2/+2.
        public override void onCardPlay(Playfield p, bool ownplay, Minion target, int choice)
        {
            if (ownplay)
            {
                Handmanager.Handcard hc = p.searchRandomMinionInHand(p.owncards, searchmode.searchLowestCost, GAME_TAGs.Mob);
                if (hc != null)
                {
                    hc.addattack += 2;
                    hc.addHp += 2;
                    p.anzOwnExtraAngrHp += 4;
                }
            }
        }
    }
}