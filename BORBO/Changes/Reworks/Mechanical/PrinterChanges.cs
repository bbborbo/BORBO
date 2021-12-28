using BepInEx;
using R2API;
using static R2API.DirectorAPI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Borbo
{
    internal partial class Main : BaseUnityPlugin
    {
        public static GameObject whitePrinter = Resources.Load<GameObject>("prefabs/networkedobjects/chest/Duplicator");
        public static GameObject greenPrinter = Resources.Load<GameObject>("prefabs/networkedobjects/chest/DuplicatorLarge");
        public static GameObject redPrinter = Resources.Load<GameObject>("prefabs/networkedobjects/chest/DuplicatorMilitary");
        public static GameObject scrapper = Resources.Load<GameObject>("prefabs/networkedobjects/chest/Scrapper");

        public static GameObject equipBarrel = Resources.Load<GameObject>("prefabs/networkedobjects/chest/EquipmentBarrel");

        void PrinterAndScrapperOccurenceChanges()
        {
            DirectorAPI.InteractableActions += DeletePrintersAndScrappers;
            //DirectorAPI.Helpers.RemoveExistingInteractableFromStage(DirectorAPI.Helpers.InteractableNames.PrinterCommon, Stage.Commencement);
        }

        private void DeletePrintersAndScrappers(List<DirectorAPI.DirectorCardHolder> cardList, DirectorAPI.StageInfo currentStage)
        {
            List<DirectorAPI.DirectorCardHolder> removeList = new List<DirectorAPI.DirectorCardHolder>();
            foreach (DirectorAPI.DirectorCardHolder dc in cardList)
            {
                GameObject cardPrefab = dc.Card.spawnCard.prefab;
                if (dc.InteractableCategory == InteractableCategory.Duplicator && dc.Card.selectionWeight != 0)
                {
                    if (IsPrinter(cardPrefab))
                    {
                        if (OnScrapperStage(currentStage.stage))
                        {
                            dc.Card.selectionWeight = 0;
                            removeList.Add(dc);
                        }
                        else
                        {
                            if(cardPrefab == greenPrinter)
                            {
                                dc.Card.selectionWeight = 10;
                            }
                            if (cardPrefab == redPrinter)
                            {
                                if (currentStage.stage == Stage.SkyMeadow)
                                {
                                    dc.Card.selectionWeight = 12;
                                }
                                else
                                {
                                    dc.Card.selectionWeight = 3;
                                }
                            }
                        }
                    }

                    if (cardPrefab == scrapper)
                    {
                        if (OnPrinterStage(currentStage.stage))
                        {
                            dc.Card.selectionWeight = 0;
                            removeList.Add(dc);
                        }
                        else
                        {
                            dc.Card.selectionWeight = 30;
                        }
                    }
                }
                else if(cardPrefab == equipBarrel)
                {
                    if(currentStage.stage == Stage.TitanicPlains || currentStage.stage == Stage.DistantRoost)
                    {
                        dc.Card.selectionWeight = 8;
                    }
                }
            }
            foreach (DirectorAPI.DirectorCardHolder dc in removeList)
            {
                cardList.Remove(dc);
            }
        }

        private bool IsPrinter(GameObject prefab)
        {
            return prefab == whitePrinter 
                || prefab == greenPrinter 
                || prefab == redPrinter;
        }
        private bool OnPrinterStage(Stage stage)
        {
            return !OnScrapperStage(stage);
        }
        private bool OnScrapperStage(Stage stage)
        {
            return stage == Stage.TitanicPlains
                || stage == Stage.DistantRoost
                || stage == Stage.RallypointDelta
                || stage == Stage.ScorchedAcres;
        }
    }
}
