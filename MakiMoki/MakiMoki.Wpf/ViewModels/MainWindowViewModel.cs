﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class MainWindowViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class CurrentTabChanged {
			public FutabaContext FutabaContext { get; }

			public CurrentTabChanged(FutabaContext futaba) {
				this.FutabaContext = futaba;
			}
		}


		private CompositeDisposable Disposable { get; } = new CompositeDisposable();

		public ReactiveProperty<Data.BordConfig[]> Bords { get; }
		public ReactiveCollection<Model.TabItem> Catalogs { get; } = new ReactiveCollection<TabItem>();
		public ReactiveCollection<Model.TabItem> Threads { get; } = new ReactiveCollection<TabItem>();

		public ReactiveProperty<Visibility> TabVisibility { get; }

		public ReactiveCommand<MouseButtonEventArgs> BordListClickCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<BordConfig> BordOpenCommand { get; } = new ReactiveCommand<BordConfig>();

		public ReactiveCommand<MouseButtonEventArgs> TabClickCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> TabCloseButtonCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();

		private Dictionary<string, Model.TabItem> SelectedTabItem { get; } = new Dictionary<string, TabItem>();
		public ReactiveProperty<Model.TabItem> TabControlSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();
		public ReactiveProperty<Model.TabItem> ThreadTabSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();

		public ReactiveCommand<Model.BindableFutaba> CatalogUpdateCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> CatalogCloseCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> CatalogCloseOtherCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> CatalogCloseRightCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();


		public ReactiveCommand<Model.BindableFutaba> ThreadUpdateCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseOtherCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseRightCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();


		public MainWindowViewModel() {
			Bords = new ReactiveProperty<Data.BordConfig[]>(Config.ConfigLoader.Bord);
			this.TabVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
			BordListClickCommand.Subscribe(x => OnBordListClick(x));
			BordOpenCommand.Subscribe(x => OnBordOpen(x));
			TabClickCommand.Subscribe(x => OnTabClick(x));
			TabCloseButtonCommand.Subscribe(x => OnTabClose(x));
			Util.Futaba.Catalog
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => OnUpdateCatalog(x));
			Util.Futaba.Threads
				.ObserveOn(UIDispatcherScheduler.Default)
				.Subscribe(x => OnUpdateThreadRes(x));
			TabControlSelectedItem.Subscribe(x => OnTabSelectedChanged(x));
			ThreadTabSelectedItem.Subscribe(x => OnTabSelectedChanged(x));

			CatalogUpdateCommand.Subscribe(x => OnCatalogThreadUpdate(x));
			CatalogCloseCommand.Subscribe(x => OnCatalogThreadClose(x));
			CatalogCloseOtherCommand.Subscribe(x => OnCatalogThreadCloseOther(x));
			CatalogCloseRightCommand.Subscribe(x => OnCatalogThreadCloseRight(x));
			ThreadUpdateCommand.Subscribe(x => OnCatalogThreadUpdate(x));
			ThreadCloseCommand.Subscribe(x => OnCatalogThreadClose(x));
			ThreadCloseOtherCommand.Subscribe(x => OnCatalogThreadCloseOther(x));
			ThreadCloseRightCommand.Subscribe(x => OnCatalogThreadCloseRight(x));
		}
		public void Dispose() {
			Disposable.Dispose();
		}

		private void OnBordListClick(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
				if(o.DataContext is Data.BordConfig bc) {
					if(e.ClickCount == 1) {
						switch(e.ChangedButton) {
						case MouseButton.Left:
						case MouseButton.Middle:
							Util.Futaba.UpdateCatalog(bc).Subscribe();
							e.Handled = true;
							break;
						}
					}
				}
			}
		}
		
		private void OnBordOpen(BordConfig bc) {
			Util.Futaba.UpdateCatalog(bc).Subscribe();
		}
		private void OnTabClick(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
				if(o.DataContext is Model.TabItem ti) {
					if((e.ClickCount == 1) && (e.ChangedButton == MouseButton.Middle)) {
						Util.Futaba.Remove(ti.Url);
						e.Handled = true;
					}
				}
			}
		}

		private void OnTabClose(MouseButtonEventArgs e) {
			if(e.ClickCount == 1) {
				switch(e.ChangedButton) {
				case MouseButton.Left:
					if((e.Source is FrameworkElement o) && (o.DataContext is Model.TabItem ti)) {
						Util.Futaba.Remove(ti.Url);
					}
					e.Handled = true;
					break;
				}
			}
			System.Diagnostics.Debug.WriteLine(e);
		}

		private void OnUpdateCatalog(Data.FutabaContext[] catalog) {
			var it = Update(this.Catalogs, catalog, false);
			//RaisePropertyChanged("Catalogs");
			if(it != null) {
				this.TabControlSelectedItem.Value = it;
			}
			this.TabVisibility.Value = this.Catalogs.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
			foreach(var c in catalog) {
				var th = this.Threads.Where(x => x.Url.BaseUrl == c.Url.BaseUrl).ToArray();
				foreach(var t in c.ResItems) {
					var tt = th.Where(x => x.Futaba.Value.ResItems.FirstOrDefault()?.Raw.Value?.ResItem.No == t.ResItem.No).FirstOrDefault();
					if(tt != null) {
						tt.Futaba.Value.CatalogResCount.Value = t.CounterCurrent;
					}
				}
			}
		}

		private void OnUpdateThreadRes(Data.FutabaContext[] threads) {
			var url = this.TabControlSelectedItem.Value?.Futaba.Value?.Url.BaseUrl ?? "";
			var it = Update(this.Threads, threads /*.Where(x => x.Url.BaseUrl == url).ToArray() */, true);
			RaisePropertyChanged(nameof(Threads)); // これがないとコンバータが起動しない
			if(it != null) {
				this.ThreadTabSelectedItem.Value = it;
			}
		}

		private Model.TabItem Update(ReactiveCollection<Model.TabItem> collection, Data.FutabaContext[] catalog, bool isThreadUpdated) {
			var act = isThreadUpdated ? this.ThreadTabSelectedItem.Value : this.TabControlSelectedItem.Value;

			var c = catalog.Select(x => x.Url).ToList();
			var t = collection.Select(x => x.Url).ToList();
			var cc = c.Except(t);
			var tt = t.Except(c);
			Model.TabItem r = null;
			if(cc.Count() != 0) {
				foreach(var it in cc) {
					collection.Add(new Model.TabItem(catalog.Where(x => x.Url == it).First()));
				}
				r = collection.Last();
			}
			if(tt.Count() != 0) {
				var idx = default(int?);
				foreach(var it in tt) {
					if(it.IsThreadUrl == isThreadUpdated) {
						if(act?.Url == it) {
							idx = t.IndexOf(it);
						}
						collection.Remove(collection.Where(x => x.Url == it).First());
					}
				}
				if(idx.HasValue && (collection.Count != 0)) {
					for(var idx2 = idx.Value; 0 <= idx2; idx2--) {
						var tr = (collection.Count <= idx2) ? collection.Last() : collection[idx2];
						if(tr.Futaba.Value.Url.BaseUrl == act.Url.BaseUrl) {
							r = tr;
							break;
						}
					}
					if(r == null) {
						for(var idx2 = idx.Value + 1; idx2 < collection.Count; idx2++) {
							if(collection[idx2].Futaba.Value.Url.BaseUrl == act.Url.BaseUrl) {
								r = collection[idx2];
								break;
							}
						}
					}
				}
			}
			for(var i = 0; i < collection.Count; i++) {
				var it = collection[i];
				if(cc.Contains(it.Url)) {
					continue;
				}
				var futaba = catalog.Where(x => x.Url == it.Url).FirstOrDefault();
				if(futaba != null) {
					if(futaba.Token != it.Futaba.Value.Raw.Token) {
						it.Futaba.Value = new BindableFutaba(futaba, it.Futaba.Value);
					}
				}
			}
			return r;
		}


		private async void OnTabSelectedChanged(TabItem tabItem) {
			// 何も選択されていない場合
			if(tabItem == null) {
				return;
			}

			if(tabItem.Futaba.Value.Url.IsCatalogUrl) {
				if(this.SelectedTabItem.TryGetValue(tabItem.Futaba.Value.Url.BaseUrl, out var ti)) {
					await Task.Delay(1);
					this.OnUpdateThreadRes(Util.Futaba.Threads.Value);
					this.ThreadTabSelectedItem.Value = ti;
				}
			} else {
				if(this.SelectedTabItem.ContainsKey(tabItem.Futaba.Value.Url.BaseUrl)) {
					this.SelectedTabItem[this.TabControlSelectedItem.Value.Futaba.Value.Url.BaseUrl] = tabItem;
				} else {
					this.SelectedTabItem.Add(this.TabControlSelectedItem.Value.Futaba.Value.Url.BaseUrl, tabItem);
				}
			}
		}

		private void OnCatalogThreadUpdate(Model.BindableFutaba futaba) {
			if(futaba.Url.IsCatalogUrl) {
				Util.Futaba.UpdateCatalog(futaba.Raw.Bord)
					.Subscribe();
			} else {
				Util.Futaba.UpdateThreadRes(futaba.Raw.Bord, futaba.Raw.Url.ThreadNo, true)
					.Subscribe();
			}
		}

		private void OnCatalogThreadClose(Model.BindableFutaba futaba) {
			Util.Futaba.Remove(futaba.Raw.Url);
		}

		private void OnCatalogThreadCloseOther(Model.BindableFutaba futaba) {
			var a = futaba.Url.IsCatalogUrl ? Util.Futaba.Catalog.Value : Util.Futaba.Threads.Value;
			foreach(var f in a.Where(x => x.Url.BaseUrl == futaba.Url.BaseUrl)
				.Where(x => x.Url != futaba.Url)) {
			
				Util.Futaba.Remove(f.Url);
			}
		}

		private void OnCatalogThreadCloseRight(Model.BindableFutaba futaba) {
			if(futaba.Url.IsCatalogUrl) {
				int i = Util.Futaba.Catalog.Value
					.Select(x => x.Url)
					.ToList()
					.IndexOf(futaba.Url);
				if(0 <= i) {
					foreach(var f in Util.Futaba.Catalog.Value.Skip(i + 1)) {
						Util.Futaba.Remove(f.Url);
					}
				}
			} else {
				var l = Util.Futaba.Threads.Value
					.Where(x => x.Url.BaseUrl == futaba.Url.BaseUrl)
					.ToList();
				int i = l.Select(x => x.Url.ThreadNo).ToList().IndexOf(futaba.Url.ThreadNo);
				if(0 <= i) {
					foreach(var f in l.Skip(i + 1)) {
						Util.Futaba.Remove(f.Url);
					}
				}
			}
		}
	}
}
