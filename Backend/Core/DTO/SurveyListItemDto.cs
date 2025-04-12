namespace Backend.Core.DTO.Survey;

public class SurveyListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}