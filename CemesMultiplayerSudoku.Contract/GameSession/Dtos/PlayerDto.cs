using System.Drawing;
using System.Globalization;

namespace CemesMultiplayerSudoku.Contract.GameSession.Dtos;

public class PlayerDto
{
    public string ConnectionId { get; set; } = string.Empty;
    public long Id { get; set; }
    public string? Name { get; set; }

    private string? _color;

    public string? Color
    {
        get => _color;
        set
        {
            _color = value;
            SetRgb();
        }
    }

    public Color Rgb { get; private set; }

    private void SetRgb()
    {
        if (string.IsNullOrEmpty(_color))
            return;

        if (_color.Length == 4)
        {
            var r = _color[1].ToString();
            var g = _color[2].ToString();
            var b = _color[3].ToString();

            var rNum = int.Parse(r + r, NumberStyles.HexNumber);
            var gNum = int.Parse(g + g, NumberStyles.HexNumber);
            var bNum = int.Parse(b + b, NumberStyles.HexNumber);

            Rgb = System.Drawing.Color.FromArgb(rNum, gNum, bNum);
            return;
        }

        if (_color.Length != 7)
            return;

        {
            var r = _color[1].ToString() + _color[2];
            var g = _color[3].ToString() + _color[4];
            var b = _color[5].ToString() + _color[6];

            var rNum = int.Parse(r, NumberStyles.HexNumber);
            var gNum = int.Parse(g, NumberStyles.HexNumber);
            var bNum = int.Parse(b, NumberStyles.HexNumber);

            Rgb = System.Drawing.Color.FromArgb(rNum, gNum, bNum);
        }
    }
}