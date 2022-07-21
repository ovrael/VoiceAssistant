using System.Reflection;

namespace VoiceAssistantBackend.Commands
{
    public static class Misc
    {
        private static readonly MethodInfo[] commandsData;

        static Misc()
        {
            List<MethodInfo> availableCommands = new List<MethodInfo>();

            var commandClasses = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.Namespace == "VoiceAssistantBackend.Commands"
                             && t.IsAbstract
                             && t.IsClass
                             && t.Name != typeof(Misc).Name
                             )
                      .ToList();

            foreach (var item in commandClasses)
            {
                var methods = item.GetMethods()
                    .Where(
                        m => m.IsPublic
                        && m.IsStatic
                    )
                    .ToList();

                availableCommands.AddRange(methods);
            }

            commandsData = availableCommands.ToArray();
        }

        public static string[] GetCommandsNames()
        {
            List<string> availableCommands = new List<string>();
            foreach (var method in commandsData)
            {

                string methodLine = method.Name + "(";
                var paremeters = method.GetParameters();
                for (int i = 0; i < paremeters.Length; i++)
                {
                    methodLine += paremeters[i].ParameterType.Name;
                    if (i < paremeters.Length - 1)
                        methodLine += ", ";
                }
                methodLine += ")";
                availableCommands.Add(methodLine);
            }

            return availableCommands.ToArray();
        }

        public static MethodInfo GetCommand(string commandName)
        {
            int bracketIndex = commandName.IndexOf('(');
            if (bracketIndex >= 0)
            {
                commandName = commandName.Substring(0, bracketIndex);
            }

            MethodInfo selectedCommand = commandsData.Where(c => c.Name == commandName).FirstOrDefault();
            return selectedCommand;
        }
    }
}
