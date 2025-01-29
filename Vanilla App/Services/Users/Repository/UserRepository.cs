using Microsoft.EntityFrameworkCore;
using Vanilla.Data;
using Vanilla_App.Helpers;
using Vanilla.Data.Entities;
using Vanilla_App.Services.Projects;
using Vanilla_App.Module;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Vanilla_App.Services.Users.Repository
{
    public class UserRepository(ApplicationDbContext _dbContext, StorageModule _storageModule, IConfiguration configuration) : IUserRepository
    {

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

        public UserEntity Create(CoreUserCreateRequestModel create)
        {
            if (_dbContext.Users.Any(x => x.Id == create.UserId)) throw new ArgumentException("User with this ID exist");

            var user = new UserEntity
            {
                Id = create.UserId,
                About = create.About,
                Links = create.Links,
                IsRadyForOrders = create.IsRadyForOrders ?? false,
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user;
        }

        public UserEntity Update(Guid userId, CoreUserUpdateRequestModel update)
        {
            //if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var user = _dbContext.Users.Include(x => x.ProfileImages).First(x => x.Id == userId);
            user.About = update.About ?? user.About;
            user.Links = update.Links ?? user.Links;
            user.IsRadyForOrders = update.IsRadyForOrders ?? user.IsRadyForOrders;

            _dbContext.Update(user);
            _dbContext.SaveChanges();
            return user;
        }

        public UserEntity Get(Guid userId)
        {
            //if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var user = _dbContext.Users.Include(x => x.ProfileImages).First(x => x.Id == userId);
            return user;
        }

        public UserEntity? GetOrDefault(Guid userId) => _dbContext.Users.Include(x => x.ProfileImages).FirstOrDefault(x => x.Id == userId);

        public List<UserEntity> GetAll() => _dbContext.Users.Include(x => x.ProfileImages).ToList();

        public async Task Delete(Guid userId)
        {
            //if (await _dbContext.Users.AnyAsync(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var user = await _dbContext.Users.Include(x => x.ProfileImages).FirstAsync(x => x.Id == userId);

            if(user.ProfileImages is not null)
            {
                foreach(var image in user.ProfileImages)
                {
                    await RemoveProfileImage(userId, image.Id);
                }
            }

            _dbContext.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public ProfileImage AddProfileImage(Guid userId, DownloadFileRequestModel downloadFileRequestModel)
        {
            //if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var filename = _storageModule.DownloadFile(downloadFileRequestModel).Result;

            var user = _dbContext.Users.Include(x => x.ProfileImages).First(x => x.Id == userId);
            var userImageEntity = new ImageEntity
            {
                FileName = filename
            };

            user.ProfileImages.Add(userImageEntity);

            _dbContext.SaveChanges();

            var domaintorageName = new Uri(configuration.GetValue<string>("cdnDomain")).ToString() + "storage/";
            return new ProfileImage { FileName = filename, Id = userImageEntity.Id, FileHref = domaintorageName + filename };
        }

        public async Task RemoveProfileImage(Guid userId, Guid imageId)
        {
            ///if (_dbContext.Users.Any(x => x.Id == userId) == false) throw new Exception("User with this ID exist");

            var userImage = _dbContext.Users.Include(x => x.ProfileImages).First(x => x.Id == userId).ProfileImages.First(x => x.Id == imageId);
            _dbContext.Remove(userImage);

            await _storageModule.RemoveFile(userImage.FileName);
            await _dbContext.SaveChangesAsync();
        }

    }
}
