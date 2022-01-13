using System;
using System.Security;
using System.Security.Permissions;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Borbo.CoreModules;
using Borbo.Equipment;
using Borbo.Items;
using Borbo.Scavengers;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
#pragma warning disable 
namespace Borbo
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.Skell.DeathMarkChange", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Borbo.ArtificerExtended", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Borbo.DuckSurvivorTweaks", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Borbo.GreenAlienHead", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.Borbo.ArtifactGesture", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin("com.Borbo.BORBO", "BORBO", "0.4.11")]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(BuffAPI), nameof(PrefabAPI), 
        nameof(EffectAPI), nameof(ResourcesAPI), nameof(DirectorAPI), 
        nameof(ItemAPI), nameof(RecalculateStatsAPI), nameof(EliteAPI))]

    internal partial class Main : BaseUnityPlugin
    {
        public static AssetBundle assetBundle = Tools.LoadAssetBundle(BORBO.Properties.Resources.borboitemicons);
        public static AssetBundle assetBundle2 = Tools.LoadAssetBundle(BORBO.Properties.Resources.borbobundle);
        public static string assetsPath = "Assets/BorboItemIcons/";
        public static string modelsPath = "Assets/Models/Prefabs/";
        public static string iconsPath = "Assets/Textures/Icons/";
        public static bool isAELoaded = Tools.isLoaded("com.Borbo.ArtificerExtended");
        public static bool isDSTLoaded = Tools.isLoaded("com.Borbo.DuckSurvivorTweaks");

        internal static ConfigFile CustomConfigFile { get; set; }
        public static ConfigEntry<bool> EnableConfig { get; set; }
        public static ConfigEntry<bool> StateOfDefenseAndHealing { get; set; }
        public static ConfigEntry<bool> StateOfHealth { get; set; }
        public static ConfigEntry<bool> StateOfInteraction { get; set; }
        public static ConfigEntry<bool> StateOfDamage { get; set; }
        public static ConfigEntry<bool> StateOfEconomy { get; set; }
        public static ConfigEntry<bool> StateOfElites { get; set; }
        public static ConfigEntry<bool> StateOfDifficulty { get; set; }

        public static ConfigEntry<bool>[] DisableConfigCategories = new ConfigEntry<bool>[(int)BalanceCategory.Count] 
        { StateOfDefenseAndHealing, StateOfHealth, StateOfInteraction, StateOfDamage, StateOfEconomy, StateOfDifficulty };
        bool IsCategoryEnabled(BalanceCategory category)
        {
            bool enabled = true;

            if (EnableConfig.Value && DisableConfigCategories[(int)category].Value)
            {
                enabled = false;
            }

            return enabled;
        }

        void Awake()
        {
            InitializeConfig();
            InitializeItems();
            InitializeEquipment();
            InitializeEliteEquipment();
            InitializeScavengers();

            if (isAELoaded)
            {
                if (IsCategoryEnabled(BalanceCategory.StateOfInteraction))
                {
                    LanguageAPI.Add("ARTIFICEREXTENDED_KEYWORD_CHILL", "<style=cKeywordName>Chilling</style>" +
                        $"<style=cSub>Has a chance to temporarily reduce <style=cIsUtility>movement speed and attack speed</style> by <style=cIsDamage>80%.</style></style>");
                }
                else
                {
                    LanguageAPI.Add("ARTIFICEREXTENDED_KEYWORD_CHILL", "<style=cKeywordName>Chilling</style>" +
                        $"<style=cSub>Has a chance to temporarily reduce <style=cIsUtility>movement speed</style> by <style=cIsDamage>80%.</style></style>");
                }
            }

            IL.RoR2.Orbs.DevilOrb.OnArrival += BuffDevilOrb;

            if (IsCategoryEnabled(BalanceCategory.StateOfDefenseAndHealing))
            {
                // ninja gear

                this.FixBuffs1();
                this.BuffNkuhana();
                this.FixPickupStats();

                this.AdjustVanillaDefense();
                this.AdjustVanillaMobility();
                this.AdjustVanillaHealing();

                //this.DoSpeedScavenger();
            }
            if (IsCategoryEnabled(BalanceCategory.StateOfHealth))
            {
                // borbos band, frozen turtle shell, flower crown, utility belt
                // tesla coil

                this.BuffBarrier();
                this.FuckingFixInfusion();
                //nerf engi turret max health?

                //this.DoBoboScavenger();
            }
            if (IsCategoryEnabled(BalanceCategory.StateOfInteraction))
            {
                // atg mk3, magic quiver, wicked band, permafrost

                this.BuffJustice();
                this.BuffSlows();
                this.NerfResDisc();
                EntityStates.LaserTurbine.FireMainBeamState.mainBeamProcCoefficient = 0.5f;
                this.FixVagrantNova();
                this.BossesDropBossItems();

                this.ReworkPlanula();
                this.ReworkShatterspleen();

                this.ChangeAIBlacklists();
                AIBlacklistSingleItem(RoR2Content.Items.NovaOnHeal);
                AIBlacklistSingleItem(RoR2Content.Items.Mushroom);
                RoR2Content.Equipment.CrippleWard.enigmaCompatible = true;
                RoR2Content.Equipment.Jetpack.enigmaCompatible = true;
                RoR2Content.Equipment.DroneBackup.cooldown = 60;

                this.StunChanges();

                //scav could have royal cap? cunning
            }
            if (IsCategoryEnabled(BalanceCategory.StateOfDamage))
            {
                // chefs stache, malware stick, new lopper, whetstone
                // old guillotine

                this.FixBuffs2();
                this.BlanketNerfProcCoefficient();
                this.FixMeteorFalloff();
                this.NerfBands();
                this.NerfCritGlasses();
                this.EditWarCry();
                this.StickyRework();

                //this.DoSadistScavenger();
            }
            if (IsCategoryEnabled(BalanceCategory.StateOfEconomy))
            {
                // golden gun

                this.PrinterAndScrapperOccurenceChanges();
                this.FixMoneyScaling();
                this.NerfBazaarStuff();

                //this.DoGreedyScavenger();
            }
            if (IsCategoryEnabled(BalanceCategory.StateOfDifficulty))
            {
                this.ChangeElites();
                this.ChangeEliteBehavior();
                this.DifficultyPlus();
                this.FixMoneyAndExpRewards();

                LanguageAPI.Add("DIFFICULTY_EASY_DESCRIPTION", $"Simplifies difficulty for players new to the game. Weeping and gnashing is replaced by laughter and tickles." +
                    $"<style=cStack>\n\n>Player Health Regeneration: <style=cIsHealing>+50%</style> " +
                    $"\n>Difficulty Scaling: <style=cIsHealing>-50%</style> " +
                    $"\n>Teleporter Visuals: <style=cIsHealing>+{Tools.ConvertDecimal(easyTeleParticleRadius / normalTeleParticleRadius - 1)}</style> " +
                    $"\n>{Tier2EliteName} Elites appear starting on <style=cIsHealing>Stage {Tier2EliteMinimumStageDrizzle + 1}</style> " +
                    $"\n>Player Damage Reduction: <style=cIsHealing>+38%</style></style>");
                // " + $"\n>Most Bosses have <style=cIsHealing>reduced skill sets</style>

                LanguageAPI.Add("DIFFICULTY_NORMAL_DESCRIPTION", $"This is the way the game is meant to be played! Test your abilities and skills against formidable foes." +
                    $"<style=cStack>\n\n>Player Health Regeneration: +0% " +
                    $"\n>Difficulty Scaling: +0% " +
                    $"\n>Teleporter Visuals: +0% " +
                    $"\n>{Tier2EliteName} Elites appear starting on Stage {Tier2EliteMinimumStageRainstorm + 1}</style></style>");

                LanguageAPI.Add("DIFFICULTY_HARD_DESCRIPTION", $"For hardcore players. Every bend introduces pain and horrors of the planet. You will die." +
                    $"<style=cStack>\n\n>Player Health Regeneration: <style=cIsHealth>-40%</style> " +
                    $"\n>Difficulty Scaling: <style=cIsHealth>+50%</style>" +
                    $"\n>Teleporter Visuals: <style=cIsHealth>{Tools.ConvertDecimal(1 - hardTeleParticleRadius / normalTeleParticleRadius)}</style> " +
                    $"\n>{Tier2EliteName} Elites appear starting on <style=cIsHealth>Stage {Tier2EliteMinimumStageMonsoon + 1}</style>" + 
                    $"\n>Most Enemies have <style=cIsHealth>unique scaling</style></style>");
            }
            //lol
            LanguageAPI.Add("ITEM_SHOCKNEARBY_PICKUP", "lol");
            LanguageAPI.Add("ITEM_AUTOCASTEQUIPMENT_PICKUP", "lol");
            LanguageAPI.Add("ITEM_EXECUTELOWHEALTHELITE_PICKUP", "lol");

            InitializeCoreModules();
            new ContentPacks().Initialize();
        }

        #region config
        private void InitializeConfig()
        {
            CustomConfigFile = new ConfigFile(Paths.ConfigPath + "\\BORBO.cfg", true);

            EnableConfig = CustomConfigFile.Bind<bool>("Allow Config Options", "Enable Config", false,
                "Set this to true to enable config options. Please keep in mind that it was not within my design intentions to play this way. " +
                "This is primarily meant for modpack users with tons of mods installed. " +
                "If you have any issues or feedback on my mod balance, please feel free to send in feedback with the contact info in the README or Thunderstore description.");

            for (int i = 0; i < DisableConfigCategories.Length; i++)
            {
                DisableConfigCategories[i] = AddConfigCategory((BalanceCategory)i);
            }
        }

        ConfigEntry<bool> AddConfigCategory(BalanceCategory category)
        {
            string categoryName = (category).ToString();

            ConfigEntry<bool> newCategoryConfig = CustomConfigFile.Bind<bool>(
                categoryName,
                "DISABLE changes?",
                false,
                $"Set this to TRUE if you would like to disable changes for the balance category: {categoryName}"
                );

            return newCategoryConfig;
        }

        void InitializeCoreModules()
        {
            var CoreModuleTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(CoreModule)));

            foreach (var coreModuleType in CoreModuleTypes)
            {
                CoreModule coreModule = (CoreModule)Activator.CreateInstance(coreModuleType);

                coreModule.Init();

                Debug.Log("Core Module: " + coreModule + " Initialized!");
            }
        }
        #endregion

        #region twisted scavs

        public List<TwistedScavengerBase> Scavs = new List<TwistedScavengerBase>();
        public static Dictionary<TwistedScavengerBase, bool> ScavStatusDictionary = new Dictionary<TwistedScavengerBase, bool>();
        private void InitializeScavengers()
        {
            var ScavTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(TwistedScavengerBase)));

            foreach (var scavType in ScavTypes)
            {
                TwistedScavengerBase scav = (TwistedScavengerBase)System.Activator.CreateInstance(scavType);

                if (ValidateScav(scav, Scavs))
                {
                    scav.PopulateItemInfos(CustomConfigFile);
                    scav.Init(CustomConfigFile);
                }
                else
                {
                    Debug.Log("Scavenger: " + scav.ScavLangTokenName + " did not initialize!");
                }
            }
        }

        bool ValidateScav(TwistedScavengerBase scav, List<TwistedScavengerBase> scavList)
        {
            BalanceCategory category = scav.Category;

            bool enabled = true;

            if (category != BalanceCategory.None && category != BalanceCategory.Count)
            {
                string name = scav.ScavName.Replace("'", "");
                enabled = IsCategoryEnabled(category) &&
                CustomConfigFile.Bind<bool>(category.ToString(), $"Enable Twisted Scavenger: {name}", true, "Should this scavenger appear in A Moment, Whole?").Value;
            }
            else
            {
                Debug.Log($"{scav.ScavLangTokenName} initializing into Balance Category: {category}!!");
            }

            //ItemStatusDictionary.Add(item, itemEnabled);

            if (enabled)
            {
                scavList.Add(scav);
            }
            return enabled;
        }
        #endregion

        #region items

        public List<ItemBase> Items = new List<ItemBase>();
        public static Dictionary<ItemBase, bool> ItemStatusDictionary = new Dictionary<ItemBase, bool>();

        void InitializeItems()
        {
            var ItemTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ItemBase)));

            foreach (var itemType in ItemTypes)
            {
                ItemBase item = (ItemBase)System.Activator.CreateInstance(itemType);
                if (item.IsHidden)
                    return;

                if (ValidateItem(item, Items))
                {
                    item.Init(CustomConfigFile);
                }
                else
                {
                    Debug.Log("Item: " + item.ItemName + " Did not initialize!");
                }
            }
        }

        bool ValidateItem(ItemBase item, List<ItemBase> itemList)
        {
            BalanceCategory category = item.Category;

            var itemEnabled = true;

            if (category != BalanceCategory.None && category != BalanceCategory.Count)
            {
                string name = item.ItemName.Replace("'", "");
                itemEnabled = IsCategoryEnabled(category) &&
                CustomConfigFile.Bind<bool>(category.ToString(), $"Enable Item: {name}", true, "Should this item appear in runs?").Value;
            }
            else
            {
                Debug.Log($"{item.ItemName} item initializing into Balance Category: {category}!!");
            }

            //ItemStatusDictionary.Add(item, itemEnabled);

            if (itemEnabled)
            {
                itemList.Add(item);
            }
            return itemEnabled;
        }
        #endregion
        
        #region equips

        public List<EquipmentBase> Equipments = new List<EquipmentBase>();
        public static Dictionary<EquipmentBase, bool> EquipmentStatusDictionary = new Dictionary<EquipmentBase, bool>();
        void InitializeEquipment()
        {
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EquipmentBase equipment = (EquipmentBase)System.Activator.CreateInstance(equipmentType);
                if (equipment.IsHidden)
                    return;

                if (ValidateEquipment(equipment, Equipments))
                {
                    equipment.Init(Config);
                }
            }
        }
        public bool ValidateEquipment(EquipmentBase equipment, List<EquipmentBase> equipmentList)
        {
            BalanceCategory category = equipment.Category;

            var itemEnabled = true;

            if (category != BalanceCategory.None && category != BalanceCategory.Count)
            {
                itemEnabled = IsCategoryEnabled(category) &&
                CustomConfigFile.Bind<bool>(category.ToString(), "Enable Equipment: " + equipment.EquipmentName, true, "Should this item appear in runs?").Value;
            }
            else
            {
                Debug.Log($"{equipment.EquipmentName} equipment initializing into Balance Category: {category}!!");
            }

            EquipmentStatusDictionary.Add(equipment, itemEnabled);

            if (itemEnabled)
            {
                equipmentList.Add(equipment);
            }
            return itemEnabled;
        }

        public static List<EquipmentDef> EliteEquipments = new List<EquipmentDef>();
        public static Dictionary<EliteEquipmentBase, bool> EliteEquipmentStatusDictionary = new Dictionary<EliteEquipmentBase, bool>();
        void InitializeEliteEquipment()
        {
            var EquipmentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(EliteEquipmentBase)));

            foreach (var equipmentType in EquipmentTypes)
            {
                EliteEquipmentBase equipment = (EliteEquipmentBase)System.Activator.CreateInstance(equipmentType);

                if (ValidateEliteEquipment(equipment))
                {
                    equipment.Init(Config);
                    EliteEquipments.Add(equipment.EliteEquipmentDef);
                }
            }
        }
        public bool ValidateEliteEquipment(EliteEquipmentBase equipment)
        {
            BalanceCategory category = equipment.Category;

            var itemEnabled = true;

            if (category != BalanceCategory.None && category != BalanceCategory.Count)
            {
                itemEnabled = IsCategoryEnabled(category) &&
                CustomConfigFile.Bind<bool>(category.ToString(), "Enable Equipment: " + equipment.EliteEquipmentName, true, "Should this item appear in runs?").Value;
            }
            else
            {
                Debug.Log($"{equipment.EliteEquipmentName} equipment initializing into Balance Category: {category}!!");
            }

            EliteEquipmentStatusDictionary.Add(equipment, itemEnabled);
            return itemEnabled;
        }
        #endregion
    }
}
