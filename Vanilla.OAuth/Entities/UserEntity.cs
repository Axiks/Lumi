﻿namespace Vanilla.OAuth.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string? Nickname { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

    }
}
