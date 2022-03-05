using Words;

namespace Crossword;

public static class CrosswordVariant
{
    private static readonly DictionaryHelper DictionaryHelper = new ();

    public static readonly IReadOnlyList<IVariantBuilder<char, CharCell>> CrosswordVariantBuilders =
        new List<IVariantBuilder<char, CharCell>>
        {
            WordListClueBuilder.Instance,
            RowStartBoxClueBuilder.Instance,
            ParallelWordClueBuilder.Instance,
            BlockVariantBuilder.Instance,
            CrosswordSymmetryVariantBuilder.Instance,

            WordListFileVariantBuilder.Instance,
            RelatedWordsVariantBuilder.Instance,
            WordStringVariantBuilder.Instance,
            NoDuplicateVariantBuilder.Instance,


            new ConjugatedWordsClueBuilder(DictionaryHelper),
            new DictionaryWordsVariantBuilder(DictionaryHelper)
        };

    public static readonly IReadOnlyDictionary<string, IVariantBuilder<char, CharCell>> CrosswordVariantBuildersDictionary =
        CrosswordVariantBuilders.ToDictionary(x => x.Name);

}