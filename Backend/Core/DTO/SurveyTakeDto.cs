namespace Backend.Core.DTO.Survey;

public class SurveyTakeDto
{
    public int SurveyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public List<QuestionDto> Questions { get; set; } = new();
}