using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Borbo.CoreModules.StatHooks;

namespace Borbo.Items
{
    class WickedBand : ItemBase<WickedBand>
    {
        float damageCoefficientThreshold = 4f;
        float cooldownRefreshBase = 0f;
        float cooldownRefreshStack = 1f;
        static ItemDisplayRuleDict IDR = new ItemDisplayRuleDict();

        public override string ItemName => "Wicked Band";

        public override string ItemLangTokenName => "WICKEDBAND";

        public override string ItemPickupDesc => "High damage hits reduce cooldowns by 1 second.";

        public override string ItemFullDescription => $"Hits that deal <style=cIsDamage>more than {Tools.ConvertDecimal(damageCoefficientThreshold)} damage</style> will also " +
            $"<style=cIsUtility>reduce ALL cooldowns by {cooldownRefreshBase + cooldownRefreshStack}s</style> <style=cStack>(+{cooldownRefreshStack} per stack)</style>. " +
            $"<style=cIsHealth>Has no cooldown.</style>";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;
        public override ItemTag[] ItemTags { get; set; } = new ItemTag[] { ItemTag.Utility };
		public override BalanceCategory Category { get; set; } = BalanceCategory.StateOfInteraction;

        public override GameObject ItemModel => LoadDropPrefab("WickedBand");

        public override Sprite ItemIcon => LoadItemIcon("texIconWickedBand");

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return IDR;
        }

        public static void GetDisplayRules(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();
            if (ItemBase.DefDictionary.ContainsKey("BorboWickedBand"))
            {
                ItemDef def = WickedBand.instance.ItemsDef;

                if (def != null)
                {
                    foreach (GameObject bodyPrefab in BodyCatalog.bodyPrefabs)
                    {
                        CharacterModel model = bodyPrefab.GetComponentInChildren<CharacterModel>();
                        if (model)
                        {
                            ItemDisplayRuleSet idrs = model.itemDisplayRuleSet;
                            if (idrs)
                            {
                                // clone the original item display rule

                                Array.Resize(ref idrs.keyAssetRuleGroups, idrs.keyAssetRuleGroups.Length + 1);
                                idrs.keyAssetRuleGroups[idrs.keyAssetRuleGroups.Length - 1].displayRuleGroup = idrs.FindDisplayRuleGroup(JunkContent.Items.CooldownOnCrit);
                                idrs.keyAssetRuleGroups[idrs.keyAssetRuleGroups.Length - 1].keyAsset = def;

                                idrs.GenerateRuntimeValues();
                            }
                        }
                    }
                }
            }
        }

        public override void Hooks()
        {
            On.RoR2.BodyCatalog.Init += GetDisplayRules;
            GetHitBehavior += WickedBandCdr;
        }

        private void WickedBandCdr(CharacterBody body, DamageInfo damageInfo, GameObject victim)
        {
		    int wickedBandCount = GetCount(body);
		    if(wickedBandCount > 0)
		    {
			    if ((damageInfo.damage / body.damage) >= damageCoefficientThreshold && !damageInfo.procChainMask.HasProc(ProcType.Rings))
			    {
				    float dt = (cooldownRefreshBase + cooldownRefreshStack * wickedBandCount) * damageInfo.procCoefficient;

				    RoR2.Util.PlaySound("Play_item_proc_crit_cooldown", body.gameObject);
				    damageInfo.procChainMask.AddProc(ProcType.Rings);

                    if (NetworkServer.active)
                    {
                        GenericSkill[] skills = body.skillLocator.allSkills;
                        foreach (GenericSkill skill in skills)
                        {
                            skill.RunRecharge(dt);
                        }
                    }

				    EquipmentSlot equipmentSlot = body.equipmentSlot;
                    if (equipmentSlot)
				    {
					    body.inventory.DeductActiveEquipmentCooldown(dt);
				    }
			    }
		    }
		}

        public override void Init(ConfigFile config)
        {
            CreateItem();
            CreateLang();
            Hooks();
        }
    }
}
