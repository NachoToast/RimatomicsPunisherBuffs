using Rimatomics;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimatomicsPunisherBuffs
{
    [StaticConstructorOnStartup]
    public class CompSpreadAdjustable : ThingComp
    {
        private const int MIN_OFFSET = 0;
        private const int MAX_OFFSET = 27;

        private static readonly Texture2D RangeUpIcon = ContentFinder<Texture2D>.Get("Rimatomics/UI/rangeUp");
        private static readonly Texture2D RangeDownIcon = ContentFinder<Texture2D>.Get("Rimatomics/UI/rangeDown");

        public int offset = 0;

        private void SetOffset(int newValue)
        {
            if (offset == newValue)
            {
                return;
            }

            offset = newValue;

            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        private bool ShouldShowSpread(out Building_Railgun railgun)
        {
            if (parent?.Faction != Faction.OfPlayer)
            {
                railgun = null;
                return false;
            }

            railgun = parent as Building_Railgun;
            return railgun != null;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder label = new StringBuilder();

            string baseLabel = base.CompInspectStringExtra();

            if (!string.IsNullOrEmpty(baseLabel))
            {
                label.AppendLine(baseLabel);
            }

            if (ShouldShowSpread(out Building_Railgun railgun))
            {
                label.Append(Translations.Spread(railgun.spread.ToTileString()));
            }

            return label.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (!ShouldShowSpread(out Building_Railgun railgun))
            {
                yield break;
            }

            if (railgun.UG?.HasUpgrade(DubDef.TargetingChip) != true)
            {
                yield break;
            }

            // Decrease Spread
            {
                Translations.DecreaseSpread(out string label, out string description);

                Command_Action decreaseSpreadAction = new Command_Action
                {
                    defaultLabel = label,
                    defaultDesc = description,
                    icon = RangeDownIcon,
                    hotKey = KeyBindingDefOf.Misc8,
                };

                if (offset <= MIN_OFFSET)
                {
                    decreaseSpreadAction.Disable(Translations.SpreadMin(railgun.spread.ToTileString().ToLower()));
                }
                else
                {
                    decreaseSpreadAction.action = () => SetOffset(offset - 1);
                }

                yield return decreaseSpreadAction;
            }

            // Increase Spread

            {
                Translations.IncreaseSpread(out string label, out string description);

                Command_Action increaseSpreadAction = new Command_Action
                {
                    defaultLabel = label,
                    defaultDesc = description,
                    icon = RangeUpIcon,
                    hotKey = KeyBindingDefOf.Misc9,
                };

                if (offset >= MAX_OFFSET)
                {
                    increaseSpreadAction.Disable(Translations.SpreadMax(railgun.spread.ToTileString().ToLower()));
                }
                else
                {
                    increaseSpreadAction.action = () => SetOffset(offset + 1);
                }

                yield return increaseSpreadAction;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref offset, "configuredLevel", 0);
        }
    }
}