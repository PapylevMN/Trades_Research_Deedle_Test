using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Trades_Research.Commands;
using Trades_Research.Entity;
using Deedle;

namespace Trades_Research.ViewModels
{
    public class VM : BaseVM
    {
        public VM()
        {
            WeekHours = Init();
        }

        #region Fields===========================================
        /// <summary>
        /// Список всех сделок (время - прибыль)
        /// </summary>

        Frame<int, string> df; // Объявляем стурктуру данных df типа Frame

        public List<List<PnlHour>> WeekHours { get; set; } = new List<List<PnlHour>>();

        decimal _max = 0;

        IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };

        #endregion

        #region Properties =======================================

        public decimal Depo
        {
            get => _depo;
            set
            {
                _depo = value;
                OnPropertyChanged(nameof(Depo));
            }
        }
        private decimal _depo = 100000;

        public decimal SummEq
        {
            get => _summEq;
            set
            {
                _summEq = value;
                OnPropertyChanged(nameof(SummEq));
            }
        }
        decimal _summEq = 0;

        public decimal SummEqFilter
        {
            get => _summEqFilter;
            set
            {
                _summEqFilter = value;
                OnPropertyChanged(nameof(SummEqFilter));
            }
        }
        decimal _summEqFilter = 0;

        public DateTime CancelSelectedDate
        {
            get => _cancelSelectedDate;

            set
            {
                _cancelSelectedDate = value;
                OnPropertyChanged(nameof(CancelSelectedDate));
            }
        }
        DateTime _cancelSelectedDate = new DateTime(2024, 1, 1);
        #endregion

        #region Commands============================================

        private DelegateCommand commandCalculate;

        public ICommand CommandCalculate
        {
            get
            {
                if (commandCalculate == null)
                {
                    commandCalculate = new DelegateCommand(Calculate);
                }
                return commandCalculate;
            }
        }

        private DelegateCommand commandCalculateFilter;

        public ICommand CommandCalculateFilter
        {
            get
            {
                if (commandCalculateFilter == null)
                {
                    commandCalculateFilter = new DelegateCommand(CalculateFilter);
                }
                return commandCalculateFilter;
            }
        }

        private DelegateCommand commandLoadCSV;

        public ICommand CommandLoadCSV
        {
            get
            {
                if (commandLoadCSV == null)
                {
                    commandLoadCSV = new DelegateCommand(LoadCSV);
                }
                return commandLoadCSV;
            }

        }

        #endregion

        #region Methods===================================================

        private List<List<PnlHour>> Init()
        {
            List<List<PnlHour>> pnlWeek = new List<List<PnlHour>>();

            for (int i = 0; i < 7; i++)
            {
                List<PnlHour> pnlDay = new List<PnlHour>();

                for (int x = 0; x < 24; x++)
                {
                    PnlHour pnlHour = new PnlHour()
                    { 
                        Hour = x
                    };
  
                    pnlDay.Add(pnlHour);
                }

                pnlWeek.Add(pnlDay);
            }
            return pnlWeek;
        }

        private void Calculate(object obj)
        {
            if (df == null)
            {
                MessageBox.Show(" Загрузите файл !");
                return;
            }

            SummEq = Convert.ToDecimal(df.GetColumn<decimal>("24").Sum());

            CalcWeekHours();
        }

        private void CalculateFilter(object obj)
        {
            decimal summEq = 0;

            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    // Фильтруем датафрейм по активным дням и часам
                    if (WeekHours[day][hour].IsActive)
                    {
                        var df_hour_distributed = df.Where(r => ((int)r.Value.TryGetAs<DateTime>("9").ValueOrDefault.DayOfWeek == day) &&
                                                                    ((int)r.Value.TryGetAs<DateTime>("10").ValueOrDefault.Hour == hour));

                        decimal result = (df_hour_distributed.RowCount == 0) ? 0 : Convert.ToDecimal(df_hour_distributed.GetColumn<double>("24").Sum());
                        summEq += result;
                    }
                    else continue;
                }
            }

            SummEqFilter = summEq;
        }

        private void CalcWeekHours()
        {
            // Фильтруем датафрейм до выбранной даты

            var df_filtered_date = df.Where(r => r.Value.TryGetAs<DateTime>("9").ValueOrDefault > CancelSelectedDate);

            // - Фильтруем датафрейм ограниченный выбранной датой по дню недели и часу
            // - Считаем накопленную сумму прибыли/убытка по всем сделкам за выбранный день и час

            for (int day = 0; day < 7; day++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var df_hour_distributed = df_filtered_date.Where(r => ((int)r.Value.TryGetAs<DateTime>("9").ValueOrDefault.DayOfWeek == day) &&
                                                                    ((int)r.Value.TryGetAs<DateTime>("10").ValueOrDefault.Hour == hour));
                    
                    WeekHours[day][hour].PnL = (df_hour_distributed.RowCount == 0) ? 0 : Convert.ToDecimal(df_hour_distributed.GetColumn<double>("24").Sum());

                    WeekHours[day][hour].Maximum = _max;
                }
            }
            
            OnPropertyChanged(nameof(WeekHours));
        }

        private void LoadCSV(object obj)
        {
            decimal _min = 0;
            
            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == true)
            { 
                string filename = fileDialog.FileName;

                if (!System.IO.File.Exists(filename))
                {
                    return;
                }

                df = Frame.ReadCsv(filename, hasHeaders: true, inferTypes: true, separators: ";"); // Считываем scv файл в датафрейм df

                _max = Convert.ToDecimal(df.GetColumn<double>("24").Max());
                _min = Convert.ToDecimal(df.GetColumn<double>("24").Min());
                if (_max < Math.Abs(_min)) { _max = Math.Abs(_min); }
            }
        }
        #endregion
    }
}
