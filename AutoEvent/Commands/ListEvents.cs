using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;

namespace AutoEvent.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ListEvents : ICommand
    {
        public string Command => "ev_list";

        public string[] Aliases => null;

        public string Description => "Показывает список всех ивентов, которые можно запустить.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string resp = String.Empty;
            resp += "<color=yellow><b>Список ивентов</color></b>:\n";
            var arr = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "AutoEvent.Events");
            foreach (var type in arr)
            {
                if (type.GetProperty("CommandName") != null)
                {
                    var instance = Activator.CreateInstance(type);
                    try
                    {
                        resp += $"<b><color=yellow>[{type.GetProperty("CommandName").GetValue(instance)}]</color></b> <= {type.GetProperty("Description").GetValue(instance)}\n";
                    }
                    catch (Exception ex)
                    {
                        response = $"Произошла ошибка при чтении ивентов: {ex.Message}";
                        return false;
                    }
                }
            }
            response = resp;
            return true;
        }

        static private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
    }
}
