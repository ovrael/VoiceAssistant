using System.Reflection;

namespace VoiceAssistantBackend
{
    public static class Misc
    {
        private static MethodInfo[] commandsData;

        public static void LoadAvailableCommands()
        {
            List<MethodInfo> availableCommands = new List<MethodInfo>();

            var commandClasses = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.Namespace == "VoiceAssistantBackend.Commands"
                             && t.IsAbstract
                             && t.IsClass
                             && t.Name != typeof(Misc).Name
                             )
                      .ToList();

            foreach (var commandClass in commandClasses)
            {
                //try
                //{
                //    var isAvailable = commandClass.GetProperty("IsAvailable").GetValue(null);
                //    System.Diagnostics.Debug.WriteLine(isAvailable);
                //}
                //catch (Exception e)
                //{
                //    System.Diagnostics.Debug.WriteLine(e.ToString());
                //}

                var methods = commandClass.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                    .Where(
                        //m => m.IsPublic
                        //&& m.IsStatic
                        m => m.ReturnType == typeof(void)
                        && !m.IsSpecialName
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
                    methodLine += paremeters[i].Name;
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
            int parameters = 0;
            if (bracketIndex >= 0)
            {
                var commandParts = commandName.Split('(');
                commandName = commandParts[0];

                if (commandParts[1][commandParts[1].IndexOf(',') + 1] != ')')
                    parameters = commandParts[1].Split(',').Length;
            }

            MethodInfo selectedCommand = commandsData.Where(c => c.Name == commandName && c.GetParameters().Length == parameters).FirstOrDefault();
            return selectedCommand;
        }

        public static bool RunCMDCommand(string command)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = command;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
            process.Dispose();

            return true;
        }
    }
}
