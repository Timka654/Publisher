//Signature for tests only

#if !Release

namespace ServerPublisher.Server.Scripts
{
    public class Globals
    {
        private IScriptableServerProjectInfo FakeProject;

        public IScriptableServerProjectInfo CurrentProject => FakeProject;

        public void SetProject(IScriptableServerProjectInfo fakeProject)
        {
            FakeProject = fakeProject;
        }
    }
}

#endif