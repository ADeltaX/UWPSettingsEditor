using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UWPSettingsEditor.Interfaces;
using static UWPSettingsEditor.UWPDeserializer;
using static UWPSettingsEditor.UWPSerializer;

namespace UWPSettingsEditor.Controls
{
    /// <summary>
    /// Interaction logic for PointRectComponent.xaml
    /// </summary>
    public partial class PointSizeComponent : IValueDataSet
    {
        private DataTypeEnum _dataType;
        private DateTimeOffset _timestamp;
        private string _firstDouble;
        private string _secondDouble;

        public string FirstDouble 
        { 
            get => _firstDouble; 
            set 
            {
                if (!Validation.GetHasError(secondBox))
                    FlagDataAsValid();

                _firstDouble = value;
            }
        }
        public string SecondDouble
        {
            get => _secondDouble;
            set
            {
                if (!Validation.GetHasError(firstBox))
                    FlagDataAsValid();

                _secondDouble = value;
            }
        }

        public PointSizeComponent(DataTypeEnum dataType)
        {
            DataContext = this;
            _dataType = dataType;
            InitializeComponent();
            _timestamp = DateTimeOffset.Now;

            firstBox.Text = "0";
            secondBox.Text = "0";

            if (_dataType == DataTypeEnum.RegUwpPoint)
            {
                firstBlock.Text = "X: ";
                secondBlock.Text = "Y: ";
            }
            else
            {
                firstBlock.Text = "Width: ";
                secondBlock.Text = "Height: ";
            }
        }

        public bool IsDataValid { get; private set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] GetValueData()
        {
            if (_dataType == DataTypeEnum.RegUwpPoint)
            {
                return FromPoint(new Point(double.Parse(_firstDouble), double.Parse(_secondDouble)), _timestamp);
            }
            else
            {
                return FromSize(new Size(double.Parse(_firstDouble), double.Parse(_secondDouble)), _timestamp);
            }
        }

        private void FlagDataAsValid()
        {
            IsDataValid = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDataValid"));
        }

        private void FlagDataAsInvalid()
        {
            IsDataValid = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDataValid"));
        }

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);
            if (_dataType == DataTypeEnum.RegUwpPoint)
            {
                var point = GetPoint(tmp.Value);
                firstBox.Text = point.X.ToString("G17");
                secondBox.Text = point.Y.ToString("G17");
            }
            else
            {
                var size = GetSize(tmp.Value);
                firstBox.Text = size.Width.ToString("G17");
                secondBox.Text = size.Height.ToString("G17");
            }
        }

        private void firstBox_Error(object sender, ValidationErrorEventArgs e)
        {
            FlagDataAsInvalid();
        }

        private void secondBox_Error(object sender, ValidationErrorEventArgs e)
        {
            FlagDataAsInvalid();
        }
    }
    class DoubleRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (double.TryParse((string)value, out _))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "Not a valid double number");
        }
    }
}
