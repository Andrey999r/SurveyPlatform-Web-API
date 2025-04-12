namespace Backend.Core.DTO.Survey;

public class ParticipantInfoSurveyDto
{
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string SurveyName { get; set; } = null!;
    public List<AnswerDto> Answers { get; set; } = new();
}