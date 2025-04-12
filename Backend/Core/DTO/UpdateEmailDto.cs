namespace Backend.Core.DTO.Survey;

public class UpdateEmailDto
{
    public int ParticipantId { get; set; }
    public string NewEmail { get; set; } = null!;
}