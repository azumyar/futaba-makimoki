using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.Converters {
	abstract class PropertyChangedConverter<T> : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, T> {
		protected override IObservable<T> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> source) {
			return source
				.Select(x => x.NewValue)
				.Cast<T>();
		}
	}

	class TreeViewSelectedConverter : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, Data.UrlContext> {
		protected override IObservable<Data.UrlContext> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> source) {
			return source
				.Select(x => x.NewValue)
				.Cast<Model.TreeItem>()
				.Select(x => x.Url);
		}
	}

	class FutabaPropertyChangedConverter : PropertyChangedConverter<Data.FutabaContext> { }
}
