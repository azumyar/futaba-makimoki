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

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	/// <summary>
	/// ThreadRegionContainer.xaml の相互作用ロジック
	/// </summary>
	partial class FutabaThreadRegionContainer : UserControl {
		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(Model.IFutabaViewerContents),
				typeof(FutabaThreadRegionContainer),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPropertyChangedEventHandler<Model.IFutabaViewerContents>),
				typeof(FutabaThreadRegionContainer));

		public Model.IFutabaViewerContents Contents {
			get => (Model.IFutabaViewerContents)this.GetValue(ContentsProperty);
			set {
				this.SetValue(ContentsProperty, value);
			}
		}

		public event RoutedPropertyChangedEventHandler<Model.IFutabaViewerContents> ContentsChanged {
			add { AddHandler(ContentsChangedEvent, value); }
			remove { RemoveHandler(ContentsChangedEvent, value); }
		}

		private bool isFirstContents = false;
		public FutabaThreadRegionContainer() {
			InitializeComponent();
			this.Unloaded += (s, e) => {
				System.Diagnostics.Debug.WriteLine("Unloaded");
			};
		}

		private static async void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is FutabaThreadRegionContainer el) {
				if(!el.isFirstContents) {
					await Task.Delay(1); // 一発目のイベント発火の時まだCommandにバインドされてないので一度遅らせる
					el.isFirstContents = true;
				}
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>(
					e.OldValue as Model.IFutabaViewerContents,
					e.NewValue as Model.IFutabaViewerContents,
					ContentsChangedEvent));
			}
		}
	}
}
