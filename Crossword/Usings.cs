﻿global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Linq;
global using GridPuzzles;
global using GridPuzzles.Cells;
global using GridPuzzles.Clues;
global using CSharpFunctionalExtensions;
global using Grid = GridPuzzles.Grid<char, GridPuzzles.Cells.CharCell>;
global using IRuleClue = GridPuzzles.Clues.IRuleClue<char, GridPuzzles.Cells.CharCell>;
global using VariantBuilder = GridPuzzles.Clues.VariantBuilder<char, GridPuzzles.Cells.CharCell>;
global using VariantBuilderArgumentPair = GridPuzzles.VariantBuilderArguments.VariantBuilderArgumentPair<char, GridPuzzles.Cells.CharCell>;
global using NoArgumentVariantBuilder = GridPuzzles.Clues.NoArgumentVariantBuilder<char, GridPuzzles.Cells.CharCell>;
global using IClueBuilder = GridPuzzles.Clues.IClueBuilder<char, GridPuzzles.Cells.CharCell>;
global using IBifurcationOption = GridPuzzles.Bifurcation.IBifurcationOption<char, GridPuzzles.Cells.CharCell>;
global using IBifurcationChoice = GridPuzzles.Bifurcation.IBifurcationChoice<char, GridPuzzles.Cells.CharCell>;
global using ClueSource = GridPuzzles.Clues.ClueSource<char, GridPuzzles.Cells.CharCell>;
global using UpdateResult = GridPuzzles.UpdateResult<char, GridPuzzles.Cells.CharCell>;
global using UpdateResultCombiner = GridPuzzles.UpdateResultCombiner<char, GridPuzzles.Cells.CharCell>;