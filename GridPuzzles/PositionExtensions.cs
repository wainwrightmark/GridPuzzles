using CSharpFunctionalExtensions;
using GridPuzzles.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GridPuzzles;

public static class PositionExtensions
{
    public static double GetX(this Position position, bool centre, double scale) =>
        position.Column * scale + (centre ? scale / 2 : 0);

    public static double GetY(this Position position, bool centre, double scale) =>
        position.Row * scale + (centre ? scale / 2 : 0);

    public static bool FormSquare(this (Position p1, Position p2, Position p3, Position p4) positions)
    {
        var (p1, p2, p3, p4) = positions;

        var columns = new[] { p1.Column, p2.Column, p3.Column, p4.Column }.ToHashSet();
        var rows = new[] { p1.Row, p2.Row, p3.Row, p4.Row }.ToHashSet();

        return columns.Count == 2 && rows.Count == 2 && columns.Min() + 1 == columns.Max() &&
               rows.Min() + 1 == rows.Max();
    }


    public static Maybe<ImmutableStack<Position>> BuildContiguousPath(this IImmutableList<Position> positions,
        bool includeDiagonal)
    {
        //TODO improve this
        foreach (var startPosition in positions)
        {
            var p = BuildContiguousPath(positions.Remove(startPosition), startPosition, includeDiagonal);
            if (p.HasValue)
                return p.Value;
        }

        return Maybe<ImmutableStack<Position>>.None;
    }


    public static Maybe<ImmutableStack<Position>> BuildContiguousPath(this IImmutableList<Position> positions,
        Position startPosition,
        bool includeDiagonal)
    {
        if (!positions.Any())
            return ImmutableStack<Position>.Empty.Push(startPosition);


        foreach (var position in positions)
        {
            var good = includeDiagonal ? startPosition.IsAdjacent(position) : startPosition.IsOrthogonal(position);

            if (good)
            {
                var nextPath = BuildContiguousPath(positions.Remove(position), position, includeDiagonal);
                if (nextPath.HasValue)
                    return nextPath.Value.Push(startPosition);
            }
        }

        return Maybe<ImmutableStack<Position>>.None;
    }

    public static Maybe<ImmutableStack<Position>> BuildStraightLine(this ImmutableSortedSet<Position> positions,
        Position start, CompassDirection compassDirection)
    {
        if (!positions.Any())
            return ImmutableStack<Position>.Empty.Push(start);
        var next = start.Move(compassDirection);
        if (!positions.Contains(next))
            return Maybe<ImmutableStack<Position>>.None;

        var nextStack = BuildStraightLine(positions.Remove(next), next, compassDirection);
        if (nextStack.HasNoValue) return nextStack;
        return nextStack.Value.Push(start);
    }

    public static Position Move(this Position position, CompassDirection compassDirection, int distance = 1)
    {
        return compassDirection switch
        {
            CompassDirection.North => new Position(position.Column, position.Row - distance),
            CompassDirection.East => new Position(position.Column + distance, position.Row),
            CompassDirection.South => new Position(position.Column, position.Row + distance),
            CompassDirection.West => new Position(position.Column - distance, position.Row),
            CompassDirection.NorthEast => new Position(position.Column + distance, position.Row - distance),
            CompassDirection.SouthEast => new Position(position.Column + distance, position.Row + distance),
            CompassDirection.NorthWest => new Position(position.Column - distance, position.Row - distance),
            CompassDirection.SouthWest => new Position(position.Column - distance, position.Row + distance),
            _ => throw new ArgumentOutOfRangeException(nameof(compassDirection), compassDirection, null)
        };
    }

    public static bool AreContiguous(this IReadOnlyCollection<Position> positions,
        Position minPosition, Position maxPosition,
        bool allowDiagonal)
    {
        if (positions.Count < 1)
            return false;
        if (positions.Count == 1)
            return true;
        if (positions.Count == 2)
        {
            var (p1, p2) = positions.GetFirstTwo();
            return allowDiagonal ? p1.IsAdjacent(p2) : p1.IsOrthogonal(p2);
        }

        var remainingPositions = positions.Skip(1).ToHashSet();

        var okayPositions = new HashSet<Position>();

        void Add(Position p)
        {
            okayPositions.Add(p);
            okayPositions.UnionWith(GetAdjacentPositions(p, minPosition, maxPosition, allowDiagonal));
        }

        Add(positions.First());

        while (remainingPositions.Any())
        {
            var newRemainingPositions = new HashSet<Position>();
            var changed = false;
            foreach (var position in remainingPositions)
            {
                if (okayPositions.Contains(position))
                {
                    Add(position);
                    changed = true;
                }
                else
                {
                    newRemainingPositions.Add(position);
                }
            }

            if (!changed)
                return false;
            remainingPositions = newRemainingPositions;
        }

        return true;
    }


    public static bool IsHorizontallyOrthogonalTo(this Position p1, Position p2) =>
        p1.Row == p2.Row && Math.Abs(p1.Column - p2.Column) == 1;

    public static bool IsVerticallyOrthogonalTo(this Position p1, Position p2) =>
        p1.Column == p2.Column && Math.Abs(p1.Row - p2.Row) == 1;

    public static bool IsOrthogonal(this Position p1, Position p2) =>
        IsHorizontallyOrthogonalTo(p1, p2) || IsVerticallyOrthogonalTo(p1, p2);

    /// <summary>
    /// Returns true if two positions are adjacent (including diagonal)
    /// Returns false if they are the same
    /// </summary>
    public static bool IsAdjacent(this Position p1, Position p2) =>
        p1 != p2 && Math.Abs(p1.Column - p2.Column) <= 1 && Math.Abs(p1.Row - p2.Row) <= 1;

    public static Maybe<CompassDirection> GetAdjacentDirection(this Position p1, Position p2)
    {
        var horizontal = p1.Column - p2.Column;
        var vertical = p1.Row - p2.Row;

        return (horizontal, vertical) switch
        {
            (-1, -1) => CompassDirection.NorthWest,
            (-1, 0) => CompassDirection.West,
            (-1, 1) => CompassDirection.SouthWest,
            (0, -1) => CompassDirection.North,
            //
            (0, 1) => CompassDirection.South,
            (1, -1) => CompassDirection.NorthEast,
            (1, 0) => CompassDirection.East,
            (1, 1) => CompassDirection.SouthEast,
            _ => Maybe<CompassDirection>.None
        };
    }

    public static CompassDirection Opposite(this CompassDirection compassDirection)
    {
        return compassDirection switch
        {
            CompassDirection.North => CompassDirection.South,
            CompassDirection.East => CompassDirection.West,
            CompassDirection.South => CompassDirection.North,
            CompassDirection.West => CompassDirection.East,
            CompassDirection.NorthEast => CompassDirection.SouthWest,
            CompassDirection.SouthEast => CompassDirection.NorthWest,
            CompassDirection.NorthWest => CompassDirection.SouthEast,
            CompassDirection.SouthWest => CompassDirection.NorthEast,
            _ => throw new ArgumentOutOfRangeException(nameof(compassDirection), compassDirection, null)
        };
    }

    public static int GetRotation(this CompassDirection compassDirection)
    {
        return compassDirection switch
        {
            CompassDirection.North => 0,
            CompassDirection.East => 90,
            CompassDirection.South => 180,
            CompassDirection.West => 270,
            CompassDirection.NorthEast => 45,
            CompassDirection.SouthEast => 135,
            CompassDirection.NorthWest => 315,
            CompassDirection.SouthWest => 225,
            _ => throw new ArgumentOutOfRangeException(nameof(compassDirection), compassDirection, null)
        };
    }


    public static IEnumerable<Position> GetDiagonalPositions(this Position left, Position right)
    {
        if (left.Column > right.Column)
            throw new Exception("Left must be to the left of right");


        if (left.Column + left.Row == right.Column + right.Row)
        {
            //this is going up from the bottom left
            for (int column = left.Column; column <= right.Column; column++)
            {
                var row = left.Row + left.Column - column;
                yield return new Position(column, row);
            }
        }
        else if (left.Column - left.Row == right.Column - right.Row)
        {
            //this is going down from the top left
            for (int column = left.Column; column <= right.Column; column++)
            {
                var row = left.Row - left.Column + column;
                yield return new Position(column, row);
            }
        }
        else
        {
            throw new Exception("The two points must be on a diagonal");
        }
    }


    public static IEnumerable<Position> GetAdjacentPositions(this Position p, Position minPosition,
        Position maxPosition, bool includeDiagonal = false)
    {
        if (p.Column > minPosition.Column)
            yield return new Position(p.Column - 1, p.Row);
        if (p.Column < maxPosition.Column)
            yield return new Position(p.Column + 1, p.Row);
        if (p.Row > minPosition.Row)
            yield return new Position(p.Column, p.Row - 1);
        if (p.Row < maxPosition.Row)
            yield return new Position(p.Column, p.Row + 1);

        if (includeDiagonal)
        {
            if (p.Column > minPosition.Column)
            {
                if (p.Row > minPosition.Row)
                    yield return new Position(p.Column - 1, p.Row - 1);
                if (p.Row < maxPosition.Row)
                    yield return new Position(p.Column - 1, p.Row + 1);
            }

            if (p.Column < maxPosition.Column)
            {
                if (p.Row > minPosition.Row)
                    yield return new Position(p.Column + 1, p.Row - 1);
                if (p.Row < maxPosition.Row)
                    yield return new Position(p.Column + 1, p.Row + 1);
            }
        }
    }

    public static ushort GetIndex(this Position p, Parallel d)
    {
        return d switch
        {
            Parallel.Row => p.Row,
            Parallel.Column => p.Column,
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };
    }

    public static IEnumerable<IEnumerable<Position>> GetPositionsBetween(this Position topLeft,
        Position bottomRight, bool acrossThenDown)
    {
        var columns = Enumerable.Range(topLeft.Column, bottomRight.Column - topLeft.Column + 1);
        var rows = Enumerable.Range(topLeft.Row, bottomRight.Row - topLeft.Row + 1);

        if (acrossThenDown)
            return rows.Select(r => columns.Select(c => new Position(c, r)));

        return columns.Select(c => rows.Select(r => new Position(c, r)));
    }


    public static IEnumerable<IEnumerable<Position>> GetPositionsUpTo(this Position p, bool acrossThenDown) =>
        GetPositionsBetween(Position.Origin, p, acrossThenDown);

    public static int GetTotalPositionsUpTo(this Position p) => p.Column * p.Row;

    public static Position GetOpposite(this Position p, int length) =>
        new(length - p.Column + 1, length - p.Row + 1);

    public static ushort GetOtherIndex(this Position p, Parallel d)
    {
        return d switch
        {
            Parallel.Row => p.Column,
            Parallel.Column => p.Row,
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };
    }

    /// <summary>
    /// Is this position within the min and max inclusive
    /// </summary>
    public static bool IsWithin(this Position position, Position minPosition, Position maxPosition)
    {
        if (position.Row < minPosition.Row || position.Column < minPosition.Column)
            return false;
        if (position.Row > maxPosition.Row || position.Column > maxPosition.Column)
            return false;
        return true;
    }

    public static Position GetAdjacent(this Position position, CompassDirection direction)
    {
        return direction switch
        {
            CompassDirection.North => new Position(position.Column, position.Row - 1),
            CompassDirection.East => new Position(position.Column + 1, position.Row),
            CompassDirection.South => new Position(position.Column, position.Row + 1),
            CompassDirection.West => new Position(position.Column - 1, position.Row),
            CompassDirection.NorthEast => new Position(position.Column + 1, position.Row - 1),
            CompassDirection.SouthEast => new Position(position.Column + 1, position.Row + 1),
            CompassDirection.NorthWest => new Position(position.Column - 1, position.Row - 1),
            CompassDirection.SouthWest => new Position(position.Column - 1, position.Row + 1),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public static Position Transform(this Position position, int quarterTurns, bool flipHorizontal,
        bool flipVertical, Position maxPosition)
    {
        var newPosition = position;
        for (var i = 0; i < quarterTurns % 4; i++)
        {
            newPosition = new Position(maxPosition.Row + 1 - newPosition.Row, newPosition.Column);
        }

        if (flipHorizontal)
            newPosition = new Position(maxPosition.Column + 1 - newPosition.Column, newPosition.Row);
        if (flipVertical) newPosition = new Position(newPosition.Column, maxPosition.Row + 1 - newPosition.Row);

        return newPosition;
    }
}