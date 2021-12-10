using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpellParser
{
    class Program
    {
        private static readonly Regex MaterialRegex = new Regex("The material component(?:s)?(?: for this spell(?:s)?)? (?:is|are) (.*)", RegexOptions.IgnoreCase);
        private static readonly Regex NegateRegex = new Regex("Neg\\.?", RegexOptions.IgnoreCase);
        
        public static void Main(string[] args)
        {
            var input = @"
Shadow Form (Necromancy)
Range: 0
Components: V, S, M
Duration: 1 round/level
Casting Time: 1 round
Area of Effect: The caster
Saving Throw: None
By means of this spell, the caster temporarily changes himself into a shadow. The caster gains the movement rate, Armor Class, hit dice, and all abilities of a shadow. His chilling touch (requiring a normal attack roll) inflicts 2-5 (1d4+1) hit points of damage on his victims as well as draining one point of Strength. Lost Strength returns in 2-8 (2d4) turns after being touched. If a human or demihuman victim is reduced to 0 hit points or 0 Strength by the caster in shadow form, the victim has lost all of his life force and is immediately drawn into the Negative Material Plane where he will forever after exist as a shadow.
All of the caster's weapons and equipment stay with him, but he is unable to use them while in shadow form. He is also unable to cast spells while in shadow form, but he is immune to sleep, charm, and hold spells, and is unaffected by cold-based attacks. He is 90 percent undetectable in all but the brightest of surroundings. Unlike normal shadows, a wizard in shadow form cannot be turned by priests. At the end of the spell's duration, there is a 5% chance that the caster will permanently remain as a shadow. Nothing short of a wish can return the caster to his normal form.
The material components for this spell are the shroud from a corpse at least 100 years old and a black glass marble.

";

            var level = "8";
            var page = "106";
            
            input = input.Trim();
            var strings = input
                .Split(new [] {"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Select(s => s.Replace('\'', '’'))
                .ToArray();
            
            var nameAndSchool = strings[0].Split(" (", StringSplitOptions.RemoveEmptyEntries);
            var name = nameAndSchool[0].Replace("’", "\\'");
            var school = nameAndSchool[1].Replace(")", "", StringComparison.OrdinalIgnoreCase);
            var range = strings[1].Replace("Range: ", "", StringComparison.OrdinalIgnoreCase);
            var components = strings[2].Replace("Components: ", "", StringComparison.OrdinalIgnoreCase);
            var duration = strings[3].Replace("Duration: ", "", StringComparison.OrdinalIgnoreCase);
            if (duration == "1 turn/level")
                duration = "[[@{level-wizard}]] turns";
            if (duration == "1 round/level")
                duration = "[[@{level-wizard}]] rounds";
            
            var castingTime = strings[4].Replace("Casting Time: ", "", StringComparison.OrdinalIgnoreCase);
            var aoe = strings[5].Replace("Area of Effect: ", "", StringComparison.OrdinalIgnoreCase);
            var save = strings[6].Replace("Saving Throw: ", "", StringComparison.OrdinalIgnoreCase);
            var material = "";
            if (NegateRegex.IsMatch(save))
                save = "Negate";
            if (save == "1/2")
                save = "½";

            var regex = new Regex($@"(?<!\*){name}(?!\*)", RegexOptions.IgnoreCase);
            var effectStrings = strings
                .Skip(7)
                .Select(s =>
                {
                    var match = regex.Match(s);
                    while (match.Success)
                    {
                        s = regex.Replace(s, $"*{match.Value}*", 1);
                        match = regex.Match(s);
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
wiz{level}['{name}'] = {{
    'level': '{level}',
    'school': '{school}',
    'range': '{range}',
    'duration': '{duration}',
    'aoe': '{aoe}',
    'components': '{components}',
    'cast-time': '{castingTime}',
    'saving-throw': '{save}',
    'materials': '{material}',
    'reference': 'p. {page}',
    'book': 'The Complete Wizard\'s Handbook',
    'damage': '',
    'damage-type': '',
    'healing': '',
    'effect': '{effect}'
}}
";
            File.WriteAllText(@"D:\git\SpellParser\spell.js", output);
            Console.WriteLine($"Done with {name}");
        }
    }
}
