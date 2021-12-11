using System;
using System.Collections.Generic;

namespace GridPuzzles.Enums;

public enum CompassDirection
{
    North,
    East,
    South,
    West,
    NorthEast,
    SouthEast,
    NorthWest,
    SouthWest
}

public static class CompassDirectionHelper
{
    public static string GetArrowSymbol(this CompassDirection compassDirection)
    {
        return compassDirection switch
        {
            CompassDirection.North => "🡑",
            CompassDirection.East => "🡒",
            CompassDirection.South => "🡓",
            CompassDirection.West => "🡐",
            CompassDirection.NorthEast => "🡕",
            CompassDirection.SouthEast => "🡖",
            CompassDirection.NorthWest => "🡔",
            CompassDirection.SouthWest => "🡗",
            _ => throw new ArgumentOutOfRangeException(nameof(compassDirection), compassDirection, null)
        };
    }

    public static int GetMaxDistance(this CompassDirection compassDirection, Position start, Position minPosition, Position maxPosition)
    {
        var distance = 0;
        var current = start;
        var (xChange, yChange) = compassDirection.GetChange();

        while (true)
        {
            distance += 1;
            var currentX =current.Column + (xChange * distance);
            var currentY = current.Row + (yChange * distance);

            if (currentX < minPosition.Column || currentY < minPosition.Row || currentX > maxPosition.Column || currentY > maxPosition.Row)
            {
                return distance - 1;
            }
        }
    }

    public static IEnumerable<Position> GetAdjacentPositions(this CompassDirection compassDirection, Position start, int length)
    {
        var (xChange, yChange) = GetChange(compassDirection);

        for (var i = 1; i <= length; i++)
        {
            var newPosition = new Position(start.Column + (xChange * i), start.Row + (yChange * i));
            yield return newPosition;
        }
    }

    public static Position GetAdjacentPosition(this CompassDirection compassDirection, Position start)
    {
        var (xChange, yChange) = GetChange(compassDirection);

        var newPosition = new Position(start.Column + xChange , start.Row + yChange);
        return newPosition;
    }

    public static (int xChange, int yChange) GetChange(this CompassDirection compassDirection)
    {
        var (xChange, yChange) = compassDirection switch
        {
            CompassDirection.North => (0, -1),
            CompassDirection.East => (1, 0),
            CompassDirection.South => (0, 1),
            CompassDirection.West => (-1, 0),
            CompassDirection.NorthEast => (1, -1),
            CompassDirection.SouthEast => (1, 1),
            CompassDirection.NorthWest => (-1, -1),
            CompassDirection.SouthWest => (-1, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(compassDirection), compassDirection, null)
        };
        return (xChange, yChange);
    }
}