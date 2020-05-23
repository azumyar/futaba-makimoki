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

		public ReactiveCommand<MouseButtonEventArgs> BordListClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<BordConfig> BordOpenCommand { get; }
		= new ReactiveCommand<BordConfig>();

		public ReactiveCommand<Data.UrlContext> TreeViewSelectedCommand { get; }
			= new ReactiveCommand<Data.UrlContext>();

		public ReactiveCommand<MouseButtonEventArgs> TabClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> TabCloseButtonCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> TreeViewClickCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();

		public ReactiveProperty<Model.TabItem> TabControlSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();


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
			TreeViewClickCommand.Subscribe(x => OnTreeViewClick(x));
			Util.Futaba.Catalog
				.ObserveOnDispatcher()
				.Subscribe(x => OnUpdateCatalog(x));
			Util.Futaba.Threads
				.ObserveOnDispatcher()
				.Subscribe(x => OnUpdateThreadRes(x));
			//TabItems.ObserveOnDispatcher()
			//	.Subscribe(x => OnUpdateTabItem(x));
			TabControlSelectedItem.Subscribe(x => OnTabSelectedChanged(x));
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

		private void OnTreeViewClick(MouseButtonEventArgs e) {
			if(e.ClickCount == 1) {
				if(e.Source is FrameworkElement o) {
					if(VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null) {
						if(o.DataContext is Model.TreeItem ti) {
							var it = this.TabItems.Value.Where(x => x.Url == ti.Url).FirstOrDefault();
							switch(e.ChangedButton) {
							case MouseButton.Left:
								if(it == null) {
									if(ti.Url.IsCatalogUrl) {
										Util.Futaba.UpdateCatalog(
											Bords.Value.Where(x => x.Url == ti.Url.BaseUrl).FirstOrDefault())
											.Subscribe();
									} else {
										// スレッドを開く
									}
								} else {
									this.TabControlSelectedItem.Value = it;
								}
								e.Handled = true;
								break;
							case MouseButton.Middle:
								if(it == null) {
								} else {
									Util.Futaba.Remove(it.Url);
								}
								e.Handled = true;
								break;
							}
						}
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

		private void OnUpdateTabItem(TabItem[] items) {
			/*
			var dic = new Dictionary<string, List<Model.TabItem>>();
			foreach (var it in items.Where(x => string.IsNullOrWhiteSpace(x.Url.ThreadNo))) {
				if (dic.Keys.Contains(it.Url.BaseUrl)) {
					dic[it.Url.BaseUrl].Add(it);
				} else {
					dic.Add(it.Url.BaseUrl, new List<TabItem>() { it });
				}
			}

			var update = false;
			foreach (var it in this.TreeItems.Value) {
				if (dic.Keys.Contains(it.Url.BaseUrl)) {
					var l = dic[it.Url.BaseUrl];
					var rm = it.ChildItems.Value.ToList();
					foreach (var c in it.ChildItems.Value) {
						var t = l.Where(x => x.Url == c.Url).FirstOrDefault();
						if (t != null) {
							l.Remove(t);
							rm.Remove(c);
						}
					}
					if (0 < rm.Count) {
						foreach (var t in rm) {
							it.ChildItems.Value.Remove(t);
						}
						update = true;
					}

					if (0 < l.Count) {
						it.ChildItems.Value.AddRange(l.Select(x => new TreeItem(x.Futaba.Value)));
						update = true;
					}
				}
			}

			if (update) {
				this.TreeItems.Value = this.TreeItems.Value;
			}
			*/
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
			UpdateTree(threads);
			if(it != null) {
				// この時点でTabコントロールの再処理が行われていないので一度UIスレッドから離れる
				// きもい
				await Task.Delay(1);
				this.TabControlSelectedItem.Value = it;
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

		private void UpdateTree(Data.FutabaContext[] threads) {
			var d1 = new Dictionary<string, List<Data.UrlContext>>();
			var d2 = new Dictionary<string, List<Data.UrlContext>>();
			var tm = new Dictionary<string, TreeItem>();
			foreach(var b in Bords.Value) {
				d1.Add(b.Url, new List<Data.UrlContext>());
				d2.Add(b.Url, new List<Data.UrlContext>());
				tm.Add(b.Url, this.TreeItems.Value.Where(x => x.Url.BaseUrl == b.Url).First());
			}

			foreach(var t in this.TreeItems.Value) {
				foreach(var ci in t.ChildItems.Value) {
					d1[ci.Url.BaseUrl].Add(ci.Url);
				}
			}
			foreach(var t in threads.Where(x => x.Url.IsThreadUrl)) {
				d2[t.Url.BaseUrl].Add(t.Url);
			}

			foreach(var b in Bords.Value) {
				var cc = d2[b.Url].Except(d1[b.Url]);
				var tt = d1[b.Url].Except(d2[b.Url]);
				var ti = tm[b.Url];

				if(cc.Count() != 0) {
					var l = new List<Data.FutabaContext>();
					foreach(var c in cc) {
						l.Add(threads.Where(x => x.Url == c).First());
					}
					ti.ChildItems.Value = ti.ChildItems.Value.Concat(
						l.Select(x => new TreeItem(x))).ToArray();
				}

				if(tt.Count() != 0) {
					var l = tm[b.Url].ChildItems.Value.ToList();
					foreach(var t in tt) {
						l.Remove(l.Where(x => x.Url == t).First());
					}
					ti.ChildItems.Value = l.ToArray();
				}

				var updated = false;
				for(var i = 0; i < ti.ChildItems.Value.Length; i++) {
					var it = ti.ChildItems.Value[i];
					if(cc.Contains(it.Url)) {
						continue;
					}
					var futaba = threads.Where(x => x.Url == it.Url).FirstOrDefault();
					if(futaba != null) {
						if(futaba.Token != it.Futaba.Value.Raw.Token) {
							it.Futaba.Value = new BindableFutaba(futaba, it.Futaba.Value);
							updated = true;
						}
					}
				}
				if(updated) {
					ti.ChildItems.Value = ti.ChildItems.Value.ToArray();
				}
			}
		}

		private void OnTabSelectedChanged(TabItem tabItem) {
			// 何も選択されていない場合
			if(tabItem == null) {
				return;
			}

			var p = this.TreeItems.Value
				.Where(x => x.Url.BaseUrl == tabItem.Futaba.Value.Url.BaseUrl)
				.FirstOrDefault();
			if(p != null) {
				if(tabItem.Futaba.Value.Url.IsCatalogUrl) {
					p.IsSelected.Value = true;
				} else {
					var c = p.ChildItems.Value
						.Where(x => x.Url == tabItem.Futaba.Value.Url)
						.FirstOrDefault();
					if(c != null) {
						p.IsExpanded.Value = true;
						c.IsSelected.Value = true;
					}
				}
			}
		}
	}
}
