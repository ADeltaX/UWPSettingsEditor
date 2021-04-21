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
    /// Interaction logic for GuidComponent.xaml
    /// </summary>
    public partial class GuidComponent : IValueDataSet
    {
        private DateTimeOffset _timestamp;
        private Guid _guid;

        private string _guidString;
        public string GuidString
        {
            get
            {
                return _guidString;
            }
            set
            {
                FlagDataAsValid();
                _guidString = value;
            }
        }

        public bool IsDataValid { get; private set; } = true;

        public GuidComponent()
        {
            DataContext = this;
            InitializeComponent();
            guidBox.Text = Guid.NewGuid().ToString();
            _timestamp = DateTimeOffset.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void guidBox_Error(object sender, ValidationErrorEventArgs e)
        {
            FlagDataAsInvalid();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            guidBox.Text = Guid.NewGuid().ToString();
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
            _guid = GetGuid(tmp.Value);

            guidBox.Text = _guid.ToString();
        }

        public byte[] GetValueData() 
            => FromGuid(Guid.Parse(guidBox.Text), _timestamp);
    }

    class GuidValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (Guid.TryParse((string)value, out _))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "Not a valid guid");
        }
    }
}
