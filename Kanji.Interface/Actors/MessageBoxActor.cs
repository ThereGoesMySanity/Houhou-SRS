using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Kanji.Interface.Actors;
public class MessageBoxActor : IMessageBoxActor
{
    public static IMessageBoxActor Instance;

    public Task<ButtonResult> ShowMessageBox(MessageBoxStandardParams param)
    {
        return MessageBoxManager.GetMessageBoxStandard(param).ShowAsPopupAsync(NavigationActor.Instance.ActiveWindow);
    }
}