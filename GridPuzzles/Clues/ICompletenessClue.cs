namespace GridPuzzles.Clues;

public interface ICompletenessClue<T, TCell> : IUniquenessClue<T, TCell> where T :struct where TCell : ICell<T, TCell>, new()
{
        
}