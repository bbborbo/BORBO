using BepInEx;
using Borbo.CoreModules;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace Borbo
{
    internal partial class Main : BaseUnityPlugin
    {
        #region defensepublic static float bucklerFreeArmor = 10;
        public static int rapFreeArmor = 2;
        public static int knurlFreeArmor = 15;
        public static int bucklerFreeArmor = 10;

        void AdjustVanillaDefense()
        {
            GetStatCoefficients += FreeBonusArmor;
            LanguageAPI.Add("ITEM_KNURL_PICKUP", "Boosts health, regeneration, and armor.");
            LanguageAPI.Add("ITEM_KNURL_DESC",
                $"<style=cIsHealing>Increase maximum health</style> by <style=cIsHealing>40</style> <style=cStack>(+40 per stack)</style>, " +
                $"<style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+1.6 hp/s <style=cStack>(+1.6 hp/s per stack)</style>, and " +
                $"<style=cIsHealing>armor</style> by <style=cIsHealing>{knurlFreeArmor} <style=cStack>(+{knurlFreeArmor} per stack)</style>.");
            LanguageAPI.Add("ITEM_SPRINTARMOR_DESC",
                $"<style=cIsHealing>Increase armor</style> by <style=cIsHealing>{bucklerFreeArmor}</style> <style=cStack>(+{bucklerFreeArmor} per stack)</style>, and another " +
                $"<style=cIsHealing>30</style> <style=cStack>(+30 per stack)</style> <style=cIsUtility>while sprinting</style>.");
            LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_PICKUP",
                "Receive damage reduction from all attacks.");
            LanguageAPI.Add("ITEM_REPULSIONARMORPLATE_DESC",
                $"Reduce all <style=cIsDamage>incoming damage</style> by " +
                $"<style=cIsDamage>5<style=cStack> (+5 per stack)</style></style>. Cannot be reduced below <style=cIsDamage>1</style>. " +
                $"Gain another <style=cIsHealing>{rapFreeArmor} armor<style=cStack>(+{rapFreeArmor} per stack)</style>.");
        }
        private void FreeBonusArmor(CharacterBody sender, StatHookEventArgs args)
        {
            float freeArmor = 0;

            if (sender.inventory != null)
            {
                Inventory inv = sender.inventory;
                freeArmor += inv.GetItemCount(RoR2Content.Items.ArmorPlate) * rapFreeArmor;
                freeArmor += inv.GetItemCount(RoR2Content.Items.SprintArmor) * bucklerFreeArmor;
                freeArmor += inv.GetItemCount(RoR2Content.Items.Knurl) * knurlFreeArmor;
            }

            args.armorAdd += freeArmor;
        }

        private void TeddyChanges()
        {
            IL.RoR2.HealthComponent.TakeDamage += TeddyChanges;
            LanguageAPI.Add("ITEM_BEAR_DESC",
                $"<style=cIsHealing>{15 * teddyNewMaxValue}%</style> " +
                $"<style=cStack>(+{15 * teddyNewMaxValue}% per stack)</style> " +
                $"chance to <style=cIsHealing>block</style> incoming damage. " +
                $"<style=cIsUtility>Unaffected by luck</style>.");
        }

        private void MeatReduceHealth(CharacterBody sender, StatHookEventArgs args)
        {
            Inventory inv = sender.inventory;
            if (inv != null)
            {
                args.baseHealthAdd -= inv.GetItemCount(RoR2Content.Items.FlatHealth) * 25;
            }
        }

        public static float teddyNewMaxValue = 0.6f; //1.0
        private void TeddyChanges(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdfld("RoR2.HealthComponent/ItemCounts", "bear")
                );
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt("RoR2.Util", nameof(RoR2.Util.ConvertAmplificationPercentageIntoReductionPercentage))
                );
            c.Emit(OpCodes.Ldc_R4, teddyNewMaxValue);
            c.Emit(OpCodes.Mul);
        }
        #endregion

        #region mobility


        public static float hoofSpeedBonusBase = 0.1f; //0.14
        public static float hoofSpeedBonusStack = 0.1f; //0.14
        private void GoatHoofNerf()
        {
            IL.RoR2.CharacterBody.RecalculateStats += HoofNerf;
            LanguageAPI.Add("ITEM_HOOF_DESC",
                $"Increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>{Tools.ConvertDecimal(hoofSpeedBonusBase)}</style> " +
                $"<style=cStack>(+{Tools.ConvertDecimal(hoofSpeedBonusStack)} per stack)</style>.");
        }
        private void HoofNerf(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int countLoc = 6;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "Hoof"),
                x => x.MatchCallOrCallvirt<RoR2.Inventory>(nameof(RoR2.Inventory.GetItemCount)),
                x => x.MatchStloc(out countLoc)
                );

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(countLoc),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(out _)
                );
            c.EmitDelegate<Func<float, float, float>>((itemCount, speedBonus) =>
            {
                float newSpeedBonus = 0;
                if (itemCount > 0)
                {
                    newSpeedBonus = hoofSpeedBonusBase + (hoofSpeedBonusStack * (itemCount - 1));
                }
                return newSpeedBonus;
            });
            c.Remove();
        }

        public static float dynamicJumpAscentHoldGravity = 0.8f; //1f
        public static float dynamicJumpAscentReleaseGravity = 1.3f; //1f
        public static float dynamicJumpDescentGravity = 1f; //1f
        private void DynamicJump(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<UnityEngine.Physics>("get_gravity"),
                x => x.MatchLdfld<Vector3>("y")
                );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, CharacterMotor, float>>((gravityIn, motor) =>
            {
                float gravityOut = gravityIn;

                if (!motor.disableAirControlUntilCollision)
                {
                    if(motor.velocity.y >= 0)
                    {
                        if (motor.body.inputBank.jump.down)
                        {
                            gravityOut *= dynamicJumpAscentHoldGravity;
                        }
                        else
                        {
                            gravityOut *= dynamicJumpAscentReleaseGravity;
                        }
                    }
                    else
                    {
                        gravityOut *= dynamicJumpDescentGravity;
                    }
                }

                return gravityOut;
            });
        }

        public static float featherJumpVerticalBonus = 1.0f; //1.5f
        public static float featherJumpHorizontalBonus = 1.3f; //1.5f
        private void FeatherNerf(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EntityStates.EntityState>("get_characterBody"),
                x => x.MatchLdfld<CharacterBody>("baseJumpCount")
                );

            int horizontalBoostLoc = 3;
            c.GotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchStloc(out horizontalBoostLoc)
                );
            c.Remove();
            c.Emit(OpCodes.Ldc_R4, featherJumpHorizontalBonus);
            c.Index++;

            int verticalBoostLoc = 4;
            c.GotoNext(MoveType.Before,
                x => x.MatchLdcR4(out _),
                x => x.MatchStloc(out verticalBoostLoc)
                );
            c.Remove();
            c.Emit(OpCodes.Ldc_R4, featherJumpVerticalBonus);
        }


        public static float drinkSpeedBonusBase = 0.2f; //0.25
        public static float drinkSpeedBonusStack = 0.15f; //0.25
        private void EnergyDrinkNerf()
        {
            if (!Main.isHBULoaded)
            {
                LanguageAPI.Add("ITEM_SPRINTBONUS_DESC",
                    $"<style=cIsUtility>Sprint speed</style> is improved by <style=cIsUtility>{Tools.ConvertDecimal(drinkSpeedBonusBase)}</style> " +
                    $"<style=cStack>(+{Tools.ConvertDecimal(drinkSpeedBonusStack)} per stack)</style>.");
                IL.RoR2.CharacterBody.RecalculateStats += DrinkNerf;
            }
        }
        private void DrinkNerf(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int countLoc = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "SprintBonus"),
                x => x.MatchCallOrCallvirt<RoR2.Inventory>(nameof(RoR2.Inventory.GetItemCount)),
                x => x.MatchStloc(out countLoc)
                );

            c.GotoNext(MoveType.After,
                x => x.MatchLdcR4(out _),
                x => x.MatchLdloc(countLoc),
                x => x.MatchConvR4()
                );
            c.EmitDelegate<Func<float, float, float>>((speedBonus, itemCount) =>
            {
                float newSpeedBonus = 0;
                if (itemCount > 0)
                {
                    newSpeedBonus = drinkSpeedBonusBase + (drinkSpeedBonusStack * (itemCount - 1));
                }
                return newSpeedBonus;
            });
            c.Remove();
        }
        #endregion

        #region healing
        public static float scytheBaseHeal = 0f; //4
        public static float scytheStackHeal = 3f; //4

        public static float monsterToothFlatHeal = 8;
        public static float monsterToothPercentHeal = 0.03f;

        public static float medkitFlatHeal = 25;
        public static float medkitPercentHeal = 0.08f;

        private void MeatBuff()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += MeatRegen;
            LanguageAPI.Add("ITEM_FLATHEALTH_PICKUP", "Regenerate health after killing an enemy.");
            LanguageAPI.Add("ITEM_FLATHEALTH_DESC", "Increases <style=cIsHealing>base health regeneration</style> by <style=cIsHealing>+2 hp/s</style> " +
                "for <style=cIsUtility>3s</style> <style=cStack>(+3s per stack)</style> after killing an enemy.");
        }

        private void ScytheNerf()
        {
            IL.RoR2.GlobalEventManager.OnCrit += ScytheNerf;
            LanguageAPI.Add("ITEM_HEALONCRIT_DESC",
                $"Gain <style=cIsDamage>5% critical chance</style>. <style=cIsDamage>Critical strikes</style> <style=cIsHealing>heal</style> for " +
                $"<style=cIsHealing>{scytheBaseHeal + scytheStackHeal}</style> <style=cStack>(+{scytheStackHeal} per stack)</style> <style=cIsHealing>health</style>.");
        }

        private void MedkitNerf()
        {
            LoadBuffDef(nameof(RoR2Content.Buffs.MedkitHeal)).isDebuff = true;
            IL.RoR2.CharacterBody.RemoveBuff_BuffIndex += MedkitHealChange;
            LanguageAPI.Add("ITEM_MEDKIT_DESC",
                $"2 seconds after getting hurt, <style=cIsHealing>heal</style> for " +
                $"<style=cIsHealing>{Tools.ConvertDecimal(medkitPercentHeal)}</style> of <style=cIsHealing>maximum health</style> " +
                $"plus an additional <style=cIsHealing>{0} health</style> <style=cStack>(+{medkitFlatHeal} FLAT per stack)</style>.");
        }

        private void MonsterToothNerf()
        {
            IL.RoR2.GlobalEventManager.OnCharacterDeath += MonsterToothHealChange;
            LanguageAPI.Add("ITEM_TOOTH_DESC",
            $"Killing an enemy spawns a <style=cIsHealing>healing orb</style> that heals for " +
            $"<style=cIsHealing>{Tools.ConvertDecimal(monsterToothPercentHeal)}</style> of <style=cIsHealing>maximum health</style> " +
            $"plus an additional <style=cIsHealing>{0} health</style> <style=cStack>(+{monsterToothFlatHeal} FLAT per stack)</style>.");
        }

        private void MonsterToothHealChange(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int countLoc = -1;
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "Tooth"),
                x => x.MatchCallOrCallvirt<RoR2.Inventory>(nameof(RoR2.Inventory.GetItemCount)),
                x => x.MatchStloc(out countLoc)
                );

            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<RoR2.HealthPickup>("flatHealing")
                );
            c.Emit(OpCodes.Ldloc, countLoc);
            c.EmitDelegate<Func<float, int, float>>((currentHealAmt, itemCount) =>
            {
                float newFlatHealAmt = monsterToothFlatHeal * (itemCount - 1);

                return newFlatHealAmt;
            });


            c.GotoNext(MoveType.Before,
                x => x.MatchStfld<RoR2.HealthPickup>("fractionalHealing")
                );
            c.EmitDelegate<Func<float, float>>((currentHealAmt) =>
            {
                float newPercentHealAmt = monsterToothPercentHeal;

                return newPercentHealAmt;
            });
        }

        private void MedkitHealChange(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int countLoc = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "Medkit"),
                x => x.MatchCallOrCallvirt<RoR2.Inventory>(nameof(RoR2.Inventory.GetItemCount)),
                x => x.MatchStloc(out countLoc)
                );

            c.GotoNext(MoveType.Before,
                x => x.MatchStloc(out _)
                );
            c.Emit(OpCodes.Ldloc, countLoc);
            c.EmitDelegate<Func<float, int, float>>((currentHealAmt, itemCount) =>
            {
                float newFlatHealAmt = medkitFlatHeal * (itemCount - 1);

                return newFlatHealAmt;
            });


            c.GotoNext(MoveType.Before,
                x => x.MatchStloc(out _)
                );
            c.EmitDelegate<Func<float, float>>((currentHealAmt) =>
            {
                float newPercentHealAmt = medkitPercentHeal;

                return newPercentHealAmt;
            });
        }

        private void ScytheNerf(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int countLoc = -1;
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld("RoR2.RoR2Content/Items", "HealOnCrit"),
                x => x.MatchCallOrCallvirt<RoR2.Inventory>(nameof(RoR2.Inventory.GetItemCount)),
                x => x.MatchStloc(out countLoc)
                );
            c.GotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt<RoR2.HealthComponent>(nameof(RoR2.HealthComponent.Heal))
                );

            c.Index -= 2;
            c.Emit(OpCodes.Ldloc, countLoc);
            c.EmitDelegate<Func<float, int, float>>((currentHealAmt, itemCount) =>
            {
                float newHealAmt = scytheBaseHeal + scytheStackHeal * itemCount;

                return newHealAmt;
            });
        }

        private void MeatRegen(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            CharacterBody attackerBody = damageReport.attackerBody;
            if (attackerBody != null && attackerBody.inventory != null)
            {
                Inventory inv = attackerBody.inventory;
                int meatCount = inv.GetItemCount(RoR2Content.Items.FlatHealth);
                if (meatCount > 0)
                {
                    attackerBody.AddTimedBuffAuthority(JunkContent.Buffs.MeatRegenBoost.buffIndex, 3 * meatCount);
                }
            }
            orig(self, damageReport);
        }
        #endregion

        #region barrier
        private float barrierDecayRate = 6f;
        private float aegisDecayIncrease = 3f;
        void BuffBarrier()
        {
            On.RoR2.CharacterBody.FixedUpdate += this.BarrierBuff;
            LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_PICKUP", "Reduces barrier decay rate. Healing past full grants you a temporary barrier.");
            LanguageAPI.Add("ITEM_BARRIERONOVERHEAL_DESC",
                "<style=cIsHealing>Reduces barrier decay rate by 33%</style> <style=cStack>(-33% per stack).</style> " +
                "Healing past full grants you a <style=cIsHealing>temporary barrier</style> for " +
                "<style=cIsHealing>50% <style=cStack>(+50% per stack)</style></style> of the amount you <style=cIsHealing>healed</style>."
                );
        }
        private void BarrierBuff(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            float num = 0f;
            if (self.inventory)
            {
                num = self.inventory.GetItemCount(RoR2Content.Items.BarrierOnOverHeal);
            }

            self.barrierDecayRate = Mathf.Max(1f, self.healthComponent.barrier / (this.barrierDecayRate + this.aegisDecayIncrease * num));
            orig(self);
        }
        #endregion
    }
}
