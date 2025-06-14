namespace RimatomicsPunisherBuffs;

public static class Translations
{
    private static class Keys
    {
        private const string Prefix = "RimatomicsPunisherBuffs";

        public static readonly string IncreaseSpread = $"{Prefix}_IncreaseSpread";
        public static readonly string IncreaseSpread_Desc = $"{IncreaseSpread}_Desc";

        public static readonly string DecreaseSpread = $"{Prefix}_DecreaseSpread";
        public static readonly string DecreaseSpread_Desc = $"{DecreaseSpread}_Desc";

        public static readonly string SpreadMin = $"{Prefix}_SpreadMin";
        public static readonly string SpreadMax = $"{Prefix}_SpreadMax";

        public static readonly string Spread = $"{Prefix}_Spread";
        public static readonly string ExplosionRadius = $"{Prefix}_ExplosionRadius";

        public static readonly string Tiles = $"{Prefix}_Tiles";
        public static readonly string Tiles_1 = $"{Tiles}_1";
    }

    public static void IncreaseSpread(out string label, out string description)
    {
        label = Keys.IncreaseSpread.Translate();
        description = Keys.IncreaseSpread_Desc.Translate();
    }

    public static void DecreaseSpread(out string label, out string description)
    {
        label = Keys.DecreaseSpread.Translate();
        description = Keys.DecreaseSpread_Desc.Translate();
    }

    public static string SpreadMin(NamedArgument amount)
    {
        return Keys.SpreadMin.Translate(amount);
    }

    public static string SpreadMax(NamedArgument amount)
    {
        return Keys.SpreadMax.Translate(amount);
    }

    public static string Spread(NamedArgument amount)
    {
        return Keys.Spread.Translate(amount);
    }

    public static string ExplosionRadius(NamedArgument radius, NamedArgument label)
    {
        return Keys.ExplosionRadius.Translate(radius, label);
    }

    public static string Tiles(NamedArgument amount)
    {
        return Keys.Tiles.Translate(amount);
    }

    public static string Tiles_1()
    {
        return Keys.Tiles_1.Translate();
    }
}
