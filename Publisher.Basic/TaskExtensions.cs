using System.Threading.Tasks;

namespace Publisher.Basic
{
    public static class TaskExtensions
    {
        public static async void RunAsync(this Task t)
        {
            await t;
        }
    }
}
