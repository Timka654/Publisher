using System.Reflection;

namespace ServerPublisher.Server.Utils
{
    public static class AssemblyUtils
    {
        public static MethodInfo GetScriptMethod(this Assembly assembly, string method)
        {
            return assembly.GetType("PublisherScript").GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}
