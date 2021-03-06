using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Borbo.CoreModules.StatHooks;
using static R2API.RecalculateStatsAPI;

namespace Borbo
{
    internal partial class Main : BaseUnityPlugin
    {
        #region buffs


        float deathMarkBonusDamage = 0.3f;


        
        private void FreshMeatStackingFix()
        {
            ChangeBuffStacking(nameof(JunkContent.Buffs.MeatRegenBoost), true);
            GetStatCoefficients += LetMeatActuallyStack;
        }

        float elephantBuffDuration = 10;
        int elephantArmor = 200;
        private void JadeElephantChanges()
        {
            ChangeBuffStacking(nameof(RoR2Content.Buffs.ElephantArmorBoost), true);
            On.RoR2.EquipmentSlot.FireGainArmor += ChangeElephantDuration;
            GetStatCoefficients += ReduceElephantArmor;
            LanguageAPI.Add("EQUIPMENT_GAINARMOR_PICKUP", "Gain massive armor for 10 seconds.");
            LanguageAPI.Add("EQUIPMENT_GAINARMOR_DESC",
                "Gain <style=cIsDamage>200 armor</style> for <style=cIsUtility>10 seconds.</style>");
        }

        private void ReduceElephantArmor(CharacterBody sender, StatHookEventArgs args)
        {
            int elephantBuffCount = sender.GetBuffCount(RoR2Content.Buffs.ElephantArmorBoost);

            if (elephantBuffCount > 0)
            {
                args.armorAdd += (elephantBuffCount * elephantArmor) - 500;
            }
        }
        private bool ChangeElephantDuration(On.RoR2.EquipmentSlot.orig_FireGainArmor orig, EquipmentSlot self)
        {
            self.characterBody.AddTimedBuff(RoR2Content.Buffs.ElephantArmorBoost, elephantBuffDuration);
            return true;
        }

        private void LetMeatActuallyStack(CharacterBody sender, StatHookEventArgs args)
        {
            int meatBuffCount = sender.GetBuffCount(JunkContent.Buffs.MeatRegenBoost);
     
            if (meatBuffCount > 1)
            {
                args.baseRegenAdd += 2 * (1 + 0.2f * (sender.level - 1)) * (meatBuffCount - 1);
            }
        }
        #endregion

        #region slows
        GameObject templarPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/ClayBruiserBody");
        GameObject chimeraWispPrefab = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/LunarWispBody");
        static float tarSlowAspdReduction = 0.3f;
        static float kitSlowAspdReduction = 0.3f;
        static float chronoSlowAspdReduction = 0.5f;
        static float chillSlowAspdReduction = 0.6f;

        void BuffSlows()
        {
            BorboStatCoefficients += AttackSpeedSlows;
            LanguageAPI.Add("ITEM_SLOWONHIT_DESC",
                "<style=cIsUtility>Slow</style> enemies on hit for <style=cIsUtility>-60% movement speed and attack speed</style> " +
                "for <style=cIsUtility>2s</style> <style=cStack>(+2s per stack)</style>.");

            this.templarPrefab.GetComponent<CharacterBody>().baseAttackSpeed *= 1 + kitSlowAspdReduction;
            this.chimeraWispPrefab.GetComponent<CharacterBody>().baseAttackSpeed = 0.7f;
        }

        private void AttackSpeedSlows(CharacterBody sender, BorboStatHookEventArgs args)
        {
            float aspdDecreaseAmt = 0;

            if (sender.HasBuff(RoR2Content.Buffs.ClayGoo)) //tar
                aspdDecreaseAmt += tarSlowAspdReduction;
            if (sender.HasBuff(RoR2Content.Buffs.Slow50)) //kit
                aspdDecreaseAmt += kitSlowAspdReduction;
            if (sender.HasBuff(RoR2Content.Buffs.Slow60)) //chronobauble
                aspdDecreaseAmt += chronoSlowAspdReduction;
            if (sender.HasBuff(RoR2Content.Buffs.Slow80)) //cold
                aspdDecreaseAmt += chillSlowAspdReduction;

            args.attackSpeedDivAdd += aspdDecreaseAmt;
        }
        #endregion

        #region damage

        private void DeathMarkFix()
        {
            if (!Tools.isLoaded("com.Skell.DeathMarkChange"))
            {
                ChangeBuffStacking(nameof(RoR2Content.Buffs.DeathMark), true);
                IL.RoR2.GlobalEventManager.OnHitEnemy += DeathMarkFix_Stacking;
                IL.RoR2.HealthComponent.TakeDamage += DeathMarkFix_Damage;
                LanguageAPI.Add("ITEM_DEATHMARK_DESC",
                    "Enemies with <style=cIsDamage>4</style> or more debuffs are <style=cIsDamage>marked for death</style>, " +
                    "increasing damage taken by <style=cIsDamage>30%</style> <style=cStack>(+30% per stack)</style> " +
                    "from all sources for <style=cIsUtility>7</style> seconds.");
            }
        }

        float critHudDamageMul = 1;
        private void OcularHudBuff()
        {
            GetStatCoefficients += HudCritDamage;
            LanguageAPI.Add("EQUIPMENT_CRITONUSE_PICKUP", "Increased 'Critical Strike' damage. Gain 100% Critical Strike Chance for 8 seconds.");
            LanguageAPI.Add("EQUIPMENT_CRITONUSE_DESC",
                "<style=cIsHealth>Passively double Critical Strike Damage</style>. " +
                "On use, gain <style=cIsDamage>+100% Critical Strike Chance</style> for 8 seconds.");
        }

        private void DeathMarkFix_Stacking(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int deathMarkCountLocation = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld("RoR2.RoR2Content/Buffs", "DeathMark"),
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(out deathMarkCountLocation)
                );
            c.Index++;
            c.Remove();
            c.Remove();
            c.EmitDelegate<Action<CharacterBody, BuffDef, float, float>>((body, buffDef, duration, itemCount) =>
            {
                int currentDebuffCount = body.GetBuffCount(RoR2Content.Buffs.DeathMark);
                int buffsNeeded = 0;

                if(currentDebuffCount < (int)itemCount)
                {
                    buffsNeeded = (int)itemCount - currentDebuffCount;
                }

                for(float i = 0; i < buffsNeeded; i++)
                {
                    body.AddTimedBuff(buffDef, duration);
                }
            });
        }

        private void DeathMarkFix_Damage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld("RoR2.RoR2Content/Buffs", "DeathMark"),
                x => x.MatchCallOrCallvirt<RoR2.CharacterBody>(nameof(CharacterBody.HasBuff))
                );
            c.GotoNext(MoveType.After,
                x => x.MatchLdcR4(out _)
                );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>((damage, hc) =>
            {
                CharacterBody body = hc.body;
                float damageOut = 1;

                int buffCount = body.GetBuffCount(RoR2Content.Buffs.DeathMark);

                damageOut += deathMarkBonusDamage * buffCount;
                Debug.Log(damage + " " + damageOut);
                return damageOut;
            });
        }

        private void HudCritDamage(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.equipmentSlot)
            {
                if(sender.equipmentSlot.equipmentIndex == RoR2Content.Equipment.CritOnUse.equipmentIndex)
                    args.critDamageMultAdd += critHudDamageMul;
            }
        }
        #endregion
    }
}
