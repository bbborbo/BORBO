using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.GivePickupsOnStart;

namespace Borbo
{
    internal partial class Main : BaseUnityPlugin
    {

        GameObject awu = Resources.Load<GameObject>("prefabs/characterbodies/SuperRoboBallBossBody");
        CharacterBody awuBody;
        float awuArmor = 40;
        float awuAdditionalArmor = 0;
        int awuAdaptiveArmorCount = 1;

        float costExponent = 1.6f;
        float costConstant = 0.5f;

        float bonusGold = 1.2f;
        int goldChestTypeCost = 10;
        int bigDroneTypeCost = 8;

        void FixMoneyScaling()
        {
            On.RoR2.DeathRewards.Awake += FixEliteGoldReward;
            On.RoR2.Run.GetDifficultyScaledCost_int_float += ChangeScaledCost;
            On.RoR2.TeleporterInteraction.Awake += ReduceTeleDirectorReward;

            // adjusting AWU armor to compensate for chest cost increases
            awuBody = awu.GetComponent<CharacterBody>();
            if (awuBody)
            {
                awuBody.baseArmor = awuArmor;
                if (awuAdaptiveArmorCount <= 0)
                {
                    awuBody.armor += awuAdditionalArmor;
                }
                else
                {
                    GivePickupsOnStart gpos = awuBody.gameObject.AddComponent<GivePickupsOnStart>();
                    if (gpos)
                    {
                        ItemInfo adaptiveArmor = new ItemInfo();
                        adaptiveArmor.count = awuAdaptiveArmorCount;
                        adaptiveArmor.itemString = RoR2Content.Items.AdaptiveArmor.nameToken;

                        gpos.itemInfos = new ItemInfo[1] { adaptiveArmor };
                    }
                }
            }
            
            On.RoR2.ShrineBloodBehavior.Start += ShrineBloodBehavior_Start;
            On.RoR2.Run.BeginStage += GetChestCostForStage;
        }

        #region Blood Shrines
        private static int teamMaxHealth;
        private const float totalHealthFraction = 2.18f; // health bars
        private static float chestAmount = 2; // chests per health bar

        public static int lastChestBaseCost = 25;
        private void GetChestCostForStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            lastChestBaseCost = Run.instance.GetDifficultyScaledCost(25);
            orig(self);
        }
        private void ShrineBloodBehavior_Start(On.RoR2.ShrineBloodBehavior.orig_Start orig, ShrineBloodBehavior self)
        {
            orig(self);
            if (NetworkServer.active) StartCoroutine(WaitForPlayerBody(self));
        }

        IEnumerator WaitForPlayerBody(ShrineBloodBehavior instance)
        {
            yield return new WaitForSeconds(2);

            if(instance.goldToPaidHpRatio != 0)
            {
                foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
                {
                    var body = playerCharacterMasterController.master.GetBody();

                    if (body)
                    {
                        var maxHealth = body.healthComponent.fullCombinedHealth;
                        if (maxHealth > teamMaxHealth) teamMaxHealth = (int)maxHealth;
                    }
                }

                float baseCost = lastChestBaseCost; //cost of a small chest
                float moneyTotal = baseCost * chestAmount; //target money granted by the shrine
                float maxMulti = moneyTotal / teamMaxHealth; //express target money as a fraction of the max health of the team

                if (maxMulti > 0)//0.5f)
                    instance.goldToPaidHpRatio = maxMulti;
            }
        }
        #endregion

        #region Economy
        private void ReduceTeleDirectorReward(On.RoR2.TeleporterInteraction.orig_Awake orig, TeleporterInteraction self)
        {
            orig(self);
            if (self.bonusDirector)
            {
                self.bonusDirector.expRewardCoefficient /= 2;
            }
        }

        private int ChangeScaledCost(On.RoR2.Run.orig_GetDifficultyScaledCost_int_float orig, RoR2.Run self, int baseCost, float difficultyCoefficient)
        {
            int costMultiplier = baseCost / 25;
            switch (costMultiplier)
            {
                case 16:
                    baseCost = 25 * goldChestTypeCost; //10, originally 16
                    break;
                case 14:
                    baseCost = 25 * bigDroneTypeCost; //8, originally 14
                    break;
            }

            float costMultiplierExponential = Mathf.Pow(difficultyCoefficient, costExponent);
            float costMultiplierLinear = (difficultyCoefficient * 2.5f - 1.5f);

            float endMultiplier = costMultiplierExponential;
            if (costMultiplierLinear < costMultiplierExponential)
            {                                                                                                     
                //endMultiplier = costMultiplierLinear;
                //Debug.Log("Using Liner multiplier!");
            }

            return (int)((float)baseCost * endMultiplier);
        }

        private void FixEliteGoldReward(On.RoR2.DeathRewards.orig_Awake orig, RoR2.DeathRewards self)
        {
            orig(self);
            CharacterBody body = self.GetComponent<CharacterBody>();
            if (!body || !body.inventory) { return; }

            int bonusHealthCount = body.inventory.GetItemCount(RoR2Content.Items.BoostHp);
            if(bonusHealthCount > 0 && bonusHealthCount <= 100)
            {
                //self.goldReward /= 0;
            }
            else if (bonusHealthCount > 100 && bonusHealthCount < 200)
            {
                self.goldReward /= 3;
            }
            else
            {
                self.goldReward /= 9;
            }
        }
        #endregion
    }
}
