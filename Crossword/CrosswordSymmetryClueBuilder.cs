using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class CrosswordSymmetryVariantBuilder : VariantBuilder<char>
{
    private CrosswordSymmetryVariantBuilder()
    {
    }

    public static IVariantBuilder<char> Instance { get; } = new CrosswordSymmetryVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Symmetry";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<char>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var blockTypeResult = RotationTypeArgument.TryGetFromDictionary(arguments);
        if (blockTypeResult.IsFailure)
            return blockTypeResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<char>>>();

        return blockTypeResult.Value switch
        {
            RotationType.Rotation2 => new[] {CrosswordSymmetryClueBuilder.Rotation2},
            RotationType.Rotation4 => new[] {CrosswordSymmetryClueBuilder.Rotation4},
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static readonly EnumArgument<RotationType> RotationTypeArgument = new("Rotation Type", Maybe<RotationType>.From(RotationType.Rotation2));

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new[] {RotationTypeArgument};

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> DefaultArguments => new Dictionary<string, string>{{RotationTypeArgument.Name, RotationType.Rotation2.ToString()}};
        
}


public enum RotationType
{
    Rotation2,
    Rotation4
}

public record CrosswordSymmetryClueBuilder(RotationType RotationalType) : IClueBuilder<char>
{
    public static CrosswordSymmetryClueBuilder Rotation2 = new(RotationType.Rotation2);
    public static CrosswordSymmetryClueBuilder Rotation4 = new(RotationType.Rotation4);
    

    /// <inheritdoc />
    public string Name => $"Box Symmetry - {RotationalType}";

    /// <inheritdoc />
    public int Level => 2;

    /// <inheritdoc />
    public IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        var blocks = lowerLevelClues.OfType<BlockClue>().SelectMany(x => x.Positions).ToImmutableHashSet();


        var clues =
            minPosition.GetPositionsBetween(maxPosition,true).SelectMany(x => x)
                .Where(x=> !blocks.Contains(x))
                .Select(p => TryMake(p, minPosition, maxPosition, RotationalType))
                .Distinct().Where(x=>x!=null);

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return clues;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    private static IClue<char>? TryMake(Position p, Position minPosition, Position maxPosition, RotationType rotationType)
    {
        return rotationType switch
        {
            RotationType.Rotation2 => CrosswordSymmetryClueRotational2.TryMake(p, minPosition,
                maxPosition),
            RotationType.Rotation4 => CrosswordSymmetryClueRotational4.TryMake(p, minPosition, maxPosition),
            _ => throw new ArgumentOutOfRangeException(nameof(rotationType), rotationType, null)
        };
    }

    /// <param name="minPosition"></param>
    /// <param name="maxPosition"></param>
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        yield break;
    }
}