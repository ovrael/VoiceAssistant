using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistantUI.VoiceAssistant
{
    public class AssistantData
    {
        public double ConfidenceThreshold { get; set; } = 0.55;
        public string Language { get; set; } = "en-US";
        public Dictionary<string, string> ChangeableVariables { get; set; } = new Dictionary<string, string>()
        {
            {"AssistantName", "Kaladin" }
        };

        public string DataFilePath { get; set; } = @"\src\data\data.json";

        public List<AssistantChoice> Choices { get; set; } = new List<AssistantChoice>();
        public List<AssistantGrammar> Grammars { get; set; } = new List<AssistantGrammar>();

        public AssistantData()
        {

        }
    }
}
