using System;
using System.Linq;
using System.Reflection;
using Reductech.Utilities.InstantConsole;

namespace PuzzleFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var methods =
            typeof(Sudoku.Finder).GetMethods(BindingFlags.Public | BindingFlags.Static)
                //.Concat(typeof(AnimatedSceneMaker).GetMethods(BindingFlags.Public | BindingFlags.Static))

                .Select(x => x.AsRunnable(null, new DocumentationCategory("Finder"))).ToList();

            ConsoleView.Run(args, methods);

        }
    }
}
