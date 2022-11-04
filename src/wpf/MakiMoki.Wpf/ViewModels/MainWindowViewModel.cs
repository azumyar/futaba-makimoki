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
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Reactive;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class MainWindowViewModel : BindableBase, IDisposable {
		public record struct InfomationRecord(string Message, InformationBindableExObject ExObject, string Token);
		public record struct WpfBugMessage(FrameworkElement Element, bool Remove = true);

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

		public ReactiveProperty<Data.BoardData[]> Boards { get; }
		public ReactiveCollection<Model.TabItem> Catalogs { get; } = new ReactiveCollection<TabItem>();
		public ReactiveProperty<object> CatalogToken { get; } = new ReactiveProperty<object>(DateTime.Now.Ticks);
		private Dictionary<string, ReactiveCollection<Model.TabItem>> ThreadsDic { get; } = new Dictionary<string, ReactiveCollection<TabItem>>();
		public ReactiveProperty<ReactiveCollection<Model.TabItem>> Threads { get; }
		public ReactiveProperty<object> ThreadToken { get; } = new ReactiveProperty<object>(DateTime.Now.Ticks);
		public ReactiveProperty<List<InfomationRecord>> Infomations { get; }
		public ReactiveProperty<CornerRadius> InfomationCornerRadius { get; } = new ReactiveProperty<CornerRadius>(App.OsCompat.IsWindows11Rtm switch {
			true => new CornerRadius(4, 4, 4, 4),
			false => new CornerRadius()
		});

		public ReactiveProperty<bool> Topmost { get; }
		public ReactiveProperty<Visibility> StartUpVisibility { get; }
		public ReactiveProperty<Visibility> TabVisibility { get; }
		public ReactiveProperty<KeyBinding[]> KeyGestures { get; } = new ReactiveProperty<KeyBinding[]>(Array.Empty<KeyBinding>());

		public MakiMokiCommand<RoutedEventArgs> LoadedCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> BordListClickCommand { get; } = new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<BoardData> BoardOpenCommand { get; } = new MakiMokiCommand<BoardData>();
		public MakiMokiCommand<RoutedEventArgs> ConfigButtonClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<MouseButtonEventArgs> TabClickCommand { get; } = new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<RoutedEventArgs> TabCloseClickButtonCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		
		public ReactiveProperty<Model.TabItem> TabControlSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();
		public ReactiveProperty<Model.TabItem> ThreadTabSelectedItem { get; } = new ReactiveProperty<Model.TabItem>();

		public MakiMokiCommand<Model.BindableFutaba> CatalogUpdateCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> CatalogCloseCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> CatalogCloseOtherCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> CatalogCloseRightCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();


		public MakiMokiCommand<Model.BindableFutaba> ThreadUpdateCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> ThreadCloseCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> ThreadCloseAllCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> ThreadCloseOtherCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> ThreadCloseDieCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();
		public MakiMokiCommand<Model.BindableFutaba> ThreadCloseRightCommand { get; } = new MakiMokiCommand<Model.BindableFutaba>();


		public MakiMokiCommand KeyBindingCurrentCatalogTabUpdateCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentCatalogTabSearchCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentCatalogTabSortCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentCatalogTabModeCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentCatalogTabPostCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentCatalogTabCloseCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingNextCatalogTabCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingPreviouseCatalogTabCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentThreadTabUpdateCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentThreadTabSearchCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentThreadTabPostCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCurrentThreadTabCloseCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingNextThreadTabCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingPreviouseThreadTabCommand { get; } = new MakiMokiCommand();

		private Action onBoardConfigUpdateNotifyer;
		private Action<PlatformData.WpfConfig> onSystemConfigUpdateNotifyer;
		private Action<PlatformData.GestureConfig> onGestureConfigUpdateNotifyer;

		private IDialogService DialogService { get; }
		private WpfHelpers.MouseGesture gesture;
		public ReactiveProperty<Visibility> TmpMouseGestureVisibility { get; }
		public ReactiveProperty<string> TmpMouseGestureText { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<string> TmpMouseGestureName { get; } = new ReactiveProperty<string>("");

		private enum MouseGestureId {
			MouseGestureCatalogOpenPost,
			MouseGestureCatalogUpdate,
			MouseGestureCatalogClose,
			MouseGestureCatalogPrevious,
			MouseGestureCatalogNext, 
			MouseGestureThreadOpenPost,
			MouseGestureThreadUpdate,
			MouseGestureThreadClose,
			MouseGestureThreadPrevious,
			MouseGestureThreadNext,
		}

		private (PlatformData.MouseGestureCommands[] Commands, MouseGestureId MouseGestureId, string Name)[] MouseGestureCommands { get; set; }
		private Dictionary<MouseGestureId, Action> MouseGestureId2Action { get; }

		public MainWindowViewModel(IDialogService dialogService) {
			TmpMouseGestureVisibility = TmpMouseGestureText.Select(x => string.IsNullOrEmpty(x) switch {
				true => Visibility.Collapsed,
				false => Visibility.Visible
			}).ToReactiveProperty();
			this.LoadedCommand.Subscribe(x => {
				if(x.Source is DependencyObject o) {
					this.gesture = new WpfHelpers.MouseGesture(o);
					gesture.Update = y => {
						var name = "";
						foreach(var mg in this.MouseGestureCommands) {
							if(mg.Commands.Cast<IEquatable<PlatformData.MouseGestureCommands>>().Any(z => z.Equals(y))) {
								name = $"：{mg.Name}";
								break;
							}
						}
						this.TmpMouseGestureName.Value = name;
						this.TmpMouseGestureText.Value = y.ToString(true);
					};
					gesture.Fire = y => {
						MouseGestureId? id = null;
						foreach(var mg in this.MouseGestureCommands) {
							if(mg.Commands.Cast<IEquatable<PlatformData.MouseGestureCommands>>().Any(z => z.Equals(y))) {
								id = mg.MouseGestureId;
								break;
							}
						}
						if((id != null) && this.MouseGestureId2Action.TryGetValue(id.Value, out var act)) {
							act();
							return true;
						} else {
							return false;
						}
					};
				}
			});

			DialogService = dialogService;
			Boards = new ReactiveProperty<Data.BoardData[]>(Config.ConfigLoader.Board.Boards);
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
			this.Infomations = new ReactiveProperty<List<InfomationRecord>>(initialValue: new ());
			Util.Futaba.Informations.Subscribe(x => {
				Observable.Return(x)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(y => {
						var list = new List<InfomationRecord>();
						using var rm = new CompositeDisposable();
						foreach(var it in y) {
							if(!this.Infomations.Value.Any(z => z.Token == it.Token)) {
								static InformationBindableExObject gen1(FutabaContext fc) {
									if((fc.ResItems.FirstOrDefault() is FutabaContext.Item fit)
										&& WpfUtil.ImageUtil.GetImageCache2(
											Util.Futaba.GetThumbImageLocalFilePath(
												fc.Url, fit.ResItem.Res)) is ImageSource isc) {

										return new InformationBindableExObject(isc);
									}
									return new InformationBindableExObject();
								}
								static InformationBindableExObject gen2(UrlContext c) {
									return gen1(Util.Futaba.Threads.Value.Where(x => x.Url == c).FirstOrDefault());
								} 

								list.Add(new InfomationRecord(
									it.Message,
									it.ExObject switch {
										BindableFutaba bf => new InformationBindableExObject(bf),
										ImageSource isc => new InformationBindableExObject(isc),
										FutabaContext fc when fc.Url.IsThreadUrl => gen1(fc),
										UrlContext uc when uc.IsThreadUrl => gen2(uc),
										_ => new InformationBindableExObject()
									}, it.Token)) ;
							}
						}
						foreach(var it in this.Infomations.Value) {
							if(y.Any(z => z.Token == it.Token)) {
								list.Add(it);
							} else {
								rm.Add(it.ExObject);
							}
						}
						this.Infomations.Value = list;
					});
			});

			this.Topmost = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledWindowTopmost);
			this.TabVisibility = new ReactiveProperty<Visibility>(Catalogs.Any() switch {
				true => Visibility.Visible,
				false => Visibility.Collapsed
			});
			this.StartUpVisibility = this.TabVisibility.Select(x => x switch {
				Visibility.Visible => Visibility.Collapsed,
				_ => Visibility.Visible
			}).ToReactiveProperty();
			this.UpdateKeyBindings();
			BordListClickCommand.Subscribe(x => OnBordListClick(x));
			ConfigButtonClickCommand.Subscribe(x => OnConfigButtonClick(x));
			BoardOpenCommand.Subscribe(x => OnBordOpen(x));
			TabClickCommand.Subscribe(x => OnTabClick(x));
			TabCloseClickButtonCommand.Subscribe(x => OnTabCloseClick(x));
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

			KeyBindingCurrentCatalogTabUpdateCommand.Subscribe(_ => OnKeyBindingCurrentCatalogTabUpdate());
			KeyBindingCurrentCatalogTabSearchCommand.Subscribe(_ => OnKeyBindingCurrentCatalogTabSearch());
			KeyBindingCurrentCatalogTabModeCommand.Subscribe(_ => OnKeyBindingCurrentCatalogTabMode());
			KeyBindingCurrentCatalogTabPostCommand.Subscribe(_ => OnKeyBindingCurrentCatalogTabPost());
			KeyBindingCurrentCatalogTabCloseCommand.Subscribe(_ => OnKeyBindingCurrentCatalogTabClose());
			KeyBindingNextCatalogTabCommand.Subscribe(_ => OnKeyBindingNextCatalogTab());
			KeyBindingPreviouseCatalogTabCommand.Subscribe(_ => OnKeyBindingPreviouseCatalogTab());
			KeyBindingCurrentThreadTabUpdateCommand.Subscribe(_ => OnKeyBindingCurrentThreadTabUpdate());
			KeyBindingCurrentThreadTabSearchCommand.Subscribe(_ => OnKeyBindingCurrentThreadTabSearch());
			KeyBindingCurrentThreadTabPostCommand.Subscribe(_ => OnKeyBindingCurrentThreadTabPost());
			KeyBindingCurrentThreadTabCloseCommand.Subscribe(_ => OnKeyBindingCurrentThreadTabClose());
			KeyBindingNextThreadTabCommand.Subscribe(_ => OnKeyBindingNextThreadTab());
			KeyBindingPreviouseThreadTabCommand.Subscribe(_ => OnKeyBindingPreviouseThreadTab());

			MouseGestureId2Action = new Dictionary<MouseGestureId, Action> {
				{ MouseGestureId.MouseGestureCatalogOpenPost, OnKeyBindingCurrentCatalogTabPost},
				{ MouseGestureId.MouseGestureCatalogUpdate, OnKeyBindingCurrentCatalogTabUpdate},
				{ MouseGestureId.MouseGestureCatalogClose, OnKeyBindingCurrentCatalogTabClose},
				{ MouseGestureId.MouseGestureCatalogPrevious, OnKeyBindingPreviouseCatalogTab},
				{ MouseGestureId.MouseGestureCatalogNext, OnKeyBindingNextCatalogTab},
				{ MouseGestureId.MouseGestureThreadOpenPost, OnKeyBindingCurrentThreadTabPost},
				{ MouseGestureId.MouseGestureThreadUpdate, OnKeyBindingCurrentThreadTabUpdate},
				{ MouseGestureId.MouseGestureThreadClose, OnKeyBindingCurrentThreadTabClose},
				{ MouseGestureId.MouseGestureThreadPrevious, OnKeyBindingPreviouseThreadTab},
				{ MouseGestureId.MouseGestureThreadNext, OnKeyBindingNextThreadTab},
			};

			onBoardConfigUpdateNotifyer = () => Boards.Value = Config.ConfigLoader.Board.Boards;
			onSystemConfigUpdateNotifyer = (_) => {
				Topmost.Value = WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledWindowTopmost;
			};
			onGestureConfigUpdateNotifyer = (_) => UpdateKeyBindings();
			Config.ConfigLoader.BoardConfigUpdateNotifyer.AddHandler(onBoardConfigUpdateNotifyer);
			WpfConfig.WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler(onSystemConfigUpdateNotifyer);
			WpfConfig.WpfConfigLoader.GestureConfigUpdateNotifyer.AddHandler(onGestureConfigUpdateNotifyer);
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void UpdateKeyBindings() {
			var kg = new List<KeyBinding>();
			var kc = new KeyGestureConverter();
			KeyBinding GetKeyBinding(string s, ICommand c) {
				if(kc.ConvertFromString(s) is KeyGesture kg) {
					return new KeyBinding(c, kg);
				}
				return null;
			}
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogUpdate
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentCatalogTabUpdateCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogSearch
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentCatalogTabSearchCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogOpenPost
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentCatalogTabPostCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogClose
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentCatalogTabCloseCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogNext
				.Select(x => GetKeyBinding(x, this.KeyBindingNextCatalogTabCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogPrevious
				.Select(x => GetKeyBinding(x, this.KeyBindingPreviouseCatalogTabCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadUpdate
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentThreadTabUpdateCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadSearch
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentThreadTabSearchCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadOpenPost
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentThreadTabPostCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadTabClose
				.Select(x => GetKeyBinding(x, this.KeyBindingCurrentThreadTabCloseCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadTabNext
				.Select(x => GetKeyBinding(x, this.KeyBindingNextThreadTabCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadTabPrevious
				.Select(x => GetKeyBinding(x, this.KeyBindingPreviouseThreadTabCommand))
				.Where(x => x != null));
			this.KeyGestures.Value = kg.ToArray();

			this.MouseGestureCommands = new[] {
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureCatalogOpenPost, MouseGestureId.MouseGestureCatalogOpenPost, "スレ立て"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureCatalogUpdate, MouseGestureId.MouseGestureCatalogUpdate, "カタ更新"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureCatalogClose, MouseGestureId.MouseGestureCatalogClose, "カタ閉じ"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureCatalogPrevious, MouseGestureId.MouseGestureCatalogPrevious, "前のカタ"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureCatalogNext, MouseGestureId.MouseGestureCatalogNext, "次のカタ"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureThreadOpenPost, MouseGestureId.MouseGestureThreadOpenPost, "レス"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureThreadUpdate, MouseGestureId.MouseGestureThreadUpdate, "スレ更新"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureThreadClose, MouseGestureId.MouseGestureThreadClose, "スレ閉じ"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureThreadPrevious, MouseGestureId.MouseGestureThreadPrevious, "前のスレ"),
				(WpfConfig.WpfConfigLoader.Gesture.MouseGestureThreadNext, MouseGestureId.MouseGestureThreadNext, "次のスレ"),
			};
		}

		private void OnBordListClick(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
				if(o.DataContext is Data.BoardData bc) {
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
			DialogService.ShowDialog(
				nameof(Windows.Dialogs.ConfigDialog),
				new DialogParameters(),
				(x) => { });
		}
		
		private void OnBordOpen(BoardData bc) {
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

		private void OnTabCloseClick(RoutedEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is Model.TabItem ti)) {
				Util.Futaba.Remove(ti.Url);
			}
			e.Handled = true;
		}

		private async void OnUpdateCatalog(Data.FutabaContext[] catalog) {
#if DEBUG
			var back = this.Catalogs.ToArray();
#endif
			// BorderからWhiteが見つからないと言われるので暫定対策
			// 再現手順:起動→なにかのカタログを開く→全部カタログを閉じる
			try {
				await Update(
					this.Catalogs,
					catalog,
					false,
					(x) => this.TabControlSelectedItem.Value = x);
			}
			catch(InvalidOperationException) {
#if DEBUG // リリース版でのみつぶす
				if(System.Diagnostics.Debugger.IsAttached) {
					System.Diagnostics.Debugger.Break();
				} else {
					throw;
				}
#endif
			}
			this.CatalogToken.Value = DateTime.Now.Ticks;
			this.TabVisibility.Value = this.Catalogs.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
#if DEBUG
			// スレが削除された場合Threadキャッシュ整合性確認
			{
				var except = back.Select(x => x.Url.BaseUrl)
					.Except(catalog.Select(x => x.Url.BaseUrl))
					.ToArray();
				if(except.Any()) {
					foreach(var e in except) {
						if(Util.Futaba.Threads.Value.Any(x => x.Url.BaseUrl == e)) {
							System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!カタログスレッドキャッシュ不整合!!!!!!!!!!!!!!!!!!!!!");
						}
					}
				}
			}
#endif
			foreach(var c in catalog) {
				var th = this.Threads.Value.Where(x => x.Url.BaseUrl == c.Url.BaseUrl).ToArray();
				foreach(var t in c.ResItems) {
					var tt = th.Where(x => x.Futaba.Value.ResItems.FirstOrDefault()?.Raw.Value?.ResItem.No == t.ResItem.No).FirstOrDefault();
					if(tt != null) {
						tt.Futaba.Value.CatalogResCount.Value = t.CounterCurrent;
					}
				}
			}

			if(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledFetchThumbnail) {
				foreach(var tab in this.Catalogs) {
					Observable.Create<object>(o => {
						var c = tab.Futaba.Value.ResItems
							.Where(x => !x.ThumbDisplay.Value.HasValue
								&& x.Raw.Value.ResItem.Res.IsHavedImage)
							.Select<BindableFutabaResItem, (BindableFutabaResItem Item, string Path)>(
								x => (x, Util.Futaba.GetThumbImageLocalFilePath(
									tab.Futaba.Value.Url, x.Raw.Value.ResItem.Res)))
							.Where(x => WpfUtil.ImageUtil.GetImageCache(x.Path) == null)
							.ToArray();
						if(c.Any()) {
							Task.Run(async () => {
								var max = 4;
								await Task.WhenAll(
									Enumerable.Range(0, max).Select(i => Task.Run(
										() => {
											var items = c.Where((_, j) => j % max == i).ToArray();
											Util.Futaba.GetThumbImages(
												tab.Futaba.Value.Url,
												items.Select(x => x.Item.Raw.Value.ResItem.Res).ToArray(),
												false)
												.Select<
													(bool Successed, string LocalPath, byte[] FileBytes),
													(System.IO.Stream Stream, string Path)
												>(
													x => x.Successed ? (WpfUtil.ImageUtil.LoadStream(x.LocalPath, x.FileBytes), x.LocalPath)
														: (null, null))
												.ObserveOn(UIDispatcherScheduler.Default)
												.Subscribe(x => {
													if(x.Stream != null) {
														var it = c.Where(y => y.Path == x.Path)
															.Select(y => y.Item)
															.FirstOrDefault();
														if(it != null) {
															it.SetThumbSource(WpfUtil.ImageUtil.CreateImage(x.Path, x.Stream));
														}
													}
													o.OnNext(x);
												});
										})
									).ToArray()
							);
								o.OnCompleted();
							});
						} else {
							o.OnCompleted();
						}
						return System.Reactive.Disposables.Disposable.Empty;
					}).Subscribe();
				}
			}
		}

		private async void OnUpdateThreadRes(Data.FutabaContext[] threads) {
			var url = this.TabControlSelectedItem.Value?.Futaba.Value?.Url.BaseUrl ?? "";
			var catalogBords = this.Catalogs.Select(x => x.Futaba.Value?.Url.BaseUrl)
				.Where(x => x != null)
				.Distinct()
				.ToArray();
			var threadBords = this.ThreadsDic.Keys.ToList();
			foreach(var c in catalogBords) {
				if(c == this.TabControlSelectedItem.Value?.Futaba.Value?.Url.BaseUrl) {
					await Update(
						this.Threads.Value,
						threads.Where(x => x.Url.BaseUrl == c).ToArray(),
						true,
						x => {
							this.ThreadTabSelectedItem.Value = x;
						});
				}
				if(threadBords.Contains(c)) {
					threadBords.Remove(c);
				}
			}
			foreach(var t in threadBords) {
				this.ThreadsDic.Remove(t);
			}
			this.ThreadToken.Value = DateTime.Now.Ticks;
		}

		private async Task<Model.TabItem> Update(
			ReactiveCollection<Model.TabItem> inputCollection,
			Data.FutabaContext[] newItem,
			bool isThreadUpdated,
			Action<Model.TabItem> apply = null) {

			var act = isThreadUpdated ? this.ThreadTabSelectedItem.Value : this.TabControlSelectedItem.Value;

			var c = newItem.Select(x => x.Url).ToList();
			var t = inputCollection.Select(x => x.Url).ToList();
			var cc = c.Except(t);
			var tt = t.Except(c);
			var r = default(Model.TabItem);
			var rl = new List<Model.TabItem>();
			using(var disp = new Helpers.AutoDisposable()) {
				if(cc.Any()) {
					foreach(var it in cc) {
						inputCollection.Add(new Model.TabItem(newItem.Where(x => x.Url == it).First()));
					}
					r = inputCollection.Last();
				}
				if(tt.Any()) {
					var copy = inputCollection.ToList();
					foreach(var it in tt) {
						if(it.IsThreadUrl == isThreadUpdated) {
							var rm = inputCollection.Where(x => x.Url == it).First();
							disp.Add(rm);
							copy.Remove(rm);
							rl.Add(rm);
						}
					}
					// 最後に見たタブを持ってくる
					r = copy.Select((x, i) => new { Value = x, Index = i })
						.OrderBy(x => x.Value.LastDisplayTime.Value)
						.ThenByDescending(x => x.Index)
						.LastOrDefault()?.Value;
				}
				for(var i = 0; i < inputCollection.Count; i++) {
					var it = inputCollection[i];
					if(cc.Contains(it.Url)) {
						continue;
					}
					var futaba = newItem.Where(x => x.Url == it.Url).FirstOrDefault();
					if(futaba != null) {
						if(futaba.Token != it.Futaba.Value.Raw.Token) {
							disp.Add(it.Futaba.Value);
							if(!isThreadUpdated) {
								disp.AddEnumerable(it.Futaba.Value.ResItems);
							}
							it.UpdateFutaba(new BindableFutaba(futaba, it.Futaba.Value));
						}
					}
				}

				if(r != null) {
					apply?.Invoke(r);
				}
				await Task.Delay(1);

				foreach(var rm in rl) {
					inputCollection.Remove(rm);
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
				if(this.ThreadsDic.TryGetValue(tabItem.Futaba.Value.Url.BaseUrl, out var tabCollection)) {
					var ti = tabCollection
						.Select((x, i) => new { Value = x, Index = i })
						.OrderBy(x => x.Value.LastDisplayTime.Value)
						.ThenByDescending(x => x.Index)
						.LastOrDefault()?.Value;
					if(ti != null) {
						await Task.Yield();
						this.OnUpdateThreadRes(Util.Futaba.Threads.Value);
						this.ThreadTabSelectedItem.Value = ti;
					}
				}
				tabItem.Activate();
			} else {
				foreach(var tab in this.ThreadsDic.ToArray()) {
					foreach(var it in tab.Value) {
						it.Deactivate();
					}
				}
				tabItem.Activate();
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
			if(futaba == null) {
				return;
			}

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

		private void OnKeyBindingCurrentCatalogTabUpdate()
			=> FutabaCatalogViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogUpdateCommandMessage>>()
				.Publish(new FutabaCatalogViewerViewModel.CatalogUpdateCommandMessage(
					this.TabControlSelectedItem.Value?.Futaba.Value));
		private void OnKeyBindingCurrentCatalogTabSearch()
			=> FutabaCatalogViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogSearchCommandMessage>>()
				.Publish(new FutabaCatalogViewerViewModel.CatalogSearchCommandMessage(
					this.TabControlSelectedItem.Value?.Futaba.Value));
		private void OnKeyBindingCurrentCatalogTabMode()
			=> FutabaCatalogViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogModeCommandMessage>>()
				.Publish(new FutabaCatalogViewerViewModel.CatalogModeCommandMessage(
					this.TabControlSelectedItem.Value?.Futaba.Value));
		private void OnKeyBindingCurrentCatalogTabPost()
			=> FutabaCatalogViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaCatalogViewerViewModel.CatalogOpenPostCommandMessage>>()
				.Publish(new FutabaCatalogViewerViewModel.CatalogOpenPostCommandMessage(
					this.TabControlSelectedItem.Value?.Futaba.Value));
		private void OnKeyBindingCurrentCatalogTabClose()
			=> OnCatalogThreadClose(this.TabControlSelectedItem.Value?.Futaba.Value);
		private void OnKeyBindingNextCatalogTab() {
			if(TabControlSelectedItem.Value == null) {
				TabControlSelectedItem.Value = this.Catalogs.FirstOrDefault();
				return;
			}
			if(this.Catalogs.Count <= 1) {
				return;
			}

			for(var i = 0; i < this.Catalogs.Count; i++) {
				if(this.Catalogs[i].Url == TabControlSelectedItem.Value.Url) {
					var p = i + 1;
					if(this.Catalogs.Count <= p) {
						TabControlSelectedItem.Value = this.Catalogs.First();
					} else {
						TabControlSelectedItem.Value = this.Catalogs.Skip(p).First();
					}
					break;
				}
			}
		}
		private void OnKeyBindingPreviouseCatalogTab() {
			if(TabControlSelectedItem.Value == null) {
				TabControlSelectedItem.Value = this.Catalogs.LastOrDefault();
				return;
			}
			if(this.Catalogs.Count <= 1) {
				return;
			}

			for(var i = 0; i < this.Catalogs.Count; i++) {
				if(this.Catalogs[i].Url == TabControlSelectedItem.Value.Url) {
					var p = i - 1;
					if(p < 0) {
						TabControlSelectedItem.Value = this.Catalogs.Last();
					} else {
						TabControlSelectedItem.Value = this.Catalogs.Skip(p).First();
					}
					break;
				}
			}
		}
		private void OnKeyBindingCurrentThreadTabUpdate()
			=> FutabaThreadResViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ThreadUpdateCommandMessage>>()
				.Publish(new FutabaThreadResViewerViewModel.ThreadUpdateCommandMessage(
					this.ThreadTabSelectedItem.Value?.Futaba.Value));

		private void OnKeyBindingCurrentThreadTabSearch()
			=> FutabaThreadResViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ThreadSearchCommandMessage>>()
				.Publish(new FutabaThreadResViewerViewModel.ThreadSearchCommandMessage(
					this.ThreadTabSelectedItem.Value?.Futaba.Value));
		private void OnKeyBindingCurrentThreadTabPost()
			=> FutabaThreadResViewerViewModel.Messenger.Instance.GetEvent<PubSubEvent<ViewModels.FutabaThreadResViewerViewModel.ThreadOpenPostCommandMessage>>()
				.Publish(new FutabaThreadResViewerViewModel.ThreadOpenPostCommandMessage(
					this.ThreadTabSelectedItem.Value?.Futaba.Value));
		private void OnKeyBindingCurrentThreadTabClose()
			=> OnCatalogThreadClose(this.ThreadTabSelectedItem.Value?.Futaba.Value);
		private void OnKeyBindingNextThreadTab() {
			if(ThreadTabSelectedItem.Value == null) {
				ThreadTabSelectedItem.Value = this.Threads.Value.FirstOrDefault();
				return;
			}
			if(this.Threads.Value.Count <= 1) {
				return;
			}

			for(var i = 0; i < this.Threads.Value.Count; i++) {
				if(this.Threads.Value[i].Url == ThreadTabSelectedItem.Value.Url) {
					var p = i + 1;
					if(this.Threads.Value.Count <= p) {
						ThreadTabSelectedItem.Value = this.Threads.Value.First();
					} else {
						ThreadTabSelectedItem.Value = this.Threads.Value.Skip(p).First();
					}
					break;
				}
			}
		}
		private void OnKeyBindingPreviouseThreadTab() {
			if(ThreadTabSelectedItem.Value == null) {
				ThreadTabSelectedItem.Value = this.Threads.Value.LastOrDefault();
				return;
			}
			if(this.Threads.Value.Count <= 1) {
				return;
			}

			for(var i =0; i< this.Threads.Value.Count; i++) {
				if(this.Threads.Value[i].Url == ThreadTabSelectedItem.Value.Url) {
					var p = i - 1;
					if(p < 0) {
						ThreadTabSelectedItem.Value = this.Threads.Value.Last();
					} else {
						ThreadTabSelectedItem.Value = this.Threads.Value.Skip(p).First();
					}
					break;
				}
			}
		}
	}
}
