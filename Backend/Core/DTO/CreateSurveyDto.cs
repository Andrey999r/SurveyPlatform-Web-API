namespace Backend.Core.DTO.Survey;

public class CreateSurveyDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<string>? Questions { get; set; }
}