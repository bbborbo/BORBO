using BepInEx.Configuration;
using Borbo.CoreModules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.RecalculateStatsAPI;

namespace Borbo.Items
{
    class FrozenShell : ItemBase<FrozenShell>
    {
        internal static BuffDef frozenShellArmorBuff;
        internal static float freeArmor = 10;
        internal static float baseArmor = 0;
        internal static float stackArmor = (100 / 3);

        public override string ItemName => "Frozen Turtle Shell";

        public override string ItemLangTokenName => "FROZENSHELL";

        public override string ItemPickupDesc => "Reduce incoming damage while at low health.";

        public override string ItemFullDescription => $"<style=cIsHealing>Increase armor</style> by " +
            $"<style=cIsHealing>{freeArmor}</style> <style=cStack>(+{freeArmor} per stack)</style>. " +
            $"Falling below <style=cIsHealth>50%</style> max health grants an <style=cIsUtility>ice barrier</style> that gives " +
            $"<style=cIsHealing>{Mathf.RoundToInt(baseArmor + stackArmor)}</style> " +
            $"<style=cStack>(+{Mathf.RoundToInt(stackArmor)} per stack)</style> additional armor.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;
        public override BalanceCategory Category { get; set; } = BalanceCategory.StateOfDefenseAndHealing;
        public override ItemTag[] ItemTags { get; set; } = new ItemTag[] { ItemTag.Utility };

        public override GameObject ItemModel => LegacyResourcesAPI.Load<GameObject>("prefabs/NullModel");

        public override Sprite ItemIcon => LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texWIPIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict IDR = new ItemDisplayRuleDict();

            return IDR;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
            GetStatCoefficients += this.GiveBonusArmor;
        }

        private void GiveBonusArmor(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = GetCount(sender);
            if (itemCount > 0)
            {
                args.armorAdd += freeArmor * itemCount;

                int buffCount = sender.GetBuffCount(frozenShellArmorBuff);
                if (buffCount > 0)
                {
                    args.armorAdd += baseArmor + Mathf.RoundToInt(stackArmor * itemCount);
                }
            }
        }

        private void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.healthComponent != null)
                {
                    self.AddItemBehavior<FrozenShellBehavior>(GetCount(self));
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

        void CreateBuff()
        {
            frozenShellArmorBuff = ScriptableObject.CreateInstance<BuffDef>();
            {
                frozenShellArmorBuff.name = "IceBarrier";
				frozenShellArmorBuff.buffColor = Color.cyan;
				frozenShellArmorBuff.canStack = false;
				frozenShellArmorBuff.isDebuff = false;
                frozenShellArmorBuff.iconSprite = LegacyResourcesAPI.Load<Sprite>("textures/bufficons/texBuffGenericShield");
            };
            Assets.buffDefs.Add(frozenShellArmorBuff);
        }
    }
    public class FrozenShellBehavior : CharacterBody.ItemBehavior
    {
        HealthComponent healthComponent;
        BuffIndex iceBarrierBuffIndex = FrozenShell.frozenShellArmorBuff.buffIndex;
        bool hasBuff = false;

        private void Start()
        {
            healthComponent = body.healthComponent;
            hasBuff = body.HasBuff(iceBarrierBuffIndex);
        }
        private void FixedUpdate()
        {
            float combinedHealthFraction = healthComponent.combinedHealthFraction;
            if (hasBuff)
            {
                if (combinedHealthFraction > 0.5f)
                {
                    this.body.RemoveBuff(iceBarrierBuffIndex);
                    hasBuff = false;
                }
            }
            else if (combinedHealthFraction <= 0.5f)
            {
                this.body.AddBuff(iceBarrierBuffIndex);
                hasBuff = true;
            }
        }
    }
}
