using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kanji.Interface.ViewModels
{
    public class SettingVocabInfoViewModel : SettingControlViewModel
    {
        #region Fields

        private bool _showBookRanking;
        private bool _showWikipediaRank;
        private bool _showJlptLevel;
        private bool _showWkLevel;

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

        public bool ShowWikipediaRank
        {
            get { return _showWikipediaRank; }
            set
            {
                if (_showWikipediaRank != value)
                {
                    _showWikipediaRank = value;
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

        #endregion

        #region Constructors

        public override void InitializeSettings()
        {
            ShowBookRanking = Properties.UserSettings.Instance.ShowVocabBookRanking;
            ShowWikipediaRank = Properties.UserSettings.Instance.ShowVocabWikipediaRank;
            ShowJlptLevel = Properties.UserSettings.Instance.ShowVocabJlptLevel;
            ShowWkLevel = Properties.UserSettings.Instance.ShowVocabWkLevel;
        }

        #endregion

        #region Methods

        public override bool IsSettingChanged()
        {
            return ShowBookRanking != Properties.UserSettings.Instance.ShowVocabBookRanking
                || ShowWikipediaRank != Properties.UserSettings.Instance.ShowVocabWikipediaRank
                || ShowJlptLevel != Properties.UserSettings.Instance.ShowVocabJlptLevel
                || ShowWkLevel != Properties.UserSettings.Instance.ShowVocabWkLevel;
        }

        protected override void DoSaveSetting()
        {
            Properties.UserSettings.Instance.ShowVocabBookRanking = ShowBookRanking;
            Properties.UserSettings.Instance.ShowVocabWikipediaRank = ShowWikipediaRank;
            Properties.UserSettings.Instance.ShowVocabJlptLevel = ShowJlptLevel;
            Properties.UserSettings.Instance.ShowVocabWkLevel = ShowWkLevel;
        }

        #endregion
    }
}
