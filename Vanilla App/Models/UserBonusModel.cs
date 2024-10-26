﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanilla_App.Models
{
    public struct UserBonusModel
    {
        public required long TgId { get; init; }
        public required int BonusId { get; init; }
        public string ShortTitle { get; init; }
        public string Title {  get; init; } // Заголовок бонуса
        public string Description {  get; set; } // Опис бонусу
        public DateTime DateOfRegistration { get; set; }  // Коли користувач його вперше зареєстрував
        public DateTime? DateOfUsed { get; set; } // Коли він його використав (Якщо ще ні, то вертається null)

        public bool IsUsed { get => DateOfUsed != null ? true : false; }

    }
}
