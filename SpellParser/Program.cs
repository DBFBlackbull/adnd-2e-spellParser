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
        private static readonly Regex ScalingRegex = new Regex(@"(\d+|One)([ \-a-z]+)\/level(?: of caster)?", RegexOptions.IgnoreCase);
        private static readonly Regex MultiplyRegex = new Regex(@"’ ?x ?", RegexOptions.IgnoreCase);

        private static readonly Regex Level1Regex = new Regex(@"^(1st|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level2Regex = new Regex(@"^(2nd|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level3Regex = new Regex(@"^(3rd|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level4Regex = new Regex(@"^(4th|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level5Regex = new Regex(@"^(5th|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level6Regex = new Regex(@"^(6th|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level7Regex = new Regex(@"^(7th|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level8Regex = new Regex(@"^(8th|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level9Regex = new Regex(@"^(9th|First)-level Spells$", RegexOptions.IgnoreCase);

        private const string BaseSpellRegex = @"(?<!\*)({0})(?!\*)";
        
        private static readonly List<Regex> SpellRegex = new List<Regex>()
        {
            new Regex(string.Format(BaseSpellRegex, "dispel magic"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "dispel evil"), RegexOptions.IgnoreCase),
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
            new Regex(string.Format(BaseSpellRegex, "cure disease"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "astral spell"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "bag of holding"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "rope trick"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "portable holes?"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "remove curse"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "corpse link"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "skeletal hands"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "spectral voice"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "forget)( spell"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "slow)( spell"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "haste)( spell"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "wish)( spell"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "limited wish"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "spirit bind"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "spirit release"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "spirit release"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "transmute metal to wood"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "heat metal"), RegexOptions.IgnoreCase),
            new Regex(string.Format(BaseSpellRegex, "MM")),
            new Regex(string.Format(BaseSpellRegex, "MC")),
            new Regex(string.Format(BaseSpellRegex, "DMG")),
        };

        private const string Class = "wiz";
        private const string Book = "The Complete Book of Necromancers";
        private const string Page = "53";
        private static string Level = "2";
        private static string ScalingClass = "";


        public static void Main(string[] args)
        {

            if (Class == "wiz")
                ScalingClass = "[[@{level-wizard}]]";
            if (Class == "pri")
                ScalingClass = "[[@{level-priest}]]";

            var input = @"
6th-Level Spells

Transmute Bone to Steel 
(Alteration, Necromancy) Reversible
Range: 30 yards
Components: V, S, M
Duration: Permanent
Casting Time: 1 round
Area of Effect: 1 creature or object
Saving Throw: Special
A wizard casting this spell makes any object made of bone, including a skeleton, as strong as steel. The spell may be cast only upon dead, inanimate bones; after they have been transmuted, the bones may now be animated by the usual means. Despite their increased strength, the bones do not change in appearance, and they retain their original weight. Bone objects make all future saving throws as if they were hard metal (DMG, page 39). Transmuted skeletons now have AC 3 and take half the usual damage from physical attacks. However, these skeletons still take normal damage from holy water and magical attacks and are also subject to spells affecting metal (transmute metal to wood or heat metal) and the attacks of creatures that especially affect metal, such as rust monsters.
The reverse of this spell, transmute steel to bone, weakens any metal by making it as brittle as dry bone (altering all saving throws appropriately). Each non-living recipient of this spell must make an item saving throw vs. disintegration. If failed, the former metal item makes all future saves as if it were fashioned from bone. Metal armor loses its effectiveness,
becoming AC 7. Whenever a successful hit is made by or upon the item, the transmuted object must make a save vs. crushing blow to remain intact and functional. Magic items weakened by this spell remain magical, with any bonuses applied to their saving throws. Weapons affected by this spell inflict –2 hp per die of damage (and must save to avoid breakage whenever they hit a target). Physical attacks versus transmuted metal creatures inflict +2 hp per die of damage. 
The material components (for both versions of the spell) are steel filings and powdered bone.
Table of Contents
";

            var split = input.Split(new [] {"Table of Contents"}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var spells = split.Select(FormatSpell);
            var output = string.Join("", spells);

            var spellFile = Path.Join(SolutionDirectory(), "spell.txt");
            File.WriteAllText(spellFile, output);
        }

        private static string FormatSpell(string input)
        {
            input = input
                .Trim()
                .Replace('\'', '’')
                .Replace("*", "&ast;")
                .Replace('·', '•');

            input = MultiplyRegex.Replace(input, "’ ✕ ");

            var strings = input
                .Split(new [] {"\n", "\r"}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            var level = FindLevel(strings[0]);
            if (level != "")
            {
                Level = level;
                strings.RemoveAt(0);
            }

            var reverseCheck = $"{strings[0]} {strings[1]} {strings[2]}";
            var reversible = "";
            if (reverseCheck.Contains("reversible", StringComparison.OrdinalIgnoreCase))
            {
                strings[0] = strings[0].Replace("reversible", "", StringComparison.OrdinalIgnoreCase).Trim();
                strings[1] = strings[1].Replace("reversible", "", StringComparison.OrdinalIgnoreCase).Trim();
                strings[2] = strings[2].Replace("reversible", "", StringComparison.OrdinalIgnoreCase).Trim();
                reversible = " (Reversible)";
            }

            var nameSchoolRev = strings[0]
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

            string school = "";
            if (nameSchoolRev.Length > 1)
                 school = nameSchoolRev[1].Replace("(", "").Replace(")", "").Trim();
            else
            {
                var firstOrDefault = strings.FirstOrDefault(s => s.StartsWith('(') && s.EndsWith(')'));
                if (!string.IsNullOrEmpty(firstOrDefault))
                {
                    strings.Remove(firstOrDefault);
                    school = firstOrDefault.Trim('(', ')');
                }
            }
            
            var sphere = strings.GetAndRemove("^Sphere: ");
            var range = strings.GetAndRemove("^Range: ");
            if (OverwriteWithScaling(range, out var scaling))
                range = scaling;

            var components = strings.GetAndRemove("^Components: ");
            var duration = strings.GetAndRemove("^Duration: ");
            if (OverwriteWithScaling(duration, out scaling))
                duration = scaling;

            var castingTime = strings.GetAndRemove("^Casting Time: ");
            if (OverwriteWithScaling(castingTime, out scaling))
                castingTime = scaling;

            var aoe = strings.GetAndRemove("^Area of Effect: ");
            if (OverwriteWithScaling(aoe, out scaling))
                aoe = scaling;

            var save = strings.GetAndRemove("^Saving Throw: ");
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
                            s = regex.Replace(s, $"*{match.Groups[1].Value}*{match.Groups[2].Value}", 1);
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
{Class}{Level}['{name}'] = {{
    'level': '{Level}',{(category == "" ? "" : "\n    'category': '{category}',\n")}
    'school': '{school}{reversible}',{(sphere == "" ? "" : "\n    'sphere': '{sphere}',\n")}
    'range': '{range}',
    'duration': '{duration}',
    'aoe': '{aoe}',
    'components': '{components}',
    'cast-time': '{castingTime}',
    'saving-throw': '{save}',
    'materials': '{material}',
    'reference': 'p. {Page}',
    'book': '{Book}',
    'damage': '',
    'damage-type': '',
    'healing': '',
    'effect': '{effect}'
}};
            ";

            Console.WriteLine($"Done with {name}");
            return output;
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

        private static string FindLevel(string input)
        {
            if (Level1Regex.IsMatch(input))
                return "1";
            if (Level2Regex.IsMatch(input))
                return "2";
            if (Level3Regex.IsMatch(input))
                return "3";
            if (Level4Regex.IsMatch(input))
                return "4";
            if (Level5Regex.IsMatch(input))
                return "5";
            if (Level6Regex.IsMatch(input))
                return "6";
            if (Level7Regex.IsMatch(input))
                return "7";
            if (Level8Regex.IsMatch(input))
                return "8";
            if (Level9Regex.IsMatch(input))
                return "9";

            return "";
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
            if (scalingAmount.Equals("One", StringComparison.OrdinalIgnoreCase))
                scalingAmount = "1";
            
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
            if (scalingAmount.Equals("One", StringComparison.OrdinalIgnoreCase))
                scalingAmount = "1";
            
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
