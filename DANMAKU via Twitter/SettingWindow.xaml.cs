using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DANMAKU_via_Twitter
{
	/// <summary>
	/// SettingWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class SettingWindow : Window
	{
		private MainWindow mainWindow;
		public string Query { get; set; }

		public SettingWindow(MainWindow mainWindow)
		{
			// stop streaming when open the window
			this.mainWindow = mainWindow;
			mainWindow.disposeStream();

			InitializeComponent();
			this.DataContext = mainWindow;
		}

		/// <summary>
		/// recalculate line height when fontfamily is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			mainWindow.calcLine();
		}

		/// <summary>
		/// recalculate line height when font size is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			mainWindow.calcLine();
		}

		/// <summary>
		/// close the window for button click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// restart streaming when closing the window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_Closed(object sender, EventArgs e)
		{
			mainWindow.createStream();
		}

		/// <summary>
		/// request reauthorize with twitter
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			mainWindow.authorize();
		}
	}

	/// <summary>
	/// validation check for empty textbox
	/// </summary>
	public class EmptyValidationRule: ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string str = Convert.ToString(value);

			if (string.IsNullOrWhiteSpace(str))
			{
				return new ValidationResult(false, "required");
			}
			return ValidationResult.ValidResult;
		}
	}

	/// <summary>
	/// converter for invert boolean
	/// </summary>
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !((bool)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return !((bool)value);
		}
	}
}
