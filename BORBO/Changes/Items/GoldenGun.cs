using BepInEx.Configuration;
using Borbo.CoreModules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
    class GoldenGun : ItemBase<GoldenGun>
    {
        public static int baseGoldChunk = 25;
        public static int maxGoldChunks = 10;
        public static bool includeDeploys = true;

        static float bonusDamagePerChunk = 0.04f;
        float bonusGold = 0.1f;
        public static BuffDef goldDamageBuff;

        string damageBoostPerChestPerStack = Tools.ConvertDecimal(bonusDamagePerChunk);

        public override string ItemName => "Golden Gun";

        public override string ItemLangTokenName => "ECONOMYWEAPON";

        public override string ItemPickupDesc => "Deal bonus damage based on the gold you have saved up.";

        public override string ItemFullDescription => $"<style=cIsUtility>Gain {Tools.ConvertDecimal(bonusGold)} extra gold</style>. " +
            $"Also deal <style=cIsDamage>{damageBoostPerChestPerStack} <style=cStack>(+{damageBoostPerChestPerStack} per stack)</style></style> " +
            $"bonus damage <style=cIsDamage>per chest you can afford</style>, for up to a maximum of <style=cIsUtility>{maxGoldChunks} chests</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;
        public override ItemTag[] ItemTags { get; set; } = new ItemTag[] { ItemTag.Utility, ItemTag.Damage };
        public override BalanceCategory Category { get; set; } = BalanceCategory.StateOfEconomy;

        public override GameObject ItemModel => Resources.Load<GameObject>("prefabs/NullModel");

        public override Sprite ItemIcon => Resources.Load<Sprite>("textures/miscicons/texWIPIcon");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            On.RoR2.CharacterBody.OnInventoryChanged += AddItemBehavior;
            On.RoR2.CharacterMaster.GiveMoney += GoldGunMoneyBoost;
            On.RoR2.HealthComponent.TakeDamage += GoldGunDamageBoost;
            GetStatCoefficients += this.GiveBonusDamage;
            On.RoR2.Run.BeginStage += GetChestCostForStage;
        }

        private void GiveBonusDamage(CharacterBody sender, StatHookEventArgs args)
        {
            int itemCount = GetCount(sender);
            int buffCount = sender.GetBuffCount(goldDamageBuff);
            if(itemCount > 0 && buffCount > 0)
            {
                float damageMult = Mathf.Sqrt(1 + bonusDamagePerChunk * buffCount * itemCount) - 1;

                args.damageMultAdd += damageMult;
            }
        }

        private void AddItemBehavior(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                if (self.master)
                {
                    GoldGunBehavior GgBehavior = self.AddItemBehavior<GoldGunBehavior>(GetCount(self));
                }
            }
        }

        private void GoldGunMoneyBoost(On.RoR2.CharacterMaster.orig_GiveMoney orig, CharacterMaster self, uint amount)
        {
            int itemCount = GetCount(self);
            if (itemCount > 0)
            {
                amount = (uint)(amount * (1 + bonusGold));
            }

            orig(self, amount);
        }

        private void GoldGunDamageBoost(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker != null)
            {
                CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
                if(body != null)
                {
                    var itemcount = GetCount(body);
                    if (itemcount > 0)
                    {
                        int damageBoostCount = body.GetBuffCount(GoldenGun.goldDamageBuff);
                        CharacterMaster master = body.master;
                        /*var money = master.money;
                        if (includeDeploys)
                        {
                            var deployable = master.GetComponent<Deployable>();
                            if (deployable) money += deployable.ownerMaster.money;
                        }

                        float damageMult = Mathf.Sqrt(1 + bonusDamagePerChunk * ((damageBoostCount + 1) * itemcount));

                        damageInfo.damage *= damageMult;*/
                        if(Util.CheckRoll((damageBoostCount / maxGoldChunks) * 100, master))
                        {
                            EffectManager.SimpleImpactEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/CoinImpact"), damageInfo.position, Vector3.up, true);
                        }
                    }
                }
            }

            orig(self, damageInfo);
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
            goldDamageBuff = ScriptableObject.CreateInstance<BuffDef>();
            {
                goldDamageBuff.name = "MoneyDamageBoost";
                goldDamageBuff.buffColor = Color.yellow;
                goldDamageBuff.canStack = true;
                goldDamageBuff.isDebuff = false;
                goldDamageBuff.iconSprite = Resources.Load<Sprite>("textures/bufficons/texBuffFullCritIcon");
            };
            Assets.buffDefs.Add(goldDamageBuff);
        }

        public static int lastChestBaseCost = 25;
        private void GetChestCostForStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            lastChestBaseCost = Run.instance.GetDifficultyScaledCost(GoldenGun.baseGoldChunk);
            orig(self);
        }
    }
    public class GoldGunBehavior : CharacterBody.ItemBehavior
    {
        public CharacterMaster master;
        public uint currentMoney = 0;
        int fixedBaseChestCost = 0;

        private void FixedUpdate()
        {
            if (currentMoney == master.money)
                return;

            currentMoney = master.money;
            if (GoldenGun.includeDeploys)
            {
                var deployable = master.GetComponent<Deployable>();
                if (deployable) currentMoney += deployable.ownerMaster.money;
            }

            int currentBuffCount = body.GetBuffCount(GoldenGun.goldDamageBuff);

            int damageBoostCount = Mathf.Clamp((int)(currentMoney / fixedBaseChestCost), 0, GoldenGun.maxGoldChunks);

            if (damageBoostCount == currentBuffCount)
                return;

            body.SetBuffCount(GoldenGun.goldDamageBuff.buffIndex, damageBoostCount);
        }

        private void Start()
        {
            master = body.master;
            fixedBaseChestCost = Run.instance.GetDifficultyScaledCost(GoldenGun.baseGoldChunk);
            if(GoldenGun.lastChestBaseCost < fixedBaseChestCost)
            {
                Debug.Log(GoldenGun.lastChestBaseCost + " was less than Golden Gun's detected amount: " + fixedBaseChestCost);
                fixedBaseChestCost = GoldenGun.lastChestBaseCost;
            }
        }
    }
}
