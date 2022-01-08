﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CSharpFunctionalExtensions;
using GridPuzzles;
using GridPuzzles.Clues;
using GridPuzzles.Overlays;
using GridPuzzles.VariantBuilderArguments;

namespace Crossword;

public class BlockVariantBuilder : VariantBuilder<char>
{
    private BlockVariantBuilder()
    {
    }

    public static IVariantBuilder<char> Instance { get; } = new BlockVariantBuilder();

    /// <inheritdoc />
    public override string Name => "Fixed Blocks";

    /// <inheritdoc />
    public override Result<IReadOnlyCollection<IClueBuilder<char>>> TryGetClueBuilders1(IReadOnlyDictionary<string, string> arguments)
    {
        var blockTypeResult = BlockTypeArgument.TryGetFromDictionary(arguments);
        if (blockTypeResult.IsFailure)
            return blockTypeResult.ConvertFailure<IReadOnlyCollection<IClueBuilder<char>>>();

        return new []{ new BlockClueBuilder(blockTypeResult.Value)};
    }

    public static readonly EnumArgument<BlockType> BlockTypeArgument = new("Block Type", Maybe<BlockType>.From(BlockType.Even));

    /// <inheritdoc />
    public override IReadOnlyList<VariantBuilderArgument> Arguments => new[] {BlockTypeArgument};

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> DefaultArguments => new Dictionary<string, string>{{BlockTypeArgument.Name, BlockType.Even.ToString()}};
        
}

public enum BlockType  {  Odd, Even, NoBlocks }
public record BlockClueBuilder(BlockType Blocks) : IClueBuilder<char>
{
    /// <inheritdoc />
    public string Name => Blocks + " Blocks";

    /// <inheritdoc />
    public int Level => 1;

    /// <inheritdoc />
    public IEnumerable<IClue<char>> CreateClues(Position minPosition, Position maxPosition, IValueSource<char> valueSource,
        IReadOnlyCollection<IClue<char>> lowerLevelClues)
    {
        yield return BlocksAreBlackClue.Instance;

        var blockPositions = minPosition.GetPositionsBetween(maxPosition, true).SelectMany(x => x)
            .Where(x=> IsBlock(x, Blocks)).ToImmutableSortedSet();

        if(blockPositions.Any())
            yield return new BlockClue(blockPositions);
    }


    private static bool IsBlock(Position position, BlockType blockType)
    {
        return blockType switch
        {
            BlockType.Odd => (position.Column % 2 == 1 && position.Row % 2 == 1),
            BlockType.Even => (position.Column % 2 == 0 && position.Row % 2 == 0),
            BlockType.NoBlocks => false,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
        
    /// <inheritdoc />
    public IEnumerable<ICellOverlay> GetOverlays(Position minPosition, Position maxPosition)
    {
        yield break;
    }

    private class BlocksAreBlackClue : IDynamicOverlayClue<char>
    {
        private BlocksAreBlackClue() {}

        public static BlocksAreBlackClue Instance { get; } = new ();

        /// <inheritdoc />
        public IEnumerable<ICellOverlay> CreateCellOverlays(Grid<char> grid)
        {
            foreach (var keyValuePair in grid.AllCells)
            {
                if (keyValuePair.MustBeABlock())
                    yield return new CellColorOverlay(Color.Black, keyValuePair.Key);
            }
        }

        /// <inheritdoc />
        public string Name => "Blocks Are Black";

        /// <inheritdoc />
        public ImmutableSortedSet<Position> Positions { get; } = ImmutableSortedSet<Position>.Empty;
    }
}