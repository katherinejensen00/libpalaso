﻿using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.SettingProtection
{
	/// <summary>
	/// This control will hide & challenge for a password, as appropriate.
	/// You can also make a custom control which fits better for your application, and use the SettingsLauncherHelper just like this does.
	/// </summary>
	public partial class SettingsLauncherButton : UserControl
	{
		private SettingsLauncherHelper _helper;

		public SettingsLauncherButton()
		{
			this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			InitializeComponent();

			_helper = new SettingsLauncherHelper(this.Container);
			_helper.CustomSettingsControl = this;
		}

		/// <summary>
		/// Provide a method which launches your settings dialog/application/etc.
		/// It will be called when the user clicks the link (and potentially enters the password, etc.)
		/// </summary>
		public Func<DialogResult> LaunchSettingsCallback { get; set; }


		private void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			_helper.LaunchSettingsIfAppropriate(() =>
												{
													return LaunchSettingsCallback();
												});
		}
	}
}