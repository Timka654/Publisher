
namespace ServerPublisher.Server.Scripts
{
    public class ScriptInvokingContext
    {
        public ScriptInvokingContext(IScriptableServerProjectInfo project, IScriptableExecutorContext executor)
        {
            Project = project;
            Executor = executor;
        }

        public IScriptableServerProjectInfo Project { get; }

        public IScriptableExecutorContext Executor { get; }
    }
}
