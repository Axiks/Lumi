namespace Vanilla_App.Services.Bonus
{
    public class UserBonusModel
    {
        public required long UserTgId { get; init; }
        public required string BonusId { get; init; }
        public required string ShortTitle { get; init; }
        public required string Title { get; init; } // Заголовок бонуса
        public required string Description { get; init; } // Опис бонусу
        public string? CoverUrl { get; set; }
        public required DateTime DateOfRegistration { get; init; }  // Коли користувач його вперше зареєстрував
        public DateTime? DateOfUsed { get; set; } // Коли він його використав (Якщо ще ні, то вертається null)

        public bool IsUsed { get => DateOfUsed != null ? true : false; }

    }
}
