namespace CronductorApp.Components.ViewModels;

public class NavigationItem
{
    public required string Title { get; init; }
    public required string Link { get; init; }
    public string? Icon { get; init; }
    public bool IsActive { get; set; } = false;
}
