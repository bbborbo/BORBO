﻿using BepInEx.Configuration;
using Borbo.CoreModules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RoR2.GivePickupsOnStart;

namespace Borbo.Scavengers
{
    public abstract class TwistedScavengerBase<T> : TwistedScavengerBase where T : TwistedScavengerBase<T>
    {
        public static T instance { get; private set; }

        public TwistedScavengerBase()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class TwistedScavengerBase
    {
        public static string baseTscavTokenName = "BorboTScav";
        MultiCharacterSpawnCard twistedScavengerSpawnCard = LegacyResourcesAPI.Load<MultiCharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscScavLunar");

        public abstract string ScavName { get; }
        public abstract string ScavTitle { get; }
        public abstract string ScavLangTokenName { get; }
        public abstract string ScavEquipDefName { get; } //Can be "" if no equipment is desired
        public virtual List<ItemInfo> ItemInfos { get; set; } = new List<ItemInfo>() { };
        public virtual BalanceCategory Category { get; set; } = BalanceCategory.None;
        public virtual string ScavFullNameOverride { get; set; } = ""; //Only use if you do not wish to use the "ScavName the ScavTitle" format

        public GameObject ScavObject;
        public CharacterBody ScavBody;

        public abstract void PopulateItemInfos(ConfigFile config);
        public abstract void Init(ConfigFile config);

        internal void AddItemInfo(string name, int count)
        {
            if (count <= 0)
                return;
            ItemInfo itemInfo = new ItemInfo();

            itemInfo.itemString = name;
            itemInfo.count = count;

            ItemInfos.Add(itemInfo);
        }

        internal void GenerateTwistedScavenger()
        {
            string fullName = (ScavFullNameOverride == "") ? $"{ScavName} the {ScavTitle}" : ScavFullNameOverride;
            Debug.Log("Generating Twisted Scavenger: " + fullName);
            string nameToken = baseTscavTokenName + ScavLangTokenName;
            LanguageAPI.Add(nameToken, fullName);

            GameObject masterObject = LegacyResourcesAPI.Load<GameObject>("prefabs/charactermasters/ScavLunar1Master").InstantiateClone($"{nameToken}Master", true);
            GameObject bodyObject = LegacyResourcesAPI.Load<GameObject>("prefabs/characterbodies/ScavLunar1Body").InstantiateClone($"{nameToken}Body", true);

            CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
            master.bodyPrefab = bodyObject;
            CharacterBody body = bodyObject.GetComponent<CharacterBody>();
            body.baseNameToken = nameToken;

            int count = twistedScavengerSpawnCard.masterPrefabs.Length;
            Array.Resize<GameObject>(ref twistedScavengerSpawnCard.masterPrefabs, count + 1);
            twistedScavengerSpawnCard.masterPrefabs[count] = masterObject;


            foreach (GivePickupsOnStart gpos in masterObject.GetComponents<GivePickupsOnStart>())
            {
                gpos.enabled = false;
            }

            GivePickupsOnStart pickupComp = masterObject.AddComponent<GivePickupsOnStart>();
            pickupComp.itemInfos = ItemInfos.ToArray();
            if (ScavEquipDefName != "")
            {
                pickupComp.equipmentString = ScavEquipDefName;;
            }

            Assets.bodyPrefabs.Add(bodyObject);
            Assets.masterPrefabs.Add(masterObject);
            ScavBody = body;
            ScavObject = bodyObject;
        }
    }
}
