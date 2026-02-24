namespace Backend.Domain.Entities;

public class ProjectTechnology
{
    public Guid ProjectId { get; private set; }
    public Guid TechnologyId { get; private set; }

    private ProjectTechnology() { }

    public ProjectTechnology(Guid projectId, Guid technologyId)
    {
        ProjectId = projectId;
        TechnologyId = technologyId;
    }
}
