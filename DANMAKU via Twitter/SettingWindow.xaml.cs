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
	/// Window1.xaml の相互作用ロジック
	/// </summary>
	public partial class SettingWindow : Window
	{
		private MainWindow mainWindow;
		public string Query { get; set; }

		public SettingWindow(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			mainWindow.disposeStream();

			InitializeComponent();
			this.DataContext = mainWindow;
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			mainWindow.calcLine();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			mainWindow.calcLine();
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			mainWindow.createStream();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			mainWindow.authorize();
		}
	}

	public class EmptyValidationRule: ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			string str = Convert.ToString(value);

			if (string.IsNullOrWhiteSpace(str))
			{
				return new ValidationResult(false, "検索する文字列を入力してください");
			}
			return ValidationResult.ValidResult;
		}
	}

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
