using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using System.IO.Enumeration;

namespace FiveLetterWordFinder
{
    internal class FiveLetterWordCLass
    {
        private int count = 0;
        private int wordLength;
        private int maxWords;
        private string fileName;
        private Dictionary<char, int> letterFrequency = new Dictionary<char, int>();
        private string[]? allWordsIncludingNotFiveLetters;
        private int[]? allWords;
        private int completedTasks = 0;
        List<Task> tasks = new List<Task>();
        public event Action<int, int> ProgressChanged; // Added total parameter
        public event Action<int> TotalFoundChanged; // New event for total found count
        public event Action<Decimal> UpdateTimer;


        public FiveLetterWordCLass(string fileNameInput, int wordLengthInput, int maxWordsInput)
        {
            fileName = fileNameInput;
            wordLength = wordLengthInput;
            maxWords = maxWordsInput;
        }

        public int? Count { get { return count; } }
        private string[] ReadFile()
        {
            if (fileName == null)
            {
                return new string[] { };
            }
            string[] lines = File.ReadAllLines(fileName);
            return allWordsIncludingNotFiveLetters = lines.Where(
                word => word.Length == wordLength && word.Distinct().Count() == wordLength
            ).ToArray();
        }

        private int GetBitMask(string word)
        {
            int bitMask = 0;
            foreach (char c in word)
            {
                bitMask |= 1 << (c - 'a');
            }
            return bitMask;
        }

        private int GetLetterFrequency(string word)
        {
            int value = 0;
            foreach (char c in word)
            {
                if (value == 0)
                {
                    value = letterFrequency[c];
                }
                else if (letterFrequency[c] < value)
                {
                    value = letterFrequency[c];
                }
            }
            return value;
        }

        private void SetLetterFrequency()
        {
            if (allWordsIncludingNotFiveLetters == null)
            {
                return;
            }

            for (int wordIndex = 0; wordIndex < allWordsIncludingNotFiveLetters.Length; wordIndex++)
            {
                string word = allWordsIncludingNotFiveLetters[wordIndex];
                foreach (char c in word)
                {
                    if (letterFrequency.ContainsKey(c))
                    {
                        letterFrequency[c]++;
                    }
                    else
                    {
                        letterFrequency.Add(c, 1);
                    }
                }
            }

            letterFrequency = letterFrequency.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            allWordsIncludingNotFiveLetters = allWordsIncludingNotFiveLetters.OrderBy(word => GetLetterFrequency(word)).ToArray();
        }


        private void Find5Words(int bitmask, int iterator, int[] currentCombination, int depth)
        {
            currentCombination[depth] = iterator;

            if (depth == maxWords - 1)
            {
                Interlocked.Increment(ref count);
                string combination = String.Join(" ", currentCombination.Select(i => allWordsIncludingNotFiveLetters[i]));
                return;
            }

            for (int wordIterator = iterator; wordIterator < allWords.Length; wordIterator++)
            {
                int word = allWords[wordIterator];
                if ((word & bitmask) == 0)
                {
                    Find5Words(bitmask | word, wordIterator, currentCombination, depth + 1);
                }
            }
        }

        private void CreateBitMasks()
        {
            allWords = new int[allWordsIncludingNotFiveLetters.Length];
            Parallel.For(0, allWordsIncludingNotFiveLetters.Length, i =>
            {
                allWords[i] = GetBitMask(allWordsIncludingNotFiveLetters[i]);
            });
        }

        public void Start()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            allWordsIncludingNotFiveLetters = ReadFile();
            SetLetterFrequency();
            CreateBitMasks();

            int[] firstWords = allWords.Take(letterFrequency.Values.ElementAt(0) + letterFrequency.Values.ElementAt(1)).ToArray();
            for (int i = 0; i < firstWords.Length; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() =>
                {
                    Find5Words(firstWords[index], index, new int[maxWords], 0);
                    completedTasks++;
                    ProgressChanged.Invoke(completedTasks, firstWords.Length);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            TotalFoundChanged(count);

            watch.Stop();
            UpdateTimer.Invoke(Decimal.Divide(watch.ElapsedMilliseconds, 1000));

        }

    }
}
