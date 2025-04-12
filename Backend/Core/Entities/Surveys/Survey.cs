namespace Backend.Core.Entities;

public class Survey
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Внешний ключ для владельца (пользователя)
    public int ApplicationUserId { get; set; }
    public ApplicationUser? Owner { get; set; }

    // Список вопросов
    public List<Question> Questions { get; set; } = new();
    // Список участников
    public List<Participant> Participants { get; set; } = new();
}