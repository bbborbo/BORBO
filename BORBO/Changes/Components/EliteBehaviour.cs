using Borbo.CoreModules;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Borbo.CoreModules.EliteModule;

namespace Borbo.Components
{
    [RequireComponent(typeof(CharacterBody))]
    public class EliteBehaviour : MonoBehaviour
    {
        public int EliteRampPropertyID
        {
            get
            {
                return Shader.PropertyToID("_EliteRamp");
            }
        }

        public CharacterBody body;
        public CharacterModel model;

        private GameObject effectInstance;
        private BorboEliteDef borboEliteDef;
        private Texture oldRamp;

        #region Check elite
        public void CheckForItems()
        {
            if (IsBorboElite())
            {
                if (model)
                {
                    model.UpdateOverlays(); //<-- not updating this will cause model.myEliteIndex to not be accurate.
                    body.RecalculateStats(); //<-- not updating recalcstats will cause isElite to be false IF it wasnt an elite before.
                    foreach (BorboEliteDef BED in EliteModule.Elites)
                    {
                        if (body.isElite && model.myEliteIndex == BED.eliteDef.eliteIndex)
                        {
                            SetNewElite(BED);
                            Debug.Log($"Setting new elite: {BED.eliteDef.name}");
                        }
                    }
                }
            }
            else
            {
                SetNewElite(null);
            }
        }

        public bool IsBorboElite()
        {
            foreach (EquipmentDef eliteEqp in Main.EliteEquipments)
            {
                if (body.inventory?.GetEquipmentIndex() == eliteEqp.equipmentIndex)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Set elite
        public void Start()
        {
            model = body?.modelLocator.modelTransform.GetComponent<CharacterModel>();
        }

        public void SetNewElite(BorboEliteDef eliteDef)
        {
            if (model && eliteDef != borboEliteDef)
            {
                oldRamp = borboEliteDef?.eliteRamp;
                borboEliteDef = eliteDef;

                if (borboEliteDef == null) //this only gets executed if an elite def has already been loaded into the behavior
                {
                    if (model && model.propertyStorage != null)
                    {
                        model.propertyStorage.SetTexture(EliteRampPropertyID, Shader.GetGlobalTexture(EliteRampPropertyID));
                    }
                    if (effectInstance)
                        Destroy(effectInstance);
                }
                if (borboEliteDef)
                {
                    if (borboEliteDef.spawnEffect)
                        effectInstance = Instantiate(borboEliteDef.spawnEffect, body.aimOriginTransform, false);
                }
            }
        }

        public void UpdateShaderRamp()
        {
            if (model && borboEliteDef)
            {
                Texture eliteRamp = borboEliteDef.eliteRamp;// == null ? EliteModule.defaultShaderRamp : borboEliteDef.eliteRamp;
                model.propertyStorage.SetTexture(EliteRampPropertyID, eliteRamp);
                //Debug.Log(EliteRampPropertyID);

                if (eliteRamp == null)
                {
                    Debug.Log($"No elite ramp available for this elite: {borboEliteDef.eliteDef.name}");
                    borboEliteDef = null;
                }
            }
            else if (model)
            {
                if (!oldRamp)
                    return;

                if (model.propertyStorage.GetTexture(EliteRampPropertyID) == oldRamp)
                {
                    model.propertyStorage.Clear();
                }
            }
        }
        #endregion
    }
}