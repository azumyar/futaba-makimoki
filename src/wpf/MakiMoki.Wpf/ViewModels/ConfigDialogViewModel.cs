using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Yarukizero.Net.MakiMoki.Reactive;
using Yarukizero.Net.MakiMoki.Wpf.PlatformData;
using Yarukizero.Net.MakiMoki.Wpf.WpfConfig;
using Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Config;
using Prism.Services.Dialogs;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class ConfigDialogViewModel : BindableBase, IDialogAware, IDisposable {
		public class ConfigListBoxItem : IDisposable {
			[Helpers.AutoDisposable.IgonoreDispose]
			public ReactiveCollection<ConfigListBoxItem> Parent { get; }
			public ReactiveProperty<string> Name { get; }
			public ReactiveProperty<object> Item { get; }
			public ReactiveProperty<Visibility> EditButtonVisibility { get; }
			public ReactiveProperty<object> Tag { get; }

			public ConfigListBoxItem(
				ReactiveCollection<ConfigListBoxItem> parent,
				string name, object item,
				bool canEdit = false,
				object tag = null) {

				this.Parent = parent;
				this.Name = new ReactiveProperty<string>(name);
				this.Item = new ReactiveProperty<object>(item);
				this.EditButtonVisibility = new ReactiveProperty<Visibility>(
					canEdit ? Visibility.Visible : Visibility.Collapsed);
				this.Tag = new ReactiveProperty<object>(tag);
			}

			public void Dispose() {
				Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
			}
		}

		public class GestureItem : IDisposable {
			private KeyGestureConverter Converter { get; } = new KeyGestureConverter();

			public ReactiveProperty<string> Input { get; } = new ReactiveProperty<string>("");
			public ReactiveProperty<bool> InputValid { get; }
			public ReactiveProperty<bool> AddCommandEnabled { get; }
			public ReactiveProperty<Visibility> InputVisibility { get; }
			public ReactiveCollection<ConfigListBoxItem> GestureCollection { get; } = new ReactiveCollection<ConfigListBoxItem>();
			public MakiMokiCommand AddCommand { get; } = new MakiMokiCommand();

			public GestureItem(string[] items) {
				GestureCollection.AddRangeOnScheduler(
					items.Select(x => new ConfigListBoxItem(GestureCollection, x, x)));
				AddCommandEnabled = Input.Select(x => {
					try {
						if(!string.IsNullOrEmpty(x)) {
							if(Converter.ConvertFromString(x) != null) {
								return true;
							}
						}
					}
					catch(NotSupportedException) { }
					catch(ArgumentException) { }
					return false;
				}).ToReactiveProperty();
				AddCommand = AddCommandEnabled.ToMakiMokiCommand();
				InputValid = new[] {
					Input.Select(x => string.IsNullOrEmpty(x)).ToReactiveProperty(),
					AddCommandEnabled,
				}.CombineLatest(x => x.Any(y => y))
					.ToReactiveProperty();
				InputVisibility = InputValid
					.Select(x => x ? Visibility.Collapsed : Visibility.Visible)
					.ToReactiveProperty();
				AddCommand.Subscribe(_ => OnAdd());
			}

			public void Dispose() {
				Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
			}

			private void OnAdd() {
				GestureCollection.AddOnScheduler(
					new ConfigListBoxItem(
						GestureCollection,
						Input.Value,
						Input.Value));
				Input.Value = "";
			}
		}

		public string Title { get { return "設定"; } }

		public event Action<IDialogResult> RequestClose;

		public ReactiveProperty<bool> CoreConfigThreadDataIncremental { get; }
		public ReactiveProperty<bool> CoreConfigSavedResponse { get; }
		public ReactiveProperty<bool> CatalogFetchImageThumbnail { get; }


		public ReactiveCollection<ConfigListBoxItem> Boards { get; } = new ReactiveCollection<ConfigListBoxItem>();

		public ReactiveProperty<bool> NgConfigCatalogIdNg { get; }
		public ReactiveProperty<bool> NgConfigThreadIdNg { get; }
		public ReactiveProperty<string> NgConfigCatalogNgWord{ get; }
		public ReactiveProperty<string> NgConfigCatalogNgWordRegex { get; }
		public ReactiveProperty<bool> NgConfigCatalogNgWordRegexValid { get; }
		public ReactiveProperty<Visibility> NgConfigCatalogNgWordRegexVisibility { get; }
		public ReactiveProperty<string> NgConfigThreadNgWord { get; }
		public ReactiveProperty<string> NgConfigThreadNgWordRegex { get; }
		public ReactiveProperty<bool> NgConfigThreadNgWordRegexValid { get; }
		public ReactiveProperty<Visibility> NgConfigThreadNgWordRegexVisibility { get; }
		public ReactiveCollection<ConfigListBoxItem> NgConfigNgImages { get; } = new ReactiveCollection<ConfigListBoxItem>();
		public ReactiveProperty<string> NgConfigCatalogWatchWord { get; }
		public ReactiveProperty<string> NgConfigCatalogWatchWordRegex { get; }
		public ReactiveProperty<bool> NgConfigCatalogWatchWordRegexValid { get; }
		public ReactiveProperty<Visibility> NgConfigCatalogWatchWordRegexVisibility { get; }
		public ReactiveCollection<ConfigListBoxItem> NgConfigWatchImages { get; } = new ReactiveCollection<ConfigListBoxItem>();
		public ReactiveProperty<int> NgConfigImageMethod { get; }
		public ReactiveProperty<int> NgConfigImageThreshold { get; }
		public ReactiveProperty<bool> NgConfigResonInput { get; }

		public ReactiveProperty<string> PostItemExpireDay { get; }
		public ReactiveProperty<bool> PostItemExpireDayValid { get; }

		public ReactiveProperty<bool> CatalogIsEnabledMovieMarker { get; }
		public ReactiveProperty<bool> CatalogIsEnabledIdMarker { get; }
		public ReactiveProperty<bool> CatalogIsEnabledOldMarker { get; }
		public ReactiveProperty<int> CatalogNgImageAction { get; }
		public ReactiveProperty<bool> CatalogIsVisibleIsolateThread { get; }
		public ReactiveProperty<int> CatalogSearchResult { get; }
		public ReactiveProperty<int> ThreadDelResVisibility { get; }
		public ReactiveProperty<bool> ThreadIsEnabledQuotLink { get; }
		public ReactiveProperty<string> ClipbordJpegQuality { get; }
		public ReactiveProperty<bool> ClipbordJpegQualityValid { get; }
		public ReactiveProperty<bool> ClipbordIsEnabledUrl { get; }
		public ReactiveProperty<bool> IsEnabledThreadCommandPalette { get; }
		public ReactiveProperty<int> CommandPalettePosition { get; }
		public ReactiveProperty<int> Canvas98Position { get; }
		public ReactiveProperty<bool> IsEnabledFailsafeMistakePost { get; }

		public ReactiveProperty<bool> PostViewSavedSubject { get; }
		public ReactiveProperty<bool> PostViewSavedName { get; }
		public ReactiveProperty<bool> PostViewSavedMail { get; }
		public ReactiveProperty<string> PostViewMinWidth { get; }
		public ReactiveProperty<bool> PostViewMinWidthValid { get; }
		public ReactiveProperty<string> PostViewMaxWidth { get; }
		public ReactiveProperty<bool> PostViewMaxWidthValid { get; }
		public ReactiveProperty<bool> PostViewIsEnabledOpacity { get; }
		public ReactiveProperty<string> PostViewOpacity { get; }
		public ReactiveProperty<bool> PostViewOpacityValid { get; }
		public ReactiveProperty<string> MediaExportPath { get; }
		public ReactiveProperty<string> MediaCacheExpireDay { get; }
		public ReactiveProperty<bool> MediaCacheExpireDayValid { get; }
		public ReactiveProperty<int> ExportOutputNgRes { get; }
		public ReactiveProperty<int> ExportOutputNgImage { get; }
		public ReactiveProperty<bool> WindowTopmost { get; }
		public ReactiveProperty<string> BrowserPath { get; }
		public ReactiveProperty<int> WindowTheme { get; }

		public ReactiveProperty<GestureItem> GestureMainWindowCatalogUpdate { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowCatalogSearch { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowCatalogOpenPost { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowCatalogClose { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowCatalogNext { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowCatalogPrevious { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadUpdate { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadSearch { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadOpenTegaki { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadOpenPost { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadClose { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadNext { get; }
		public ReactiveProperty<GestureItem> GestureMainWindowThreadPrevious { get; }
		public ReactiveProperty<GestureItem> GesturePostViewPost { get; }
		public ReactiveProperty<GestureItem> GesturePostViewOpenImage { get; }
		public ReactiveProperty<GestureItem> GesturePostViewOpenUploader { get; }
		public ReactiveProperty<GestureItem> GesturePostViewDelete { get; }
		public ReactiveProperty<GestureItem> GesturePostViewClose { get; }
		public ReactiveProperty<GestureItem> GesturePostViewPasteImage { get; }
		public ReactiveProperty<GestureItem> GesturePostViewPasteUploader { get; }

		public ReactiveProperty<string> Canvas98Bookmarklet { get; }
		public ReactiveProperty<string> Canvas98ExtendsLayer { get; }
		public ReactiveProperty<string> Canvas98ExtendsAlbam { get; }
		public ReactiveProperty<string> Canvas98ExtendsMenu { get; }
		public ReactiveProperty<string> Canvas98ExtendsTimelapse { get; }

		public ReactiveProperty<Visibility> WebView2RuntimeVisiblity { get; }

		public ReactiveProperty<bool> OptoutAppCenterCrashes { get; }

		public ReactiveProperty<bool> IsEnabledOkButton { get; }

		public MakiMokiCommand AddBoardConfigCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand<ConfigListBoxItem> ConfigEditCommand { get; } = new MakiMokiCommand<ConfigListBoxItem>();
		public MakiMokiCommand<ConfigListBoxItem> ConfigRemoveCommand { get; } = new MakiMokiCommand<ConfigListBoxItem>();


		public MakiMokiCommand<RoutedEventArgs> OkButtonClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<RoutedEventArgs> CancelButtonClickCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();
		public MakiMokiCommand<Uri> LinkClickCommand { get; } = new MakiMokiCommand<Uri>();

		private IDialogService DialogService { get; }

		public ConfigDialogViewModel(IDialogService dialogService) {
			DialogService = dialogService;
			CoreConfigThreadDataIncremental = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental);
			CoreConfigSavedResponse = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaResponseSave);
			CatalogFetchImageThumbnail = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledFetchThumbnail);

			Boards.AddRangeOnScheduler(
				Config.ConfigLoader.UserConfBoard.Boards
					.OrderBy(x => x.SortIndex)
					.Select(x => new ConfigListBoxItem(Boards, x.Name, x, true)));

			NgConfigCatalogIdNg = new ReactiveProperty<bool>(Ng.NgConfig.NgConfigLoader.NgConfig.EnableCatalogIdNg);
			NgConfigThreadIdNg = new ReactiveProperty<bool>(Ng.NgConfig.NgConfigLoader.NgConfig.EnableThreadIdNg);
			NgConfigCatalogNgWord = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.NgConfig.CatalogWords));
			NgConfigCatalogNgWordRegex = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.NgConfig.CatalogRegex));
			NgConfigCatalogNgWordRegexValid = NgConfigCatalogNgWordRegex.Select(x => {
				foreach(var r in x.Replace("\r", "").Split('\n')) {
					try {
						_ = new Regex(r);
					}
					catch(ArgumentException) {
						return false;
					}
				}
				return true;
			}).ToReactiveProperty();
			NgConfigCatalogNgWordRegexVisibility = NgConfigCatalogNgWordRegexValid.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			NgConfigThreadNgWord = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.NgConfig.ThreadWords));
			NgConfigThreadNgWordRegex = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.NgConfig.ThreadRegex));
			NgConfigThreadNgWordRegexValid = NgConfigThreadNgWordRegex.Select(x => {
				foreach(var r in x.Replace("\r", "").Split('\n')) {
					try {
						_ = new Regex(r);
					}
					catch(ArgumentException) {
						return false;
					}
				}
				return true;
			}).ToReactiveProperty();
			NgConfigThreadNgWordRegexVisibility = NgConfigThreadNgWordRegexValid.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			NgConfigNgImages.AddRangeOnScheduler(
				Ng.NgConfig.NgConfigLoader.NgImageConfig.Images
					.Select(x => new ConfigListBoxItem(
						NgConfigNgImages,
						string.IsNullOrEmpty(x.Comment) ? "<コメントなし>" : x.Comment,
						x)));
			NgConfigCatalogWatchWord = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.WatchConfig.CatalogWords));
			NgConfigCatalogWatchWordRegex = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.WatchConfig.CatalogRegex));
			NgConfigCatalogWatchWordRegexValid = NgConfigCatalogWatchWordRegex.Select(x => {
				foreach(var r in x.Replace("\r", "").Split('\n')) {
					try {
						_ = new Regex(r);
					}
					catch(ArgumentException) {
						return false;
					}
				}
				return true;
			}).ToReactiveProperty();
			NgConfigCatalogWatchWordRegexVisibility = NgConfigCatalogWatchWordRegexValid.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			NgConfigWatchImages.AddRangeOnScheduler(
				Ng.NgConfig.NgConfigLoader.WatchImageConfig.Images
					.Select(x => new ConfigListBoxItem(
						NgConfigWatchImages,
						string.IsNullOrEmpty(x.Comment) ? "<コメントなし>" : x.Comment,
						x)));
			NgConfigImageMethod = new ReactiveProperty<int>((int)Ng.NgConfig.NgConfigLoader.NgImageConfig.NgMethod);
			NgConfigImageThreshold = new ReactiveProperty<int>(Ng.NgConfig.NgConfigLoader.NgImageConfig.Threshold);
			NgConfigResonInput = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledNgReasonInput);

			PostItemExpireDay = new ReactiveProperty<string>(Config.ConfigLoader.MakiMoki.FutabaPostDataExpireDay.ToString());
			PostItemExpireDayValid = PostItemExpireDay.Select(x => {
				if(int.TryParse(x, out var v)) {
					return 0 <= v;
				} else {
					return false;
				}
			}).ToReactiveProperty();

			CatalogIsEnabledMovieMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledMovieMarker);
			CatalogIsEnabledIdMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledIdMarker);
			CatalogIsEnabledOldMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledOldMarker);
			CatalogNgImageAction = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.CatalogNgImage);
			CatalogIsVisibleIsolateThread = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsVisibleCatalogIsolateThread);
			CatalogSearchResult = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.CatalogSearchResult);
			ThreadDelResVisibility = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility);
			ThreadIsEnabledQuotLink = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledQuotLink);
			IsEnabledThreadCommandPalette = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledThreadCommandPalette);
			CommandPalettePosition = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.CommandPalettePosition);
			Canvas98Position = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.Canvas98Position);
			IsEnabledFailsafeMistakePost = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledFailsafeMistakePost);

			ClipbordJpegQuality = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.ClipbordJpegQuality.ToString());
			ClipbordJpegQualityValid = ClipbordJpegQuality.Select(x => {
				if(int.TryParse(x, out var v)) {
					return (1 <= v) && (v <= 100);
				} else {
					return false;
				}
			}).ToReactiveProperty();
			ClipbordIsEnabledUrl = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.ClipbordIsEnabledUrl);
			PostViewSavedSubject = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaPostSavedSubject);
			PostViewSavedName = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaPostSavedName);
			PostViewSavedMail = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaPostSavedMail);
			PostViewMinWidth = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.MinWidthPostView.ToString());
			PostViewMinWidthValid = PostViewMinWidth.Select(x => {
				if(int.TryParse(x, out var v)) {
					if((v == 0) || (360 <= v)) {
						return true;
					}
				}
				return false;
			}).ToReactiveProperty();
			PostViewMaxWidth = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.MaxWidthPostView.ToString());
			PostViewMaxWidthValid = PostViewMaxWidth.Select(x => {
				if(int.TryParse(x, out var v) && int.TryParse(PostViewMinWidth.Value, out var min)) {
					if((v == 0) || ((360 <= v)) && (min <= v)) {
						return true;
					}
				}
				return false;
			}).ToReactiveProperty();
			PostViewIsEnabledOpacity = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledOpacityPostView);
			PostViewOpacity = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.OpacityPostView.ToString());
			PostViewOpacityValid = PostViewOpacity.Select(x => {
				if(int.TryParse(x, out var v)) {
					if((0 <= v) && (v <= 100)) {
						return true;
					}
				}
				return false;
			}).ToReactiveProperty();
			MediaExportPath = new ReactiveProperty<string>(string.Join(Environment.NewLine, WpfConfig.WpfConfigLoader.SystemConfig.MediaExportPath));
			MediaCacheExpireDay = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.CacheExpireDay.ToString());
			MediaCacheExpireDayValid = MediaCacheExpireDay.Select(x => {
				if(int.TryParse(x, out var v)) {
					return 0 <= v;
				} else {
					return false;
				}
			}).ToReactiveProperty();
			ExportOutputNgRes = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.ExportNgRes);
			ExportOutputNgImage = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.ExportNgImage);
			WindowTopmost = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledWindowTopmost);
			BrowserPath = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.BrowserPath);

			WindowTheme = new ReactiveProperty<int>((int)WpfConfigLoader.SystemConfig.WindowTheme);

			GestureMainWindowCatalogUpdate = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogUpdate));
			GestureMainWindowCatalogSearch = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogSearch));
			GestureMainWindowCatalogOpenPost = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogOpenPost));
			GestureMainWindowCatalogClose = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogClose));
			GestureMainWindowCatalogNext = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogNext));
			GestureMainWindowCatalogPrevious = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureCatalogPrevious));
			GestureMainWindowThreadUpdate = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadUpdate));
			GestureMainWindowThreadSearch = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadSearch));
			GestureMainWindowThreadOpenTegaki = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadOpenTegaki));
			GestureMainWindowThreadOpenPost = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadOpenPost));
			GestureMainWindowThreadClose = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadTabClose));
			GestureMainWindowThreadNext = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadTabNext));
			GestureMainWindowThreadPrevious = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGestureThreadTabPrevious));
			GesturePostViewPost = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewPost));
			GesturePostViewOpenImage = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewOpenImage));
			GesturePostViewOpenUploader = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewOpenUploader));
			GesturePostViewDelete = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewDelete));
			GesturePostViewClose = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewClose));
			GesturePostViewPasteImage = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewPasteImage));
			GesturePostViewPasteUploader = new ReactiveProperty<GestureItem>(
				new GestureItem(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewPasteUploader));

			Canvas98Bookmarklet = new ReactiveProperty<string>(Canvas98ConfigLoader.Bookmarklet.Value.Bookmarklet ?? "");
			Canvas98ExtendsLayer = new ReactiveProperty<string>(Canvas98ConfigLoader.Bookmarklet.Value.BookmarkletLayer ?? "");
			Canvas98ExtendsAlbam = new ReactiveProperty<string>(Canvas98ConfigLoader.Bookmarklet.Value.BookmarkletAlbam ?? "");
			Canvas98ExtendsMenu = new ReactiveProperty<string>(Canvas98ConfigLoader.Bookmarklet.Value.BookmarkletMenu ?? "");
			Canvas98ExtendsTimelapse = new ReactiveProperty<string>(Canvas98ConfigLoader.Bookmarklet.Value.BookmarkletTimelapse ?? "");

			WebView2RuntimeVisiblity = new ReactiveProperty<Visibility>(Canvas98.Canvas98Util.Util.IsInstalledWebView2Runtime() ? Visibility.Collapsed : Visibility.Visible);

			OptoutAppCenterCrashes = new ReactiveProperty<bool>(Config.ConfigLoader.Optout.AppCenterCrashes);

			IsEnabledOkButton = new[] {
				NgConfigCatalogNgWordRegexValid,
				NgConfigThreadNgWordRegexValid,
				NgConfigCatalogWatchWordRegexValid,
				PostItemExpireDayValid,
				ClipbordJpegQualityValid,
				MediaCacheExpireDayValid,
				PostViewMaxWidthValid,
				PostViewMinWidthValid,
				PostViewOpacityValid,
			}.CombineLatest(x => x.All(y => y)).ToReactiveProperty();
			AddBoardConfigCommand.Subscribe(_ => OnAddBoardConfig());
			ConfigEditCommand.Subscribe(x => OnConfigEditButtonClick(x));
			ConfigRemoveCommand.Subscribe(x => OnConfigRemoveButtonClick(x));
			OkButtonClickCommand.Subscribe(x => OnOkButtonClick(x));
			CancelButtonClickCommand.Subscribe(x => OnCancelButtonClick(x));
			LinkClickCommand.Subscribe(x => OnLinkClick(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public bool CanCloseDialog() {
			return true;
		}

		public void OnDialogOpened(IDialogParameters parameters) {
		}

		public void OnDialogClosed() {}

		private void OnAddBoardConfig(Data.BoardData boardData = null) {
			var key = typeof(Data.BoardData).FullName;
			var param = new DialogParameters();
			if(boardData != null) {
				param.Add(key, boardData);
			}
			DialogService.ShowDialog(
				nameof(Windows.Dialogs.BoardEditDialog),
				param,
				(x) => {
					if((x.Result == ButtonResult.OK)
						&& x.Parameters.TryGetValue<Data.BoardData>(key, out var bd)) {

						var ls = Boards
							.Select(x => x.Item.Value)
							.Cast<Data.BoardData>()
							.Where(x => (boardData != null) ? (x.Name != boardData.Name) : true)
							.ToList();
						for(var i = 0; i < ls.Count; i++) {
							if(ls[i].Name == bd.Name) {
								ls[i] = bd;
								goto end;
							}
						}
						ls.Add(bd);
					end:
						Boards.Clear();
						Boards.AddRangeOnScheduler(ls
							.OrderBy(x => x.SortIndex)
							.Select(x => new ConfigListBoxItem(Boards, x.Name, x, true)));
					}
				});
		}

		private void OnConfigEditButtonClick(ConfigListBoxItem x) {
			if(x.Item.Value is Data.BoardData bd) {
				OnAddBoardConfig(bd);
			}
		}

		private void OnConfigRemoveButtonClick(ConfigListBoxItem x) {
			x.Parent.Remove(x);
		}


		private void OnOkButtonClick(RoutedEventArgs _) {
			Config.ConfigLoader.UpdateMakiMokiConfig(
				threadGetIncremental: CoreConfigThreadDataIncremental.Value,
				responseSave: CoreConfigSavedResponse.Value,
				postDataExpireDay: int.Parse(PostItemExpireDay.Value),
				// 2020070500
				isSavedPostSubject: PostViewSavedSubject.Value,
				isSavedPostName: PostViewSavedName.Value,
				isSavedPostMail: PostViewSavedMail.Value
			);
			if(!CoreConfigSavedResponse.Value) {
				Config.ConfigLoader.RemoveSaveFutabaResponseFile();
			}
			Config.ConfigLoader.UpdateUserBoardConfig(Boards
				.Select(x => x.Item.Value)
				.Cast<Data.BoardData>()
				.ToArray());

			Ng.NgConfig.NgConfigLoader.UpdateCatalogIdNg(NgConfigCatalogIdNg.Value);
			Ng.NgConfig.NgConfigLoader.UpdateThreadIdNg(NgConfigThreadIdNg.Value);
			Ng.NgConfig.NgConfigLoader.ReplaceCatalogNgWord(
				NgConfigCatalogNgWord.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceCatalogNgRegex(
				NgConfigCatalogNgWordRegex.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceThreadNgWord(
				NgConfigThreadNgWord.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceThreadNgRegex(
				NgConfigThreadNgWordRegex.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceNgImage(
				NgConfigNgImages
					.Select(x => x.Item.Value)
					.Cast<Ng.NgData.NgImageData>()
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceWatchWord(
				NgConfigCatalogWatchWord.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceWatchRegex(
				NgConfigCatalogWatchWordRegex.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoader.ReplaceWatchImage(
				NgConfigWatchImages
					.Select(x => x.Item.Value)
					.Cast<Ng.NgData.NgImageData>()
					.ToArray());
			Ng.NgConfig.NgConfigLoader.UpdateNgImageMethod((Ng.NgData.ImageNgMethod)NgConfigImageMethod.Value);
			Ng.NgConfig.NgConfigLoader.UpdateNgImageThreshold(NgConfigImageThreshold.Value);

			WpfConfig.WpfConfigLoader.UpdateSystemConfig(PlatformData.WpfConfig.Create(
				isEnabledMovieMarker: CatalogIsEnabledMovieMarker.Value,
				isEnabledOldMarker: CatalogIsEnabledOldMarker.Value,
				catalogNgImage: (PlatformData.CatalogNgImage)CatalogNgImageAction.Value,
				threadDelResVisibility: (PlatformData.ThreadDelResVisibility)ThreadDelResVisibility.Value,
				clipbordJpegQuality: int.Parse(ClipbordJpegQuality.Value),
				clipbordIsEnabledUrl: ClipbordIsEnabledUrl.Value,
				mediaExportPath: MediaExportPath.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray(),
				cacheExpireDay: int.Parse(MediaCacheExpireDay.Value),
				exportNgRes: (PlatformData.ExportNgRes)ExportOutputNgRes.Value,
				exportNgImage: (PlatformData.ExportNgImage)ExportOutputNgImage.Value,
				browserPath: BrowserPath.Value,
				// 2020070500
				catalogSearchResult: (PlatformData.CatalogSearchResult)CatalogSearchResult.Value,
				isVisibleCatalogIsolateThread: CatalogIsVisibleIsolateThread.Value,
				minWidthPostView: int.Parse(PostViewMinWidth.Value),
				maxWidthPostView: int.Parse(PostViewMaxWidth.Value),
				isEnabledOpacityPostView: PostViewIsEnabledOpacity.Value,
				opacityPostView: int.Parse(PostViewOpacity.Value),
				// 2020071900
				isEnabledQuotLink: ThreadIsEnabledQuotLink.Value,
				windowTopmost: WindowTopmost.Value,
				ngResonInput: NgConfigResonInput.Value,
				//2020102900
				windowTheme: (PlatformData.WindowTheme)WindowTheme.Value,
				isEnabledIdMarker: CatalogIsEnabledIdMarker.Value,
				isEnabledThreadCommandPalette: IsEnabledThreadCommandPalette.Value,
				// 2021012000
				isEnabledFetchThumbnail: CatalogFetchImageThumbnail.Value,
				commandPalettePosition: (PlatformData.UiPosition)CommandPalettePosition.Value,
				canvas98Position: (PlatformData.UiPosition)Canvas98Position.Value,
				// 2021020100
				isEnabledFailsafeMistakePost: IsEnabledFailsafeMistakePost.Value
			));
			WpfConfig.WpfConfigLoader.UpdateGestureConfig(PlatformData.GestureConfig.From(
				keyGestureCatalogUpdate: GestureMainWindowCatalogUpdate.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureCatalogSearch: GestureMainWindowCatalogSearch.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureCatalogOpenPost: GestureMainWindowCatalogOpenPost.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureCatalogClose: GestureMainWindowCatalogClose.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureCatalogNext: GestureMainWindowCatalogNext.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureCatalogPrevious: GestureMainWindowCatalogPrevious.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),

				keyGestureThreadUpdate: GestureMainWindowThreadUpdate.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureThreadSearch: GestureMainWindowThreadSearch.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureThreadOpenTegaki: GestureMainWindowThreadOpenTegaki.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureThreadOpenPost: GestureMainWindowThreadOpenPost.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureThreadTabClose: GestureMainWindowThreadClose.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureThreadTabNext: GestureMainWindowThreadNext.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGestureThreadTabPrevious: GestureMainWindowThreadPrevious.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),

				keyGesturePostViewPost: GesturePostViewPost.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGesturePostViewOpenImage: GesturePostViewOpenImage.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGesturePostViewOpenUploader: GesturePostViewOpenUploader.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGesturePostViewDelete: GesturePostViewDelete.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGesturePostViewClose: GesturePostViewClose.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGesturePostViewPasteImage: GesturePostViewPasteImage.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),
				keyGesturePostViewPasteUploader: GesturePostViewPasteUploader.Value.GestureCollection.Select(x => x.Item.Value.ToString()).ToArray(),

				// 2022063000
				mouseGestureCatalogOpenPost: WpfConfigLoader.Gesture.MouseGestureCatalogOpenPost,
				mouseGestureCatalogUpdate: WpfConfigLoader.Gesture.MouseGestureCatalogUpdate,
				mouseGestureCatalogClose: WpfConfigLoader.Gesture.MouseGestureThreadClose,
				mouseGestureCatalogNext: WpfConfigLoader.Gesture.MouseGestureCatalogNext,
				mouseGestureCatalogPrevious: WpfConfigLoader.Gesture.MouseGestureCatalogPrevious,

				mouseGestureThreadOpenPost: WpfConfigLoader.Gesture.MouseGestureThreadOpenPost,
				mouseGestureThreadUpdate: WpfConfigLoader.Gesture.MouseGestureThreadUpdate,
				mouseGestureThreadClose: WpfConfigLoader.Gesture.MouseGestureThreadClose,
				mouseGestureThreadNext: WpfConfigLoader.Gesture.MouseGestureThreadNext,
				mouseGestureThreadPrevious: WpfConfigLoader.Gesture.MouseGestureThreadPrevious
			));
			Canvas98ConfigLoader.UpdateBookmarklet(
				Canvas98Bookmarklet.Value,
				Canvas98ExtendsLayer.Value,
				Canvas98ExtendsAlbam.Value,
				Canvas98ExtendsMenu.Value,
				Canvas98ExtendsTimelapse.Value);
			// 今のところひとつしかないので不用意に設定ファイルを作らない
			if(Config.ConfigLoader.Optout.AppCenterCrashes != OptoutAppCenterCrashes.Value) {
				Config.ConfigLoader.UpdateOptout(Data.MakiMokiOptout.From(
					appcenterCrashes: OptoutAppCenterCrashes.Value
				));
			}
			RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
		}

		private void OnCancelButtonClick(RoutedEventArgs _) { 
			RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
		}

		private void OnLinkClick(Uri e) {
			WpfUtil.PlatformUtil.StartBrowser(e);
		}
	}
}
