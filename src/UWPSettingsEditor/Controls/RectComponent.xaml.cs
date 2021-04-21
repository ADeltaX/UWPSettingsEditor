using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for RectComponent.xaml
    /// </summary>
    public partial class RectComponent : IValueDataSet
    {
        private DateTimeOffset _timestamp;
        private string _xDouble;
        private string _yDouble;
        private string _widthDouble;
        private string _heightDouble;

        public RectComponent()
        {
            DataContext = this;
            InitializeComponent();
            _timestamp = DateTimeOffset.Now;
            xBox.Text = "0";
            yBox.Text = "0";
            widthBox.Text = "0";
            heightBox.Text = "0";
        }

        public bool IsDataValid { get; private set; } = true;

        public string XDouble
        {
            get { return _xDouble; }
            set
            {
                if (IsAllValid())
                    FlagDataAsValid();

                _xDouble = value;
            }
        }

        public string YDouble
        {
            get { return _yDouble; }
            set
            {
                if (IsAllValid())
                    FlagDataAsValid();

                _yDouble = value;
            }
        }
        public string WidthDouble
        {
            get { return _widthDouble; }
            set
            {
                if (IsAllValid())
                    FlagDataAsValid();

                _widthDouble = value;
            }
        }
        public string HeightDouble
        {
            get { return _heightDouble; }
            set
            {
                if (IsAllValid())
                    FlagDataAsValid();

                _heightDouble = value;
            }
        }

        private bool IsAllValid() 
            => !Validation.GetHasError(xBox) && !Validation.GetHasError(yBox) 
            && !Validation.GetHasError(widthBox) && !Validation.GetHasError(heightBox);

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

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] GetValueData() 
            => FromRect(new Rect(double.Parse(_xDouble), double.Parse(_yDouble), double.Parse(_widthDouble), double.Parse(_heightDouble)), _timestamp);

        public void SetValueData(byte[] dataRaw)
        {
            var tmp = MethodHelpers.SplitDataRaw(dataRaw);

            _timestamp = GetDateTimeOffset(tmp.Key);
            var rect = GetRect(tmp.Value);
            xBox.Text = rect.X.ToString("G17");
            yBox.Text = rect.Y.ToString("G17");
            widthBox.Text = rect.Width.ToString("G17");
            heightBox.Text = rect.Height.ToString("G17");
        }

        private void Valid_Error(object sender, ValidationErrorEventArgs e)
        {
            FlagDataAsInvalid();
        }
    }
}
