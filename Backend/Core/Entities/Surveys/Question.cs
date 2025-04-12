namespace Backend.Core.Entities;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;

    public int SurveyId { get; set; }
    public Survey? Survey { get; set; }
}