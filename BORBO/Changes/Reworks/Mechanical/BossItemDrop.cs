using BepInEx;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Borbo
{
    internal partial class Main : BaseUnityPlugin
    {
        public static GameObject overgrownPrinterPrefab = Resources.Load<GameObject>("prefabs/networkedobjects/chest/DuplicatorWild");
        public static bool affectAurelionite = true;

        float baseDropChance = 4;
        float eliteBonusDropChance = 3;
        float specialBonusDropChance = 7;

        public static Dictionary<BodyIndex, ItemDef> BossItemDictionary = new Dictionary<BodyIndex, ItemDef>();

        void BossesDropBossItems()
        {
            affectAurelionite = Main.CustomConfigFile.Bind<bool>(BalanceCategory.StateOfInteraction.ToString(),
                "Enable boss item drop changes for Aurelionite", true,
                "The boss item drop changes make Aurel drop his item directly and have greens drop from the portal instead. " +
                "Turn this off if you dont want that.").Value;
            DirectorAPI.InteractableActions += FuckingDeleteYellowPrinters;

            On.RoR2.BodyCatalog.Init += PopulateBossItemDictionary;
            On.RoR2.BossGroup.Awake += RemoveBossItemDropsFromTeleporter;
            On.RoR2.GlobalEventManager.OnCharacterDeath += HookOnBodyDeath;
        }

        private void FuckingDeleteYellowPrinters(List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo stage)
        {
            List<DirectorAPI.DirectorCardHolder> removeList = new List<DirectorAPI.DirectorCardHolder>();
            foreach (DirectorAPI.DirectorCardHolder dc in cardList)
            {
                if (dc.InteractableCategory == DirectorAPI.InteractableCategory.Duplicator)
                {
                    if(dc.Card.spawnCard.prefab == overgrownPrinterPrefab)
                    {
                        dc.Card.selectionWeight = 0;
                        removeList.Add(dc);
                    }
                }
            }
            foreach (DirectorAPI.DirectorCardHolder dc in removeList)
            {
                cardList.Remove(dc);
            }
        }

        private void PopulateBossItemDictionary(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();

            TryAddBossItem("VagrantBody", RoR2Content.Items.NovaOnLowHealth);
            TryAddBossItem("TitanBody", RoR2Content.Items.Knurl);
            TryAddBossItem("BeetleQueen2Body", RoR2Content.Items.BeetleGland);
            TryAddBossItem("GravekeeperBody", RoR2Content.Items.SprintWisp);
            TryAddBossItem("MagmaWormBody", RoR2Content.Items.FireballsOnHit);
            TryAddBossItem("ImpBossBody", RoR2Content.Items.BleedOnHitAndExplode);
            TryAddBossItem("ClayBossBody", RoR2Content.Items.SiphonOnLowHealth);
            TryAddBossItem("RoboBallBossBody", RoR2Content.Items.RoboBallBuddy);
            TryAddBossItem("GrandParentBody", RoR2Content.Items.ParentEgg);

            //TryAddBossItem("ScavBody", RoR2Content.Items.FireballsOnHit);
            TryAddBossItem("ElectricWormBody", RoR2Content.Items.LightningStrikeOnHit);
            TryAddBossItem("TitanGoldBody", RoR2Content.Items.TitanGoldDuringTP);
            TryAddBossItem("SuperRoboBallBossBody", RoR2Content.Items.RoboBallBuddy);
        }

        private static void TryAddBossItem(string bodyName, ItemDef itemDef)
        {
            BodyIndex index = BodyCatalog.FindBodyIndex(bodyName);
            if(index != BodyIndex.None)
            {
                if(itemDef != null)
                {
                    BossItemDictionary.Add(index, itemDef);
                }
            }
            else
            {
                Debug.Log($"A CharacterBody of the name {bodyName} could not be found!");
            }
        }

        private void RemoveBossItemDropsFromTeleporter(On.RoR2.BossGroup.orig_Awake orig, BossGroup self)
        {
            orig(self);
            self.bossDropChance = 0;
        }

        public void HookOnBodyDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (damageReport.victimTeamIndex == TeamIndex.Player)
                return;

            CharacterBody enemy = damageReport.victimBody;
            BodyIndex enemyBodyIndex = enemy.bodyIndex;

            if(enemyBodyIndex == BodyCatalog.FindBodyIndex("TitanGoldBody") &&
                affectAurelionite)
            {
                return;
            }

            if (enemy.healthComponent.alive)
            {
                return;
            }

            CharacterMaster killerMaster = damageReport.attackerMaster;


            ItemDef itemToDrop = null;

            int players = Run.instance.participatingPlayerCount;

            if (BossItemDictionary.TryGetValue(enemyBodyIndex, out itemToDrop))
            {
                float dropChance = 0;

                if(itemToDrop == RoR2Content.Items.TitanGoldDuringTP)
                {
                    dropChance = 100;
                }
                else if(enemyBodyIndex == BodyCatalog.FindBodyIndex("SuperRoboBallBossBody"))
                {
                    dropChance = specialBonusDropChance;
                }
                else
                {
                    dropChance = baseDropChance;
                }

                if (enemy.isElite || itemToDrop == RoR2Content.Items.LightningStrikeOnHit)
                {
                    dropChance += eliteBonusDropChance;
                }

                if (Util.CheckRoll(dropChance, killerMaster))
                {
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemToDrop.itemIndex), enemy.transform.position, Vector3.up * 20f);
                }
            }
        }
    }
}
