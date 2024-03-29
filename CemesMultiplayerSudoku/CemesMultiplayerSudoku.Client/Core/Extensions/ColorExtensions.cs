using System.Drawing;

namespace CemesMultiplayerSudoku.Client.Core.Extensions;

public static class ColorExtensions
{
    public static Color CalculateColors(this Color[] source)
    {
        if (source.Length == 0)
            throw new InvalidOperationException("Jeremy ist dumm, hier sollte mindestens eine Farbe gegeben sein...");

        var result = source[0];

        for (var i = 1; i < source.Length; i++)
            result = ((Color?)result).CalculateColors(source[i]);

        return result;
    }

    public static Color CalculateColors(this Color? source, IEnumerable<Color> multipliers)
    {
        if (!multipliers.Any())
            return source ?? new Color();

        if (source is null)
            return ((Color?)multipliers.FirstOrDefault()).CalculateColors(multipliers.Skip(1));

        var newColor = source.Value;
        foreach (var multiplier in multipliers)
            newColor = ((Color?)newColor).CalculateColors(multiplier);

        return newColor;
    }

    public static Color CalculateColors(this Color? source, Color multiplier)
    {
        var baseColor = source ?? multiplier;

        return Color.FromArgb(
            (baseColor.R + multiplier.R) / 2,
            (baseColor.G + multiplier.G) / 2,
            (baseColor.B + multiplier.B) / 2);
    }
}