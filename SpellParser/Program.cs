using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpellParser
{
    class Program
    {
        private static readonly Regex MaterialRegex = new Regex(@"^The ?(?:spell’s)? material components? ?(?:for|of|comprises)? ?(?:this|the)? ?(?:spells?)? ?(?:is|are)? ?(.*)", RegexOptions.IgnoreCase);
        private static readonly Regex NegateRegex = new Regex(@"Neg\.?", RegexOptions.IgnoreCase);
        private static readonly Regex StaticRegex = new Regex(@"(\d+)([ \-a-z]+)", RegexOptions.IgnoreCase);
        private static readonly Regex ScalingRegex = new Regex(@"(\d+)([ \-a-z]+)\/level(?: of caster)?", RegexOptions.IgnoreCase);
        private static readonly Regex MultiplyRegex = new Regex(@"’ ?x ?", RegexOptions.IgnoreCase);

        private const string ScalingClass = "[[@{level-priest}]]";
        private const string BaseSpellRegex = @"(?<!\*){0}(?!\*)";
        
        private static readonly List<Regex> SpellRegex = new List<Regex>()
        {
            new Regex(string.Format(BaseSpellRegex, "dispel magic"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "detect evil"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "detect magic"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "summon insects"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "protection from evil"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "protection from good"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "charm person"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "potions? of invisibility"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "stinking cloud"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "burning hands"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "mystic transfer"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "cure light wounds"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "cure serious wounds"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "cure critical wounds"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "astral spell"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "bag of holding"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "rope trick"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "portable holes?"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "remove curse"), RegexOptions.IgnoreCase),
        };

        private static string Level = "7";

        public static void Main(string[] args)
        {
            var input = @"
Unwilling Wood (Enchantment/Charm)
Sphere: Plant
Range: 5 yards/level of caster
Components: V, S, M
Duration: Permanent
Casting Time: 1 round
Area of Effect: 10-yard radius
Saving Throw: Special
A caster can transform one or more living creatures within a 10-yard radius into unwilling wood, causing them to sprout roots, branches, and leaves. The victims become trees of a type native to the region and of the characters' age before the transformation. The spell works only if cast on beings occupying ground that could support a tree; recipients flying or suspended in water at the time of casting remain unaffected.
This spell can mutate a number of creatures equal in total Hit Dice (or levels) to the caster's level within the area of effect, of course. If this area holds a group of creatures with Hit Dice (or levels) totaling a number greater than the caster's experience level, the character may decide the order in which the creatures become affected.
For instance, say a 14th-level druid casts unwilling wood into a target area containing a giant with 12 Hit Dice and two 3rd-level warriors. The druid can transform either the giant or two warriors, but not all three. “Leftover” Hit Dice or levels are lost.
            Each creature affected may attempt to save vs. polymorph. The spell mutates all those failing their saving throw, along with any items they carry. A new tree has a height of 5 feet per level (or Hit Die) of the victim. The effect is permanent; a person transformed into a tree ages as a tree and dies as a tree. However, affected characters retain awareness, memories, personality, and intelligence. Only damage severe enough to kill the tree can kill an unwilling wood victim.
                Tree-characters can return to normal if a spellcaster of greater level than the original caster uses remove curse. The original caster can release a transformed entity at will.
                The material components are a bit of tree root and the priest's holy symbol.
                Table of Contents

";
            
            var page = "96";

            input = input
                .Trim()
                .Replace("Table of Contents", "")
                .Replace('\'', '’')
                .Replace("*", "&ast;")
                .Replace('·', '•');

            input = MultiplyRegex.Replace(input, "’ ✕ ");
            
            var strings = input
                .Split(new [] {"\n", "\r"}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var reverseCheck = $"{strings[0]} {strings[1]} {strings[2]}";
            var reversible = "";
            if (reverseCheck.Contains("reversible", StringComparison.OrdinalIgnoreCase))
            {
                strings[0] = strings[0].Replace("reversible", "", StringComparison.OrdinalIgnoreCase);
                strings[1] = strings[1].Replace("reversible", "", StringComparison.OrdinalIgnoreCase);
                strings[2] = strings[2].Replace("reversible", "", StringComparison.OrdinalIgnoreCase);
            }
            
            var nameSchoolRev = strings[0]
                .Replace("reversible", "", StringComparison.OrdinalIgnoreCase)
                .Split(new []{'(', ')'}, StringSplitOptions.RemoveEmptyEntries);
            strings.RemoveAt(0);
            
            var name = nameSchoolRev[0].Trim();
            SpellRegex.Add(new Regex(string.Format(BaseSpellRegex, name), RegexOptions.IgnoreCase));
            name = name.Replace("’", "\\'");
            
            var category = "";
            if (name.EndsWith("*"))
            {
                category = "Wild Magic";
                name = name.Replace("*", "").Trim();
            }
            var school = nameSchoolRev[1].Replace("(", "").Replace(")", "").Trim();
            var sphere = strings.GetAndRemove("Sphere: ");
            var range = strings.GetAndRemove("Range: ");
            if (OverwriteWithScaling(range, out var scaling))
                range = scaling;

            var components = strings.GetAndRemove("Components: ");
            var duration = strings.GetAndRemove("Duration: ");
            if (OverwriteWithScaling(duration, out scaling))
                duration = scaling;
            
            var castingTime = strings.GetAndRemove("Casting Time: ");
            if (OverwriteWithScaling(castingTime, out scaling))
                castingTime = scaling;

            var aoe = strings.GetAndRemove("Area of Effect: ");
            if (OverwriteWithScaling(aoe, out scaling))
                aoe = scaling;
            
            var save = strings.GetAndRemove("Saving Throw: ");
            var material = "";
            if (NegateRegex.IsMatch(save))
                save = "Negate";
            if (save == "1/2")
                save = "½";

            var effectStrings = strings
                .Select(s =>
                {
                    foreach (var regex in SpellRegex)
                    {
                        var match = regex.Match(s);
                        while (match.Success)
                        {
                            s = regex.Replace(s, $"*{match.Value}*", 1);
                            match = regex.Match(s);
                        }                        
                    }

                    return s;
                })
                .ToList();
            
            var materialString = effectStrings.FirstOrDefault(s => MaterialRegex.IsMatch(s));
            if (materialString != null)
            {
                effectStrings.Remove(materialString);
                var match = MaterialRegex.Match(materialString);
                material = match.Groups[1].Value.FirstCharToUpper();
            }
            
            var effect = string
                .Join("\\n&emsp;", effectStrings);
            
                
            var output = @$"
pri{Level}['{name}'] = {{
    'level': '{Level}',
    'school': '{school}{reversible}',
    'sphere': '{sphere}',
    'range': '{range}',
    'duration': '{duration}',
    'aoe': '{aoe}',
    'components': '{components}',
    'cast-time': '{castingTime}',
    'saving-throw': '{save}',
    'materials': '{material}',
    'reference': 'p. {page}',
    'book': 'The Complete Druid\'s Handbook',
    'damage': '',
    'damage-type': '',
    'healing': '',
    'effect': '{effect}'
}};
";

            var spellFile = Path.Join(SolutionDirectory(), "spell.txt");
            File.WriteAllText(spellFile, output);
            Console.WriteLine($"Done with {name}");
        }

        private static string SolutionDirectory()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory.FullName;
        }
        
        private static bool OverwriteWithScaling(string s, out string scaling)
        {
            scaling = s;
            var matchScaling = ScalingRegex.Match(s);
            if (!matchScaling.Success)
                return false;

            if (!s.Contains('+'))
                return OverwriteWithScalingBase(s, matchScaling, out scaling);
            
            var staticString = s.Split('+', StringSplitOptions.TrimEntries)[0];
            var matchStatic = StaticRegex.Match(staticString);
            if (!matchStatic.Success)
                return OverwriteWithScalingBase(s, matchScaling, out scaling);
            
            var staticAmount = matchStatic.Groups[1].Value;
            var staticUnit = matchStatic.Groups[2].Value.TrimEnd('s');
            
            var scalingAmount = matchScaling.Groups[1].Value;
            var scalingUnit = matchScaling.Groups[2].Value.TrimEnd('s');
            
            if (staticUnit != scalingUnit)
                return OverwriteWithScalingBase(s, matchScaling, out scaling);

            if (int.TryParse(scalingAmount, out var amount) && amount == 1)
            {
                scaling = $"[[{staticAmount}+{ScalingClass} ]]{scalingUnit}s";
                return true;
            }

            if (amount > 1)
            {
                scaling = $"[[{staticAmount}+{scalingAmount}*{ScalingClass} ]]{scalingUnit}s";
                return true;
            }

            return false;
        }

        private static bool OverwriteWithScalingBase(string s, Match match, out string scaling)
        {
            scaling = s;
            var scalingAmount = match.Groups[1].Value;
            var scalingUnit = match.Groups[2].Value.TrimEnd('s');
            if (int.TryParse(scalingAmount, out var amount) && amount == 1)
            {
                var plural = "s";
                scaling = s.Replace(match.Groups[0].Value, $"{ScalingClass}{scalingUnit}{plural}");
                return true;
            }

            if (amount > 1)
            {
                scaling = s.Replace(match.Groups[0].Value, $"[[{amount}*{ScalingClass} ]]{scalingUnit}s");
                return true;
            }

            return false;
        }
    }
}
