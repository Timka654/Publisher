using System.Threading.Tasks;

namespace ServerPublisher.Shared
{
    public static class TaskExtensions
    {
        public static async void RunAsync(this Task t)
        {
            await t;
        }
    }
}
