using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Installer.Utils.TaskDialog
{
	public partial class TaskDialog
	{
		private const string HtmlHyperlinkPattern = "<a href=\".+\">.+</a>";
		private const string HtmlHyperlinkCapturePattern = "<a href=\"(?<link>.+)\">(?<text>.+)</a>";

		private static readonly Regex _hyperlinkRegex = new Regex(HtmlHyperlinkPattern);
		private static readonly Regex _hyperlinkCaptureRegex = new Regex(HtmlHyperlinkCapturePattern);

		internal const int CommandButtonIDOffset = 2000;
		internal const int RadioButtonIDOffset = 1000;
		internal const int CustomButtonIDOffset = 500;

		/// <summary>
		/// Forces the WPF-based TaskDialog window instead of using native calls.
		/// </summary>
		public static bool ForceEmulationMode { get; set; }

		/// <summary>
		/// Occurs when a task dialog is about to show.
		/// </summary>
		/// <remarks>
		/// Use this event for both notification and modification of all task
		/// dialog showings. Changes made to the configuration options will be
		/// persisted.
		/// </remarks>
		public static event TaskDialogShowingEventHandler Showing;
		/// <summary>
		/// Occurs when a task dialog has been closed.
		/// </summary>
		public static new event TaskDialogClosedEventHandler Closed;

		/// <summary>
		/// Displays a task dialog with the given configuration options.
		/// </summary>
		/// <param name="options">
		/// A <see cref="T:TaskDialogInterop.TaskDialogOptions"/> that specifies the
		/// configuration options for the dialog.
		/// </param>
		/// <returns>
		/// A <see cref="T:TaskDialogInterop.TaskDialogResult"/> value that specifies
		/// which button is clicked by the user.
		/// </returns>
		public static TaskDialogResult Show(TaskDialogOptions options)
		{
			TaskDialogResult result = null;

			// Make a copy since we'll let Showing event possibly modify them
			TaskDialogOptions configOptions = options;

			OnShowing(new TaskDialogShowingEventArgs(ref configOptions));

			if (!VistaTaskDialog.IsAvailableOnThisOS)
				throw new PlatformNotSupportedException("Windows 7 or newer is required for task dialogs.");
			result = ShowTaskDialog(configOptions);

			OnClosed(new TaskDialogClosedEventArgs(result));

			return result;
		}

		/// <summary>
		/// Displays a task dialog that has a message and that returns a result.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="T:System.Windows.Window"/> that owns this dialog.
		/// </param>
		/// <param name="messageText">
		/// A <see cref="T:System.String"/> that specifies the text to display.
		/// </param>
		/// <returns>
		/// A <see cref="T:TaskDialogInterop.TaskDialogSimpleResult"/> value that
		/// specifies which button is clicked by the user.
		/// </returns>
		public static TaskDialogSimpleResult ShowMessage(Window owner, string messageText)
		{
			TaskDialogOptions options = TaskDialogOptions.Default;

			options.Owner = owner;
			options.Content = messageText;
			options.CommonButtons = TaskDialogCommonButtons.Close;

			return Show(options).Result;
		}
		/// <summary>
		/// Displays a task dialog that has a message and that returns a result.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="T:System.Windows.Window"/> that owns this dialog.
		/// </param>
		/// <param name="messageText">
		/// A <see cref="T:System.String"/> that specifies the text to display.
		/// </param>
		/// <param name="caption">
		/// A <see cref="T:System.String"/> that specifies the title bar
		/// caption to display.
		/// </param>
		/// <returns>
		/// A <see cref="T:TaskDialogInterop.TaskDialogSimpleResult"/> value that
		/// specifies which button is clicked by the user.
		/// </returns>
		public static TaskDialogSimpleResult ShowMessage(Window owner, string messageText, string caption)
		{
			return ShowMessage(owner, messageText, caption, TaskDialogCommonButtons.Close);
		}
		/// <summary>
		/// Displays a task dialog that has a message and that returns a result.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="T:System.Windows.Window"/> that owns this dialog.
		/// </param>
		/// <param name="messageText">
		/// A <see cref="T:System.String"/> that specifies the text to display.
		/// </param>
		/// <param name="caption">
		/// A <see cref="T:System.String"/> that specifies the title bar
		/// caption to display.
		/// </param>
		/// <param name="buttons">
		/// A <see cref="T:TaskDialogInterop.TaskDialogCommonButtons"/> value that
		/// specifies which button or buttons to display.
		/// </param>
		/// <returns>
		/// A <see cref="T:TaskDialogInterop.TaskDialogSimpleResult"/> value that
		/// specifies which button is clicked by the user.
		/// </returns>
		public static TaskDialogSimpleResult ShowMessage(Window owner, string messageText, string caption, TaskDialogCommonButtons buttons)
		{
			return ShowMessage(owner, messageText, caption, buttons, VistaTaskDialogIcon.None);
		}
		/// <summary>
		/// Displays a task dialog that has a message and that returns a result.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="T:System.Windows.Window"/> that owns this dialog.
		/// </param>
		/// <param name="messageText">
		/// A <see cref="T:System.String"/> that specifies the text to display.
		/// </param>
		/// <param name="caption">
		/// A <see cref="T:System.String"/> that specifies the title bar
		/// caption to display.
		/// </param>
		/// <param name="buttons">
		/// A <see cref="T:TaskDialogInterop.TaskDialogCommonButtons"/> value that
		/// specifies which button or buttons to display.
		/// </param>
		/// <param name="icon">
		/// A <see cref="T:TaskDialogInterop.VistaTaskDialogIcon"/> that specifies the
		/// icon to display.
		/// </param>
		/// <returns>
		/// A <see cref="T:TaskDialogInterop.TaskDialogSimpleResult"/> value that
		/// specifies which button is clicked by the user.
		/// </returns>
		public static TaskDialogSimpleResult ShowMessage(Window owner, string messageText, string caption, TaskDialogCommonButtons buttons, VistaTaskDialogIcon icon)
		{
			TaskDialogOptions options = TaskDialogOptions.Default;

			options.Owner = owner;
			options.Title = caption;
			options.Content = messageText;
			options.CommonButtons = buttons;
			options.MainIcon = icon;

			return Show(options).Result;
		}
		/// <summary>
		/// Displays a task dialog that has a message and that returns a result.
		/// </summary>
		/// <param name="owner">
		/// The <see cref="T:System.Windows.Window"/> that owns this dialog.
		/// </param>
		/// <param name="title">
		/// A <see cref="T:System.String"/> that specifies the title bar
		/// caption to display.
		/// </param>
		/// <param name="mainInstruction">
		/// A <see cref="T:System.String"/> that specifies the main text to display.
		/// </param>
		/// <param name="content">
		/// A <see cref="T:System.String"/> that specifies the body text to display.
		/// </param>
		/// <param name="expandedInfo">
		/// A <see cref="T:System.String"/> that specifies the expanded text to display when toggled.
		/// </param>
		/// <param name="verificationText">
		/// A <see cref="T:System.String"/> that specifies the text to display next to a checkbox.
		/// </param>
		/// <param name="footerText">
		/// A <see cref="T:System.String"/> that specifies the footer text to display.
		/// </param>
		/// <param name="buttons">
		/// A <see cref="T:TaskDialogInterop.TaskDialogCommonButtons"/> value that
		/// specifies which button or buttons to display.
		/// </param>
		/// <param name="mainIcon">
		/// A <see cref="T:TaskDialogInterop.VistaTaskDialogIcon"/> that specifies the
		/// main icon to display.
		/// </param>
		/// <param name="footerIcon">
		/// A <see cref="T:TaskDialogInterop.VistaTaskDialogIcon"/> that specifies the
		/// footer icon to display.
		/// </param>
		/// <returns></returns>
		public static TaskDialogSimpleResult ShowMessage(Window owner, string title, string mainInstruction, string content, string expandedInfo, string verificationText, string footerText, TaskDialogCommonButtons buttons, VistaTaskDialogIcon mainIcon, VistaTaskDialogIcon footerIcon)
		{
			TaskDialogOptions options = TaskDialogOptions.Default;

			if (owner != null)
				options.Owner = owner;
			if (!String.IsNullOrEmpty(title))
				options.Title = title;
			if (!String.IsNullOrEmpty(mainInstruction))
				options.MainInstruction = mainInstruction;
			if (!String.IsNullOrEmpty(content))
				options.Content = content;
			if (!String.IsNullOrEmpty(expandedInfo))
				options.ExpandedInfo = expandedInfo;
			if (!String.IsNullOrEmpty(verificationText))
				options.VerificationText = verificationText;
			if (!String.IsNullOrEmpty(footerText))
				options.FooterText = footerText;
			options.CommonButtons = buttons;
			options.MainIcon = mainIcon;
			options.FooterIcon = footerIcon;

			return Show(options).Result;
		}

		/// <summary>
		/// Gets the zero-based index for a common button.
		/// </summary>
		/// <param name="commonButtons">The common button set to use.</param>
		/// <param name="buttonId">The button's id.</param>
		/// <returns>An integer representing the button index, or -1 if not found.</returns>
		/// <remarks>
		/// When Alt+F4, Esc, and other non-button close commands are issued, the dialog
		/// will simulate a Cancel button click. In this case, -1 for index and a buttonid
		/// of Cancel will let you know how the user closed the dialog.
		/// </remarks>
		public static int GetButtonIndexForCommonButton(TaskDialogCommonButtons commonButtons, int buttonId)
		{
			int index = -1;

			switch (commonButtons)
			{
				default:
				case TaskDialogCommonButtons.None:
					index = -1;
					break;
				case TaskDialogCommonButtons.Close:
					index = 0;
					break;
				case TaskDialogCommonButtons.OKCancel:
					if (buttonId == (int)TaskDialogSimpleResult.Ok
						|| buttonId == (int)VistaTaskDialogCommonButtons.OK)
						index = 0;
					else if (buttonId == (int)TaskDialogSimpleResult.Cancel
						|| buttonId == (int)VistaTaskDialogCommonButtons.Cancel)
						index = 1;
					break;
				case TaskDialogCommonButtons.RetryCancel:
					if (buttonId == (int)TaskDialogSimpleResult.Retry
						|| buttonId == (int)VistaTaskDialogCommonButtons.Retry)
						index = 0;
					else if (buttonId == (int)TaskDialogSimpleResult.Cancel
						|| buttonId == (int)VistaTaskDialogCommonButtons.Cancel)
						index = 1;
					break;
				case TaskDialogCommonButtons.YesNo:
					if (buttonId == (int)TaskDialogSimpleResult.Yes
						|| buttonId == (int)VistaTaskDialogCommonButtons.Yes)
						index = 0;
					else if (buttonId == (int)TaskDialogSimpleResult.No
						|| buttonId == (int)VistaTaskDialogCommonButtons.No)
						index = 1;
					break;
				case TaskDialogCommonButtons.YesNoCancel:
					if (buttonId == (int)TaskDialogSimpleResult.Yes
						|| buttonId == (int)VistaTaskDialogCommonButtons.Yes)
						index = 0;
					else if (buttonId == (int)TaskDialogSimpleResult.No
						|| buttonId == (int)VistaTaskDialogCommonButtons.No)
						index = 1;
					else if (buttonId == (int)TaskDialogSimpleResult.Cancel
						|| buttonId == (int)VistaTaskDialogCommonButtons.Cancel)
						index = 2;
					break;
			}

			return index;
		}
		/// <summary>
		/// Gets the buttonId for a common button. If the common button set includes more than
		/// one button, the index number specifies which.
		/// </summary>
		/// <param name="commonButtons">The common button set to use.</param>
		/// <param name="index">The zero-based index into the button set.</param>
		/// <returns>An integer representing the button, used for example with callbacks and the ClickButton method.</returns>
		public static int GetButtonIdForCommonButton(TaskDialogCommonButtons commonButtons, int index)
		{
			int buttonId = 0;

			switch (commonButtons)
			{
				default:
				case TaskDialogCommonButtons.None:
				case TaskDialogCommonButtons.Close:
					// We'll set to 0 even for Close, as it doesn't matter that we
					//get the value right since there is only one button anyway
					buttonId = 0;
					break;
				case TaskDialogCommonButtons.OKCancel:
					if (index == 0)
						buttonId = (int)VistaTaskDialogCommonButtons.OK;
					else if (index == 1)
						buttonId = (int)VistaTaskDialogCommonButtons.Cancel;
					else
						buttonId = 0;
					break;
				case TaskDialogCommonButtons.RetryCancel:
					if (index == 0)
						buttonId = (int)VistaTaskDialogCommonButtons.Retry;
					else if (index == 1)
						buttonId = (int)VistaTaskDialogCommonButtons.Cancel;
					else
						buttonId = 0;
					break;
				case TaskDialogCommonButtons.YesNo:
					if (index == 0)
						buttonId = (int)VistaTaskDialogCommonButtons.Yes;
					else if (index == 1)
						buttonId = (int)VistaTaskDialogCommonButtons.No;
					else
						buttonId = 0;
					break;
				case TaskDialogCommonButtons.YesNoCancel:
					if (index == 0)
						buttonId = (int)VistaTaskDialogCommonButtons.Yes;
					else if (index == 1)
						buttonId = (int)VistaTaskDialogCommonButtons.No;
					else if (index == 2)
						buttonId = (int)VistaTaskDialogCommonButtons.Cancel;
					else
						buttonId = 0;
					break;
			}

			return buttonId;
		}
		/// <summary>
		/// Gets the buttonId for a command button.
		/// </summary>
		/// <param name="index">The zero-based index into the array of command buttons.</param>
		/// <returns>An integer representing the button, used for example with callbacks and the ClickButton method.</returns>
		/// <remarks>
		/// When creating the config options for the dialog and specifying command buttons,
		/// typically you pass in an array of button label strings. The index specifies which
		/// button to get an id for. If you passed in Save, Don't Save, and Cancel, then index 2
		/// specifies the Cancel button.
		/// </remarks>
		public static int GetButtonIdForCommandButton(int index)
		{
			return CommandButtonIDOffset + index;
		}
		/// <summary>
		/// Gets the buttonId for a radio button.
		/// </summary>
		/// <param name="index">The zero-based index into the array of radio buttons.</param>
		/// <returns>An integer representing the button, used for example with callbacks and the ClickButton method.</returns>
		/// <remarks>
		/// When creating the config options for the dialog and specifying radio buttons,
		/// typically you pass in an array of radio label strings. The index specifies which
		/// button to get an id for. If you passed in Automatic, Manual, and Disabled, then index 1
		/// specifies the Manual radio button.
		/// </remarks>
		public static int GetButtonIdForRadioButton(int index)
		{
			return RadioButtonIDOffset + index;
		}
		/// <summary>
		/// Gets the buttonId for a custom button.
		/// </summary>
		/// <param name="index">The zero-based index into the array of custom buttons.</param>
		/// <returns>An integer representing the button, used for example with callbacks and the ClickButton method.</returns>
		/// <remarks>
		/// When creating the config options for the dialog and specifying custom buttons,
		/// typically you pass in an array of button label strings. The index specifies which
		/// button to get an id for. If you passed in Save, Don't Save, and Cancel, then index 2
		/// specifies the Cancel custom button.
		/// </remarks>
		public static int GetButtonIdForCustomButton(int index)
		{
			return CustomButtonIDOffset + index;
		}

		internal static VistaTaskDialogCommonButtons ConvertCommonButtons(TaskDialogCommonButtons commonButtons)
		{
			VistaTaskDialogCommonButtons vtdCommonButtons = VistaTaskDialogCommonButtons.None;

			switch (commonButtons)
			{
				default:
				case TaskDialogCommonButtons.None:
					vtdCommonButtons = VistaTaskDialogCommonButtons.None;
					break;
				case TaskDialogCommonButtons.Close:
					vtdCommonButtons = VistaTaskDialogCommonButtons.Close;
					break;
				case TaskDialogCommonButtons.OKCancel:
					vtdCommonButtons = VistaTaskDialogCommonButtons.OK | VistaTaskDialogCommonButtons.Cancel;
					break;
				case TaskDialogCommonButtons.RetryCancel:
					vtdCommonButtons = VistaTaskDialogCommonButtons.Retry | VistaTaskDialogCommonButtons.Cancel;
					break;
				case TaskDialogCommonButtons.YesNo:
					vtdCommonButtons = VistaTaskDialogCommonButtons.Yes | VistaTaskDialogCommonButtons.No;
					break;
				case TaskDialogCommonButtons.YesNoCancel:
					vtdCommonButtons = VistaTaskDialogCommonButtons.Yes | VistaTaskDialogCommonButtons.No | VistaTaskDialogCommonButtons.Cancel;
					break;
			}

			return vtdCommonButtons;
		}
		internal static TaskDialogButtonData ConvertCommonButton(VistaTaskDialogCommonButtons commonButton, System.Windows.Input.ICommand command = null, bool isDefault = false, bool isCancel = false)
		{
			int id = 0;

			switch (commonButton)
			{
				default:
				case VistaTaskDialogCommonButtons.None:
					id = (int)TaskDialogSimpleResult.None;
					break;
				case VistaTaskDialogCommonButtons.OK:
					id = (int)TaskDialogSimpleResult.Ok;
					break;
				case VistaTaskDialogCommonButtons.Yes:
					id = (int)TaskDialogSimpleResult.Yes;
					break;
				case VistaTaskDialogCommonButtons.No:
					id = (int)TaskDialogSimpleResult.No;
					break;
				case VistaTaskDialogCommonButtons.Cancel:
					id = (int)TaskDialogSimpleResult.Cancel;
					break;
				case VistaTaskDialogCommonButtons.Retry:
					id = (int)TaskDialogSimpleResult.Retry;
					break;
				case VistaTaskDialogCommonButtons.Close:
					id = (int)TaskDialogSimpleResult.Close;
					break;
			}

			return new TaskDialogButtonData(id, "_" + commonButton.ToString(), command, isDefault, isCancel);
		}

		/// <summary>
		/// Raises the <see cref="E:Showing"/> event.
		/// </summary>
		/// <param name="e">The <see cref="TaskDialogInterop.TaskDialogShowingEventArgs"/> instance containing the event data.</param>
		private static void OnShowing(TaskDialogShowingEventArgs e)
		{
			if (Showing != null)
			{
				Showing(null, e);
			}
		}
		/// <summary>
		/// Raises the <see cref="E:Closed"/> event.
		/// </summary>
		/// <param name="e">The <see cref="TaskDialogInterop.TaskDialogClosedEventArgs"/> instance containing the event data.</param>
		private static void OnClosed(TaskDialogClosedEventArgs e)
		{
			if (Closed != null)
			{
				Closed(null, e);
			}
		}
		private static TaskDialogResult ShowTaskDialog(TaskDialogOptions options)
		{
			VistaTaskDialog vtd = new VistaTaskDialog();

			vtd.WindowTitle = options.Title;
			vtd.MainInstruction = options.MainInstruction;
			vtd.Content = options.Content;
			vtd.ExpandedInformation = options.ExpandedInfo;
			vtd.Footer = options.FooterText;

			if (options.CommandButtons != null && options.CommandButtons.Length > 0)
			{
				List<VistaTaskDialogButton> lst = new List<VistaTaskDialogButton>();
				for (int i = 0; i < options.CommandButtons.Length; i++)
				{
					try
					{
						VistaTaskDialogButton button = new VistaTaskDialogButton();
						button.ButtonId = GetButtonIdForCommandButton(i);
						button.ButtonText = options.CommandButtons[i];
						lst.Add(button);
					}
					catch (FormatException)
					{
					}
				}
				vtd.Buttons = lst.ToArray();
				if (options.DefaultButtonIndex.HasValue
					&& options.DefaultButtonIndex >= 0
					&& options.DefaultButtonIndex.Value < vtd.Buttons.Length)
					vtd.DefaultButton = vtd.Buttons[options.DefaultButtonIndex.Value].ButtonId;
			}
			else if (options.RadioButtons != null && options.RadioButtons.Length > 0)
			{
				List<VistaTaskDialogButton> lst = new List<VistaTaskDialogButton>();
				for (int i = 0; i < options.RadioButtons.Length; i++)
				{
					try
					{
						VistaTaskDialogButton button = new VistaTaskDialogButton();
						button.ButtonId = GetButtonIdForRadioButton(i);
						button.ButtonText = options.RadioButtons[i];
						lst.Add(button);
					}
					catch (FormatException)
					{
					}
				}
				vtd.RadioButtons = lst.ToArray();
				vtd.NoDefaultRadioButton = (!options.DefaultButtonIndex.HasValue || options.DefaultButtonIndex.Value == -1);
				if (options.DefaultButtonIndex.HasValue
					&& options.DefaultButtonIndex >= 0
					&& options.DefaultButtonIndex.Value < vtd.RadioButtons.Length)
					vtd.DefaultButton = vtd.RadioButtons[options.DefaultButtonIndex.Value].ButtonId;
			}

			bool hasCustomCancel = false;

			if (options.CustomButtons != null && options.CustomButtons.Length > 0)
			{
				List<VistaTaskDialogButton> lst = new List<VistaTaskDialogButton>();
				for (int i = 0; i < options.CustomButtons.Length; i++)
				{
					try
					{
						VistaTaskDialogButton button = new VistaTaskDialogButton();
						button.ButtonId = GetButtonIdForCustomButton(i);
						button.ButtonText = options.CustomButtons[i];

						if (!hasCustomCancel)
						{
							hasCustomCancel =
								(button.ButtonText == "Close"
								|| button.ButtonText == "Cancel");
						}

						lst.Add(button);
					}
					catch (FormatException)
					{
					}
				}

				vtd.Buttons = lst.ToArray();
				if (options.DefaultButtonIndex.HasValue
					&& options.DefaultButtonIndex.Value >= 0
					&& options.DefaultButtonIndex.Value < vtd.Buttons.Length)
					vtd.DefaultButton = vtd.Buttons[options.DefaultButtonIndex.Value].ButtonId;
				vtd.CommonButtons = VistaTaskDialogCommonButtons.None;
			}
			else
			{
				vtd.CommonButtons = ConvertCommonButtons(options.CommonButtons);

				if (options.DefaultButtonIndex.HasValue
					&& options.DefaultButtonIndex >= 0)
					vtd.DefaultButton = GetButtonIdForCommonButton(options.CommonButtons, options.DefaultButtonIndex.Value);
			}

			vtd.MainIcon = options.MainIcon;
			vtd.CustomMainIcon = options.CustomMainIcon;
			vtd.FooterIcon = options.FooterIcon;
			vtd.CustomFooterIcon = options.CustomFooterIcon;
			vtd.EnableHyperlinks = DetectHyperlinks(options.Content, options.ExpandedInfo, options.FooterText);
			vtd.AllowDialogCancellation =
				(options.AllowDialogCancellation
				|| hasCustomCancel
				|| options.CommonButtons == TaskDialogCommonButtons.Close
				|| options.CommonButtons == TaskDialogCommonButtons.OKCancel
				|| options.CommonButtons == TaskDialogCommonButtons.YesNoCancel);
			vtd.CallbackTimer = options.EnableCallbackTimer;
			vtd.ExpandedByDefault = options.ExpandedByDefault;
			vtd.ExpandFooterArea = options.ExpandToFooter;
			vtd.PositionRelativeToWindow = true;
			vtd.RightToLeftLayout = false;
			vtd.NoDefaultRadioButton = false;
			vtd.CanBeMinimized = false;
			vtd.ShowProgressBar = options.ShowProgressBar;
			vtd.ShowMarqueeProgressBar = options.ShowMarqueeProgressBar;
			vtd.UseCommandLinks = (options.CommandButtons != null && options.CommandButtons.Length > 0);
			vtd.UseCommandLinksNoIcon = false;
			vtd.VerificationText = options.VerificationText;
			vtd.VerificationFlagChecked = options.VerificationByDefault;
			vtd.ExpandedControlText = "Hide details";
			vtd.CollapsedControlText = "Show details";
			vtd.Callback = options.Callback;
			vtd.CallbackData = options.CallbackData;
			vtd.Config = options;

			TaskDialogResult result = null;
			int diagResult = 0;
			TaskDialogSimpleResult simpResult = TaskDialogSimpleResult.None;
			bool verificationChecked = false;
			int radioButtonResult = -1;
			int? commandButtonResult = null;
			int? customButtonResult = null;

			diagResult = vtd.Show((vtd.CanBeMinimized ? null : options.Owner), out verificationChecked, out radioButtonResult);

			if (diagResult >= CommandButtonIDOffset)
			{
				simpResult = TaskDialogSimpleResult.Command;
				commandButtonResult = diagResult - CommandButtonIDOffset;
			}
			else if (radioButtonResult >= RadioButtonIDOffset)
			{
				simpResult = (TaskDialogSimpleResult)diagResult;
				radioButtonResult -= RadioButtonIDOffset;
			}
			else if (diagResult >= CustomButtonIDOffset)
			{
				simpResult = TaskDialogSimpleResult.Custom;
				customButtonResult = diagResult - CustomButtonIDOffset;
			}
			else
			{
				simpResult = (TaskDialogSimpleResult)diagResult;
			}

			result = new TaskDialogResult(
				simpResult,
				(String.IsNullOrEmpty(options.VerificationText) ? null : (bool?)verificationChecked),
				((options.RadioButtons == null || options.RadioButtons.Length == 0) ? null : (int?)radioButtonResult),
				((options.CommandButtons == null || options.CommandButtons.Length == 0) ? null : commandButtonResult),
				((options.CustomButtons == null || options.CustomButtons.Length == 0) ? null : customButtonResult));

			return result;
		}
		private static bool DetectHyperlinks(string content, string expandedInfo, string footerText)
		{
			return DetectHyperlinks(content) || DetectHyperlinks(expandedInfo) || DetectHyperlinks(footerText);
		}
		private static bool DetectHyperlinks(string text)
		{
			if (String.IsNullOrEmpty(text))
				return false;
			return _hyperlinkRegex.IsMatch(text);
		}
	}
}
