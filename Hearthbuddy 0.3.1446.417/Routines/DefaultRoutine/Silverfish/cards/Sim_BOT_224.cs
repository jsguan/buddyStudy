using System;
using System.Collections.Generic;
using System.Text;


namespace HREngine.Bots
{
    class Sim_BOT_224 : SimTemplate //* Doubling Imp
    {
        // Battlecry: Summon a copy of this minion.


        CardDB.Card kid = CardDB.Instance.getCardDataFromID(CardDB.cardIDEnum.BOT_224); //Ë«ÉúÐ¡¹í


        public override void getBattlecryEffect(Playfield p, Minion m, Minion target, int choice)
        {
            p.callKid(kid, m.zonepos, m.own);
        }
    }
}