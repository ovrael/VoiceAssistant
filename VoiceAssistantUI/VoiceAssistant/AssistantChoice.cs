using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using Newtonsoft.Json;
using VoiceAssistantBackend.Commands;

namespace VoiceAssistantUI
{
    public class AssistantChoice
    {
        public string Name { get; set; }
        [JsonIgnore]
        public Choices Choice { get; private set; }
        public List<string> Sentences { get; private set; } = new List<string>();
        [JsonIgnore]
        public List<string> CatchSentences { get; private set; } = new List<string>();
        public bool CanBeEdited { get; set; } = true;
        public bool CanBeDeleted { get; set; } = true;
        public bool IsSpecial { get; set; } = false;

        public AssistantChoice()
        {
        }

        public void Init()
        {
            if (IsSpecial)
            {
                if (Name.ToLower() == "$number")
                {
                    var numbers = Helpers.GetStringNumbers(min: 0, max: 100);
                    SetCatchSentences(numbers.ToList());
                    Choice = new Choices(numbers);
                }

                if (Name.ToLower() == "$artist")
                {
                    var artists = FoobarControl.GetArtistsFromMusicDirectory();
                    SetCatchSentences(artists.ToList());
                    Choice = new Choices(artists);
                }

                if (Name.ToLower() == "$songtitle")
                {
                    var songs = FoobarControl.GetSongsTitlesFromMusicDirectory();
                    SetCatchSentences(songs.ToList());
                    Choice = new Choices(songs);
                }
            }
            else
            {
                SetCatchSentences(Sentences);
                Choice = new Choices(CatchSentences.ToArray());
            }
        }

        public AssistantChoice(string name, List<string> choicesValues, bool canBeEdited = true, bool canBeDeleted = true, bool isSpecial = false)
        {
            CanBeEdited = canBeEdited;
            CanBeDeleted = canBeDeleted;
            IsSpecial = isSpecial;

            Name = name;
            Sentences = choicesValues;
            CatchSentences = choicesValues;

            Choice = new Choices(choicesValues.ToArray());
        }

        public AssistantChoice(string name, List<string> choicesValues, string canBeEdited, string canBeDeleted, string isSpecial)
        {
            CanBeEdited = bool.Parse(canBeEdited);
            CanBeDeleted = bool.Parse(canBeDeleted);
            IsSpecial = bool.Parse(isSpecial);

            Name = name;
            Sentences = choicesValues;
            CatchSentences = choicesValues;

            Choice = new Choices(choicesValues.ToArray());
        }

        public void SetCatchSentences(List<string> newCatchSentences)
        {
            CatchSentences = new List<string>();
            foreach (var item in newCatchSentences)
            {
                CatchSentences.Add(Assistant.ReplaceSpecialVariablesKeysToValues(item));
            }
        }

        public void AddChoiceSentence(string value)
        {
            if (Sentences.Contains(value))
                return;

            Sentences.Add(value);
            Choice = new Choices(Sentences.ToArray());
        }

        public void RemoveChoiceSentence(string value)
        {
            if (!Sentences.Contains(value))
                return;

            Sentences.Remove(value);
            Choice = new Choices(Sentences.ToArray());
        }
    }
}
