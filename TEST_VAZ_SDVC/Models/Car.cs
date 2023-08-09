using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TEST_VAZ_SDVC.Models
{
	internal class Car : DependencyObject, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region prisets
		/// Индекс присета - верхняя граница скорости при достижении которой звук увеличится на 1 дБ.
		static private int acceleratePrisetIndex = 0;
		/// Индекс присета - нижняя граница скорости при достижении которой звук уменьшиться на 1 дБ.
		static private int deceleratePrisetIndex = 0;
		/// Множество значений скоростей для приссетов ускорения
		/// При достижении последнего присета из представленного списка, ставим недостижимую скорость для переключения звука выше.
		static private int[] accelerate = new int[] { 48,			71, 93, 114, 134, 155, 172, 255, int.MaxValue	};
		/// Множество значений скоростей для приссетов замедления
		/// При достижении нижнего присета из представленного списка, ставим недостижимую скорость для переключения звука ниже.
		static private int[] decelerate = new int[] { int.MinValue,	32, 55, 76,  97,  118, 138, 156, 255			};
		#endregion

		#region speed
		private int _speed { get; set; }

		/// <summary>
		/// Отвечает за скорость автомобиля. 
		/// В зависимости от режима (ускорение или замедления), а также значения скорости изменяет volumeSDVC
		/// </summary>
		public int speed
		{
			get { return _speed; }
			set
			{
				int speedDifference = value - _speed;
				if (speedDifference != 0) 
				{
					ChangeVolumeSDVC(value, speedDifference > 0);
					_speed = value;
					OnPropertyChanged(nameof(speed));
				}
			}
		}
		private void ChangeVolumeSDVC(int speed, bool accelerating)
		{
			if (accelerating) 
			{
				if (speed > accelerate[acceleratePrisetIndex]) 
				{
					acceleratePrisetIndex++;
					volumeSDVC = deceleratePrisetIndex = acceleratePrisetIndex;
				}
			}
			else
			{
				if(speed < decelerate[deceleratePrisetIndex]) 
				{
					deceleratePrisetIndex--;
					volumeSDVC = acceleratePrisetIndex = deceleratePrisetIndex;
				}
			}
		}
		#endregion

		#region volumeReg

		#region volumeStepDict
		static private Dictionary<int, int?> volumeStepDict = new Dictionary<int, int?>()
		{
			{0, null},
			{1, -79},
			{2, -71},
			{3, -63},
			{4, -57},
			{5, -53},
			{6, -50},
			{7, -47},
			{8, -45},
			{9, -43},
			{10, -41},
			{11, -39},
			{12, -37},
			{13, -35},
			{14, -34},
			{15, -32},
			{16, -31},
			{17, -30},
			{18, -29},
			{19, -27},
			{20, -26},
			{21, -25},
			{22, -23},
			{23, -22},
			{24, -21},
			{25, -19},
			{26, -18},
			{27, -17},
			{28, -15},
			{29, -14},
			{30, -13},
			{31, -12},
			{32, -10},
			{33, -9},
			{34, -8},
			{35, -6},
			{36, -5},
			{37, -4},
			{38, -2},
			{39, -1},
			{40, 0}
		};
		#endregion
		private int _volumeStep { get; set; }

		/// <summary>
		/// Отвечает за шаг громкости мультимедиа системы
		/// </summary>
		public int volumeStep 
		{
			get { return _volumeStep; }
			set 
			{
				_volumeStep = value;
				volumeGain = volumeStepDict[value];
				volumeSDVC = deceleratePrisetIndex;
				OnPropertyChanged(nameof(volumeStep));
			}
		}

		private int? _volumeGain { get; set; }

		/// <summary>
		/// Отвечает за импенсандт системы ТОЛЬКО от ползунка
		/// </summary>
		private int? volumeGain 
		{ 
			get { return _volumeGain; } 
			set 
			{ 
				_volumeGain = value;
				OnPropertyChanged(nameof(volumeSum));
			} 
		}

		private int _volumeSDVC { get; set; }

		/// <summary>
		/// Отвечает за импенсандт системы от скорости
		/// </summary>
		private int volumeSDVC
		{
			get { return _volumeSDVC; }
			set
			{
				_volumeSDVC = value;
				OnPropertyChanged(nameof(volumeSum));
			}
		}

		/// <summary>
		/// Суммарная скорость = volumeSDVC + volumeGain с ограничением сверху. И значением MUTE
		/// </summary>
		public string volumeSum 
		{
			get 
			{
				if (volumeSDVC + volumeGain > 0) 
				{
					volumeSDVC = Math.Abs(volumeGain ?? 0);
				}
				if (volumeGain == null)
					return "MUTE";
				return (volumeSDVC + volumeGain).ToString() + " дБ"; 
			}
			set { }
		}

		#endregion
		public Car() 
		{
			volumeGain = null;
			volumeStep = 0;
			_volumeSDVC = 0;
			speed = 0;
		}
	}
}
