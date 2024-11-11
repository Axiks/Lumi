namespace Vanilla.TelegramBot.Interfaces
{
    public delegate void ChangePagesFlowByPagesEventHandler(List<IPage> pages);

    public interface IPageChangeFlowExtension
    {
        public event ChangePagesFlowByPagesEventHandler? ChangePagesFlowByPagesPagesEvent;
    }
}
