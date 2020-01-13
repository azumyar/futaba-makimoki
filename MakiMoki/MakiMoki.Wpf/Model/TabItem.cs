using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	class TabItem : IFutabaViewerContents, INotifyPropertyChanged, IDisposable {
		public event PropertyChangedEventHandler PropertyChanged;
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();

		public string Id { get; }

		public Data.UrlContext Url { get; }
		public ReactiveProperty<string> Name { get; }
		public ReactiveProperty<ImageSource> ThumbSource { get; }
		public ReactiveProperty<Visibility> ThumbVisibility { get; }
		public ReactiveProperty<BindableFutaba> Futaba { get; }

		public ReactiveProperty<PlatformData.FutabaMedia> MediaContents { get; } 
			= new ReactiveProperty<PlatformData.FutabaMedia>();

		ReactiveProperty<object> IFutabaViewerContents.LastVisibleItem { get; }
			= new ReactiveProperty<object>();
		ReactiveProperty<double> IFutabaViewerContents.ScrollVerticalOffset { get; }
			= new ReactiveProperty<double>(0);
		ReactiveProperty<double> IFutabaViewerContents.ScrollHorizontalOffset { get; }
			= new ReactiveProperty<double>(0);

		public TabItem(Data.FutabaContext f) {
			this.Url = f.Url;
			this.Name = new ReactiveProperty<string>(
				string.IsNullOrWhiteSpace(this.Url.ThreadNo)
					? Config.ConfigLoader.Bord.Where(x => x.Url == this.Url.BaseUrl).FirstOrDefault()?.Name
						: "Nol." + this.Url.ThreadNo);
			this.Futaba = new ReactiveProperty<BindableFutaba>(new BindableFutaba(f));
			this.Futaba.Subscribe(x => this.Name.Value = x.Name);
			this.ThumbSource = WpfUtil.ImageUtil.ToThumbProperty(this.Futaba);
			this.ThumbVisibility = this.ThumbSource
				.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
		}

		public void Dispose() {
			Disposable.Dispose();
		}
	}
}
