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
		public ReactiveProperty<PostHolder> PostData { get; }

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

		public ReactiveProperty<Prism.Regions.IRegion> Region { get; }

		public ReactiveProperty<object> ThreadView { get; }
		public ReactiveProperty<int> LastRescount { get; }
		private bool isActivated = false;
		private IFutabaContainer container = null;

#pragma warning disable IDE0052
		// AutoDisposableで使用する
		private IDisposable SubscribeFutaba { get; }
#pragma warning restore IDE0052

		public TabItem(Data.FutabaContext f) {
			this.Url = f.Url;
			this.ThumbSource = new ReactiveProperty<ImageSource>();
			this.Region = new ReactiveProperty<Prism.Regions.IRegion>();
			this.ThreadView = new ReactiveProperty<object>();
			this.Futaba = new ReactiveProperty<BindableFutaba>(new BindableFutaba(f));
			this.LastRescount = new ReactiveProperty<int>(this.Futaba.Value.ResCount.Value);
			this.PostData = new ReactiveProperty<PostHolder>(new PostHolder(f.Bord, f.Url));
			this.Name = this.Futaba
				.Select(x => {
					if(this.Url.IsCatalogUrl) {
						return Config.ConfigLoader.Board.Boards
							.Where(x => x.Url == this.Url.BaseUrl)
							.FirstOrDefault()?.Name ?? "";
					} else {
						return string.IsNullOrEmpty(x?.Name) ? $"No.{ this.Url.ThreadNo }" : x.Name;
					}
				}).ToReactiveProperty();
			this.SubscribeFutaba = this.Futaba.Subscribe(x => {
				if(x == null) {
					this.ThumbSource.Value = null;
					return;
				}
				if(this.isActivated) {
					this.LastRescount.Value = x.ResCount.Value;
				}

				var res = x.ResItems.FirstOrDefault();
				if(res == null) {
					this.ThumbSource.Value = null;
					return;
				}

				if(res.ThumbSource.Value != null) {
					this.ThumbSource.Value = res.ThumbSource.Value;
				}

				if(!res.Raw.Value.ResItem.Res.IsHavedImage) {
					return;
				}

				Util.Futaba.GetThumbImage(this.Url, res.Raw.Value.ResItem.Res)
					.Select(x => x.Successed ? WpfUtil.ImageUtil.LoadStream(x.LocalPath, x.FileBytes) : null)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						this.ThumbSource.Value = WpfUtil.ImageUtil.CreateImage(x);
					});
			});
			this.ThumbVisibility = this.ThumbSource
				.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();

			this.SearchButtonVisibility = SearchBoxVisibility
				.Select(x => (x == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible)
				.ToReactiveProperty();
			this.SearchColumnWidth = SearchBoxVisibility
				.Select(x => (x == Visibility.Visible) ? new GridLength(320, GridUnitType.Star) : new GridLength(0, GridUnitType.Auto))
				.ToReactiveProperty();
		}

		public void Dispose() {
			if(this.Region.Value != null) {
				this.Region.Value.Remove(this.ThreadView.Value);
				this.ThreadView.Dispose();
				this.Region.Value = null;
				this.ThreadView.Value = null;
			}
			this.container?.DestroyContainer();
			new Helpers.AutoDisposable(this)
				.AddEnumerable(this.Futaba.Value?.ResItems)
				.Dispose();
		}

		public void Bind(IFutabaContainer container) {
			this.container = container;
		}

		public void Unbind() {
			this.container = null;
		}


		public void ShowSearchBox() {
			SearchBoxVisibility.Value = Visibility.Visible;
		}

		public void HideSearchBox() {
			SearchBoxVisibility.Value = Visibility.Collapsed;
		}

		public void Activate() {
			this.isActivated = true;
			this.LastRescount.Value = this.Futaba.Value.ResCount.Value;
		}


		public void Deactivate() {
			this.isActivated = false;
		}
	}
}
