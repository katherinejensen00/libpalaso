﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Palaso.WinForms.LogBox {


	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
	internal sealed partial class LogBoxSettings : global::System.Configuration.ApplicationSettingsBase {

		private static LogBoxSettings defaultInstance = ((LogBoxSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new LogBoxSettings())));

		public static LogBoxSettings Default {
			get {
				return defaultInstance;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("")]
		public string FontName {
			get {
				return ((string)(this["FontName"]));
			}
			set {
				this["FontName"] = value;
			}
		}

		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("0")]
		public float FontSize {
			get {
				return ((float)(this["FontSize"]));
			}
			set {
				this["FontSize"] = value;
			}
		}
	}
}
