using System;
using System.Collections.Generic;
using System.Text;

namespace HREngine.Bots
{
	class Sim_GIL_622 : SimTemplate //* Arcane Blast
	{
		//敌方英雄收到3点伤害,我方英雄回复3点生命

		public override void getBattlecryEffect(Playfield p, Minion own, Minion target, int choice)
		{
            int heal = p.getMinionHeal(3);
			p.minionGetDamageOrHeal(p.ownHero, -heal);
			p.minionGetDamageOrHeal(own.own ? p.ownHero :p.enemyHero, 3);
		}
	}
}