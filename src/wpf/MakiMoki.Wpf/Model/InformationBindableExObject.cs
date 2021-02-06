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
	class InformationBindableExObject {
		public ReactiveProperty<Visibility> ObjectVisibility { get; }
		public ReactiveProperty<ImageSource> ThumbSource { get; }

		public InformationBindableExObject() {
			ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
			ThumbSource = new ReactiveProperty<ImageSource>(default(ImageSource));
		}

		public InformationBindableExObject(BindableFutaba futaba) {
			var item = futaba.ResItems.FirstOrDefault();
			if(futaba.Url.IsThreadUrl && (item != null)) {
				ThumbSource = item.ThumbSource
					.Cast<ImageSource>()
					.ToReactiveProperty();
				ObjectVisibility = ThumbSource
					.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
					.ToReactiveProperty();
			} else {
				ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
				ThumbSource = new ReactiveProperty<ImageSource>(default(ImageSource));
			}
		}

		public InformationBindableExObject(ImageSource image) {
			if(image != null) {
				ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
				ThumbSource = new ReactiveProperty<ImageSource>(image);
			} else {
				ObjectVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
				ThumbSource = new ReactiveProperty<ImageSource>(default(ImageSource));
			}
		}
	}
}
