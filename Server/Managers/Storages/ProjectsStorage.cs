using Publisher.Server.Info;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Publisher.Server.Managers.Storages
{
    public class ProjectsStorage
    {
        protected ConcurrentDictionary<string, ProjectInfo> storage;

        protected ProjectsStorage()
        {
            storage = new ConcurrentDictionary<string, ProjectInfo>();
        }

        public bool AddProject(ProjectInfo project)
        {
            return storage.TryAdd(project.Info.Id, project);
        }

        public bool RemoveProject(ProjectInfo project)
        {
            return RemoveProject(project.Info.Id);
        }

        public bool RemoveProject(string projectId)
        {
            return storage.TryRemove(projectId, out var dummy);
        }

        public ProjectInfo GetProject(ProjectInfo project)
        {
            return GetProject(project.Info.Id);
        }

        public ProjectInfo GetProject(string projectId)
        {
            storage.TryGetValue(projectId, out var proj);

            return proj;
        }

        public ProjectInfo GetProjectByName(string projectName)
        {
            var projList = storage.Values.Where(x => x.Info.Name == projectName);
            if (projList.Count() > 1)
                throw new Exception($"ERROR: Duplicate project by name {projectName}");
            return projList.FirstOrDefault();
        }

        internal bool ExistProject(string directory)
        {
            return storage.Any(x => x.Value.ProjectDirPath.Equals(directory, StringComparison.OrdinalIgnoreCase));
        }
    }
}
