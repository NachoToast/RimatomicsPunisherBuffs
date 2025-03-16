using Rimatomics;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimatomicsPunisherBuffs
{
    [StaticConstructorOnStartup]
    public class CompSpreadAdjustable : ThingComp
    {
        private const int MIN_LEVEL = 0;
        private const int MAX_LEVEL = 27;

        private static readonly Texture2D RangeUpIcon = ContentFinder<Texture2D>.Get("Rimatomics/UI/rangeUp");
        private static readonly Texture2D RangeDownIcon = ContentFinder<Texture2D>.Get("Rimatomics/UI/rangeDown");

        private int configuredLevel = 0;

        public int ConfiguredLevel
        {
            get
            {
                return configuredLevel;
            }

            set
            {
                if (value < MIN_LEVEL)
                {
                    value = MIN_LEVEL;
                }
                else if (value > MAX_LEVEL)
                {
                    value = MAX_LEVEL;
                }

                if (value == configuredLevel)
                {
                    return;
                }

                configuredLevel = value;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent?.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            if (!(parent is Building_EnergyWeapon weapon))
            {
                yield break;
            }

            if (weapon.UG?.HasUpgrade(DubDef.TargetingChip) != true)
            {
                yield break;
            }

            // Decrease Spread

            Command_Action decreaseSpreadAction = new Command_Action
            {
                defaultLabel = TranslationKeys.DecreaseSpread.Translate(),
                defaultDesc = TranslationKeys.DecreaseSpread_Desc.Translate(),
                icon = RangeDownIcon,
                hotKey = KeyBindingDefOf.Misc8,
                action = () => ConfiguredLevel--,
            };

            if (configuredLevel <= MIN_LEVEL)
            {
                decreaseSpreadAction.Disable(TranslationKeys.SpreadMin.Translate(MIN_LEVEL));
            }

            yield return decreaseSpreadAction;

            // Increase Spread

            Command_Action increaseSpreadAction = new Command_Action
            {
                defaultLabel = TranslationKeys.IncreaseSpread.Translate(),
                defaultDesc = TranslationKeys.IncreaseSpread_Desc.Translate(),
                icon = RangeUpIcon,
                hotKey = KeyBindingDefOf.Misc9,
                action = () => ConfiguredLevel++,
            };

            if (configuredLevel >= MAX_LEVEL)
            {
                increaseSpreadAction.Disable(TranslationKeys.SpreadMax.Translate(MAX_LEVEL));
            }

            yield return increaseSpreadAction;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref configuredLevel, "configuredLevel", 0);
        }
    }
}