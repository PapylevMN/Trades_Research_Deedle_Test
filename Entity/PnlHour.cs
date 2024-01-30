using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Trades_Research.ViewModels;

namespace Trades_Research.Entity
{
    public class PnlHour : BaseVM
    {
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        bool _isActive = true;

        public int Hour
        {
            get => _hour;
            set
            {
                _hour = value;
                OnPropertyChanged(nameof(Hour));
            }
        }
        int _hour = 0;

        public decimal PnL
        {
            get => _pnL;
            set
            {
                _pnL = value;
                Value = Math.Abs(_pnL);

                if (PnL > 0)
                {
                    Color = Brushes.Green;
                }
                else
                {
                    Color = Brushes.Red;
                }
            }
        }
        decimal _pnL = 0;

        public decimal Value
        {
            get => _value;
            set
            { 
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        decimal _value = 0;

        public decimal Maximum
        { 
            get => _maximum;
            set 
            {
                _maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }
        decimal _maximum = 0;

        public SolidColorBrush Color
        {
            get => _color;
            set
            { 
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        SolidColorBrush _color = Brushes.Transparent;
    }
}
