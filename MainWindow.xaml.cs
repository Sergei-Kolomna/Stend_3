//проект работает в связке с test_3.pro
using System;
using System.Collections.Generic;
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
using Modbus.Device;
using System.Net.Sockets;
using System.ComponentModel;
using System.Dynamic;
using System.Windows.Threading;
using System.Threading.Tasks;


namespace Stend_3
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	/// 

	public partial class MainWindow : Window
	{
		private double a = 50;
		private ushort b = 50;

		public MainWindow()
		{
			InitializeComponent();
			TagSource = new Depoller(Dispatcher);
		}

		ModbusIpMaster mbMaster;
		Task masterLoop;

		private void Update()
		{
			while (true)
			{
				TagSource.Input(mbMaster.ReadHoldingRegisters(1, 0, 2));  //последнее чисол - количество считываемых регистров
			}
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			mbMaster = ModbusIpMaster.CreateIp(new TcpClient("192.168.0.30", 502));
			masterLoop = Task.Factory.StartNew(new Action(Update));
		}

		public Depoller TagSource
		{
			get;
			private set;
		}
		//----------------------------------------------------------------------------------------
		private void Connect_but_Click(object sender, RoutedEventArgs e)
		{
			mbMaster.WriteSingleRegister(1, 0, 0);
		}
		
		private void Pusk_Click(object sender, RoutedEventArgs e)
		{
			a = MySlider.Value;
			b = (ushort)a;
			mbMaster.WriteSingleRegister(1, 0, b);
		}

		private void progressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{

		}
		
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			a = 50;
		}
	}
}
