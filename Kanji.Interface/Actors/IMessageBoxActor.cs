using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Kanji.Interface.Actors;
public interface IMessageBoxActor
{
    Task<ButtonResult> ShowMessageBox(MessageBoxStandardParams param);
}