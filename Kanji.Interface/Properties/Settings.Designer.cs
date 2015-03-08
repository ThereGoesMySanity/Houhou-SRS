﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kanji.Interface.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Hiragana")]
        public global::Kanji.Interface.Models.KanaTypeEnum KunYomiReadingType {
            get {
                return ((global::Kanji.Interface.Models.KanaTypeEnum)(this["KunYomiReadingType"]));
            }
            set {
                this["KunYomiReadingType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Hiragana")]
        public global::Kanji.Interface.Models.KanaTypeEnum OnYomiReadingType {
            get {
                return ((global::Kanji.Interface.Models.KanaTypeEnum)(this["OnYomiReadingType"]));
            }
            set {
                this["OnYomiReadingType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Hiragana")]
        public global::Kanji.Interface.Models.KanaTypeEnum NanoriReadingType {
            get {
                return ((global::Kanji.Interface.Models.KanaTypeEnum)(this["NanoriReadingType"]));
            }
            set {
                this["NanoriReadingType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int VocabPerPage {
            get {
                return ((int)(this["VocabPerPage"]));
            }
            set {
                this["VocabPerPage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("40")]
        public int KanjiPerPage {
            get {
                return ((int)(this["KanjiPerPage"]));
            }
            set {
                this["KanjiPerPage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Default")]
        public string RadicalSetName {
            get {
                return ((string)(this["RadicalSetName"]));
            }
            set {
                this["RadicalSetName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Frequency")]
        public global::Kanji.Interface.Models.RadicalSortModeEnum RadicalSortMode {
            get {
                return ((global::Kanji.Interface.Models.RadicalSortModeEnum)(this["RadicalSortMode"]));
            }
            set {
                this["RadicalSortMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Default")]
        public string SrsLevelSetName {
            get {
                return ((string)(this["SrsLevelSetName"]));
            }
            set {
                this["SrsLevelSetName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastSrsTagsValue {
            get {
                return ((string)(this["LastSrsTagsValue"]));
            }
            set {
                this["LastSrsTagsValue"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10000")]
        public int SrsEntriesPerPage {
            get {
                return ((int)(this["SrsEntriesPerPage"]));
            }
            set {
                this["SrsEntriesPerPage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Home")]
        public global::Kanji.Interface.Models.StartPageEnum StartPage {
            get {
                return ((global::Kanji.Interface.Models.StartPageEnum)(this["StartPage"]));
            }
            set {
                this["StartPage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool TrayShowNotifications {
            get {
                return ((bool)(this["TrayShowNotifications"]));
            }
            set {
                this["TrayShowNotifications"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("01:00:00")]
        public global::System.TimeSpan TrayCheckInterval {
            get {
                return ((global::System.TimeSpan)(this["TrayCheckInterval"]));
            }
            set {
                this["TrayCheckInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public long TrayNotificationCountThreshold {
            get {
                return ((long)(this["TrayNotificationCountThreshold"]));
            }
            set {
                this["TrayNotificationCountThreshold"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://www.houhou-srs.com/soft/update.xml")]
        public string UpdateCheckUri {
            get {
                return ((string)(this["UpdateCheckUri"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.DateTime LastUpdateDate {
            get {
                return ((global::System.DateTime)(this["LastUpdateDate"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("24.00:00:00")]
        public global::System.TimeSpan UpdateCheckMinInterval {
            get {
                return ((global::System.TimeSpan)(this["UpdateCheckMinInterval"]));
            }
            set {
                this["UpdateCheckMinInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsAutoUpdateCheckEnabled {
            get {
                return ((bool)(this["IsAutoUpdateCheckEnabled"]));
            }
            set {
                this["IsAutoUpdateCheckEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("[userdir]")]
        public string UserDirectoryPath {
            get {
                return ((string)(this["UserDirectoryPath"]));
            }
            set {
                this["UserDirectoryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsIgnoreAnswerShortcutEnabled {
            get {
                return ((bool)(this["IsIgnoreAnswerShortcutEnabled"]));
            }
            set {
                this["IsIgnoreAnswerShortcutEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string WkApiKey {
            get {
                return ((string)(this["WkApiKey"]));
            }
            set {
                this["WkApiKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("WaniKani,WK%level%")]
        public string WkTags {
            get {
                return ((string)(this["WkTags"]));
            }
            set {
                this["WkTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AutoSkipReviews {
            get {
                return ((bool)(this["AutoSkipReviews"]));
            }
            set {
                this["AutoSkipReviews"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int CollapseMeaningsLimit {
            get {
                return ((int)(this["CollapseMeaningsLimit"]));
            }
            set {
                this["CollapseMeaningsLimit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AudioUri {
            get {
                return ((string)(this["AudioUri"]));
            }
            set {
                this["AudioUri"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ReviewPlayAudio {
            get {
                return ((bool)(this["ReviewPlayAudio"]));
            }
            set {
                this["ReviewPlayAudio"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public float AudioVolume {
            get {
                return ((float)(this["AudioVolume"]));
            }
            set {
                this["AudioVolume"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Warn")]
        public global::Kanji.Interface.Models.WindowCloseActionEnum WindowCloseAction {
            get {
                return ((global::Kanji.Interface.Models.WindowCloseActionEnum)(this["WindowCloseAction"]));
            }
            set {
                this["WindowCloseAction"] = value;
            }
        }
    }
}
