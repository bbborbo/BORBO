﻿using BepInEx;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Borbo
{
    internal partial class Main : BaseUnityPlugin
    {
        GameObject healPack = Resources.Load<GameObject>("prefabs/networkedobjects/HealPack");
        float toothDuration = 15; //5

        GameObject ammoPack = Resources.Load<GameObject>("prefabs/networkedobjects/AmmoPack");
        GameObject moneyPack = Resources.Load<GameObject>("prefabs/networkedobjects/BonusMoneyPack");

        public void FixPickupStats()
        {
            BuffPickupRange(healPack);
            healPack.GetComponent<DestroyOnTimer>().duration = toothDuration;
            healPack.GetComponent<BeginRapidlyActivatingAndDeactivating>().delayBeforeBeginningBlinking = (toothDuration - 2f);

            BuffPickupRange(ammoPack);
            BuffPickupRange(moneyPack);

            On.RoR2.GravitatePickup.OnTriggerEnter += ChangeGravitateTargetBehavior;
        }

        private void ChangeGravitateTargetBehavior(On.RoR2.GravitatePickup.orig_OnTriggerEnter orig, GravitatePickup self, Collider other)
        {
            if (NetworkServer.active && TeamComponent.GetObjectTeam(other.gameObject) == self.teamFilter.teamIndex)
            {
                if (self.gravitateTarget)
                {
                    if (other.gameObject.transform == self.gravitateTarget)
                        return;

                    HealthComponent targetHealthComponent = self.gravitateTarget.GetComponent<HealthComponent>();
                    if (targetHealthComponent && targetHealthComponent.body.isPlayerControlled)
                        return;
                }

                HealthComponent component = other.gameObject.GetComponent<HealthComponent>();
                if (component != null && (self.gravitateAtFullHealth || component.health < component.fullHealth))
                {
                    if (component.body.isPlayerControlled)
                    {
                        self.gravitateTarget = other.gameObject.transform;
                        return;
                    }
                }

                if (!self.gravitateTarget)
                {
                    if (self.gravitateAtFullHealth)
                    {
                        self.gravitateTarget = other.gameObject.transform;
                    }
                }
            }
        }

        void BuffPickupRange(GameObject pack)
        {
            GravitatePickup gravPickup = pack.GetComponentInChildren<GravitatePickup>();
            if(gravPickup != null)
            {
                Collider gravitateTrigger = gravPickup.gameObject.GetComponent<Collider>();
                if (gravitateTrigger.isTrigger)
                {
                    gravitateTrigger.transform.localScale *= 2.5f;
                }
            }
            else
            {
                Debug.Log($"GameObject {pack.name} has no GravitatePickup component!");
            }
        }
    }
}