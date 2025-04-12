namespace Backend.Core.Entities;

public class Answer
{
    public int Id { get; set; }
    public string ResponseText { get; set; } = null!;

    public int? QuestionId { get; set; }
    public Question? Question { get; set; }

    public int? ParticipantId { get; set; }
    public Participant? Participant { get; set; }
}