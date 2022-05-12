using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace SpellParser
{
    class Program
    {
        private static readonly Regex MaterialRegex = new (@"The ?(?:spell’s)? material ?(?:spell)? components? ?(?:for|of|comprises)? ?(?:this|the)? ?(?:spells?)? ?(?:is|are)? ?(.*)", RegexOptions.IgnoreCase);
        private static readonly Regex NegateRegex = new (@"Neg\.?", RegexOptions.IgnoreCase);
        private static readonly Regex StaticRegex = new (@"(\d+[d0-9]*)([ \-’a-z]*)", RegexOptions.IgnoreCase);
        private static readonly Regex ScalingRegex = new (@"(\d+|One)([ \-’a-z\.]+)(?:\/|per )levels?(?: of caster)?", RegexOptions.IgnoreCase);
        private static readonly Regex MultiplyRegex = new (@"’ ?x ?", RegexOptions.IgnoreCase);
        private static readonly Regex KnockdownRegex = new (@"(d\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex CriticalRegex = new (@"(\w+ ?(?:\(.*\))?),? ?(.*)", RegexOptions.IgnoreCase);

        private static readonly Regex Level1Regex = new (@"^(1st|First)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level2Regex = new (@"^(2nd|Second)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level3Regex = new (@"^(3rd|Third)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level4Regex = new (@"^(4th|Fourth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level5Regex = new (@"^(5th|Fifth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level6Regex = new (@"^(6th|Sixth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level7Regex = new (@"^(7th|Seventh)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level8Regex = new (@"^(8th|Eighth)-level Spells$", RegexOptions.IgnoreCase);
        private static readonly Regex Level9Regex = new (@"^(9th|Ninth)-level Spells$", RegexOptions.IgnoreCase);

        private const string BaseSpellRegex = @"(?<!\*){0}(?!\*)";
        private const string DelimiterRegex = @"(?:,|\.| spell)";

        private static readonly List<string> SpellRegexPatterns = new List<string>()
        {
            $", (heal){DelimiterRegex}",
            "(animate dead)",
            "(astral spell)",
            "(bag of holding)",
            "(beguiling)",
            $"(blink(?:ing)?){DelimiterRegex}",
            "(burning hands)",
            $"(charm){DelimiterRegex}",
            "(charm person)",
            "(chill touch)",
            "(cloudkill)",
            "(confusion)",
            "(contagion)",
            "(contact other plane)",
            "(contingency)",
            "(corpse link)",
            "(cure disease)",
            "(cause disease)",
            "(cure insanity)",
            "(cause insanity)",
            "(cure light wounds)",
            "(cause light wounds)",
            "(cure moderate wounds)",
            "(cause moderate wounds)",
            "(cure serious wounds)",
            "(cause serious wounds)",
            "(cure critical wounds)",
            "(cause critical wounds)",
            "(cause wounds?)",
            "(death spell)",
            "(death’s door)",
            "(detect evil)",
            "(detect invisibility)",
            "(detect magic)",
            "(detect phase)",
            "(dimension door)",
            "(dispel evil)",
            "(dispel magic)",
            $"(domination){DelimiterRegex}",
            "(drain undead)",
            "(etherealness)",
            "(elixir of madness)",
            "(emotion)[, ]",
            "(empathic wound transfer)",
            "(enchant an item)",
            "(enfeeblement)",
            $"(fear){DelimiterRegex}",
            "(feeblemind(?:edness)?)",
            "(flaming sphere)",
            "(?:spell )(focus)",
            $"(forget){DelimiterRegex}",
            $"(fortify){DelimiterRegex}",
            $"(gates?){DelimiterRegex}",
            "(geas)",
            $"(haste){DelimiterRegex}",
            "(heat metal)",
            "(hold person)",
            $"(hold){DelimiterRegex}",
            "(Leomund’s secure shelter)",
            "(limited wish)",
            "(log of everburning)",
            "(magic font)",
            "(magic jar)",
            "(magic missiles?)",
            $"(maze){DelimiterRegex}",
            "(Melf’s acid arrow)",
            "(mirror image)",
            "(Mordenkainen’s disjunction)",
            "(mystic transfer)",
            "(neautralize poison)",
            "(pain touch)",
            "(permanency)",
            "(phasing)",
            "(plane shift)",
            "(portable holes?)",
            "(potions? of invisibility)",
            $"(prayer){DelimiterRegex}",
            "(prismatic spray)",
            "(protection from evil 10’ radius)",
            "(protection from evil)",
            "(protection from good)",
            "(raise dead)",
            "(ray of enfeeblement)",
            "(ray of fatigue)",
            "(reflecting pool)",
            "(remove curse)",
            "(resist turning)",
            "(resurrection)",
            "(restoration)",
            "(revoke life force exchange)",
            "(rod of smiting)",
            "(rope trick)",
            "(scarab of insanity)",
            "(scarab of protection)",
            "(shadow walk)",
            "(skeletal hands)",
            $"(sleep){DelimiterRegex}",
            $"(slow){DelimiterRegex}",
            "(slow poison)",
            "(speak with dead)",
            "(spectral sense)",
            "(spectral voice)",
            "(spirit bind)",
            "(spirit release)",
            "(stinking cloud)",
            "(summon insects)",
            "(symbol of insanity)",
            $"(teleportation){DelimiterRegex}",
            "(transmute metal to wood)",
            "(true seeing)",
            "(undead alacrity)",
            "(wall of fire)",
            $"(?:or )?(wish){DelimiterRegex}",
            "(wraithform)",
            
            @"(chain mail \+\d)",
            @"(plate mail \+\d)",
            
            @"(long sword \+\d)",
        };

        private static readonly List<Regex> SpellRegex = new List<Regex>()
        {
            new (string.Format(BaseSpellRegex, "(MM)")),
            new (string.Format(BaseSpellRegex, "(MC)")),
            new (string.Format(BaseSpellRegex, "(DMG)")),
            new (string.Format(BaseSpellRegex, "(PHB)")),
            new (string.Format(BaseSpellRegex, "(Player’s Option: Combat & Tactics)")),
            new (string.Format(BaseSpellRegex, "(Tome of Magic)")),
        };

        private const string Class = "pri";
        private const string Book = @"Player\'s Option: Spells & Magic";
        private static string Level = "7";
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

            const string page = "179";
            
            var input = @"
Tsunami
(Conjuration/Summoning)
Sphere: Elemental (Water)

Range: 200 yds. + 50 yds./level
Components: V, S, M
Duration: Special
Casting Time: 3 rds.
Area of Effect: Wave 2 ft. high and 10 ft. long per level
Saving Throw: None


Subtlety: +6
Knockdown: Special
Sensory: Gargantuan visual, huge audio
Critical: None


This mighty spell summons a tsunami, or gigantic wave, from any major body of water. The body of water must be at least 1 mile in width, so in most circumstances the tsunami can only be summoned from the sea, large lakes, or extremely big rivers. The wave is 2 feet high and 10 feet long for each level of experience of the caster, so a 15th-level priest would summon a tsunami 30 feet high and 150 feet wide. The wave can appear anywhere within the spell’s range and immediately sweeps forward in the direction specified by the caster. This may take it out of the allowed range or even back at the casting priest. The tsunami moves at a rate of 24 (240 yards per round) and lasts one round at 14th level, two rounds at 18th level, or three rounds at 22nd or higher level.
Ships caught by the tsunami must make a seaworthiness check (see Table 77 : Ship Types in the DMG) with a penalty equal to the wave’s height in feet. For example, a tsunami created by a 15th-level caster would inflict a –30% penalty to a vessel’s seaworthiness check. If the check is failed, the vessel capsizes and sinks in 1d10 rounds, with the possible loss of those aboard. Human or humanoid swimmers caught in the wave must make a saving throw vs. death magic or be drowned in the wave; any creature in the water in the wave’s path will be carried along as long as it lasts.
If the priest sent the wave towards the shore, the tsunami loses 5 feet of height for every 20 yards it travels; a 30-foot wave could wash 120 yards inland before there was nothing left of it. Creatures caught in the area sustain 1d4 points of damage for every 5 feet of height the tsunami currently possesses and are carried along until it ends. Air-breathing creatures must make saving throws vs. death magic or be drowned outright by this treatment. Wooden buildings have a chance equal to three times the wave’s current height of being destroyed by the tsunami (90% for a 30-foot wave, for example) while stone buildings have a chance equal to the wave’s height (or 30% for a 30-foot wave). Topography may influence or channel the wave’s advance, so a good-sized hill could stop a tsunami cold, although its seaward face may be denuded of creatures and vegetation by the wave.
Note that this spell in the hands of a high-level character can blanket an awesome amount of territory and literally destroy or drown anything in its path. The tsunami is so strenuous a spell that the priest is exhausted and helpless for 1d6 hours after summoning it.
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
            range = OverwriteWithScaling(range);

            var components = strings.GetAndRemove("^Components:");
            var duration = strings.GetAndRemove("^Duration:");
            duration = OverwriteWithScaling(duration);

            var castingTime = strings.GetAndRemove("^Casting Time:");
            castingTime = OverwriteWithScaling(castingTime);

            var aoe = strings.GetAndRemove("^Area of Effect:");
            aoe = OverwriteWithScaling(aoe);

            var save = strings.GetAndRemove("^Saving Throw:");
            var material = "";
            if (NegateRegex.IsMatch(save))
                save = "Negate";
            if (save == "1/2")
                save = "½";

            var subtlety = strings.GetAndRemove("^Subtlety:");
            var knockdown = strings.GetAndRemove("^Knockdown:");
            var knockdownMatch = KnockdownRegex.Match(knockdown);
            if (knockdownMatch.Success)
                knockdown = knockdown.Replace(knockdownMatch.Groups[1].Value, $"[[{knockdownMatch.Groups[1].Value}]]");

            var sensory = strings.GetAndRemove("^Sensory:");
            var critical = strings.GetAndRemove("^Critical:");
            var critSize = "";
            var critType = "";
            if (!string.IsNullOrEmpty(critical))
            {
                var match = CriticalRegex.Match(critical);
                critSize = match.Groups[1].Value
                    // .Replace("–", "d")
                    .Trim();
                if (match.Groups.Count == 3)
                    critType = match.Groups[2].Value.Trim().FirstCharToUpper();
            }
                
            var effectStrings = strings
                .Select(s =>
                {
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
                    
                    var materialMatch = MaterialRegex.Match(s);
                    if (materialMatch.Success)
                    {
                        s = s.Replace(materialMatch.Groups[0].Value, "")
                            .Trim();
                        material = materialMatch.Groups[1].Value.FirstCharToUpper();
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
        
        private static string OverwriteWithScaling(string s)
        {
            var matchScaling = ScalingRegex.Match(s);
            Match matchStatic;
            if (!matchScaling.Success)
            {
                matchStatic = StaticRegex.Match(s);
                if (matchStatic.Success)
                {
                    var staticPlural = matchStatic.Groups[2].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) 
                        ? "s"
                        : "";
                    return $"{ParseUnit(s)}{staticPlural}";
                }
            }

            if (!s.Contains('+'))
                return OverwriteWithScalingBase(s, matchScaling);
            
            var staticString = s.Split('+', StringSplitOptions.TrimEntries)[0];
            matchStatic = StaticRegex.Match(staticString);
            if (!matchStatic.Success)
                return OverwriteWithScalingBase(s, matchScaling);
            
            var staticAmount = matchStatic.Groups[1].Value;
            var staticUnit = ParseUnit(matchStatic.Groups[2].Value);
            
            var scalingAmount = matchScaling.Groups[1].Value;
            if (scalingAmount.Equals("One", StringComparison.OrdinalIgnoreCase))
                scalingAmount = "1";
            
            var scalingUnit = ParseUnit(matchScaling.Groups[2].Value);
            
            if (!string.IsNullOrWhiteSpace(staticUnit) && staticUnit != scalingUnit)
                return OverwriteWithScalingBase(s, matchScaling);

            var plural = scalingUnit.EndsWith("feet", StringComparison.OrdinalIgnoreCase)
                ? ""
                : "s";
            
            if (int.TryParse(scalingAmount, out var amount) && amount == 1)
            {
                return $"[[{staticAmount}+{ScalingClass} ]] {scalingUnit}{plural}";
            }

            if (amount > 1)
            {
                return $"[[{staticAmount}+{scalingAmount}*{ScalingClass} ]] {scalingUnit}{plural}";
            }

            return s;
        }

        private static string OverwriteWithScalingBase(string s, Match match)
        {
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
                return s.Replace(match.Groups[0].Value, $"{ScalingClass} {scalingUnit}{plural}");
            }

            if (amount > 1)
            {
                return s.Replace(match.Groups[0].Value, $"[[{amount}*{ScalingClass} ]] {scalingUnit}{plural}");
            }

            return s;
        }

        private static string ParseUnit(string unit)
        {
            var trim = unit.Trim().TrimEnd('.').TrimEnd('s').Trim();
            trim = ReplaceMatch(trim, @"(rd)$", "round");
            trim = ReplaceMatch(trim, @"(yd)$", "yard");
            trim = ReplaceMatch(trim, @"(hr)$", "hour");
            trim = ReplaceMatch(trim, @"(sq\.?)", "square");
            trim = ReplaceMatch(trim, @"(cu\.?)", "cube");
            trim = ReplaceMatch(trim, @"(ft\.?)(?: \w+)", "foot");
            trim = ReplaceMatch(trim, @"(?:\w+ )?(ft\.?)", "feet");
            trim = ReplaceMatch(trim, @"(ft\.?)", "feet");
            
            return trim;
        }
        
        private static string ReplaceMatch(string trim, string regex, string replace)
        {
            var match = new Regex(regex).Match(trim);
            return !match.Success 
                ? trim 
                : trim.Replace(match.Groups[1].Value, replace);
        }
    }
}
