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
using Reactive.Bindings.Extensions;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class MainWindowViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class CurrentCatalogChanged {
			public FutabaContext FutabaContext { get; }

			public CurrentCatalogChanged(FutabaContext futaba) {
				this.FutabaContext = futaba;
			}
		}

		internal class CurrentThreadChanged {
			public FutabaContext FutabaContext { get; }

			public CurrentThreadChanged(FutabaContext futaba) {
				this.FutabaContext = futaba;
			}
		}

		public ReactiveProperty<Data.BordData[]> Bords { get; }
		public ReactiveCollection<Model.TabItem> Catalogs { get; } = new ReactiveCollection<TabItem>();
		public ReactiveProperty<object> CatalogToken { get; } = new ReactiveProperty<object>(DateTime.Now.Ticks);
		private Dictionary<string, ReactiveCollection<Model.TabItem>> ThreadsDic { get; } = new Dictionary<string, ReactiveCollection<TabItem>>();
		public ReactiveProperty<ReactiveCollection<Model.TabItem>> Threads { get; }
		public ReactiveProperty<object> ThreadToken { get; } = new ReactiveProperty<object>(DateTime.Now.Ticks);

		public ReactiveProperty<Visibility> TabVisibility { get; }

		public ReactiveCommand<MouseButtonEventArgs> BordListClickCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<BordData> BordOpenCommand { get; } = new ReactiveCommand<BordData>();
		public ReactiveCommand<RoutedEventArgs> ConfigButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();

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
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseAllCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseOtherCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseDieCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> ThreadCloseRightCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();


		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentCatalogTabUpdateCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentCatalogTabSearchCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentCatalogTabSortCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentCatalogTabModeCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentCatalogTabPostCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentThreadTabUpdateCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentThreadTabSearchCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentThreadTabPostCommand { get; } = new ReactiveCommand<Windows.MainWindow>();
		public ReactiveCommand<Windows.MainWindow> KeyBindingCurrentThreadTabCloseCommand { get; } = new ReactiveCommand<Windows.MainWindow>();


		public MainWindowViewModel() {
			Bords = new ReactiveProperty<Data.BordData[]>(Config.ConfigLoader.Bord.Bords);
			foreach(var c in Util.Futaba.Catalog.Value) {
				Catalogs.Add(new TabItem(c));
				ThreadsDic.Add(c.Url.BaseUrl, new ReactiveCollection<TabItem>());
			}
			foreach(var t in Util.Futaba.Threads.Value) {
				if(ThreadsDic.TryGetValue(t.Url.BaseUrl, out var r)) {
					r.Add(new TabItem(t));
				}
			}

			Threads = TabControlSelectedItem.Select(x => {
				var u = x?.Futaba.Value?.Url.BaseUrl;
				if(u != null) {
					if(ThreadsDic.TryGetValue(u, out var v)) {
						return v;
					} else {
						var r = new ReactiveCollection<Model.TabItem>();
						ThreadsDic.Add(u, r);
						return r;
					}
				} else {
					return new ReactiveCollection<Model.TabItem>();
				}
			}).ToReactiveProperty();
			this.TabVisibility = new ReactiveProperty<Visibility>((Catalogs.Count == 0) ? Visibility.Collapsed : Visibility.Visible);
			BordListClickCommand.Subscribe(x => OnBordListClick(x));
			ConfigButtonClickCommand.Subscribe(x => OnConfigButtonClick(x));
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
			ThreadCloseAllCommand.Subscribe(x => OnThreadCloseAll(x));
			ThreadCloseDieCommand.Subscribe(x => OnThreadCloseDie(x));

			KeyBindingCurrentCatalogTabUpdateCommand.Subscribe(x => OnKeyBindingCurrentCatalogTabUpdate(x));
			KeyBindingCurrentCatalogTabSearchCommand.Subscribe(x => OnKeyBindingCurrentCatalogTabSearch(x));
			KeyBindingCurrentCatalogTabSortCommand.Subscribe(x => OnKeyBindingCurrentCatalogTabSort(x));
			KeyBindingCurrentCatalogTabModeCommand.Subscribe(x => OnKeyBindingCurrentCatalogTabMode(x));
			KeyBindingCurrentCatalogTabPostCommand.Subscribe(x => OnKeyBindingCurrentCatalogTabPost(x));
			KeyBindingCurrentThreadTabUpdateCommand.Subscribe(x => OnKeyBindingCurrentThreadTabUpdate(x));
			KeyBindingCurrentThreadTabSearchCommand.Subscribe(x => OnKeyBindingCurrentThreadTabSearch(x));
			KeyBindingCurrentThreadTabPostCommand.Subscribe(x => OnKeyBindingCurrentThreadTabPost(x));
			KeyBindingCurrentThreadTabCloseCommand.Subscribe(x => OnKeyBindingCurrentThreadTabClose(x));
		}
		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void OnBordListClick(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
				if(o.DataContext is Data.BordData bc) {
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

		private void OnConfigButtonClick(RoutedEventArgs e) {
			new Windows.ConfigWindow() {
				Owner = App.Current.MainWindow,
				ShowInTaskbar = false,
			}.ShowDialog();
		}
		
		private void OnBordOpen(BordData bc) {
			var t = this.Catalogs.Where(x => x.Url.BaseUrl == bc.Url).FirstOrDefault();
			if(t != null) {
				this.TabControlSelectedItem.Value = t;
			} else {
				Util.Futaba.UpdateCatalog(bc).Subscribe();
			}
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
			this.CatalogToken.Value = DateTime.Now.Ticks;
			this.TabVisibility.Value = this.Catalogs.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
			foreach(var c in catalog) {
				var th = this.Threads.Value.Where(x => x.Url.BaseUrl == c.Url.BaseUrl).ToArray();
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
			var it = Update(this.Threads.Value, threads.Where(x => x.Url.BaseUrl == url).ToArray(), true);
			//RaisePropertyChanged(nameof(Threads)); // これがないとコンバータが起動しない
			if(it != null) {
				this.ThreadTabSelectedItem.Value = it;
			}
			this.ThreadToken.Value = DateTime.Now.Ticks;
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
				Util.Futaba.UpdateThreadRes(futaba.Raw.Bord, futaba.Raw.Url.ThreadNo, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental)
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

		private void OnThreadCloseAll(Model.BindableFutaba futaba) {
			var l = Util.Futaba.Threads.Value
				.Where(x => x.Url.BaseUrl == futaba.Url.BaseUrl)
				.ToList();
			foreach(var f in l) {
				Util.Futaba.Remove(f.Url);
			}
		}

		private void OnThreadCloseDie(Model.BindableFutaba futaba) {
			var l = Util.Futaba.Threads.Value
				.Where(x => x.Url.BaseUrl == futaba.Url.BaseUrl)
				.Where(x => x.Raw.IsDie)
				.ToList();
			foreach(var f in l) {
				Util.Futaba.Remove(f.Url);
			}
		}

		private System.Windows.Controls.TabControl GetCatalogTab(Windows.MainWindow w) {
			System.Diagnostics.Debug.Assert(w != null);

			return WpfUtil.WpfHelper.FindFirstChild<System.Windows.Controls.TabControl>(w);
		}
		private Controls.FutabaCatalogViewer GetCatalogView(System.Windows.Controls.TabControl t) {
			if(t == null) {
				return null;
			}

			return WpfUtil.WpfHelper.FindFirstChild<Controls.FutabaCatalogViewer>(t);
		}
		private System.Windows.Controls.TabControl GetThreadTab(Windows.MainWindow w) {
			System.Diagnostics.Debug.Assert(w != null);

			var t = GetCatalogTab(w);
			if(t != null) {
				return WpfUtil.WpfHelper.FindFirstChild<System.Windows.Controls.TabControl>(t);
			}
			return null;
		}
		private Controls.FutabaThreadResViewer GetThreadView(System.Windows.Controls.TabControl t) {
			if(t == null) {
				return null;
			}

			return WpfUtil.WpfHelper.FindFirstChild<Controls.FutabaThreadResViewer>(t);
		}

		private (bool Successed, Model.BindableFutaba Futaba, ViewModels.FutabaCatalogViewerViewModel ViewModel) GetCatalog(Windows.MainWindow w) {
			System.Diagnostics.Debug.Assert(w != null);

			var ct = GetCatalogTab(w);
			var cv = GetCatalogView(ct);
			if(ct?.SelectedItem is TabItem ti && cv?.DataContext is ViewModels.FutabaCatalogViewerViewModel vm) {
				return (true, ti.Futaba.Value, vm);
			} else {
				return (false, null, null);
			}
		}

		private (bool Successed, Model.BindableFutaba Futaba, ViewModels.FutabaThreadResViewerViewModel ViewModel) GetThread(Windows.MainWindow w) {
			System.Diagnostics.Debug.Assert(w != null);

			var tt = GetThreadTab(w);
			var tv = GetThreadView(tt);
			if(tt?.SelectedItem is TabItem ti && tv?.DataContext is ViewModels.FutabaThreadResViewerViewModel vm) {
				return (true, ti.Futaba.Value, vm);
			} else {
				return (false, null, null);
			}
		}

		private void OnKeyBindingCurrentCatalogTabUpdate(Windows.MainWindow w) {
			var c = GetCatalog(w);
			if(c.Successed) {
				c.ViewModel.KeyBindingUpdateCommand.Execute(c.Futaba);
			}
		}
		private void OnKeyBindingCurrentCatalogTabSearch(Windows.MainWindow w) {
			var c = GetCatalog(w);
			if(c.Successed) {
				c.ViewModel.KeyBindingSearchCommand.Execute(c.Futaba);
			}
		}
		private void OnKeyBindingCurrentCatalogTabSort(Windows.MainWindow w) {
			var c = GetCatalog(w);
			if(c.Successed) {
				c.ViewModel.KeyBindingSortCommand.Execute(c.Futaba);
			}
		}
		private void OnKeyBindingCurrentCatalogTabMode(Windows.MainWindow w) {
			var c = GetCatalog(w);
			if(c.Successed) {
				c.ViewModel.KeyBindingModeCommand.Execute(c.Futaba);
			}
		}
		private void OnKeyBindingCurrentCatalogTabPost(Windows.MainWindow w) {
			var c = GetCatalog(w);
			if(c.Successed) {
				c.ViewModel.KeyBindingPostCommand.Execute(c.Futaba);
			}
		}
		private void OnKeyBindingCurrentThreadTabUpdate(Windows.MainWindow w) {
			var t = GetThread(w);
			if(t.Successed) {
				t.ViewModel.KeyBindingUpdateCommand.Execute(t.Futaba);
			}
		}
		private void OnKeyBindingCurrentThreadTabSearch(Windows.MainWindow w) {
			var t = GetThread(w);
			if(t.Successed) {
				t.ViewModel.KeyBindingSearchCommand.Execute(t.Futaba);
			}
		}
		private void OnKeyBindingCurrentThreadTabPost(Windows.MainWindow w) {
			var t = GetThread(w);
			if(t.Successed) {
				t.ViewModel.KeyBindingPostCommand.Execute(t.Futaba);
			}
		}
		private void OnKeyBindingCurrentThreadTabClose(Windows.MainWindow w) {
			var t = GetThread(w);
			if(t.Successed) {
				OnCatalogThreadClose(t.Futaba);
			}
		}
	}
}
