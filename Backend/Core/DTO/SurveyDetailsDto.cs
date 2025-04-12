namespace Backend.Core.DTO.Survey;

public class SurveyDetailsDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
    public List<ParticipantDto> Participants { get; set; } = new();
}