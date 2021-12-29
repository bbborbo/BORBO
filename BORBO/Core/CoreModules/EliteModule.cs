using Borbo.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Borbo.CoreModules
{
    public class EliteModule : CoreModule
    {
        public static List<BorboEliteDef> Elites = new List<BorboEliteDef>();
        public static Texture defaultShaderRamp = Main.assetBundle.LoadAsset<Texture>(Main.assetsPath + "texRampFrenzied.tex");

        public override void Init()
        {
            RoR2Application.onLoad += AddElites;
        }

        #region Hooks
        private static void AddElites()
        {
            foreach (var BED in Elites)
            {
                switch (BED.eliteTier)
                {
                    case EliteTiers.Tier1:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[1].eliteTypes, BED.eliteDef);
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[2].eliteTypes, BED.eliteDef);
                        break;
                    case EliteTiers.Tier2:
                        HG.ArrayUtils.ArrayAppend(ref CombatDirector.eliteTiers[3].eliteTypes, BED.eliteDef);
                        break;
                    case EliteTiers.Other:
                        break;
                }
            }
        }
        #endregion

        #region EliteDef
        public class BorboEliteDef : ScriptableObject
        {
            public EliteDef eliteDef;
            [Tooltip("The tier of the elite. Choose other if your Elite has its own tier or a different spawning system.")]
            public EliteTiers eliteTier;
            public Color lightColor = Color.clear;
            [Tooltip("The color ramp of the elite.")]
            public Texture eliteRamp;
            [Tooltip("The overlay material of the elite, used mostly for post loop elites.")]
            public Material overlayMaterial;
            [Tooltip("Effect thats spawned once the elite spawns.")]
            public GameObject spawnEffect;
        }
        public enum EliteTiers
        {
            Tier1,
            Tier2,
            Other
        }
        #endregion
    }
}
