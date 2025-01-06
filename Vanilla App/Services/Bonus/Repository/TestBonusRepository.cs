using Vanilla_App.Services.Bonus;

namespace Vanilla_App.Services.Bonus.Repository
{
    public class TestBonusRepository : IBonusRepository
    {
        private static Random random = new Random();
        private List<UserBonusModel> _userBonusModels { get; init; }

        public TestBonusRepository()
        {
            _userBonusModels = GenerateUsersBonus(64);
        }

        public List<long> GetUsersWithBonus() => _userBonusModels.Select(x => x.UserTgId).ToList();

        public bool TakeBonus(string bonusId)
        {
            var bonus = _userBonusModels.First(x => x.BonusId.Equals(bonusId));
            var index = _userBonusModels.IndexOf(bonus);
            bonus.DateOfUsed = DateTime.Now;
            _userBonusModels[index] = bonus;

            return true;
        }

        List<UserBonusModel> IBonusRepository.GetUserBonuses(long tgId) => _userBonusModels.Where(x => x.UserTgId == _userBonusModels.First().UserTgId).ToList();

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
            for (int i = 0; i < lenght; i++)
            {
                if (random.Next(2) != 1) _lastTgId = random.Next(64);

                userBonusModels.Add(new UserBonusModel
                {
                    UserTgId = _lastTgId,
                    BonusId = random.Next(32).ToString(),
                    ShortTitle = "Bon " + i.ToString(),
                    Title = "Bonus Name " + i.ToString(),
                    Description = "Bonus Description " + i.ToString(),
                    DateOfRegistration = RandomDay(),
                    DateOfUsed = RandomBool() ? RandomDay() : null,
                });
            }
            return userBonusModels;
        }

        public UserBonusModel GetBonus(string bonusId) => _userBonusModels.FirstOrDefault(x => x.BonusId.Equals(bonusId));
    }
}
