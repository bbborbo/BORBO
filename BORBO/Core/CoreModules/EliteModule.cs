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
        //public static List<BorboEliteDef> Elites = new List<BorboEliteDef>();
        public static Texture defaultShaderRamp = Main.assetBundle.LoadAsset<Texture>(Main.assetsPath + "texRampFrenzied.tex");

        public override void Init()
        {

        }
    }
}
