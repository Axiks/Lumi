using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanilla_App.Interfaces;
using Vanilla_App.Models;

namespace Vanilla_App.Repository
{
    public class TestBonusRepository : IBonusRepository
    {
        private static Random random = new Random();
        private List<UserBonusModel> _userBonusModels {  get; init; }

        public TestBonusRepository() {
            _userBonusModels = GenerateUsersBonus(64);
        }

        public List<long> GetUsersWithBonus() => _userBonusModels.Select(x => x.TgId).ToList();

        public void TakeBonus(int bonusId)
        {
            var bonus = _userBonusModels.First(x => x.BonusId == bonusId);
            var index = _userBonusModels.IndexOf(bonus);
            bonus.DateOfUsed = DateTime.Now;
            _userBonusModels[index] = bonus;
        }

        List<UserBonusModel> IBonusRepository.GetUserBonuses(long tgId) => _userBonusModels.Where(x => x.TgId == _userBonusModels.First().TgId).ToList();

        private static DateTime RandomDay()
        {
            DateTime start = new DateTime(1999, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));
        }

        private static bool RandomBool() => random.Next(2) == 1;

        private static List<UserBonusModel> GenerateUsersBonus(int lenght)
        {
            List<UserBonusModel> userBonusModels = new List<UserBonusModel>();

            long _lastTgId = random.Next(64);
            for (int i = 0; i < lenght; i++) {
                if(random.Next(2) != 1) _lastTgId = random.Next(64);

                userBonusModels.Add(new UserBonusModel
                {
                    TgId = _lastTgId,
                    BonusId = random.Next(32),
                    ShortTitle = "Bon " + i.ToString(),
                    Title = "Bonus Name " + i.ToString(),
                    Description = "Bonus Description " + i.ToString(),
                    DateOfRegistration = RandomDay(),
                    DateOfUsed = RandomBool() ? RandomDay() : null,
                });
            }
            return userBonusModels;
        }

        public UserBonusModel GetBonus(long bonusId) => _userBonusModels.FirstOrDefault(x => x.BonusId == bonusId);
    }
}
