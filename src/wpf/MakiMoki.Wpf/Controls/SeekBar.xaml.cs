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
using System.Xml.Linq;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls;
/// <summary>
/// SeekBar.xaml の相互作用ロジック
/// </summary>
public partial class SeekBar : UserControl {
	public static readonly DependencyProperty ValueProperty
		= DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(SeekBar),
			new PropertyMetadata(0d, new PropertyChangedCallback(OnPropertyChangedCallback)));
	public static readonly DependencyProperty SeekLeftProperty
		= DependencyProperty.Register(
			nameof(SeekLeft),
			typeof(GridLength),
			typeof(SeekBar),
			new PropertyMetadata(new GridLength(0d, GridUnitType.Star)));
	public static readonly DependencyProperty SeekRightProperty
		= DependencyProperty.Register(
			nameof(SeekRight),
			typeof(GridLength),
			typeof(SeekBar),
			new PropertyMetadata(new GridLength(1d, GridUnitType.Star)));

	public static readonly DependencyProperty SeekHeightProperty
		= DependencyProperty.Register(
			nameof(SeekHeight),
			typeof(double),
			typeof(SeekBar),
			new PropertyMetadata(4d));
	public static readonly DependencyProperty SeekLeftColorProperty
		= DependencyProperty.Register(
			nameof(SeekLeftColor),
			typeof(Brush),
			typeof(SeekBar),
			new PropertyMetadata(Brushes.White));
	public static readonly DependencyProperty SeekRightColorProperty
		= DependencyProperty.Register(
			nameof(SeekRightColor),
			typeof(Brush),
			typeof(SeekBar),
			new PropertyMetadata(Brushes.Transparent));
	public static readonly RoutedEvent ValueChangedEvent
		= EventManager.RegisterRoutedEvent(
			nameof(ValueChanged),
			RoutingStrategy.Tunnel,
			typeof(RoutedPropertyChangedEventHandler<double>),
			typeof(SeekBar));

	public GridLength SeekLeft {
		get { return (GridLength)this.GetValue(SeekLeftProperty); }
		set { this.SetValue(SeekLeftProperty, value); }
	}
	public GridLength SeekRight {
		get { return (GridLength)this.GetValue(SeekRightProperty); }
		set { this.SetValue(SeekRightProperty, value); }
	}

	public double SeekHeight {
		get { return (double)this.GetValue(SeekHeightProperty); }
		set { this.SetValue(SeekHeightProperty, value); }
	}

	public Brush SeekLeftColor {
		get { return (Brush)this.GetValue(SeekLeftColorProperty); }
		set { this.SetValue(SeekLeftColorProperty, value); }
	}

	public Brush SeekRightColor {
		get { return (Brush)this.GetValue(SeekRightColorProperty); }
		set { this.SetValue(SeekRightColorProperty, value); }
	}

	public double Value {
		get { return (double)this.GetValue(ValueProperty); }
		set {
			var v = Math.Min(value, 1);
			this.SetValue(ValueProperty, v);
		}
	}

	public event RoutedPropertyChangedEventHandler<double> ValueChanged {
		add { AddHandler(ValueChangedEvent, value); }
		remove { RemoveHandler(ValueChangedEvent, value); }
	}

	public SeekBar() {
		InitializeComponent();
	}

	protected override void OnMouseDown(MouseButtonEventArgs e) {
		if(this.IsMouseCaptured && (e.ChangedButton != MouseButton.Left)) {
			this.ReleaseMouseCapture();
		}　else if(e.ChangedButton == MouseButton.Left) {
			this.CaptureMouse();
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e) {
		if(this.IsMouseCaptured) {
			var p = e.GetPosition(this);
			var @new = p.X / this.ActualWidth;
			this.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
				this.Value,
				@new,
				ValueChangedEvent));
			this.Value = @new;
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseButtonEventArgs e) {
		if(this.IsMouseCaptured) {
			this.ReleaseMouseCapture();
		}
		base.OnMouseUp(e);
	}

	private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if(d is SeekBar el) {
			var v = (double)e.NewValue;
			el.SeekLeft = new GridLength(v, GridUnitType.Star);
			el.SeekRight = new GridLength(1d - v, GridUnitType.Star);
		}
	}
}

