using System.Reflection;

namespace VoiceAssistantBackend.Commands
{
    public static class Misc
    {
        public static void PrintAvailableCommands()
        {
            var theList = Assembly.GetExecutingAssembly().GetTypes()
                      .Where(t => t.Namespace == "VoiceAssistantBackend.Commands"
                             && t.IsAbstract
                             && t.IsClass
                             && t.Name != typeof(Misc).Name
                             )
                      .ToList();

            foreach (var item in theList)
            {
                var methods = item.GetMethods()
                    .Where(
                    m => m.IsPublic
                    && m.IsStatic
                    ).ToList();

                foreach (var method in methods)
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
                    Console.WriteLine(methodLine);
                }
            }
            Console.WriteLine("Here are commands!");
            Console.ReadKey();
        }
    }
}
