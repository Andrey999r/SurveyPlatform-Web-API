namespace Backend.Core.DTO.Survey;

public class ParticipantDto
{
    public string ParticipantName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CompletedAt { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}