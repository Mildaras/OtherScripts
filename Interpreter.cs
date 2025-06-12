using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string reiskinys = "1+2*3+4*5";
        List<int> reiksmes1 = EvalRecursive(reiskinys);
        int maziausiaReiksmeRek = reiksmes1.Min();
        Console.WriteLine("(Rekurisja)Mažiausia reikšmė: " + maziausiaReiksmeRek);

        int maziausiaReiksme = Eval(reiskinys);
        Console.WriteLine("(Dinaminis)Mažiausia reikšmė: " + maziausiaReiksme);
    }

    static List<int> EvalRecursive(string reiskinys)
    {
        List<int> reiksmes = new List<int>();

        int depth = 0;
        for (int i = 0; i < reiskinys.Length; i++)
        {
            if (reiskinys[i] == '(')
                depth++;
            else if (reiskinys[i] == ')')
                depth--;
            else if (depth == 0 && (reiskinys[i] == '+' || reiskinys[i] == '*'))
            {
                List<int> kairiojiReiksmes = EvalRecursive(reiskinys.Substring(0, i));
                List<int> desiniojiReiksmes = EvalRecursive(reiskinys.Substring(i + 1));

                foreach (int kairioji in kairiojiReiksmes)
                {
                    foreach (int desinioji in desiniojiReiksmes)
                    {
                        int reiksme = 0;
                        if (reiskinys[i] == '+')
                            reiksme = kairioji + desinioji;
                        else if (reiskinys[i] == '*')
                            reiksme = kairioji * desinioji;

                        reiksmes.Add(reiksme);
                    }
                }
            }
        }

        if (reiksmes.Count == 0)
            reiksmes.Add(int.Parse(reiskinys));

        return reiksmes;
    }

    static int Eval(string reiskinys)
    {
        Stack<int> operandai = new Stack<int>();
        Stack<char> operatoriai = new Stack<char>();

        for (int i = 0; i < reiskinys.Length; i++)
        {
            if (reiskinys[i] == ' ')
                continue;

            if (char.IsDigit(reiskinys[i]))
            {
                int skaičius = 0;
                while (i < reiskinys.Length && char.IsDigit(reiskinys[i]))
                {
                    skaičius = skaičius * 10 + (reiskinys[i] - '0');
                    i++;
                }
                operandai.Push(skaičius);
                i--;
            }
            else if (reiskinys[i] == '+')
            {
                while (operatoriai.Count > 0 && operatoriai.Peek() != '(')
                {
                    int desinysis = operandai.Pop();
                    int kairysis = operandai.Pop();
                    char operatorius = operatoriai.Pop();
                    operandai.Push(ApvalintiRezultatą(kairysis, operatorius, desinysis));
                }
                operatoriai.Push(reiskinys[i]);
            }
            else if (reiskinys[i] == '*')
            {
                operatoriai.Push(reiskinys[i]);
            }
            else if (reiskinys[i] == '(')
            {
                operatoriai.Push(reiskinys[i]); //O(1)
            }
            else if (reiskinys[i] == ')')
            {
                while (operatoriai.Count > 0 && operatoriai.Peek() != '(')
                {
                    int desinysis = operandai.Pop();
                    int kairysis = operandai.Pop();
                    char operatorius = operatoriai.Pop();
                    operandai.Push(ApvalintiRezultatą(kairysis, operatorius, desinysis));
                }
                operatoriai.Pop(); // Pašaliname '(' iš operatorių steko O(1)
            }
        }

        while (operatoriai.Count > 0)
        {
            int desinysis = operandai.Pop();
            int kairysis = operandai.Pop();
            char operatorius = operatoriai.Pop();
            operandai.Push(ApvalintiRezultatą(kairysis, operatorius, desinysis));
        }

        return operandai.Pop();
    }

    static int ApvalintiRezultatą(int kairysis, char operatorius, int desinysis)
    {
        if (operatorius == '+')
        {
            return kairysis + desinysis;
        }
        else if (operatorius == '*')
        {
            return kairysis * desinysis;
        }
        return 0;
    }
}
