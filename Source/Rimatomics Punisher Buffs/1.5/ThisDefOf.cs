namespace RimatomicsPunisherBuffs;

[DefOf]
public static class ThisDefOf
{
    public static ThingDef RimatomicsPunisherBuffs_DriveCylinders;

    public static ThingDef RimatomicsPunisherBuffs_PropellantBoosters;

    static ThisDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThisDefOf));
    }
}
