using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	class InformationBindableExObject : IDisposable {
		public ReactiveProperty<Visibility> ObjectVisibility { get; }
		public ImageObject ThumbSourceObject { get; }
		public ReactiveProperty<ImageSource> ThumbSource { get; }
		public Reactive.MakiMokiCommand<RoutedEventArgs> LoadedCommand { get; } = new();

		public InformationBindableExObject() {
			this.ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
			this.ThumbSource = new ReactiveProperty<ImageSource>(default(ImageSource));
			this.LoadedCommand.Subscribe(x => this.OnLoaded(x));
		}

		public InformationBindableExObject(BindableFutaba futaba) {
			var item = futaba.ResItems.FirstOrDefault();
			if(futaba.Url.IsThreadUrl && (item != null)) {
				ThumbSource = item.LoadBitmapSource()
					.Cast<ImageSource>()
					.ToReactiveProperty();
				ObjectVisibility = ThumbSource
					.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
					.ToReactiveProperty();
			} else {
				ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
				ThumbSource = new ReactiveProperty<ImageSource>(default(ImageSource));
			}
			this.LoadedCommand.Subscribe(x => this.OnLoaded(x));
		}

		public InformationBindableExObject(ImageObject image) {
			if(image != null) {
				ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
				ThumbSourceObject = image;
				ThumbSource = new ReactiveProperty<ImageSource>(image.Image);
			} else {
				ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
				ThumbSourceObject = null;
				ThumbSource = new ReactiveProperty<ImageSource>(default(ImageSource));
			}
			this.LoadedCommand.Subscribe(x => this.OnLoaded(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void OnLoaded(RoutedEventArgs e) { }
	}
}
