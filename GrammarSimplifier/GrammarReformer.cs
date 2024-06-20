using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

/*
Напишите программу, которая считывает из файла описание грамматики, удаляет из нее 
бесполезные символы и выводит текст с описанием новой грамматики. Грамматику описывать 
можно так: “  D-> ...” где  правила разделяются внутри строки пробелами и \e обозначает 
эпсилон.
 */

namespace GrammarSimplifier
{
    class GrammarReformer
    {
        private Dictionary<string, List<string>> P;
        private string Start;
        private string Terminals;
        private StringBuilder str_representation;

        public GrammarReformer()
        {
            P = new Dictionary<string, List<string>>();
            Start = string.Empty;
            Terminals = string.Empty;
            str_representation = new StringBuilder();
        }

        public override string ToString()
        {
            return str_representation.ToString();
        }

        public void Read(string fname)
        {
            using (StreamReader fs = new StreamReader(fname))
            {
                bool f = false;
                while (!fs.EndOfStream)
                {
                    string line = fs.ReadLine();
                    str_representation.Append(line + '\n');
                    if (!f && line != "P:")
                    {
                        var NT_set = Recognise(line, "NT");
                        var NT = NT_set.Substring(1, NT_set.Length - 2).Split(", ");
                        Start = line.Substring(line.Length - 2, 1);
                        Terminals = Recognise(line, "T");
                        foreach (string k in NT)
                        {
                            P.Add(k, new List<string>());
                        }
                    }
                    if (f)
                    {
                        if (line == "") break;
                        var parts = line.Split("->");
                        if (parts.Length == 2)
                        {
                            var Y = parts[1].Split(" ");
                            foreach (string elem in Y)
                            {
                                P[parts[0]].Add(elem);
                            }
                        }
                    }
                    if (line == "P:") f = true;
                }
            }
        }

        private string Recognise(string line, string type)
        {
            Regex regex;
            switch (type)
            {
                case "NT":
                    regex = new Regex(@"\{([A-Z], )*[A-Z]{1}\}");
                    break;
                case "T":
                    regex = new Regex(@"\{([a-z], )*[a-z]{1}\}");
                    break;
                default:
                    regex = new Regex(@"");
                    break;
            }
            Match m = regex.Match(line);
            return m.Groups[0].Value;
        }

        private HashSet<string> DetermineFruitlessSymbols()
        {
            HashSet<string> Generatives = new HashSet<string>();
            HashSet<string> Generatives_prev = new HashSet<string>();
            int i = 1;
            while ((Generatives.Count != Generatives_prev.Count || i == 1) && Generatives.Count != P.Keys.Count)
            {
                Generatives_prev = Generatives_prev.Union(Generatives).ToHashSet();
                foreach (string k in P.Keys)
                {
                    foreach (var line in P[k])
                    {
                        if (line == line.ToLower() || Take(line, "NT").IsSubsetOf(Generatives_prev))
                        {
                            Generatives.Add(k);
                        }
                    }
                }
                i++;
            }
            return P.Keys.Except(Generatives).ToHashSet();
        }

        private HashSet<string> Take(string s, string type) //type "T" - terminals, "NT" - nonterminals
        {
            HashSet<string> res = new HashSet<string>();
            switch (type)
            {
                case "NT":
                    foreach (char x in s)
                    {
                        string xx = x.ToString();
                        if (xx.ToUpper().Equals(xx))
                        {
                            res.Add(xx);
                        }
                    }
                    break;
                case "T":
                    foreach (char x in s)
                    {
                        string xx = x.ToString();
                        if (String.Compare(xx, "a") >= 0 && String.Compare(xx, "z") <= 0)
                        {
                            res.Add(xx);
                        }
                    }
                    break;
                default:
                    break;
            }
            return res;
        }

        public void RemoveFruitlessSymbols()
        {
            HashSet<string> SymbolsToDelete = DetermineFruitlessSymbols();
            foreach (string k in P.Keys)
            {
                if (SymbolsToDelete.Contains(k))
                {
                    P.Remove(k);
                }
                else
                {
                    P[k].RemoveAll(elem => Take(elem, "NT").Intersect(SymbolsToDelete).Count() >= 1);
                }
            }
        }

        private HashSet<string> DetermineUnachievableSymbols()
        {
            HashSet<string> Reachable = new HashSet<string> { Start };
            HashSet<string> Reachable_prev = new HashSet<string> { Start };
            int i = 1;
            while ((Reachable.Count != Reachable_prev.Count || i == 1) && Reachable.Count != P.Keys.Count)
            {
                Reachable_prev = Reachable_prev.Union(Reachable).ToHashSet();
                foreach (string k in P.Keys)
                {
                    if (Reachable.Contains(k))
                    {
                        foreach (var line in P[k])
                        {
                            Reachable = Reachable.Union(Take(line, "NT")).ToHashSet();
                        }
                    }
                }
                i++;
            }
            return P.Keys.Except(Reachable).ToHashSet();
        }

        private void RemoveUnachievableSymbols()
        {
            HashSet<string> SymbolsToDelete = DetermineUnachievableSymbols();
            foreach (string k in P.Keys)
            {
                if (SymbolsToDelete.Contains(k))
                {
                    P.Remove(k);
                }
            }
        }

        public void RemoveUslessSymbols()
        {
            RemoveFruitlessSymbols();
            RemoveUnachievableSymbols();
            RestructGrammar();
        }

        private void RestructGrammar()
        {
            str_representation = new StringBuilder();
            foreach (string k in P.Keys)
            {
                string Q = String.Join(" ", P[k]);
                if (k == Start)
                {
                    str_representation.Insert(0, $"{k}->{Q}\n");
                }
                else str_representation.Append($"{k}->{Q}\n");
            }
            string NT = String.Join(", ", P.Keys);
            str_representation.Insert(0, "G1 = ({" + NT + "}, {" + Terminals + "}, P, " + Start + ")\nP:\n");
        }
    }
}
