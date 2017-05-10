using CoreTweet;
using CoreTweet.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static CoreTweet.OAuth;

namespace DANMAKU_via_Twitter
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
    {
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hwnd, int index);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		public const int WS_EX_TRANSPARENT = 0x00000020;
		public const int GWL_EXSTYLE = (-20);

		private System.Windows.Forms.NotifyIcon notifyIcon;
		private Tokens t;
		private IDisposable disporsable;
		private int[] spaces;
		private double lineHeight;

		private static string ConsumerKey = "(ConsumerKey)";
		private static string ConsumerSecret = "(ConsumerSecret)";
		public string Token;
		public string TokenSecret;

		public bool StreamTimeline { get; set; }
		public string Query { get; set; }
		public string Color { get; set; }

		/// <summary>
		/// make the window click-through
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			// Get this window's handle
			IntPtr hwnd = new WindowInteropHelper(this).Handle;

			// Change the extended window style to include WS_EX_TRANSPARENT
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
		}

		/// <summary>
		///// show main window
		/// </summary>
		public MainWindow()
        {
            InitializeComponent();

			initialize();
		}

		/// <summary>
		/// initializing
		/// </summary>
		private void initialize()
		{
			loadSettings();
			calcLine();
			createToken();
			createTaskIcon();
			createStream();
		}

		/// <summary>
		/// load settings
		/// </summary>
		private void loadSettings()
		{
			var propertyInfo = typeof(FontStyles).GetProperty(Properties.Settings.Default.FontStyle, BindingFlags.Static|BindingFlags.Public);

			Token			= Properties.Settings.Default.Token;
			TokenSecret		= Properties.Settings.Default.TokenSecret;
			StreamTimeline	= Properties.Settings.Default.StreamTimeline;
			Query			= Properties.Settings.Default.Query;
			FontFamily		= new FontFamily(Properties.Settings.Default.FontFamily);
			FontSize		= Properties.Settings.Default.FontSize;
			FontStyle		= (FontStyle)propertyInfo.GetValue(null, null);
			Color			= Properties.Settings.Default.Color;
		}

		/// <summary>
		/// save settings
		/// </summary>
		private void saveSettings()
		{
			Properties.Settings.Default.Token = Token;
			Properties.Settings.Default.TokenSecret = TokenSecret;
			Properties.Settings.Default.StreamTimeline = StreamTimeline;
			Properties.Settings.Default.Query = Query;
			Properties.Settings.Default.FontFamily = FontFamily.ToString();
			Properties.Settings.Default.FontSize = FontSize;
			Properties.Settings.Default.FontStyle = FontStyle.ToString();
			Properties.Settings.Default.Color = Color;
			Properties.Settings.Default.Save();
		}

		/// <summary>
		/// create token to connect twitter
		/// </summary>
		private void createToken()
		{
			if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(TokenSecret))
			{
				authorize();
			}
			else
			{
				t = Tokens.Create(ConsumerKey,ConsumerSecret,Token,TokenSecret);
			}
		}

		/// <summary>
		/// authorize with Twitter using OAuth
		/// </summary>
		public void authorize()
		{
			OAuthSession session = OAuth.Authorize(ConsumerKey, ConsumerSecret);
			System.Diagnostics.Process.Start(session.AuthorizeUri.AbsoluteUri);

			InputBox inputBox = new InputBox();
			inputBox.Closed += delegate (object sender, EventArgs e) {
				try
				{
					t = session.GetTokens(inputBox.textBox.Text);
					Token = t.AccessToken;
					TokenSecret = t.AccessTokenSecret;
				}
				catch (TwitterException)
				{
					authorize();
				}
			};
			inputBox.ShowDialog();
		}

		/// <summary>
		/// calclate the line height and available rows
		/// </summary>
		public void calcLine()
		{
			lineHeight = FontFamily.LineSpacing * FontSize;
			spaces = new int[(int)(SystemParameters.PrimaryScreenHeight / lineHeight)];
		}

		/// <summary>
		/// create task icon
		/// </summary>
		private void createTaskIcon()
		{
			// initializing task icon
			ShowInTaskbar = false;
			notifyIcon = new NotifyIcon();
			notifyIcon.Text = Title;
			notifyIcon.Icon = new System.Drawing.Icon("app.ico");
			notifyIcon.Visible = true;

			// initializing context menu
			ContextMenuStrip menuStrip = new ContextMenuStrip();

			ToolStripMenuItem setItem = new ToolStripMenuItem();
			ToolStripMenuItem exitItem = new ToolStripMenuItem();

			setItem.Text = "Setting";
			exitItem.Text = "Exit";

			menuStrip.Items.Add(setItem);
			menuStrip.Items.Add(exitItem);

			setItem.Click += new EventHandler(setItem_Click);
			exitItem.Click += new EventHandler(exitItem_Click);

			notifyIcon.ContextMenuStrip = menuStrip;
		}

		/// <summary>
		/// connect to twitter streaming
		/// </summary>
		public void createStream()
		{
			// if the query is empty, connect to timeline
			if (string.IsNullOrWhiteSpace(Query))
			{
				StreamTimeline = true;
			}

			// start streaming
			IConnectableObservable<StreamingMessage> stream;
			if (StreamTimeline)
			{
				stream = t.Streaming.UserAsObservable().Publish();
			}
			else
			{
				stream = t.Streaming.FilterAsObservable(track: Query).Publish();
			}
			stream.OfType<StatusMessage>().Subscribe(x => add( x.Status.Text));

			disporsable = stream.Connect();
		}

		/// <summary>
		/// disconnect from twitter streaming
		/// </summary>
		public void disposeStream()
		{
			if (disporsable != null)
				disporsable.Dispose();
		}

		/// <summary>
		/// add received tweet to screen
		/// </summary>
		/// <param name="str">tweet text</param>
		private void add(string str)
		{
			int line = str.Count(c => c == '\n') + 1;
			int pos = getPos(line);

			fillSpaces(line,pos);
			createLabel(str,pos);
		}

		/// <summary>
		/// calclate position to draw tweet
		/// </summary>
		/// <param name="line">number of lines of tweet</param>
		/// <returns>position to draw tweet</returns>
		private int getPos(int line)
		{
			int pos = 0;

			if (spaces.Length > line)
			{
				int i = 0;
				bool flag = true;

				while (flag)
				{
					for (int j = 0; j < spaces.Length - line; j++)
					{
						int sum = 0;
						for (int k = 0; k < line && k < spaces.Length; k++)
						{
							sum += spaces[j + k];
						}

						if (sum <= i)
						{
							pos = j;

							flag = false;
							break;
						}
					}
					i++;
				}
			}

			return pos;
		}

		/// <summary>
		/// occupy spaces to draw tweet
		/// </summary>
		/// <param name="line">number of lines of tweet</param>
		/// <param name="pos">position to draw tweet</param>
		private void fillSpaces(int line,int pos)
		{
			for (int k = 0; k < line && k < spaces.Length; k++)
			{
				spaces[pos + k]++;
			}

			Task.Run(async () => {
				await Task.Delay(5000);
				emptySpaces(line,pos);
			});
		}

		/// <summary>
		/// clear the spaces
		/// </summary>
		/// <param name="line">number of lines of tweet</param>
		/// <param name="pos">position to draw tweet</param>
		private void emptySpaces(int line,int pos)
		{
			for (int k = 0; k < line && k < spaces.Length; k++)
			{
				if (spaces[pos + k] > 0)
					spaces[pos + k]--;
			}
		}

		/// <summary>
		/// draw tweet
		/// </summary>
		/// <param name="str">tweet text</param>
		/// <param name="pos">position to draw tweet</param>
		private void createLabel(string str,int pos)
		{
			Dispatcher.InvokeAsync(() =>
			{
				// create label
				System.Windows.Controls.Label label = new System.Windows.Controls.Label();
				label.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(Color);
				label.Content = str;
				Canvas.SetBottom(label, pos * lineHeight);
				canvas.Children.Add(label);

				// set animation
				DoubleAnimation myDoubleAnimation = new DoubleAnimation();
				myDoubleAnimation.From = SystemParameters.PrimaryScreenWidth;
				myDoubleAnimation.To = str.Length * -100;
				myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(10));
				Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath("(Canvas.Left)"));
				Storyboard.SetTarget(myDoubleAnimation, label);
				var myStoryboard = new Storyboard();
				myStoryboard.FillBehavior = FillBehavior.HoldEnd;
				myStoryboard.Children.Add(myDoubleAnimation);
				myStoryboard.Completed += (s, e) => { canvas.Children.Remove(label); };
				myStoryboard.Begin();
			});
		}

		/// <summary>
		/// show setting window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void setItem_Click(object sender, EventArgs e)
		{
			SettingWindow settingWindow = new SettingWindow(this);
			settingWindow.Show();
		}

		/// <summary>
		/// shutdown the application
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void exitItem_Click(object sender, EventArgs e)
		{
			// save settings
			saveSettings();

			try
			{
				// remove task tray icon
				notifyIcon.Dispose();
				System.Windows.Application.Current.Shutdown();
			}
			catch { }
		}
	}
}
