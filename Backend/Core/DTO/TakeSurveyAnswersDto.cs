namespace Backend.Core.DTO.Survey;

public class TakeSurveyAnswersDto
{
    public string ParticipantName { get; set; } = "Аноним";
    public string ParticipantEmail { get; set; } = null!;
    public string[] Answers { get; set; } = Array.Empty<string>();
}