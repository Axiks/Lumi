using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Models
{
    internal struct UserBonusModel
    {
        public required string TgId { get; init; }
        public required int BonusId { get; init; }
        string Title {  get; init; } // Заголовок бонуса
        string Description {  get; set; } // Опис бонусу
        DateTime DateOfRegistration { get; init; }  // Коли користувач його вперше зареєстрував
        DateTime? DateOfUsed { get; init; } // Коли він його використав (Якщо ще ні, то вертається null)

        bool IsUsed { get => DateOfUsed != null ? true : false; }

    }
}
