using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UWPSettingsEditor.Controls
{
	/// <summary>
	/// A WPF DateTimePicker control that allows the user to edit the date with a 
	/// drop-down calendar, edit the date and time with the arrow keys, type a date
	/// by hand, or type a date componentwise. Based on a Visual Basic control by 
	/// Magnus Gudmundsson: http://www.codeproject.com/KB/WPF/wpfDateTimePicker.aspx
	/// </summary>
	/// <remarks>
	/// Improvements over the VB version:
	/// <ul>
	/// <li>Bug fix: doesn't crash if the cursor is at the end of the textbox with 
	///     no selection, and user presses the left arrow key.</li>
	/// <li>Bug fix: when the user clicks one of the punctuation marks in the
	///     textbox, the field to the right is selected (instead of something weird)</li>
	/// <li>Bug fix: user can enter a 4-digit year (in VB, it was cleared after the
	///     second digit)</li>
	/// <li>Bug fix: after typing a single-digit month, day or hour, the left/right
	///     arrow keys will select the adjacent field correctly.</li>
	/// <li>Bug fix: date formats that contain both "dd" and "ddd" are handled 
	///     correctly, e.g. "ddd MM dd, yyyy HH:mm".</li>
	/// <li>Bug fix: any custom DateFormat is now permitted, not just formats that
	///     are understood by Visual Basic CDate(). However, you should not use
	///     one-digit fields in the DateFormat string because the code is designed
	///     for fixed-length dates. For example, you must use "MM" instead of "M"
	///     and "dd" instead of "d". Despite this limitation (which existed in the 
	///     original VB version too), the user is allowed to input single digit 
	///     values.</li>
	/// <li>Shift+Tab now selects the previous field instead of the next field</li>
	/// <li>Left/right arrow keys no longer let the TextBox lose keyboard focus</li>
	/// <li>XAML changed so that the control expands to fill the space it is given,
	///     because IMO the control looks strange if it changes width as the user is
	///     typing.</li>
	/// <li>After typing a new value for a field, the user can type the appropriate
	///     punctuation character to move to the next field. For example, given the
	///     default format "yyyy-MM-dd HH:mm", the user could click the month and
	///     type "2-28" to change the date to Febuary 28. When the user presses the 
	///     "-" key, the day field will become selected automatically. "2/28" is
	///     also accepted.</li>
	/// <li>Free-form editing of the date is now permitted. The user can delete the
	///     entire date and re-enter it, even in an unexpected format. For example,
	///     although the date format may be "yyyy-MM-dd hh:mm tt", the control can 
	///     accept a typed (or pasted) date in a different (standard) format such as 
	///     "August 12, 2001 23:00". If the typed date cannot not be parsed, the
	///     date will revert to the original date (stored internally in
	///     SelectedDate) when the control loses focus.</li>
	/// <li>Added up and down arrows (WPF Adorners) that appear when you select a 
	///     field of the date or time, allowing you to change the date or time 
	///     incrementally with the mouse. This feature will not work automatically
	///     in all windows; you may need to wrap the DateTimePicker, or the entire
	///     contents of your window, in an &lt;AdornerDecorator> object to allow
	///     the arrows to appear. See
	///     http://stackoverflow.com/questions/13389772/how-to-draw-wpf-adorners-on-top-of-everything-else
	/// </li></ul>
	/// Note: I removed support for "null" as a date value because I didn't need
	/// it for my application, so I didn't want to take responsibility for ensuring 
	/// that it works correctly.
	/// <para/>
	/// The control switches to free-form edit mode when the user types something 
	/// that makes the date invalid, such as an unexpected character or an invalid
	/// month number. The control reverts to normal "assisted" editing when the
	/// date becomes valid again, or when the control loses focus.
	/// <para/>
	/// A pleasant side-effect of the free-form input logic is that the user can now 
	/// type non-numeric fields. For example, if the date format contains a MMM
	/// field, the user can select it and type "jul" to change the month to July.
	/// Editing with up/down arrow keys is also supported for the MMM and ddd fields,
	/// but not the t or tt fields (AM/PM).
	/// <para/>
	/// Although the date format can contain both "dd" and "ddd", please note that
	/// this currently thwarts user editing if they attempt to type any component
	/// (instead of using the arrow keys or calendar). The reason is that you have 
	/// to update the day-of-month and day-of-week at the same time, otherwise 
	/// parsing will tend to fail. For example, suppose the date is currently 
	/// 2011-08-16 (Tue). If the user selects the day and types "19", DateTimePicker 
	/// refuses to accept the new day because August 19, 2011 is not a Tuesday. 
	/// Consequently, parsing fails unless the user manually changes "Tue" to "Fri" 
	/// or deletes "(Tue)" from the end.
	/// <para/>
	/// License: The Code Project Open License (CPOL)
	/// http://www.codeproject.com/info/cpol10.aspx
	/// <para/>
	/// [2012-11] Changed (1) to change the SelectedDate immediately when text is 
	/// typed instead of waiting for the text box to lose focus, and (2) not to 
	/// update DateDisplay.Text when the SelectedDate changes and the text box has 
	/// the focus. Instead, a trigger changes TextBox.Background to gray to indicate 
	/// the discrepancy.
	/// [2012-11] Added up-down arrows
	/// </remarks>
	public partial class DateTimePicker : UserControl
	{
		private const int FormatLengthOfLast = 2;
		private enum Direction : int
		{
			Previous = -1,
			Next = 1
		}
		TextBoxUpDownAdorner _upDownButtons;

		public DateTimePicker()
		{
			InitializeComponent();
			CalDisplay.SelectedDatesChanged += CalDisplay_SelectedDatesChanged;
			DateDisplay.PreviewMouseUp += DateDisplay_PreviewMouseUp;
			DateDisplay.LostFocus += DateDisplay_LostFocus;
			DateDisplay.PreviewKeyDown += DateTimePicker_PreviewKeyDown;
			DateDisplay.TextChanged += new TextChangedEventHandler(DateDisplay_TextChanged);

			this.Loaded += (s, e) =>
			{
				AdornerLayer adLayer = GetAdornerLayer(DateDisplay);
				if (adLayer != null)
				{
					adLayer.Add(_upDownButtons = new TextBoxUpDownAdorner(DateDisplay));
					_upDownButtons.Click += (textBox, direction) => { OnUpDown(direction); };
				}
			};
		}

		static AdornerLayer GetAdornerLayer(FrameworkElement subject)
		{
			AdornerLayer layer = null;
			do
			{
				if ((layer = AdornerLayer.GetAdornerLayer(subject)) != null)
					break;
			} while ((subject = subject.Parent as FrameworkElement) != null);
			return layer;
		}

		#region "Properties"

		public DateTime SelectedDate
		{
			get { return (DateTime)GetValue(SelectedDateProperty); }
			set { SetValue(SelectedDateProperty, value); }
		}

		public string DateFormat
		{
			get { return Convert.ToString(GetValue(DateFormatProperty)); }
			set { SetValue(DateFormatProperty, value); }
		}

		public bool ShowCalendarButton
		{
			get { return PopUpCalendarButton.Visibility == Visibility.Visible; }
			set { PopUpCalendarButton.Visibility = (value ? Visibility.Visible : Visibility.Collapsed); }
		}

		public string _inputDateFormat;
		public string InputDateFormat()
		{
			if (_inputDateFormat == null)
			{
				string df = DateFormat;
				if (!df.Contains("MMM"))
					df = df.Replace("MM", "M");
				if (!df.Contains("ddd"))
					df = df.Replace("dd", "d");
				// Note: do not replace Replace("tt", "t") because a single "t" will not accept "AM" or "PM".
				_inputDateFormat = df.Replace("hh", "h").Replace("HH", "H").Replace("mm", "m").Replace("ss", "s");
			}
			return _inputDateFormat;
		}

		public DateTime MinimumDate
		{
			get { return Convert.ToDateTime(GetValue(MinimumDateProperty)); }
			set { SetValue(MinimumDateProperty, value); }
		}

		public DateTime MaximumDate
		{
			get { return Convert.ToDateTime(GetValue(MaximumDateProperty)); }
			set { SetValue(MaximumDateProperty, value); }
		}

		#endregion

		#region "Events"

		public event RoutedEventHandler DateChanged
		{
			add { AddHandler(DateChangedEvent, value); }
			remove { RemoveHandler(DateChangedEvent, value); }
		}

		public static readonly RoutedEvent DateChangedEvent = EventManager.RegisterRoutedEvent("DateChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DateTimePicker));

		public event RoutedEventHandler DateFormatChanged
		{
			add { this.AddHandler(DateFormatChangedEvent, value); }
			remove { this.RemoveHandler(DateFormatChangedEvent, value); }
		}

		public static readonly RoutedEvent DateFormatChangedEvent = EventManager.RegisterRoutedEvent("DateFormatChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DateTimePicker));

		#endregion

		#region "DependencyProperties"

		public static readonly DependencyProperty DateFormatProperty = DependencyProperty.Register("DateFormat", typeof(string), typeof(DateTimePicker), new FrameworkPropertyMetadata("yyyy-MM-dd HH:mm:ss", OnDateFormatChanged));

		public static DependencyProperty MaximumDateProperty = DependencyProperty.Register("MaximumDate", typeof(DateTime), typeof(DateTimePicker), new FrameworkPropertyMetadata(Convert.ToDateTime("3000-01-01 00:00"), null, new CoerceValueCallback(CoerceMaxDate)));

		public static DependencyProperty MinimumDateProperty = DependencyProperty.Register("MinimumDate", typeof(DateTime), typeof(DateTimePicker), new FrameworkPropertyMetadata(Convert.ToDateTime("1900-01-01 00:00"), null, new CoerceValueCallback(CoerceMinDate)));

		public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register("SelectedDate",
			typeof(DateTime), typeof(DateTimePicker),
			new FrameworkPropertyMetadata(DateTime.Now,
				new PropertyChangedCallback(OnSelectedDateChanged),
				new CoerceValueCallback(CoerceDate)));

		/// <summary>true when user is busy editing DateDisplay and the SelectedDate
		/// becomes different from the date shown on the text box.</summary>
		public static readonly DependencyProperty DateTextIsWrongProperty = DependencyProperty.Register("DateTextIsWrong", typeof(bool), typeof(DateTimePicker), new FrameworkPropertyMetadata(false));

		protected bool DateTextIsWrong
		{
			get { return (bool)GetValue(DateTextIsWrongProperty); }
			set { SetValue(DateTextIsWrongProperty, value); }
		}

		#endregion

		#region "EventHandlers"

		bool _forceTextUpdateNow = true;

		private void CalDisplay_SelectedDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			PopUpCalendarButton.IsChecked = false;
            TimeSpan timeOfDay = SelectedDate.TimeOfDay;
            SelectedDate = CalDisplay.SelectedDate.Value.Date + timeOfDay;
		}

		private void DateDisplay_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (DateDisplay.SelectionLength == 0)
				FocusOnDatePart(DateDisplay.SelectionStart);
		}

		bool IsDateInExpectedFormat()
		{
			return ParseDateText(false) != null;
		}
		DateTime? ParseDateText(bool flexible)
		{
			DateTime selectedDate;

			if (!DateTime.TryParseExact(DateDisplay.Text, InputDateFormat(), null, DateTimeStyles.AllowWhiteSpaces, out selectedDate))
				if (!flexible || !DateTime.TryParse(DateDisplay.Text, out selectedDate))
					return null;
			return selectedDate;
		}

		void ReformatDateText()
		{
			// Changes DateDisplay.Text to match the current DateFormat
			DateTime? date = ParseDateText(true);
			if (date != null)
			{
				string newText = date.Value.ToString(DateFormat);
				if (DateDisplay.Text != newText)
					DateDisplay.Text = newText;
			}
		}

		private void DateDisplay_LostFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			DateDisplay.Text = SelectedDate.ToString(DateFormat);
			// When the user selects a field again, then the box loses focus, then
			// the user clicks the same field again, the selection is cleared, 
			// causing the arrows not to appear. To fix, clear selection in advance.
			try
			{
				DateDisplay.SelectionLength = 0;
			}
			catch (NullReferenceException)
			{
				// Occurs during shutdown. Bug in WPF? Ain't documented, that's for sure.
			}
		}
		void DateDisplay_TextChanged(object sender, TextChangedEventArgs e)
		{
			DateTime? date = ParseDateText(true);
			if (date != null)
				SelectedDate = date.Value;
		}

		private void DateTimePicker_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			int selstart = DateDisplay.SelectionStart;

			if (!IsDateInExpectedFormat())
				return;

			switch (e.Key)
			{
				case Key.Up:
					OnUpDown(+1);
					break;
				case Key.Down:
					OnUpDown(-1);
					break;
				case Key.Left:
					if (Keyboard.Modifiers != ModifierKeys.None)
						return;
					SelectPosition(selstart, Direction.Previous);
					break;
				case Key.Right:
					if (Keyboard.Modifiers != ModifierKeys.None)
						return;
					SelectPosition(selstart, Direction.Next);
					break;
				case Key.Tab:
					var dir = Direction.Next;
					if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
						dir = Direction.Previous;
					e.Handled = SelectPosition(selstart, dir);
					break;
				default:
					char nextChar = '\0';
					if (selstart < DateDisplay.Text.Length)
						nextChar = DateDisplay.Text[selstart];

					if ((e.Key == Key.OemMinus || e.Key == Key.Subtract || e.Key == Key.OemQuestion || e.Key == Key.Divide) &&
						(nextChar == '/' || nextChar == '-') ||
						e.Key == Key.Space && nextChar == ' ' ||
						e.Key == Key.OemSemicolon && nextChar == ':')
						SelectPosition(selstart, Direction.Next);
					else
						return;
					break;
			}
			e.Handled = true;
		}

		private void OnUpDown(int increment)
		{
			int selstart = DateDisplay.SelectionStart;
			_forceTextUpdateNow = true;
			SelectedDate = Increase(selstart, increment);
			FocusOnDatePart(selstart);
		}

		private static object CoerceDate(DependencyObject d, object value)
		{
			DateTimePicker me = (DateTimePicker)d;
			DateTime current = Convert.ToDateTime(value);
			if (current < me.MinimumDate)
				current = me.MinimumDate;
			if (current > me.MaximumDate)
				current = me.MaximumDate;
			return current;
		}

		private static object CoerceMinDate(DependencyObject d, object value)
		{
			DateTimePicker me = (DateTimePicker)d;
			DateTime current = Convert.ToDateTime(value);
			if (current >= me.MaximumDate)
				throw new ArgumentException("MinimumDate can not be equal to, or more than maximum date");

			if (current > me.SelectedDate)
				me.SelectedDate = current;

			return current;
		}

		private static object CoerceMaxDate(DependencyObject d, object value)
		{
			DateTimePicker me = (DateTimePicker)d;
			DateTime current = Convert.ToDateTime(value);
			if (current <= me.MinimumDate)
				throw new ArgumentException("MaximimumDate can not be equal to, or less than MinimumDate");

			if (current < me.SelectedDate)
				me.SelectedDate = current;

			return current;
		}

		public static void OnDateFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			var me = (DateTimePicker)obj;
			me._inputDateFormat = null; // will be recomputed on-demand
			me.DateDisplay.Text = me.SelectedDate.ToString(me.DateFormat);
		}

		public static void OnSelectedDateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			var me = (DateTimePicker)obj;

			var date = (DateTime)args.NewValue;
			me.CalDisplay.SelectedDate = date;
			me.CalDisplay.DisplayDate = date;
			if (me.DateDisplay.IsFocused && !me._forceTextUpdateNow)
			{
				DateTime? oldDate = me.ParseDateText(true);
				if (oldDate != null)
					me.DateTextIsWrong = date != oldDate.Value;
			}
			else
			{
				me.DateTextIsWrong = false;
				me._forceTextUpdateNow = false;
				me.DateDisplay.Text = date.ToString(me.DateFormat);
			}
		}

		#endregion

		// Selects next or previous date value, depending on the incrementor value  
		// Alternatively moves focus to previous control or the calender button
		private bool SelectPosition(int selstart, Direction direction)
		{
			selstart = CalcPosition(selstart, direction);
			if (selstart > -1)
			{
				return FocusOnDatePart(selstart);
			}
			else
				return false;
		}

		static char At(string s, int index)
		{
			if ((uint)index < (uint)s.Length)
				return s[index];
			return '\0';
		}

		// Gets location of next/previous date field, depending on the incrementor value.
		// Returns -1 if there is no next/previous field.
		private int CalcPosition(int selStart, Direction direction)
		{
			string df = DateFormat;
			if (selStart >= df.Length)
				selStart = df.Length - 1;
			char startChar = df[selStart];
			int i = selStart;

			for (; ; )
			{
				i += (int)direction;
				if ((uint)i >= (uint)df.Length)
					return -1;
				if (df[i] == startChar)
					continue;
				if (char.IsLetter(df[i]))
					break;
				startChar = '\0'; // to handle cases like "yyyy-MM-dd (ddd)" correctly
			}

			if (direction < 0)
				// move to the beginning of the field
				while (i > 0 && df[i - 1] == df[i])
					i--;

			return i;
		}

		private bool FocusOnDatePart(int selStart)
		{
			ReformatDateText();

			// Find beginning of field to select
			string df = DateFormat;
			if (selStart > df.Length - 1)
				selStart = df.Length - 1;
			char firstchar = df[selStart];
			while (!char.IsLetter(firstchar) && selStart + 1 < df.Length)
			{
				selStart++;
				firstchar = df[selStart];
			}
			while (selStart > 0 && df[selStart - 1] == firstchar)
				selStart--;

			int selLength = 1;
			while (selStart + selLength < df.Length && df[selStart + selLength] == firstchar)
				selLength++;

			// don't select AM/PM: we have no interface to change it.
			if (firstchar == 't')
				return false;

			DateDisplay.Focus();
			DateDisplay.Select(selStart, selLength);
			return true;
		}

		private DateTime Increase(int selstart, int value)
		{
			DateTime retval = (ParseDateText(false) ?? SelectedDate);

			try
			{
				switch (DateFormat.Substring(selstart, 1))
				{
					case "h":
					case "H":
						retval = retval.AddHours(value);
						break;
					case "y":
						retval = retval.AddYears(value);
						break;
					case "M":
						retval = retval.AddMonths(value);
						break;
					case "m":
						retval = retval.AddMinutes(value);
						break;
					case "d":
						retval = retval.AddDays(value);
						break;
					case "s":
						retval = retval.AddSeconds(value);
						break;
				}
			}
			catch (ArgumentException ex)
			{
				//Catch dates with year over 9999 etc, dont throw
			}

			return retval;
		}
	}


	// Adorners must subclass the abstract base class Adorner. 
	public class TextBoxUpDownAdorner : Adorner
	{
		StreamGeometry _triangle = new StreamGeometry();
		bool _shown;
		double _x, _top, _bottom;
		public Pen Outline = new Pen(new SolidColorBrush(Color.FromArgb(255, 00, 00, 00)), 2);
		public Brush Fill = Brushes.White;

		private int tickingCount = 0;
		private int tickingSpeed = 1000;
		private DispatcherTimer dispatcherTimer;
		private MouseButtonEventArgs mouseButtonEventArgs;


		public TextBoxUpDownAdorner(TextBox adornedBox) : base(adornedBox)
		{
			_triangle = new StreamGeometry();
			_triangle.FillRule = FillRule.Nonzero;
			using (StreamGeometryContext c = _triangle.Open())
			{
				c.BeginFigure(new System.Windows.Point(-10, 0), true /* filled */, true /* closed */);
				c.LineTo(new System.Windows.Point(10, 0), true, false);
				c.LineTo(new System.Windows.Point(0, 15), true, false);
			}
			_triangle.Freeze();

			dispatcherTimer = new DispatcherTimer();
			dispatcherTimer.Tick += (s, e) => TickClick();

			MouseUp += (s, e) =>
			{
				dispatcherTimer.Stop();
			};

			MouseLeave += (s, e) =>
			{
				dispatcherTimer.Stop();
			};

			MouseDown += (s, e) => 
			{
				tickingCount = 0;
				tickingSpeed = 500;
				mouseButtonEventArgs = e;
				TickClick();
				dispatcherTimer.Interval = TimeSpan.FromMilliseconds(tickingSpeed);
				dispatcherTimer.Start();
			};

			adornedBox.LostFocus += RelevantEventOccurred;
			adornedBox.SelectionChanged += RelevantEventOccurred;
		}

		void TickClick()
        {
			tickingCount++;

			if (tickingCount == 1)
				tickingSpeed = 125;
			else if (tickingCount == 3)
				tickingSpeed = 60;
			else if (tickingCount == 6)
				tickingSpeed = 40;
			else if (tickingCount == 10)
				tickingSpeed = 16;

			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(tickingSpeed);

			if (Click != null)
			{
				bool up = mouseButtonEventArgs.GetPosition(AdornedElement).Y < (_top + _bottom) / 2;
				Click((TextBox)AdornedElement, up ? 1 : -1);
			}
		}

		void RelevantEventOccurred(object sender, RoutedEventArgs e)
		{
			// In OnRender, GetRectFromCharacterIndex may return Infinity values,
			// so measure the location of the selection here instead.
			var box = AdornedElement as TextBox;
			if (box.IsFocused)
			{
				int start = box.SelectionStart, len = box.SelectionLength;
				if (_shown = len > 0)
				{
					var rect1 = box.GetRectFromCharacterIndex(start);
					var rect2 = box.GetRectFromCharacterIndex(start + len);
					_top = rect1.Top - 2;
					_bottom = rect1.Bottom + 2;
					_x = (rect1.Left + rect2.Left) / 2;
				}
			}
			else
				_shown = false;

			InvalidateVisual();
		}

		public event Action<TextBox, int> Click;

		// A common way to implement an adorner's rendering behavior is to override the OnRender 
		// method, which is called by the layout system as part of a rendering pass. 
		protected override void OnRender(DrawingContext drawingContext)
		{
			if (_shown)
			{
				drawingContext.PushTransform(new TranslateTransform(_x, _top));
				drawingContext.PushTransform(new ScaleTransform(1, -1));
				drawingContext.DrawGeometry(Fill, Outline, _triangle);
				drawingContext.Pop();
				drawingContext.Pop();
				drawingContext.PushTransform(new TranslateTransform(_x, _bottom));
				drawingContext.DrawGeometry(Fill, Outline, _triangle);
				drawingContext.Pop();
			}
		}
	}

	public class BoolInverter : MarkupExtension, IValueConverter
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{ return this; }
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{ return !(bool)value; }
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{ return !(bool)value; }
	}
}
