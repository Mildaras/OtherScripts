using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        const int CASE_COUNT = 4; //Duomenu variantų kiekis

        for (int caseNo = 1; caseNo <= CASE_COUNT; caseNo++)
        {
            Console.WriteLine($"=== Variantas {caseNo} ===");

            string fileName = $"variantas{caseNo}.txt"; //failo pavadinimas
            string path     = Path.Combine(Directory.GetCurrentDirectory(), fileName); //pilnas kelias iki failo

            if (!File.Exists(path))
            {
                Console.WriteLine($"Failas {fileName} nerastas\n");
                continue;
            }

            var lines = File.ReadAllLines(path);
            if (lines.Length == 0)
            {
                Console.WriteLine("Tuščias failas.\n");
                continue;
            }

            int m = int.Parse(lines[0]); //Nuskaito pirmą eilutę, kuri nurodo žaidėjų kiekį
            
            Console.WriteLine("Pradiniai duomenys:");
            System.Console.WriteLine("  " + m);
            for (int i = 1; i <= m; i++) 
                Console.WriteLine($"  {lines[i]}");
            Console.WriteLine();

            if (m < 5 || m > 20)
            {
                Console.WriteLine($"Žaidėjų kiekis turi būti tarp 5 ir 20. Dabartinis Žaidėjų kiekis: {m}\n");
                continue;
            }


            var totalByCategory = new int[m];
            var seenCategories  = new List<(int category, int sum)>();

            for (int i = 0; i < m; i++)
            {
                var parts = lines[i + 1]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries); //Padalina eilutę į dalis

                int k    = int.Parse(parts[0]); //Pirmas skaičius yra kategorija
                int[] sc = parts.Skip(1)
                                .Take(5)
                                .Select(int.Parse)
                                .ToArray(); //Kiti penki skaičiai yra žaidėjo taškai

                if (k < 1 || k > 10)
                {
                    Console.WriteLine($"Kategorija {k} yra netinkama. Žaidimas nevertinamas.");
                    goto NextCase;
                }

                totalByCategory[k - 1] += sc.Sum(); //Pridedame žaidėjo taškus prie atitinkamos kategorijos
            }

            Console.WriteLine("Kategorijų taškai:");
            for (int cat = 0; cat < totalByCategory.Length; cat++)
            {
                int sum = totalByCategory[cat];
                Console.WriteLine($"  {cat + 1}: {sum}");
                if (sum > 0)
                    seenCategories.Add((cat + 1, sum)); //Naudojame tik tas kategorijas, kurių taškų kiekis > 0
            }

            MinMax(seenCategories, out var min, out var max);
            Console.WriteLine($"\nMažiausiai taškų surinko {min.category} kategorija ({min.sum} taškai).");
            Console.WriteLine($"Daugiausiai taškų surinko {max.category} kategorija ({max.sum} taškai).");

        NextCase:
            Console.WriteLine();
        }
    }


    /// <summary>
    /// Apskaičiuoja mažiausią ir didžiausią taškų sumą iš pateiktų kategorijų.
    /// </summary>
    static void MinMax(
        List<(int category, int sum)> scores,
        out (int category, int sum) min,
        out (int category, int sum) max
    ){
        min = max = scores[0]; //Pradžioje prilyginame min ir max pirmajam sarašo elementui

        for (int i = 1; i < scores.Count; i++)
        {
            if (scores[i].sum < min.sum) min = scores[i];
            if (scores[i].sum > max.sum) max = scores[i];
        }
    }
}
