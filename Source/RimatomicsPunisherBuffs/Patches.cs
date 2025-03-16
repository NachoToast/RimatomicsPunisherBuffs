using HarmonyLib;
using Rimatomics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace RimatomicsPunisherBuffs
{
    [StaticConstructorOnStartup]
    internal static class Patches
    {
        static Patches()
        {
            Harmony harmony = new Harmony("NachoToast.RimatomicsPunisherBuffs");

            harmony.Patch(
                original: AccessTools.Method(
                    type: typeof(Verb),
                    name: nameof(Verb.TryFindShootLineFromTo)),
                transpiler: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(ReplaceVerbPropsLineOfSightReferences_Transpiler)));

            harmony.Patch(
                original: AccessTools.Method(
                    type: typeof(Verb),
                    name: "CanHitCellFromCellIgnoringRange"),
                transpiler: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(ReplaceVerbPropsLineOfSightReferences_Transpiler)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(
                    type: typeof(Building_Railgun),
                    name: nameof(Building_Railgun.PulseSize)),
                postfix: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(GetWeaponPulseSize_Postfix)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(
                    type: typeof(Building_Railgun),
                    name: nameof(Building_Railgun.WorldRange)),
                postfix: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(GetWorldRange_Postfix)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(
                    type: typeof(Building_Railgun),
                    name: nameof(Building_Railgun.spread)),
                postfix: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(GetSpread_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(
                    type: typeof(Building_Railgun),
                    name: "ChoseWorldTarget"),
                postfix: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(ChoseWorldTarget_Postfix)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(
                    type: typeof(WorldObject_Sabot),
                    name: "TraveledPctStepPerTick"),
                postfix: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(TraveledPctStepPerTick_Postfix)));
        }

        private static IEnumerable<CodeInstruction> ReplaceVerbPropsLineOfSightReferences_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool IsLoadFieldInstruction<T1>(CodeInstruction instruction, string operandName)
            {
                if (instruction.opcode != OpCodes.Ldfld)
                {
                    return false;
                }

                if (!(instruction.operand is FieldInfo fieldInfo))
                {
                    return false;
                }

                if (fieldInfo.FieldType != typeof(T1))
                {
                    return false;
                }

                if (fieldInfo.Name != operandName)
                {
                    return false;
                }

                return true;
            }

            bool successfullyDidPatch = false;

            List<CodeInstruction> codes = instructions.ToList();

            for (int i = 0; i < codes.Count - 1; i++)
            {
                if (!IsLoadFieldInstruction<VerbProperties>(codes[i], nameof(Verb.verbProps)))
                {
                    continue;
                }

                if (!IsLoadFieldInstruction<bool>(codes[i + 1], nameof(VerbProperties.requireLineOfSight)))
                {
                    continue;
                }

                codes[i].opcode = OpCodes.Nop;

                codes[i + 1].opcode = OpCodes.Call;

                codes[i + 1].operand = AccessTools.Method(
                    type: typeof(Extensions),
                    name: nameof(Extensions.RequiresLineOfSight));

                successfullyDidPatch = true;

                break;
            }

            if (!successfullyDidPatch)
            {
                Log.Error("[Rimatomics Punisher Buffs] Failed to find correct code to patch");
            }

            return codes.AsEnumerable();
        }

        private static void GetWeaponPulseSize_Postfix(Building_EnergyWeapon __instance, ref float __result)
        {
            CompUpgradable upgradeComp = __instance?.UG;

            if (upgradeComp == null)
            {
                return;
            }

            if (upgradeComp.HasUpgrade(ThisDefOf.RimatomicsPunisherBuffs_DriveCylinders))
            {
                __result += __result * 0.2f;
            }

            if (upgradeComp.HasUpgrade(ThisDefOf.RimatomicsPunisherBuffs_PropellantBoosters))
            {
                __result += __result * 2f;
            }
        }

        private static void GetWorldRange_Postfix(Building_Railgun __instance, ref int __result)
        {
            if (__instance?.UG?.HasUpgrade(ThisDefOf.RimatomicsPunisherBuffs_PropellantBoosters) == true)
            {
                __result += 1000;
            }
        }

        private static void GetSpread_Postfix(Building_Railgun __instance, ref int __result)
        {
            __result += __instance.GetSpreadOffset();
        }

        private static void ChoseWorldTarget_Postfix(Building_Railgun __instance, bool __result)
        {
            if (!__result)
            {
                return;
            }

            Find.Targeter.SetHighlightAction(delegate (LocalTargetInfo target)
            {
                GenDraw.DrawTargetHighlight(target);

                GenDraw.DrawFieldEdges(new CellRect(target.Cell, __instance.spread).ToList(), Color.white);

                GenDrawExt.DrawFireMissionExplosiveRadius(__instance, target);
            });

            Find.Targeter.SetOnGuiAction(delegate (LocalTargetInfo target)
            {
                GenDrawExt.DrawSpreadLabel(__instance, target);

                GenDrawExt.DrawExplosionRadiusLabel(__instance, target);
            });
        }

        private static void TraveledPctStepPerTick_Postfix(WorldObject_Sabot __instance, ref float __result)
        {
            if (!(__instance.railgun is Building_Railgun railgun))
            {
                return;
            }

            if (railgun.UG?.HasUpgrade(ThisDefOf.RimatomicsPunisherBuffs_PropellantBoosters) == true)
            {
                __result *= 41f;
            }
        }
    }
}