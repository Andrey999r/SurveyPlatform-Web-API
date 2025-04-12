namespace Backend.Core.Entities;

public class Participant
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string ParticipantName { get; set; } = "Аноним";
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow.AddHours(3);

    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }

    public List<Answer> Answers { get; set; } = new();
}