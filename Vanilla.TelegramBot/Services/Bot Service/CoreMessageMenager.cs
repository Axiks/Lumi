namespace Vanilla.TelegramBot.Services.Bot_Service
{
    public class CoreMessageMenager
    {
        public List<int> SendMessages { get; init; } = new List<int>();
        public void Add(int messageId) => SendMessages.Add(messageId);
        public void Clear() => SendMessages.Clear();
    }
}
