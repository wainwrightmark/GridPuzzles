using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sudoku;

namespace PuzzleFinder
{
    public static class AnimatedSceneMaker
    {
        public static void MassMasFf(string resPath= @"C:\Users\wainw\source\repos\MarkPersonal\Godot\ReachTheBreach",
            string relativePath = @"Assets\Characters",
            string output = @"C:\Users\wainw\source\repos\MarkPersonal\Godot\ReachTheBreach\Scenes",
            float speed = 5.0f
            )
        {
            var superFolderPath = Path.Combine(resPath, relativePath);
            var directories = Directory.GetDirectories(superFolderPath);

            foreach (var directory in directories)
                MasFF(resPath, Path.Combine(relativePath, directory), Path.Combine(output, directory + ".tscn"), speed);
        }

        public static void MasFF(string resPath
            = @"C:\Users\wainw\source\repos\MarkPersonal\Godot\ReachTheBreach",
            string relativePath = @"Assets\Characters\Player",
            string output = @"C:\Users\wainw\source\repos\MarkPersonal\Godot\ReachTheBreach\Scenes\NewScene.tscn",

            float speed = 5.0f)
        {
            var folderPath = Path.Combine(resPath, relativePath);

            var pngsList = GetAllFiles(folderPath).Distinct().ToList();

             var goodPngs = pngsList
                .Select(x => GetDetails(x, resPath, folderPath))
                .WhereNotNull()
                .OrderBy(x=>x.animationName)
                .ThenBy(x=>x.number).ToList();

                 var animations = goodPngs.GroupBy(x => x.animationName);

            var fileStringBuilder = new StringBuilder();
            var spriteFramesStringBuilder = new StringBuilder();

            fileStringBuilder.AppendLine($"[gd_scene load_steps={goodPngs.Count + 2} format=2]");
            fileStringBuilder.AppendLine();

            spriteFramesStringBuilder.AppendLine(@"[sub_resource type=""SpriteFrames"" id=1]");


            var spriteId = 1;
            var first = true;
            foreach (var animation in animations)
            {
                if(first)
                    spriteFramesStringBuilder.AppendLine(@"animations = [ {");
                else
                    spriteFramesStringBuilder.AppendLine("}, {");
                first = false;

                var ids = new List<int>();

                foreach (var sprite in animation)
                {
                    var extResourceLine =  $@"[ext_resource path=""{sprite.relativePath}"" type=""Texture"" id={spriteId}]";
                    fileStringBuilder.AppendLine(extResourceLine);
                    ids.Add(spriteId);
                    spriteId++;
                }
                var frames = string.Join(", ",  ids.Select(id=> $@"ExtResource( {id} )"));

                spriteFramesStringBuilder.AppendLine($@"""frames"": [ {frames} ],");
                spriteFramesStringBuilder.AppendLine($@"""name"": ""{animation.Key}"",");
                spriteFramesStringBuilder.AppendLine($@"""loop"": false,");
                spriteFramesStringBuilder.AppendLine($@"""speed"": {speed}");
            }

            spriteFramesStringBuilder.AppendLine("} ]");
            fileStringBuilder.AppendLine();

            fileStringBuilder.Append(spriteFramesStringBuilder);

            fileStringBuilder.AppendLine(@"[node name=""AnimatedSprite"" type=""AnimatedSprite""]");
            fileStringBuilder.AppendLine(@"frames = SubResource( 1 )");
            fileStringBuilder.AppendLine($@"animation = ""{goodPngs.First().animationName}""");

            File.WriteAllText(output, fileStringBuilder.ToString());


        }

        private static readonly Regex AnimRegex = new Regex(@"(?<name>.+?)(?<number>\d+)\.png", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static (string relativePath, string animationName, int number)? GetDetails(string path, string resPath, string folderPath)
        {
            var relativePath =Path.Combine("res://", Path.GetRelativePath(resPath, path)).Replace('\\', '/');

            var fileName = Path.GetFileName(path);// Path.GetRelativePath(folderPath, path);

            var match = AnimRegex.Match(fileName);

            if (!match.Success) return null;

            var animationName = match.Groups["name"].Value
                .Replace('/', ' ')
                .Replace('-', ' ')
                .Replace('\\', ' ')
                .Replace('_', ' ')

                .Trim();
            var animationNumber = int.Parse(match.Groups["number"].Value);

            return (relativePath, animationName, animationNumber);
        }

        static IEnumerable<string> GetAllFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.png");
            foreach (var f in files)
                yield return f;

            foreach (var subDirectory in Directory.GetDirectories(path))
            foreach (var file in GetAllFiles(subDirectory))
                yield return file;
        }
    }
}
