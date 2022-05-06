using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpellParser
{
    class Program
    {
        private static readonly Regex MaterialRegex = new (@"The ?(?:spell’s)? material components? ?(?:for|of|comprises)? ?(?:this|the)? ?(?:spells?)? ?(?:is|are)? ?(.*)", RegexOptions.IgnoreCase);
        private static readonly Regex NegateRegex = new (@"Neg\.?", RegexOptions.IgnoreCase);
        private static readonly Regex StaticRegex = new (@"(\d+[d0-9]*)([ \-’a-z]+)", RegexOptions.IgnoreCase);
        private static readonly Regex ScalingRegex = new (@"(\d+|One)([ \-’a-z\.]+)(?:\/|per )levels?(?: of caster)?", RegexOptions.IgnoreCase);
        private static readonly Regex MultiplyRegex = new (@"’ ?x ?", RegexOptions.IgnoreCase);
        private static readonly Regex CriticalRegex = new (@"(\w+ ?(?:\(.*\))?),? ?(.*)", RegexOptions.IgnoreCase);

        private static readonly Regex Level1Regex = new (@"^(1st|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level2Regex = new (@"^(2nd|Second)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level3Regex = new (@"^(3rd|Third)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level4Regex = new (@"^(4th|Fourth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level5Regex = new (@"^(5th|Fifth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level6Regex = new (@"^(6th|Sixth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level7Regex = new (@"^(7th|Seventh)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level8Regex = new (@"^(8th|Eigth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level9Regex = new (@"^(9th|Ninth)-level Spells$", RegexOptions.IgnoreCase);

        private const string BaseSpellRegex = @"(?<!\*){0}(?!\*)";

        private static readonly List<string> SpellRegexPatterns = new List<string>()
        {
            ", (heal),",
            "(animate dead)",
            "(astral spell)",
            "(bag of holding)",
            "(burning hands)",
            "(charm person)",
            "(confusion)",
            "(contact other plane)",
            "(corpse link)",
            "(cure disease)",
            "(cause disease)",
            "(cure insanity)",
            "(cause insanity)",
            "(cure light wounds)",
            "(cause light wounds)",
            "(cure serious wounds)",
            "(cause serious wounds)",
            "(cure critical wounds)",
            "(cause critical wounds)",
            "(death spell)",
            "(death’s door)",
            "(detect evil)",
            "(detect magic)",
            "(dispel evil)",
            "(dispel magic)",
            "(drain undead)",
            "(elixir of madness)",
            "(empathic wound transfer)",
            "(fear)( spell)",
            "(feeblemind(?:edness)?)",
            "(forget)( spell)",
            "(geas)",
            "(haste)( spell)",
            "(heat metal)",
            "(limited wish)",
            "(magic font)",
            "(magic jar)",
            "(magic missile)",
            "(mirror image)",
            "(mystic transfer)",
            "(permanency)",
            "(portable holes?)",
            "(potions? of invisibility)",
            "(prismatic spray)",
            "(protection from evil 10’ radius)",
            "(protection from evil)",
            "(protection from good)",
            "(raise dead)",
            "(reflecting pool)",
            "(remove curse)",
            "(resist turning)",
            "(resurrection)",
            "(restoration)",
            "(revoke life force exchange)",
            "(rope trick)",
            "(scarab of insanity)",
            "(scarab of protection)",
            "(skeletal hands)",
            "(slow) spell",
            "(speak with dead)",
            "(spectral sense)",
            "(spectral voice)",
            "(spirit bind)",
            "(spirit release)",
            "(stinking cloud)",
            "(summon insects)",
            "(symbol of insanity)",
            "(transmute metal to wood)",
            "(undead alacrity)",
            @"(wish)[, \.]"
        };

        private static readonly List<Regex> SpellRegex = new List<Regex>()
        {
            new (string.Format(BaseSpellRegex, "(MM)")),
            new (string.Format(BaseSpellRegex, "(MC)")),
            new (string.Format(BaseSpellRegex, "(DMG)")),
            new (string.Format(BaseSpellRegex, "(PHB)")),
        };

        private const string Class = "wiz";
        private const string Book = @"Player\'s Option: Spells & Magic";
        private static string Level = "4";
        private static string ScalingClass = "";


        public static void Main(string[] args)
        {
            var regexes = SpellRegexPatterns
                .Select(s => new Regex(string.Format(BaseSpellRegex, s), RegexOptions.IgnoreCase));
            SpellRegex.AddRange(regexes);

            if (Class == "wiz")
                ScalingClass = "[[@{level-wizard}]]";
            if (Class == "pri")
                ScalingClass = "[[@{level-priest}]]";

            const string page = "147";
            
            var input = @"


Vitriolic Sphere

(Conjuration/Summoning, Elemental Water, Alchemy)

Range: 150 yards
Components: V, S, M
Duration: Special
Casting Time: 4
Area of Effect: 5-ft. radius
Saving Throw: 1/2
Subtlety: +4
Knockdown: d8
Sensory: Medium visual, large olfactory
Critical: Large (1d3 hits) acid


This spell conjures a one-foot sphere of glowing emerald acid that the caster can direct to strike any target within range. When it reaches its target, the sphere explodes and drenches the victim in potent acid. The victim suffers 1d4 points of damage per caster level (to a maximum damage of 12d4) and may attempt a saving throw vs. spell for half damage. If the victim fails his saving throw, he continues to suffer acid damage in the following rounds, sustaining two less dice of damage each round. For example, an 8th-level wizard inflicts 8d4 damage with this spell on the first round, 6d4 on the second round, 4d4 on the third round, 2d4 on the fourth round, and the spell ends in the fifth round. Each round, the subject is entitled to a saving throw—the spell ends when he succeeds, or when the acid damage runs its course. The acid can also be neutralized with soda, ash, lye, charcoal, or removed with a large quantity of water.

The vitriolic sphere also splashes acid in a 5-foot radius around the primary target. Any creatures within the splash radius must save vs. paralyzation or suffer a splash hit that inflicts 1d4 points of damage per every five caster levels. Splash hits do not cause continuing damage. The material component for this spell is a drop of giant slug bile.

Table of Contents
";

            var split = input.Split(new [] {"Table of Contents"}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var spells = split.Select(s => FormatSpell(s, page));
            var output = string.Join("\n", spells);

            var spellFile = Path.Join(SolutionDirectory(), "spell.txt");
            File.WriteAllText(spellFile, output);
        }

        private static string FormatSpell(string input, string page)
        {
            input = input
                .Trim()
                .Replace('\'', '’')
                .Replace("*", "&ast;")
                .Replace('·', '•');

            input = MultiplyRegex.Replace(input, "’✕");

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
            SpellRegex.Insert(0, new Regex(string.Format(BaseSpellRegex, $"({name})"), RegexOptions.IgnoreCase));
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
            
            var sphere = strings.GetAndRemove("^Sphere:");
            var range = strings.GetAndRemove("^Range:");
            if (OverwriteWithScaling(range, out var scaling))
                range = scaling;

            var components = strings.GetAndRemove("^Components:");
            var duration = strings.GetAndRemove("^Duration:");
            if (OverwriteWithScaling(duration, out scaling))
                duration = scaling;

            var castingTime = strings.GetAndRemove("^Casting Time:");
            if (OverwriteWithScaling(castingTime, out scaling))
                castingTime = scaling;

            var aoe = strings.GetAndRemove("^Area of Effect:");
            if (OverwriteWithScaling(aoe, out scaling))
                aoe = scaling;

            var save = strings.GetAndRemove("^Saving Throw:");
            var material = "";
            if (NegateRegex.IsMatch(save))
                save = "Negate";
            if (save == "1/2")
                save = "½";

            var subtlety = strings.GetAndRemove("^Subtlety:");
            var knockdown = strings.GetAndRemove("^Knockdown:");
            var sensory = strings.GetAndRemove("^Sensory:");
            var critical = strings.GetAndRemove("^Critical:");
            var critSize = "";
            var critType = "";
            if (!string.IsNullOrEmpty(critical))
            {
                var match = CriticalRegex.Match(critical);
                critSize = match.Groups[1].Value.Trim();
                if (match.Groups.Count == 3)
                    critType = match.Groups[2].Value.Trim().FirstCharToUpper();
            }
                
            var effectStrings = strings
                .Select(s =>
                {
                    
                    var materialMatch = MaterialRegex.Match(s);
                    if (materialMatch.Success)
                    {
                        s = s.Replace(materialMatch.Groups[0].Value, "")
                            .Trim();
                        material = materialMatch.Groups[1].Value.FirstCharToUpper();
                    }

                    foreach (var spellRegex in SpellRegex)
                    {
                        var spellMatch = spellRegex.Match(s);
                        while (spellMatch.Success)
                        {
                            var replace = spellMatch.Value.Replace(spellMatch.Groups[1].Value, $"*{spellMatch.Groups[1].Value}*");
                            s = spellRegex.Replace(s, replace, 1);
                            spellMatch = spellRegex.Match(s);
                        }
                    }

                    return s;
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var effect = string
                .Join("\\n&emsp;", effectStrings);

            var list = new List<string>();
            list.Add($"{Class}{Level}['{name}'] = {{");
            list.Add($"'level': '{Level}',");
            if (!string.IsNullOrWhiteSpace(category))
                list.Add($"'category': '{category}',");
            list.Add($"'school': '{school}{reversible}',");
            if (!string.IsNullOrWhiteSpace(sphere))
                list.Add($"'sphere': '{sphere}',");
            list.Add($"'range': '{range}',");
            list.Add($"'duration': '{duration}',");
            list.Add($"'aoe': '{aoe}',");
            list.Add($"'components': '{components}',");
            list.Add($"'cast-time': '{castingTime}',");
            list.Add($"'saving-throw': '{save}',");
            if (!string.IsNullOrWhiteSpace(subtlety))
                list.Add($"'subtlety': '{subtlety}',");
            if (!string.IsNullOrWhiteSpace(sensory))
                list.Add($"'sensory': '{sensory}',");
            if (!string.IsNullOrWhiteSpace(knockdown))
                list.Add($"'knockdown': '{knockdown}',");
            if (!string.IsNullOrWhiteSpace(critSize))
                list.Add($"'crit-size': '{critSize}',");
            list.Add($"'materials': '{material}',");
            list.Add($"'reference': 'p. {page}',");
            list.Add($"'book': '{Book}',");
            list.Add($"'damage': '',");
            list.Add($"'damage-type': '{critType}',");
            list.Add($"'healing': '',");
            list.Add($"'effect': '{effect}'\n}};");
            var output = string.Join("\n    ", list);

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
            var staticUnit = ParseUnit(matchStatic.Groups[2].Value);
            
            var scalingAmount = matchScaling.Groups[1].Value;
            if (scalingAmount.Equals("One", StringComparison.OrdinalIgnoreCase))
                scalingAmount = "1";
            
            var scalingUnit = ParseUnit(matchScaling.Groups[2].Value);
            
            if (staticUnit != scalingUnit)
                return OverwriteWithScalingBase(s, matchScaling, out scaling);

            var plural = scalingUnit.EndsWith("feet", StringComparison.OrdinalIgnoreCase)
                ? ""
                : "s";
            
            if (int.TryParse(scalingAmount, out var amount) && amount == 1)
            {
                scaling = $"[[{staticAmount}+{ScalingClass} ]] {scalingUnit}{plural}";
                return true;
            }

            if (amount > 1)
            {
                scaling = $"[[{staticAmount}+{scalingAmount}*{ScalingClass} ]] {scalingUnit}{plural}";
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
            
            var scalingUnit = ParseUnit(match.Groups[2].Value);
            int.TryParse(scalingAmount, out var amount);
            var isFeet = scalingUnit.EndsWith("feet", StringComparison.OrdinalIgnoreCase);
            string plural; 
            if (isFeet)
                plural = "";
            else if (Level == "1" && amount == 1)
                plural = "(s)";
            else
                plural = "s";   
            

            if (amount == 1)
            {
                scaling = s.Replace(match.Groups[0].Value, $"{ScalingClass} {scalingUnit}{plural}");
                return true;
            }

            if (amount > 1)
            {
                scaling = s.Replace(match.Groups[0].Value, $"[[{amount}*{ScalingClass} ]] {scalingUnit}{plural}");
                return true;
            }

            return false;
        }

        private static string ParseUnit(string unit)
        {
            var trim = unit.Trim().TrimEnd('.').TrimEnd('s').Trim();
            trim = trim == "rd" ? "round" : trim;
            trim = trim == "yd" ? "yard" : trim;
            trim = trim == "hr" ? "hour" : trim;
            var preFeet = new Regex(@"(ft\.?)(?: \w+)").Match(trim);
            var postFeet = new Regex(@"(?:\w+ )?(ft\.?)").Match(trim);
            if (preFeet.Success)
                trim = trim.Replace(preFeet.Groups[1].Value, "foot");
            if (postFeet.Success)
                trim = trim.Replace(postFeet.Groups[1].Value, "feet");
            
            return trim;
        }
    }
}
