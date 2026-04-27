using TestTrace_V1.Contracts;
using TestTrace_V1.Domain;

namespace TestTrace_V1.Persistence;

public interface IProjectRepository
{
    TestTraceProject Load(ProjectLocation location);
    SaveResult Save(TestTraceProject project, ProjectLocation location);
}
