using Publisher.Server.Info;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Publisher.Server.Managers.Storages
{
    public class ProjectsStorage
    {
        protected ConcurrentDictionary<string, ServerProjectInfo> storage;

        protected ProjectsStorage()
        {
            storage = new ConcurrentDictionary<string, ServerProjectInfo>();
        }

        public bool AddProject(ServerProjectInfo project)
        {
            return storage.TryAdd(project.Info.Id, project);
        }

        public bool RemoveProject(ServerProjectInfo project)
        {
            return RemoveProject(project.Info.Id);
        }

        public bool RemoveProject(string projectId)
        {
            return storage.TryRemove(projectId, out var dummy);
        }

        public IEnumerable<ServerProjectInfo> GetProjects()
        {
            return storage.Values.ToArray();
        }

        public ServerProjectInfo GetProject(ServerProjectInfo project)
        {
            return GetProject(project.Info.Id);
        }

        public ServerProjectInfo GetProject(string projectId)
        {
            storage.TryGetValue(projectId, out var proj);

            return proj;
        }

        public ServerProjectInfo GetProjectByName(string projectName)
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
