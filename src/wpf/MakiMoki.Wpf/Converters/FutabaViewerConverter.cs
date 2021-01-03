using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	class ViewerElementToFutabaConverter : ReactiveConverter<RoutedEventArgs, Data.FutabaContext> {
		protected override IObservable<Data.FutabaContext> OnConvert(IObservable<RoutedEventArgs> source) {
			return source
				.Select(x => (x.Source as Button)?.DataContext)
				.Cast<Data.FutabaContext>();
		}
	}
}
