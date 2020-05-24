using System;
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
		public ReactiveProperty<Model.TabItem[]> TabItems { get; }
		public ReactiveProperty<Model.TreeItem[]> TreeItems { get; }

		public ReactiveProperty<Visibility> TabVisibility { get; }

		public ReactiveCommand<MouseButtonEventArgs> BordListClickCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<BordConfig> BordOpenCommand { get; } = new ReactiveCommand<BordConfig>();

		public ReactiveCommand<MouseButtonEventArgs> TabClickCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> TabCloseButtonCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();

		private Dictionary<string, Model.TabItem> SelectedTabItem { get; } = new Dictionary<string, TabItem>();
		public ReactiveProperty<Model.TabItem> TabControlSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();
		public ReactiveProperty<Model.TabItem> ThreadTabSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();

		public MainWindowViewModel() {
			Bords = new ReactiveProperty<Data.BordConfig[]>(Config.ConfigLoader.Bord);
			TabItems = new ReactiveProperty<Model.TabItem[]>(new Model.TabItem[0]);
			TreeItems = new ReactiveProperty<Model.TreeItem[]>(
				Config.ConfigLoader.Bord.Select((x) => new Model.TreeItem(x)).ToArray());
			this.TabVisibility = this.TabItems
				.Select(x => (x.Length == 0) ? Visibility.Collapsed : Visibility.Visible)
				.ToReactiveProperty();
			//TreeViewSelectedCommand.Subscribe(x => OnTreeViewItemSelectedChanged(x));
			BordListClickCommand.Subscribe(x => OnBordListClick(x));
			BordOpenCommand.Subscribe(x => OnBordOpen(x));
			TabClickCommand.Subscribe(x => OnTabClick(x));
			TabCloseButtonCommand.Subscribe(x => OnTabClose(x));
			Util.Futaba.Catalog
				.ObserveOnDispatcher()
				.Subscribe(x => OnUpdateCatalog(x));
			Util.Futaba.Threads
				.ObserveOnDispatcher()
				.Subscribe(x => OnUpdateThreadRes(x));
			//TabItems.ObserveOnDispatcher()
			//	.Subscribe(x => OnUpdateTabItem(x));
			TabControlSelectedItem.Subscribe(x => OnTabSelectedChanged(x));
			ThreadTabSelectedItem.Subscribe(x => OnTabSelectedChanged(x));
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

		private async void OnUpdateCatalog(Data.FutabaContext[] catalog) {
			var it = Update(catalog, false);
			if(it != null) {
				// この時点でTabコントロールの再処理が行われていないので一度UIスレッドから離れる
				// きもい
				await Task.Delay(1);
				this.TabControlSelectedItem.Value = it;
			}
		}

		private async void OnUpdateThreadRes(Data.FutabaContext[] threads) {
			var it = Update(threads, true);
			if(it != null) {
				// この時点でTabコントロールの再処理が行われていないので一度UIスレッドから離れる
				// きもい
				await Task.Delay(1);
				this.ThreadTabSelectedItem.Value = it;
			}
		}

		private Model.TabItem Update(Data.FutabaContext[] catalog, bool isThreadUpdated) {
			var l = this.TabItems.Value.ToList();
			var act = this.TabControlSelectedItem.Value;

			var c = catalog.Select(x => x.Url).ToList();
			var t = this.TabItems.Value.Select(x => x.Url).ToList();
			var cc = c.Except(t);
			var tt = t.Except(c);
			Model.TabItem r = null;
			if(cc.Count() != 0) {
				foreach(var it in cc) {
					l.Add(new Model.TabItem(catalog.Where(x => x.Url == it).First()));
				}
				r = l.Last();
			}
			if(tt.Count() != 0) {
				var rm = new List<Model.TabItem>();
				var idx = -1;
				foreach(var it in tt) {
					if(it.IsThreadUrl == isThreadUpdated) {
						if(act?.Url == it) {
							idx = t.IndexOf(it);
						}
						l.Remove(l.Where(x => x.Url == it).First());
					}
				}
				if((0 <= idx) && (l.Count != 0)) {
					r = (l.Count <= idx) ? l.Last() : l[idx];
				}
			}
			for(var i = 0; i < l.Count; i++) {
				var it = l[i];
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
			this.TabItems.Value = l.ToArray();
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
	}
}
