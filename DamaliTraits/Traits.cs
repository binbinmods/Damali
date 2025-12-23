using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Damali.CustomFunctions;
using static Damali.Plugin;
using static Damali.DescriptionFunctions;
using static Damali.CharacterFunctions;

namespace Damali
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance, ref _theEvent, ref _character, ref _target, ref _auxInt, ref _auxString, ref _castedCard);
                return false;
            }
            return true;
        }

        public static void DoCustomTrait(string _trait, ref Trait __instance, ref Enums.EventActivation _theEvent, ref Character _character, ref Character _target, ref int _auxInt, ref string _auxString, ref CardData _castedCard)
        {
            // get info you may need
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character))
            {
                return;
            }

            if (_trait == trait0)
            {
                // Fury on you increases Speed by 1 per charge.
            }


            else if (_trait == trait2a)
            {
                // trait2a
                // Whenever you apply Slow, gain 4 block for each applied
                string traitName = traitData.TraitName;
                string traitId = _trait;

                if (_auxString == "slow")
                {
                    // LogDebug($"Handling Trait {traitId}: {traitName}");
                    _character.SetAuraTrait(_character, "block", 4);
                }
            }



            else if (_trait == trait2b)
            {
                // trait2b:
                // Increases All Damage by 3% per speed difference between you and the target.
                string traitName = traitData.TraitName;
                string traitId = _trait;

            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // At the start of your turn, reduce the cost of all cards by 1 for every 20 Fury on you.
                string traitName = traitData.TraitName;
                string traitId = _trait;

                LogDebug($"Handling Trait {traitId}: {traitName}");
                ReduceCostByStacks(Enums.CardType.None, "fury", 20, ref _character, ref heroHand, ref cardDataList, traitName, true);

            }

            else if (_trait == trait4b)
            {
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "GetTraitDamagePercentModifiers")]

        public static void GetTraitDamagePercentModifiersPostfix(Character __instance, ref int __result, Enums.DamageType DamageType)
        {
            // Trait2b Increases All Damage by 3% per Speed above 15. Slow on Enemies reduces All Resistances by 3% per Charge.
            // Fury on you increases All Resistances by 0.5% per charge. Taunt on you increases all Resistances by 10% per charge. Chase Down applies to all Heroes.
            if (__instance.HaveTrait(trait2b) || (AtOManager.Instance.TeamHaveTrait(trait2b) && AtOManager.Instance.TeamHaveTrait(trait4b)))
            {
                int amountToAdd = 3 * (__instance.GetSpeed()[0] - 15);
                __result += Mathf.Max(0, amountToAdd);
            }
        }




        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            string enchant;
            switch (_acId)
            {
                // trait0:
                // Fury on you increases Speed by 1 per charge.

                // trait2b:
                // Trait2b Increases All Damage by 3% per Speed above 15.
                //  Slow on Enemies reduces Blunt Resistance by 4% per Charge.

                // trait 4b:
                // Fury on you increases All Resistances by 0.5% per charge. 
                // Taunt on you increases All Resistances by 10% per charge. 
                // Chase Down applies to all Heroes.

                case "fury":
                    traitOfInterest = trait0;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.CharacterStatModified = Enums.CharacterStat.Speed;
                        __result.CharacterStatAbsoluteValuePerStack = 1;
                        __result.CharacterStatChargesMultiplierNeededForOne = 2;

                    }
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.ResistModified = Enums.DamageType.All;
                        __result.ResistModifiedPercentagePerStack = 0.5f;
                    }

                    break;

                case "taunt":
                    traitOfInterest = trait4b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.ResistModified = Enums.DamageType.All;
                        __result.ResistModifiedPercentagePerStack = 10f;
                    }
                    break;
                case "slow":
                    traitOfInterest = trait2b;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        __result.ResistModified = Enums.DamageType.Blunt;
                        __result.ResistModifiedPercentagePerStack = -4;
                    }
                    enchant = "minitaurkeepkicking";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Enchantment, enchant, AppliesTo.Monsters))
                    {
                        __result.GainCharges = true;
                    }
                    break;
                case "fast":
                    enchant = "minitaurbullishbovine";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Enchantment, enchant, AppliesTo.Monsters))
                    {
                        __result.GainCharges = true;
                    }
                    break;
            }
        }




        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isDamagePreviewActive = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }





    }
}

