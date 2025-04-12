namespace Backend.Core.DTO.Survey;

public class SurveyWithAnswersDto
{
    public int SurveyId { get; set; }
    public string Name { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public DateTime CompletedAt { get; set; }
    public List<AnswerDto>? Answers { get; set; }
}