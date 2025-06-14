namespace RimatomicsPunisherBuffs;

public static class GenDrawExt
{
    private static readonly Color ExplosionColor = new(0.8f, 0.8f, 0.4f);

    public static void DrawLabel(IntVec3 cell, int offset, string label)
    {
        Vector2 screenPos = GenMapUI.LabelDrawPosFor(new IntVec3(cell.x, cell.y, cell.z - offset));

        GUI.color = Color.white;
        Text.Font = GameFont.Small;

        Rect textRect = new(screenPos, Text.CalcSize(label));

        textRect.y -= textRect.height / 2;
        textRect.x -= textRect.width / 2;

        Widgets.Label(textRect, label);
    }

    public static void DrawFireMissionExplosiveRadius(Building_Railgun railgun, LocalTargetInfo target)
    {
        if (!railgun.HasMeaningfulProjectileRadius(out _, out float radius))
        {
            return;
        }

        // Isolating a single quadrant of corner cells from a radial generation.

        List<IntVec3> initialCellGroup = [.. GenRadial.RadialPatternInRadius(radius)];

        int topRowZ = int.MaxValue;
        int leftColumnX = int.MaxValue;

        foreach (IntVec3 cell in initialCellGroup)
        {
            if (cell.z < topRowZ)
            {
                topRowZ = cell.z;
            }

            if (cell.x < leftColumnX)
            {
                leftColumnX = cell.x;
            }
        }

        IntVec3 leftMostTopCell = initialCellGroup
            .Where(c => c.z == topRowZ)
            .OrderBy(c => c.x)
            .FirstOrFallback();

        if (leftMostTopCell == null)
        {
            return;
        }

        IntVec3 topMostLeftCell = initialCellGroup
            .Where(c => c.x == leftColumnX)
            .OrderBy(c => c.z)
            .FirstOrFallback();

        if (topMostLeftCell == null)
        {
            return;
        }

        int spread = railgun.spread;

        // Offsetting corner cells by the spread.

        IEnumerable<IntVec3> cornerCells = initialCellGroup
            .Where(c => c.x >= topMostLeftCell.x && c.x <= leftMostTopCell.x && c.z >= leftMostTopCell.z && c.z <= topMostLeftCell.z)
            .Select(c => new IntVec3(c.x - spread, c.y, c.z - spread));

        List<IntVec3> cells = [];

        IntVec3 center = target.Cell;

        // Rotating for each cardinal direction.

        foreach (IntVec3 cell in cornerCells)
        {
            cells.Add(center + cell);
            cells.Add(center + cell.RotatedBy(RotationDirection.Clockwise));
            cells.Add(center + cell.RotatedBy(RotationDirection.Opposite));
            cells.Add(center + cell.RotatedBy(RotationDirection.Counterclockwise));
        }

        // Filling in the bits between corners (forms a plus shape).

        cells.AddRange(new CellRect( // Vertical line of the plus.
            xStart: center.x + leftMostTopCell.x - spread + 1,
            xEnd: center.x - leftMostTopCell.x + spread - 1,
            zStart: center.z + leftMostTopCell.z - spread,
            zEnd: center.z - leftMostTopCell.z + spread));

        cells.AddRange(new CellRect( // Left line of the plus.
            xStart: center.x + topMostLeftCell.x - spread,
            xEnd: center.x + leftMostTopCell.x - spread,
            zStart: center.z + topMostLeftCell.z - spread + 1,
            zEnd: center.z - topMostLeftCell.z + spread - 1));

        cells.AddRange(new CellRect( // Right line of the plus.
             xStart: center.x - leftMostTopCell.x + spread,
             xEnd: center.x - topMostLeftCell.x + spread,
             zStart: center.z + topMostLeftCell.z - spread + 1,
             zEnd: center.z - topMostLeftCell.z + spread - 1));

        GenDraw.DrawFieldEdges(cells, ExplosionColor);
    }
}
