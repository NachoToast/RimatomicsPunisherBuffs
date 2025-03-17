using HarmonyLib;
using Multiplayer.API;
using Rimatomics;
using RimWorld.Planet;
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
                    type: typeof(Building_Railgun),
                    name: "ChoseWorldTarget"),
                transpiler: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(ChoseWorldTarget_Transpiler)));

            harmony.Patch(
                original: AccessTools.PropertyGetter(
                    type: typeof(Building_Railgun),
                    name: nameof(Building_Railgun.PulseSize)),
                postfix: new HarmonyMethod(
                    methodType: typeof(Patches),
                    methodName: nameof(GetPulseSize_Postfix)));

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

            if (MP.enabled)
            {
                MP.RegisterAll();
            }
        }

        private static bool CanFireMissionTargetMap(Map targetMap, Map railgunMap, Building_Railgun railgun)
        {
            if (targetMap != railgunMap)
            {
                return true;
            }

            if (railgun.UG?.HasUpgrade(ThisDefOf.RimatomicsPunisherBuffs_DriveCylinders) == true)
            {
                return true;
            }

            return false;
        }

        private static IEnumerable<CodeInstruction> ChoseWorldTarget_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool IsGetMapCall<T>(CodeInstruction instruction)
            {
                if (!(instruction.operand is MethodInfo methodInfo))
                {
                    return false;
                }

                if (methodInfo.DeclaringType != typeof(T))
                {
                    return false;
                }

                if (methodInfo.Name != "get_Map")
                {
                    return false;
                }

                return true;
            }

            bool successfullyDidPatch = false;

            List<CodeInstruction> codes = instructions.ToList();

            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].opcode != OpCodes.Callvirt)
                {
                    continue;
                }

                if (!IsGetMapCall<MapParent>(codes[i]))
                {
                    continue;
                }

                if (codes[i + 1].opcode != OpCodes.Ldarg_0)
                {
                    continue;
                }

                if (codes[i + 2].opcode != OpCodes.Call)
                {
                    continue;
                }

                if (!IsGetMapCall<Thing>(codes[i + 2]))
                {
                    continue;
                }

                if (codes[i + 3].opcode != OpCodes.Bne_Un_S)
                {
                    continue;
                }

                codes[i + 3].opcode = OpCodes.Brtrue_S;

                codes.InsertRange(i + 3, new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(CanFireMissionTargetMap))),
                });

                successfullyDidPatch = true;

                break;
            }

            if (!successfullyDidPatch)
            {
                Log.Error("[Rimatomics Punisher Buffs] Failed to find correct code to patch");
            }

            return codes.AsEnumerable();
        }

        private static void GetPulseSize_Postfix(Building_Railgun __instance, ref float __result)
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
            if (__instance.UG?.HasUpgrade(DubDef.TargetingChip) != true)
            {
                return;
            }

            CompSpreadAdjustable comp = __instance.TryGetComp<CompSpreadAdjustable>();

            if (comp == null)
            {
                return;
            }

            __result += comp.offset;
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

                GenDraw.DrawFieldEdges(new CellRect(target.Cell, __instance.spread).ToList());

                GenDrawExt.DrawFireMissionExplosiveRadius(__instance, target);
            });

            Find.Targeter.SetOnGuiAction(delegate (LocalTargetInfo target)
            {
                int spread = __instance.spread;

                GenDrawExt.DrawLabel(target.Cell, spread, Translations.Spread(spread.ToTileString()));

                if (__instance.HasMeaningfulProjectileRadius(out ThingDef projectile, out float radius))
                {
                    GenDrawExt.DrawLabel(
                        cell: target.Cell,
                        offset: spread + Mathf.FloorToInt(radius),
                        label: Translations.ExplosionRadius(radius.ToTileString(), projectile.label));
                }
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