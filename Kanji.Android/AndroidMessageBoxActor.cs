using System;
using System.Threading.Tasks;
using Android.Media;
using AndroidX.AppCompat.App;
using Kanji.Interface.Actors;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Kanji.Android;
public class AndroidMessageBoxActor : IMessageBoxActor
{
    AndroidNavigationActor Actor => NavigationActor.Instance as AndroidNavigationActor;
    public Task<ButtonResult> ShowMessageBox(MessageBoxStandardParams param)
    {
        TaskCompletionSource<ButtonResult> res = new();
        AlertDialog.Builder builder = new(Actor.Activity);
        builder.SetTitle(param.ContentTitle);
        builder.SetMessage(param.ContentMessage);
        ButtonResult positiveButton;
        ButtonResult? negativeButton = null;
        switch (param.ButtonDefinitions)
        {
            case ButtonEnum.Ok:
                positiveButton = ButtonResult.Ok;
                break;
            case ButtonEnum.YesNo:
                positiveButton = ButtonResult.Yes;
                negativeButton = ButtonResult.No;
                break;
            case ButtonEnum.OkCancel:
                positiveButton = ButtonResult.Ok;
                negativeButton = ButtonResult.Cancel;
                break;
            case ButtonEnum.OkAbort:
                positiveButton = ButtonResult.Ok;
                negativeButton = ButtonResult.Abort;
                break;
            default: throw new NotSupportedException("Button not supported");

        }
        builder.SetPositiveButton(positiveButton.ToString(), (o, e) => res.SetResult(positiveButton));
        if (negativeButton.HasValue) builder.SetNegativeButton(negativeButton.Value.ToString(), (o, e) => res.SetResult(negativeButton.Value));

        builder.Show();
        return res.Task;
    }
}