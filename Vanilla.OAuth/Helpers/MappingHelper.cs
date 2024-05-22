using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
