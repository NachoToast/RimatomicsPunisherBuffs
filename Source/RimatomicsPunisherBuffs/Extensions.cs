using Rimatomics;
using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace RimatomicsPunisherBuffs
{
    [StaticConstructorOnStartup]
    public static class Extensions
    {
        private static readonly FieldInfo HighlightActionField = typeof(Targeter).GetField("highlightAction", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo OnGuiActionField = typeof(Targeter).GetField("onGuiAction", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void SetHighlightAction(this Targeter targeter, Action<LocalTargetInfo> highlightAction)
        {
            HighlightActionField.SetValue(targeter, highlightAction);
        }

        public static void SetOnGuiAction(this Targeter targeter, Action<LocalTargetInfo> highlightAction)
        {
            OnGuiActionField.SetValue(targeter, highlightAction);
        }

        public static int GetSpreadOffset(this Building_Railgun railgun)
        {
            if (!railgun.UG.HasUpgrade(DubDef.TargetingChip))
            {
                return 0;
            }

            CompSpreadAdjustable comp = railgun.TryGetComp<CompSpreadAdjustable>();

            if (comp != null)
            {
                return comp.ConfiguredLevel;
            }

            return 0;
        }

        public static bool HasMeaningfulProjectileRadius(this Building_Railgun railgun, out ThingDef projectile, out float radius)
        {
            if (!(railgun.AttackVerb is Verb_Railgun railgunVerb))
            {
                projectile = null;
                radius = 0;
                return false;
            }

            projectile = railgunVerb.Projectile;

            if (projectile == null)
            {
                radius = 0;
                return false;
            }

            radius = projectile.projectile.explosionRadius + projectile.projectile.explosionRadiusDisplayPadding;

            return radius > 0.2f;
        }

        public static bool RequiresLineOfSight(this Verb verb)
        {
            if (verb is Verb_RimatomicsVerb rimatomicsVerb &&
                rimatomicsVerb.GetWep?.UG?.HasUpgrade(ThisDefOf.RimatomicsPunisherBuffs_DriveCylinders) == true)
            {
                return false;
            }

            return verb.verbProps.requireLineOfSight;
        }
    }
}