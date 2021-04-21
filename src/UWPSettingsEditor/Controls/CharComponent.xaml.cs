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
    /// Interaction logic for CharComponent.xaml
    /// </summary>
    public partial class CharComponent : IValueDataSet
    {
        private DateTimeOffset _timestamp;
        private char _char;
        private string _charString;
        public string CharString
        {
            get
            {
                return _charString;
            }
            set 
            {
                FlagDataAsValid();
                charBox.Text = ((char)ushort.Parse(value)).ToString();
                _charString = value;
            } 
        }

        public CharComponent()
        {
            DataContext = this;
            InitializeComponent();
            charBox.Text = "A";
            _timestamp = DateTimeOffset.Now;
        }

        public bool IsDataValid { get; private set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] GetValueData()
        {
            return FromChar(charBox.Text[0], _timestamp);
        }

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);
            _char = GetChar(tmp.Value);

            charBox.Text = _char.ToString();
            numberBox.Text = ((ushort)_char).ToString();
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

        private void numberBox_Error(object sender, ValidationErrorEventArgs e)
        {
            FlagDataAsInvalid();
        }

        private void charBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (charBox.Text.Length > 0)
                numberBox.Text = ((int)charBox.Text[0]).ToString();
            else
            {
                numberBox.Text = "";
                FlagDataAsInvalid();
            }
        }
    }

    class UshortRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (ushort.TryParse((string)value, out _))
                return ValidationResult.ValidResult;
            else
                return new ValidationResult(false, "Not a valid ushort number");
        }
    }
}
