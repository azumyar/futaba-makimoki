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
#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

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

		public ReactiveProperty<Visibility> SearchBoxVisibility { get; }
			= new ReactiveProperty<Visibility>(Visibility.Collapsed);
		public ReactiveProperty<Visibility> SearchButtonVisibility { get; }
		public ReactiveProperty<GridLength> SearchColumnWidth { get; }

		public TabItem(Data.FutabaContext f) {
			this.Url = f.Url;
			this.Name = new ReactiveProperty<string>(
				string.IsNullOrWhiteSpace(this.Url.ThreadNo)
					? Config.ConfigLoader.Bord.Bords.Where(x => x.Url == this.Url.BaseUrl).FirstOrDefault()?.Name
						: "No." + this.Url.ThreadNo);
			this.Futaba = new ReactiveProperty<BindableFutaba>(new BindableFutaba(f));
			this.Futaba.Subscribe(x => this.Name.Value = x.Name);
			this.ThumbSource = WpfUtil.ImageUtil.ToThumbProperty(this.Futaba);
			this.ThumbVisibility = this.ThumbSource
				.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();

			SearchButtonVisibility = SearchBoxVisibility
				.Select(x => (x == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible)
				.ToReactiveProperty();
			SearchColumnWidth = SearchBoxVisibility
				.Select(x => (x == Visibility.Visible) ? new GridLength(320, GridUnitType.Star) : new GridLength(0, GridUnitType.Auto))
				.ToReactiveProperty();
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public void ShowSearchBox() {
			SearchBoxVisibility.Value = Visibility.Visible;
		}

		public void HideSearchBox() {
			SearchBoxVisibility.Value = Visibility.Collapsed;
		}
	}
}
