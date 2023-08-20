using System;
using System.ComponentModel;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.ConstraintLayout.Widget;
using AndroidX.Fragment.App;
using Java.Util;
using Kanji.Android.Extensions;
using Kanji.Interface.Models;
using Kanji.Interface.ViewModels;
using SQLitePCL.lib;

namespace Kanji.Android.Fragments;
public class SrsReviewNativeFragment : Fragment
{
    private readonly SrsReviewViewModel viewModel;

    public SrsReviewNativeFragment(SrsReviewViewModel viewModel) : base(Resource.Layout.srs_review) 
    {
        this.viewModel = viewModel;
    }

    public override void OnViewCreated(View view, Bundle savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);
        viewModel.PropertyChanged += ViewModelPropertyChanged;

        view.FindViewById<Button>(Resource.Id.wrap_up).Click += (_, _) => viewModel.WrapUpCommand.Execute(null);
        view.FindViewById<Button>(Resource.Id.quit).Click += (_, _) => 
        {
            ParentFragmentManager.PopBackStack();
        };
        view.FindViewById<Button>(Resource.Id.ignore_button).Click += (_, _) => viewModel.IgnoreAnswerCommand.Execute(null);
        view.FindViewById<Button>(Resource.Id.add_meaning_button).Click += (_, _) => viewModel.AddAnswerCommand.Execute(null);

        view.FindViewById<EditText>(Resource.Id.answer_text).EditorAction += (_, e) => 
        {
            if (e.ActionId == ImeAction.Done) viewModel.SubmitCommand.Execute(null);
            else e.Handled = false;
        };
        view.FindViewById<EditText>(Resource.Id.answer_text)
            .AddTextChangedListener(new BindingTextWatcher(() => viewModel.CurrentAnswer, (s) => viewModel.CurrentAnswer = s));
        
        viewModel.StartSession().ContinueWith((_) =>
                ViewModelPropertyChanged(this, new PropertyChangedEventArgs(null)));
    }
    private void ViewModelPropertyChanged(object o, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsWrappingUp" || o == this)
        {
            View.FindViewById<Button>(Resource.Id.wrap_up).Enabled = !viewModel.IsWrappingUp;
            View.FindViewById<Button>(Resource.Id.wrap_up).Text = viewModel.IsWrappingUp?
                "Finishing the current batch of review items..."
                : "Wrap up";
        }

        if (e.PropertyName == "TotalReviewsCount" || o == this)
            View.FindViewById<ProgressBar>(Resource.Id.reviews_progress).Max = viewModel.TotalReviewsCount;

        if (e.PropertyName == "AnsweredReviewsCount" || o == this)
            View.FindViewById<ProgressBar>(Resource.Id.reviews_progress).Progress = viewModel.AnsweredReviewsCount;

        if (e.PropertyName == "TotalReviewsCount" || e.PropertyName == "AnsweredReviewsCount" || o == this)
            View.FindViewById<TextView>(Resource.Id.reviews_count).Text = $"{viewModel.AnsweredReviewsCount}/{viewModel.TotalReviewsCount}";

        if (e.PropertyName == "CurrentQuestion" || o == this)
        {
            if (viewModel.CurrentQuestion != null)
            {
                var color = new Color(unchecked((int)(viewModel.CurrentQuestion.ParentGroup.IsKanji? 0xFF2070A9 : 0xFF8B00C9)));
                View.FindViewById<ConstraintLayout>(Resource.Id.question_box).SetBackgroundColor(color);
                View.FindViewById<TextView>(Resource.Id.question_text).Text = viewModel.CurrentQuestion.ParentGroup.Item;

                color = new Color(unchecked((int)(viewModel.CurrentQuestion.Question == SrsQuestionEnum.Meaning? 0xFFCCCCCC : 0xFF333333)));
                View.FindViewById<TextView>(Resource.Id.question).SetBackgroundColor(color);
                color = new Color(unchecked((int)(viewModel.CurrentQuestion.Question == SrsQuestionEnum.Meaning? 0xFF000000 : 0xFFFFFFFF)));
                View.FindViewById<TextView>(Resource.Id.question).SetTextColor(color);

                var text = $"Enter the {(viewModel.CurrentQuestion.ParentGroup.IsKanji? "kanji" : "vocab")} {viewModel.CurrentQuestion.Question.ToString().ToLower()}";
                View.FindViewById<TextView>(Resource.Id.question).Text = text;

                View.FindViewById<TextView>(Resource.Id.accepted_answers).Text = "Accepted answers: " + viewModel.CurrentQuestion.AcceptedAnswers;

                var note = viewModel.CurrentQuestion.Question switch
                {
                    SrsQuestionEnum.Meaning => viewModel.CurrentQuestion.ParentGroup.Reference.MeaningNote,
                    SrsQuestionEnum.Reading => viewModel.CurrentQuestion.ParentGroup.Reference.ReadingNote,
                    _ => "",
                };
                View.FindViewById<TextView>(Resource.Id.notes).Text = $"{viewModel.CurrentQuestion.Question} notes: {note}";

                var suggestText = viewModel.CurrentQuestion.Question switch
                {
                    SrsQuestionEnum.Meaning => "Answer",
                    SrsQuestionEnum.Reading => "答え",
                    _ => "",
                };
                var imeHint = viewModel.CurrentQuestion.Question switch
                {
                    SrsQuestionEnum.Meaning => Locale.English,
                    SrsQuestionEnum.Reading => Locale.Japanese,
                    _ => null,
                };
                View.FindViewById<EditText>(Resource.Id.answer_text).ImeHintLocales = new LocaleList(imeHint);
            }
        }

        if (e.PropertyName == "ReviewState" || o == this)
        {
            var color = viewModel.ReviewState switch
            {
                SrsReviewStateEnum.Input => 0xFFFFFFFF,
                SrsReviewStateEnum.Success => 0xFF38BF38,
                SrsReviewStateEnum.Failure => 0xFFD82626,
                _ => 0xFFFFFFFF,
            };
            View.FindViewById<EditText>(Resource.Id.answer_text).BackgroundTintList = ColorStateList.ValueOf(new Color(unchecked((int)color)));
            // View.FindViewById<EditText>(Resource.Id.answer_text).Focusable = viewModel.ReviewState == SrsReviewStateEnum.Input;
            View.FindViewById<LinearLayout>(Resource.Id.action_buttons).Visibility = viewModel.ReviewState == SrsReviewStateEnum.Input? 
                    ViewStates.Invisible : ViewStates.Visible;
            View.FindViewById<Button>(Resource.Id.add_meaning_button).Visibility = viewModel.ReviewState == SrsReviewStateEnum.Failure? 
                    ViewStates.Visible : ViewStates.Invisible;
            View.FindViewById<ConstraintLayout>(Resource.Id.incorrect_info).Visibility = viewModel.ReviewState == SrsReviewStateEnum.Failure? 
                    ViewStates.Visible : ViewStates.Invisible;
        }
        if (e.PropertyName == "CurrentAnswer" && viewModel.CurrentAnswer != View.FindViewById<EditText>(Resource.Id.answer_text).Text)
        {
            View.FindViewById<EditText>(Resource.Id.answer_text).Text = viewModel.CurrentAnswer;
        }
    }
    public override void OnDestroyView()
    {
        base.OnDestroyView();
        viewModel.StopSessionCommand.Execute(null);
        viewModel.Dispose();
    }
}