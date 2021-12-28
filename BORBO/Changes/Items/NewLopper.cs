using BepInEx.Configuration;
using Borbo.CoreModules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Borbo.CoreModules.StatHooks;
using static R2API.RecalculateStatsAPI;

namespace Borbo.Items
{
    class NewLopper : ItemBase
    {
        internal static float maxHealthThreshold = 0.3f;
        int freeCritChance = 15;
        float freeCritDamage = 0.1f;
        int dangerCritChance = 50;
        float bonusCritDamageLowHealthBase = 0;
        float bonusCritDamageLowHealthStack = 3;

        public static BuffDef dangerCritBuff;

        public override string ItemName => "The New Lopper";

        public override string ItemLangTokenName => "DANGERCRIT";

        public override string ItemPickupDesc => "Massively increase 'Critical Strike' damage at low health.";

        public override string ItemFullDescription => $"Gain <style=cIsDamage>{freeCritChance}% critical chance.</style> " +
            $"Falling below <style=cIsHealth>{Tools.ConvertDecimal(maxHealthThreshold)} health</style> sends you into a rampage, increasing " +
            $"<style=cIsDamage>critical strike damage by {Tools.ConvertDecimal(bonusCritDamageLowHealthBase + bonusCritDamageLowHealthStack)}</style> " +
            $"<style=cStack>(+{Tools.ConvertDecimal(bonusCritDamageLowHealthStack)} per stack)</style>, and " +
            $"<style=cIsDamage>critical strike chance by another {dangerCritChance - freeCritChance}%</style>.";

        public override string ItemLore => "Order: The New Lopper" +
            "\nTracking Number: 598********" +
            "\nEstimated Delivery: 2/16/2071" +
            "\nShipping Method: High Priority/Fragile" +
            "\nShipping Address: Box 11, Sues Drive, Jupiter" +
            "\nShipping Details:\n" +
            "\nNothing here. Try again later.";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags { get; set; } = new ItemTag[] { ItemTag.Damage };
        public override BalanceCategory Category { get; set; } = BalanceCategory.StateOfDamage;
        public override HookType Type { get; set; } = HookType.CritDamage;

        public override GameObject ItemModel => Resources.Load<GameObject>("prefabs/NullModel");

        public override Sprite ItemIcon => Resources.Load<Sprite>("textures/miscicons/texWIPIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict IDR = new ItemDisplayRuleDict();

            return IDR;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
            GetStatCoefficients += this.GiveBonusCritChance;
            BorboStatCoefficients += this.GiveBonusCritDamage;
        }

        private void GiveBonusCritDamage(CharacterBody sender, BorboStatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(dangerCritBuff);
            if (buffCount > 0)
            {
                args.critDamageMultAdd += bonusCritDamageLowHealthBase + bonusCritDamageLowHealthStack * buffCount;
            }
        }

        private void GiveBonusCritChance(CharacterBody sender, StatHookEventArgs args)
        {
            if(GetCount(sender) > 0)
            {
                int critAdd = freeCritChance;

                int buffCount = sender.GetBuffCount(dangerCritBuff);
                if (buffCount > 0)
                {
                    critAdd = dangerCritChance;
                }
                args.critAdd += critAdd;
            }
        }

        private void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.healthComponent != null)
                {
                    self.AddItemBehavior<NewLopperBehavior>(GetCount(self));
                }
            }
        }

        public override void Init(ConfigFile config)
        {
            CreateItem();
            CreateLang();
            CreateBuff();
            Hooks();
        }


        private void CreateBuff()
        {
            dangerCritBuff = ScriptableObject.CreateInstance<BuffDef>();
            {
                dangerCritBuff.buffColor = Color.black;
                dangerCritBuff.canStack = true;
                dangerCritBuff.isDebuff = false;
                dangerCritBuff.name = "NewLopperCritBonus";
                dangerCritBuff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffFullCritIcon");
            };
            Assets.buffDefs.Add(dangerCritBuff);
        }
    }

    public class NewLopperBehavior : RoR2.CharacterBody.ItemBehavior
    {
        bool isEnraged = false;
        void FixedUpdate()
        {
            if(stack > 0)
            {
                float combinedHealthFraction = this.body.healthComponent.combinedHealthFraction;
                BuffIndex dangerCrit = NewLopper.dangerCritBuff.buffIndex;
                int buffCount = this.body.GetBuffCount(dangerCrit);


                if (combinedHealthFraction <= NewLopper.maxHealthThreshold)
                {
                    for (int i = 0; i < buffCount; i++)
                    {
                        this.body.RemoveBuff(dangerCrit);
                    }
                    for (int i = 0; i < stack; i++)
                    {
                        this.body.AddBuff(dangerCrit);
                    }
                    isEnraged = true;
                }
                else if (isEnraged)
                {
                    isEnraged = false;
                    for (int i = 0; i < buffCount; i++)
                    {
                        this.body.RemoveBuff(dangerCrit);
                        this.body.AddTimedBuffAuthority(dangerCrit, 5f);
                    }
                }
            }
        }
    }
}
