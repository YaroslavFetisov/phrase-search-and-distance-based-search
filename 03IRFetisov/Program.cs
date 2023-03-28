using System;
using System.Collections.Generic;
using System.IO;

namespace Indexing
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "C:\\Users\\PC\\source\\repos\\03IRFetisov\\03IRFetisov\\Collection\\";
            string res = "C:\\Users\\PC\\source\\repos\\03IRFetisov\\03IRFetisov\\res.txt";

            char[] delimiterChars = { ' ', ' ', ' ', ',', '.', '—', '-', '–', '?', ';', ':', ')', '(', '[', ']', '!', '_', '=', '1', '2',
                '3', '4', '5', '6', '7', '8', '9', '0', '\"', '\'', '«', '»', '{', '}', '‘', '’', '“', '”', '…', '°', '*', '/', '&', '$',
                '+', '©', '>', '|', '~', '%', '„', '\\', '\t'};


            List<string> documents = new List<string>();
            foreach (string file in Directory.GetFiles(path))
            {
                documents.Add(File.ReadAllText(file));
            }

            Dictionary<string, List<int>> index = new Dictionary<string, List<int>>();
            for (int i = 0; i < documents.Count; i++)
            {
                string[] words = documents[i].Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                    string key = word.ToLowerInvariant().Substring(0, Math.Min(2, word.Length));
                    if (index.ContainsKey(key))
                    {
                        index[key].Add(i);
                    }
                    else
                    {
                        index[key] = new List<int>() { i };
                    }
                }
            }

            Dictionary<string, Dictionary<string, List<int>>> invertedIndex = new Dictionary<string, Dictionary<string, List<int>>>();

            foreach (string file in Directory.EnumerateFiles(path, "*.txt"))
            {
                int docId = int.Parse(Path.GetFileNameWithoutExtension(file));
                string content = File.ReadAllText(file);
                string[] words = content.Split(delimiterChars);

                foreach (string word in words)
                {
                    if (!invertedIndex.ContainsKey(word))
                    {
                        invertedIndex.Add(word, new Dictionary<string, List<int>>());
                    }

                    if (!invertedIndex[word].ContainsKey(file))
                    {
                        invertedIndex[word].Add(file, new List<int>());
                    }

                    invertedIndex[word][file].Add(Array.IndexOf(words, word));
                }
            }

            using (StreamWriter sw = new StreamWriter(res))
            {
                foreach (KeyValuePair<string, List<int>> entry in index)
                {
                    sw.WriteLine("\n");
                    sw.WriteLine("{0}: ", entry.Key);
                    foreach (int docId in entry.Value)
                    {
                        sw.WriteLine("{0} ", docId);
                    }
                    sw.WriteLine();
                }


                sw.WriteLine("\n\n\n");

                foreach (KeyValuePair<string, Dictionary<string, List<int>>> postingList in invertedIndex)
                {

                    sw.WriteLine("\n");
                    sw.WriteLine("{0}", postingList.Key);
                    foreach (KeyValuePair<string, List<int>> postings in postingList.Value)
                    {
                        sw.WriteLine("\tDocument: {0}, Positions: ", postings.Key);
                        foreach (int pos in postings.Value)
                        {
                            sw.WriteLine("{0} ", pos);
                        }
                        sw.WriteLine();
                    }
                }
            }

            while (true)
            {

                Console.WriteLine("Enter first search term:");
                string searchTerm1 = Console.ReadLine();
                Console.WriteLine("Enter second search term:");
                string searchTerm2 = Console.ReadLine();
                Console.WriteLine("Enter maximum distance between terms:");
                int maxDistance = int.Parse(Console.ReadLine());


                foreach (string file in Directory.EnumerateFiles(path, "*.txt"))
                {
                    int docId = int.Parse(Path.GetFileNameWithoutExtension(file));
                    string content = File.ReadAllText(file);
                    string[] words = content.Split(' ');

                    List<int> positions1 = new List<int>();
                    if (invertedIndex.ContainsKey(searchTerm1) && invertedIndex[searchTerm1].ContainsKey(file))
                    {
                        positions1 = invertedIndex[searchTerm1][file];
                    }

                    List<int> positions2 = new List<int>();
                    if (invertedIndex.ContainsKey(searchTerm2) && invertedIndex[searchTerm2].ContainsKey(file))
                    {
                        positions2 = invertedIndex[searchTerm2][file];
                    }

                    int minDistance = int.MaxValue;
                    foreach (int pos1 in positions1)
                    {
                        foreach (int pos2 in positions2)
                        {
                            int distance = Math.Abs(pos2 - pos1);
                            if (distance <= maxDistance && distance < minDistance)
                            {
                                minDistance = distance;
                            }
                        }
                    }

                    if (minDistance < int.MaxValue)
                    {
                        Console.WriteLine("Found match in document {0}: distance = {1}", docId, minDistance);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Found no matches in document {0}", docId);
                        continue;
                    }
                }
            }
        }
    }
}
