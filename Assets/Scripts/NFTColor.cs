[System.Flags]
public enum NFTColor
{
    Black = 0,
    Blue = 1,
    Green = 2,
    Red = 4,

    Cyan = Blue | Green,
    Magenta = Blue | Red,
    Yellow = Green | Red,
    White = Red | Green | Blue
}
