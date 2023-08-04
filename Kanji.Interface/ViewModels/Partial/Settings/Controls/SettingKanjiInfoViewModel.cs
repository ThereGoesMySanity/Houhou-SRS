using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingKanjiInfoViewModel : SettingControlViewModel
    {
        #region Fields

        private bool _showBookRanking;
        private bool _showStrokes;
        private bool _showGrade;
        private bool _showJlptLevel;
        private bool _showWkLevel;
        private bool _showNanori;
        private bool _animateStrokes;
        private double _strokeAnimationDelay;

        #endregion

        #region Properties

        public bool ShowBookRanking
        {
            get { return _showBookRanking; }
            set
            {
                if (_showBookRanking != value)
                {
                    _showBookRanking = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowGrade
        {
            get { return _showGrade; }
            set
            {
                if (_showGrade != value)
                {
                    _showGrade = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowJlptLevel
        {
            get { return _showJlptLevel; }
            set
            {
                if (_showJlptLevel != value)
                {
                    _showJlptLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowWkLevel
        {
            get { return _showWkLevel; }
            set
            {
                if (_showWkLevel != value)
                {
                    _showWkLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowNanori
        {
            get { return _showNanori; }
            set
            {
                if (_showNanori != value)
                {
                    _showNanori = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowStrokes
        {
            get { return _showStrokes; }
            set
            {
                if (_showStrokes != value)
                {
                    _showStrokes = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool AnimateStrokes
        {
            get { return _animateStrokes; }
            set
            {
                if (_animateStrokes != value)
                {
                    _animateStrokes = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double StrokeAnimationDelay
        {
            get { return _strokeAnimationDelay; }
            set
            {
                if (_strokeAnimationDelay != value)
                {
                    _strokeAnimationDelay = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            ShowBookRanking = Properties.UserSettings.Instance.ShowKanjiBookRanking;
            ShowGrade = Properties.UserSettings.Instance.ShowKanjiGrade;
            ShowJlptLevel = Properties.UserSettings.Instance.ShowKanjiJlptLevel;
            ShowWkLevel = Properties.UserSettings.Instance.ShowKanjiWkLevel;
            ShowStrokes = Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiStrokes;
            AnimateStrokes = Kanji.Interface.Properties.UserSettings.Instance.AnimateStrokes;
            StrokeAnimationDelay = Kanji.Interface.Properties.UserSettings.Instance.StrokeAnimationDelay;
            ShowNanori = Kanji.Interface.Properties.UserSettings.Instance.ShowNanori;
        }

        #endregion

        #region Methods

        public override bool IsSettingChanged()
        {
            return ShowBookRanking != Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiBookRanking
                || ShowGrade != Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiGrade
                || ShowJlptLevel != Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiJlptLevel
                || ShowWkLevel != Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiWkLevel
                || ShowStrokes != Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiStrokes
                || AnimateStrokes != Kanji.Interface.Properties.UserSettings.Instance.AnimateStrokes
                || StrokeAnimationDelay != Kanji.Interface.Properties.UserSettings.Instance.StrokeAnimationDelay
                || ShowNanori != Kanji.Interface.Properties.UserSettings.Instance.ShowNanori;
        }

        protected override void DoSaveSetting()
        {
            Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiBookRanking = ShowBookRanking;
            Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiGrade = ShowGrade;
            Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiJlptLevel = ShowJlptLevel;
            Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiWkLevel = ShowWkLevel;
            Kanji.Interface.Properties.UserSettings.Instance.ShowKanjiStrokes = ShowStrokes;
            Kanji.Interface.Properties.UserSettings.Instance.AnimateStrokes = AnimateStrokes;
            Kanji.Interface.Properties.UserSettings.Instance.StrokeAnimationDelay = StrokeAnimationDelay;
            Kanji.Interface.Properties.UserSettings.Instance.ShowNanori = ShowNanori;
        }

        #endregion
    }
}
