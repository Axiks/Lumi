using Microsoft.EntityFrameworkCore;
using Vanilla.Data;
using Vanilla_App.Helpers;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;
using AutoMapper;
using Vanilla.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Vanilla_App.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProjectModel>> GetProjectsAsync(Guid userId)
        {
            List<ProjectModel> userProjexts = new List<ProjectModel>();
            var userProjextsEntity = await _dbContext.Projects.Where(x => x.OwnerId == userId).ToListAsync();
            foreach (var project in userProjextsEntity)
            {
                userProjexts.Add(MapperHelper.ProjectEntityToProjectModel(project));
            }

            return userProjexts;
        }

        public UserEntity Create(UserCreateRequestModel create)
        {
            if (_dbContext.Users.Any(x => x.Id == create.UserId)) throw new Exception("User with this ID exist");

            var user = new UserEntity { 
                Id = create.UserId,
                About = create.About,
                Links = create.Links,
                IsRadyForOrders = create.IsRadyForOrders ?? false,
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user;
        }

        public UserEntity Update(Guid userId, UserUpdateRequestModel update)
        {
            if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var user = _dbContext.Users.First(x => x.Id == userId);
            user.About = update.About ?? user.About;
            user.Links = update.Links ?? user.Links;
            user.IsRadyForOrders = update.IsRadyForOrders ?? user.IsRadyForOrders;

            _dbContext.Update(user);
            _dbContext.SaveChanges();
            return user;
        }

        public UserEntity Get(Guid userId)
        {
            if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var user = _dbContext.Users.First(x => x.Id == userId);
            return user;
        }

        public void Delete(Guid userId)
        {
            if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var user = _dbContext.Users.First(x => x.Id == userId);
            _dbContext.Remove(user);
        }
    }
}
