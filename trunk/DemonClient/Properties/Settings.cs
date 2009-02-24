namespace DemonClient.Properties
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "8.0.0.0")]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings) SettingsBase.Synchronized(new Settings()));

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
        }

        private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
        {
        }

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [DebuggerNonUserCode, DefaultSettingValue(""), UserScopedSetting]
        public string password
        {
            get
            {
                return (string) this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }

        [DebuggerNonUserCode, UserScopedSetting, DefaultSettingValue("False")]
        public bool remember
        {
            get
            {
                return (bool) this["remember"];
            }
            set
            {
                this["remember"] = value;
            }
        }

        [UserScopedSetting, DefaultSettingValue(""), DebuggerNonUserCode]
        public string username
        {
            get
            {
                return (string) this["username"];
            }
            set
            {
                this["username"] = value;
            }
        }
    }
}

