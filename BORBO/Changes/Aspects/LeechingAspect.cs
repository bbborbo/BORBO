using BepInEx.Configuration;
using Borbo.Components;
using Borbo.CoreModules;
using EntityStates.TeleporterHealNovaController;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Borbo.CoreModules.EliteModule;
using static EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse;
using static RoR2.CombatDirector;

namespace Borbo.Equipment
{
    class LeechingAspect : EliteEquipmentBase
    {
        public static float healPulseRadius = 25.1354563f;
        public static float healFraction = 0.1f;
        public static float maxHealFraction = 2f;


        public override string EliteEquipmentName => "N\u2019Kuhana\u2019s Respite";

        public override string EliteAffixToken => "AFFIX_LEECH";

        public override string EliteEquipmentPickupDesc => "Become an aspect of eternity.";

        public override string EliteEquipmentFullDescription => "";

        public override string EliteEquipmentLore => "";

        public override string EliteModifier => "Serpentine";

        public override GameObject EliteEquipmentModel => Resources.Load<GameObject>("prefabs/NullModel");

        public override Sprite EliteEquipmentIcon => Resources.Load<Sprite>("textures/miscicons/texWIPIcon");


        public override Sprite EliteBuffIcon => RoR2Content.Equipment.AffixHaunted.passiveBuffDef.iconSprite;
        public override Color EliteBuffColor => Color.magenta;

        //public override Material EliteOverlayMaterial { get; set; } = Resources.Load<Material>("materials/matElitePoisonOverlay");
        public override Material EliteOverlayMaterial { get; set; } = Main.assetBundle.LoadAsset<Material>(Main.assetsPath + "matLeeching.mat");
        public override string EliteRampTextureName { get; set; } = "texRampLeeching";
        public override EliteTiers EliteTier { get; set; } = EliteTiers.Tier2;

        public override BalanceCategory Category { get; set; } = BalanceCategory.StateOfDifficulty;

        public override bool CanDrop { get; } = false;

        public override float Cooldown { get; } = 0f; 


        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return null;
        }

        public override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += LeechingOnHit;
            On.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.OnEnter += LeechingHealingPulse;
            //IL.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.HealPulse.Update += LeechingHealing;
            //On.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.HealPulse.ctor += LeechingHealingShitttt;
        }

        private void LeechingHealingShitttt(On.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.HealPulse.orig_ctor orig, 
            object self, Vector3 origin, float finalRadius, float healFractionValue, float duration, TeamIndex teamIndex)
        {
            if (finalRadius == healPulseRadius)
            {
                healFractionValue = 0;

            }
            orig(self, origin, finalRadius, healFractionValue, duration, teamIndex);
        }

        private void LeechingHealing(ILContext il)
        {
            ILCursor c = new ILCursor(il);


            /*if((HealPulse)self.finalRadius == healPulseRadius)
            {

            }
            else
            {
                orig(self);
            }*/
        }

        private void LeechingHealingPulse(On.EntityStates.TeleporterHealNovaController.TeleporterHealNovaPulse.orig_OnEnter orig, TeleporterHealNovaPulse self)
        {
            orig(self);
            LeechingHealingPulseComponent LHP = self.gameObject.GetComponent<LeechingHealingPulseComponent>();
            if (LHP != null)
            {
                self.radius = healPulseRadius;
                self.healPulse.finalRadius = healPulseRadius;
                self.healPulse.healFractionValue = 0;

                TeamFilter teamFilter = self.GetComponent<TeamFilter>();
                TeamIndex teamIndex = teamFilter ? teamFilter.teamIndex : TeamIndex.None;
                float healMax = LHP.maxHealth * maxHealFraction;
                float procCoeff = LHP.procCoefficient;

                SphereSearch sphereSearch = new SphereSearch
                {
                    mask = LayerIndex.entityPrecise.mask,
                    origin = self.transform.position,
                    queryTriggerInteraction = QueryTriggerInteraction.Collide,
                    radius = healPulseRadius
                };
                TeamMask teamMask = default(TeamMask);
                List<HurtBox> hurtBoxesList = new List<HurtBox>();
                List<HealthComponent> healedTargets = new List<HealthComponent>();

                teamMask.AddTeam(teamIndex);
                sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(teamMask).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes(hurtBoxesList);

                for (int i = 0; i < hurtBoxesList.Count; i++)
                {
                    HealthComponent healthComponent = hurtBoxesList[i].healthComponent;
                    if (!healedTargets.Contains(healthComponent))
                    {
                        healedTargets.Add(healthComponent);
                        if (!IsElite(healthComponent.body, EliteBuffDef))
                        {
                            float baseHeal = healthComponent.fullHealth * healFraction;

                            float endHeal = Mathf.Max(baseHeal, healMax) * procCoeff;

                            healthComponent.Heal(endHeal, default(ProcChainMask));
                        }
                    }
                }
            }
        }

        private void LeechingOnHit(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if(damageInfo.attacker && victim)
            {
                CharacterBody aBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody vBody = victim.GetComponent<CharacterBody>();

                if (aBody && vBody)
                {
                    if (IsElite(aBody, EliteBuffDef))
                    {
                        Debug.Log("Leeching Healing Pulse Here!");
                        Pulse(aBody, damageInfo);
                    }
                }
            }

            orig(self, damageInfo, victim);
        }
        protected void Pulse(CharacterBody body, DamageInfo damageInfo)
        {
            TeamIndex team = body.teamComponent.teamIndex;
            Transform transform = damageInfo.attacker.transform;

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(TeleporterHealNovaGeneratorMain.pulsePrefab, transform);
            gameObject.GetComponent<TeamFilter>().teamIndex = team;
            NetworkServer.Spawn(gameObject);

            LeechingHealingPulseComponent LHP = gameObject.AddComponent<LeechingHealingPulseComponent>();
            LHP.procCoefficient = damageInfo.procCoefficient;
            LHP.maxHealth = body.healthComponent.fullCombinedHealth;
        }

        public override void Init(ConfigFile config)
        {
            /*Material mat = Resources.Load<Material>("materials/matEliteHauntedOverlay");
            mat.color = Color.magenta;
            EliteMaterial = mat;*/

            CanAppearInEliteTiers = VanillaTier2(); 

            CreateEliteEquipment();
            CreateLang();
            CreateElite();
            Hooks();
        }

        void AssignEliteTier()
        {
            foreach (CombatDirector.EliteTierDef etd in CombatDirector.eliteTiers)
            {
                EliteDef[] eliteTypes = new EliteDef[] { RoR2Content.Elites.Poison, RoR2Content.Elites.Haunted };

                if (etd.eliteTypes == eliteTypes)
                {
                    CanAppearInEliteTiers = new EliteTierDef[1] { etd };
                }
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            return false;
        }
    }
}
