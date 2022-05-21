using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Catalyst;
using Catalyst.Models;
using Mosaik.Core;
using Version = Mosaik.Core.Version;

namespace GroupManager.Common.Extensions
{
    public record Words([property: JsonPropertyName("word")] List<string> Word);

    public class TextFilter
    {
        private static string _path = $"{Environment.CurrentDirectory}{Globals.Globals.SlashOrBackSlash}badWords.json";
        private readonly Words _words;
        private LanguageDetector? _detector;

        /// <summary>
        /// Create New Filter For Persian Words
        /// </summary>
        /// <param name="customPath">Optional Custom Path For Json File</param>
        /// <exception cref="ArgumentException">If Json File Is Corrupted This Exception Will Throw</exception>
        public TextFilter(string? customPath = "")
        {
            if (!string.IsNullOrEmpty(customPath))
                _path = customPath;

            var readFile = File.ReadAllText(_path);
            var deserialize = JsonSerializer.Deserialize<Words>(readFile);
            _words = deserialize ?? throw new ArgumentException($"{nameof(deserialize)} cant parse json object");
            Storage.Current = new DiskStorage("catalyst-models");
        }

        /// <summary>
        /// check if the given word is sensitive,
        /// </summary>
        /// <param name="word">Incoming Word To Check</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Return <c>true</c> if word is offensive otherwise return <c>false</c> </returns> 
        public bool IsBadWord(string word)
        {
            return word switch
            {
                null => throw new ArgumentNullException($"{nameof(word)} cant be null"),
                "" or " " => false,
                _ => _words.Word.Any(p => p == word || p.Contains(word) || p.StartsWith(word) || p.EndsWith(word))
            };
        }

        /// <summary>
        /// Check If There Is bad word Inside the Sentence
        /// </summary>
        /// <param name="sentence">The Incoming Sentence/Comment To Check</param>
        /// <param name="expectCount">How Many Bad Words Are Expected If Founded Word's Count In Sentence Are Than <c>expectCount</c> Method Will Return True</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Return True If Bad Words Count Is More Than <c>expectCount</c></returns>
        public bool IsBadSentence(string sentence, ushort expectCount = 1)
        {
            if (string.IsNullOrEmpty(sentence))
                throw new ArgumentNullException($"{nameof(sentence)} cant be null");

            var inputWordList = sentence.Split(' ');
            var counter = inputWordList.Count(IsBadWord);
            return counter >= expectCount;
        }

        /// <summary>
        /// Remove All The Bad Words From The Sentence.
        /// </summary>
        /// <param name="sentence">InComing Sentence/Comment To Check</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Return Filtered Text.</returns>
        public string RemoveBadWords(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
                throw new ArgumentNullException($"{nameof(sentence)} cant be null");

            var splitLines = Regex.Split(sentence, "\r\n|\r|\n");


            foreach (var (value, index) in splitLines.Select((value, index) => (value, index)))
            {
                splitLines[index] = string.Join(" ", value.Split(' ').Except(_words.Word));
            }

            return splitLines.Aggregate("", (current, splitLine) => current + ("\n" + splitLine));
        }

        /// <summary>
        /// Find All Bad Worlds Inside A Sentence.
        /// </summary>
        /// <param name="sentence">Incoming sentence/Comment To Check</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Return <c>List</c> of bad Worlds Exists In Given Sentence.</returns>
        public List<string> GetBadWords(string sentence)
        {
            if (string.IsNullOrEmpty(sentence))
                throw new ArgumentNullException($"{nameof(sentence)} cant be null");

            var splitLines = Regex.Split(sentence, "\r\n|\r|\n");

            var result = new List<string>();

            foreach (var lines in splitLines)
                result.AddRange(lines.Split(' ').Where(IsBadWord).ToList());

            return result;
        }

        /// <summary>
        /// add word to list if its not already exists
        /// </summary>
        /// <param name="word"></param>
        /// <param name="ct"></param>
        /// <returns>return 0 when if exists , 1 on success and 2 on exception</returns>
        public async ValueTask<ushort> AddWordIfNotExistsAsync(string word, CancellationToken ct = default)
        {
            if (_words.Word.Any(p => p == word))
                return 0;

            try
            {
                _words.Word.Add(word);
                var serialize = JsonSerializer.Serialize(_words.Word);
                await File.WriteAllTextAsync(_path, serialize, ct);
                return 1;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public async ValueTask<bool> IsEnglishAsync(string sentence)
        {
            English.Register();

            _detector = await LanguageDetector.FromStoreAsync(Language.Any, Version.Latest, "");
            var doc = new Document(sentence);
            _detector.Process(doc);
            return doc.Language is Language.English;
        }
        public async ValueTask<bool> IsPersianAsync(string sentence)
        {
            Persian.Register();

            _detector = await LanguageDetector.FromStoreAsync(Language.Any, Version.Latest, "");
            var doc = new Document(sentence);
            _detector.Process(doc);
            return doc.Language is Language.Persian;
        }
        public async ValueTask<Language> DetectAsync(string sentence)
        {
            English.Register();
            Persian.Register();

            _detector = await LanguageDetector.FromStoreAsync(Language.Any, Version.Latest, "");
            return _detector.Detect(sentence);
        }
    }
}