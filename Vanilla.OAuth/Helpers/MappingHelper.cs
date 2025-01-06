using Vanilla.OAuth.Entities;
using Vanilla.OAuth.Models;

namespace Vanilla.OAuth.Helpers
{
    public static class MappingHelper
    {
        public static BasicUserModel UserEntityToBasicUserModel(UserEntity userEntity)
        {
            return new BasicUserModel
            {
                Id = userEntity.Id,
                Nickname = userEntity.Nickname,
                CreatedAt = userEntity.Created,
                Updated = userEntity.Updated
            };
        }
    }
}
