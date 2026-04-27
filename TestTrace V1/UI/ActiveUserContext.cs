using TestTrace_V1.Domain;

namespace TestTrace_V1.UI;

public sealed class ActiveUserContext
{
    public Guid UserId { get; }
    public string DisplayName { get; }
    public string Initials { get; }

    private ActiveUserContext(Guid userId, string displayName, string initials)
    {
        UserId = userId;
        DisplayName = displayName;
        Initials = initials;
    }

    public static ActiveUserContext FromUser(UserAccount user)
    {
        return new ActiveUserContext(user.UserId, user.DisplayName, user.Initials);
    }

    public static UserAccount? PickSensibleDefault(TestTraceProject project)
    {
        if (project.Users.Count == 0)
        {
            return null;
        }

        var windowsName = (Environment.UserName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(windowsName))
        {
            var byWindowsName = project.Users.FirstOrDefault(user =>
                user.IsActive &&
                string.Equals(user.DisplayName, windowsName, StringComparison.OrdinalIgnoreCase));
            if (byWindowsName is not null)
            {
                return byWindowsName;
            }
        }

        var leadAssignment = project.AuthorityAssignments
            .FirstOrDefault(assignment =>
                assignment.IsActive &&
                assignment.Role == AuthorityRole.LeadTestEngineer &&
                assignment.ScopeType == AuthorityScopeType.Project);
        if (leadAssignment is not null)
        {
            var lead = project.Users.FirstOrDefault(user => user.UserId == leadAssignment.UserId && user.IsActive);
            if (lead is not null)
            {
                return lead;
            }
        }

        return project.Users.FirstOrDefault(user => user.IsActive);
    }
}
