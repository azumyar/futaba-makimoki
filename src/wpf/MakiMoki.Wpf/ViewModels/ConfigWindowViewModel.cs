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
using Yarukizero.Net.MakiMoki.Wpf.PlatformData;
using Yarukizero.Net.MakiMoki.Wpf.WpfConfig;
using Yarukizero.Net.MakiMoki.Wpf.Canvas98.Canvas98Config;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class ConfigWindowViewModel : BindableBase, IDisposable {
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

		public class Canvas98ExtendItem : IDisposable {
			public ReactiveProperty<string> Name { get; }
			public ReactiveProperty<string> Bookmarklet { get; }

			public Canvas98ExtendItem(string name, string bookmarklet) {
				this.Name = new ReactiveProperty<string>(name);
				this.Bookmarklet = new ReactiveProperty<string>(bookmarklet);
			}

			public void Dispose() {
				Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
			}
		}

		public ReactiveProperty<bool> CoreConfigThreadDataIncremental { get; }
		public ReactiveProperty<bool> CoreConfigSavedResponse { get; }


		public ReactiveCollection<ConfigListBoxItem> Bords { get; } = new ReactiveCollection<ConfigListBoxItem>();

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

		public ReactiveProperty<string> Canvas98Bookmarklet { get; }
		public ReactiveProperty<string> Canvas98ExtendsName { get; } = new ReactiveProperty<string>(initialValue: "");
		public ReactiveProperty<string> Canvas98ExtendsBookmarklet { get; } = new ReactiveProperty<string>(initialValue: "");
		public ReactiveCollection<Canvas98ExtendItem> Canvas98Extends { get; } = new ReactiveCollection<Canvas98ExtendItem>();
		public ReactiveCommand Canvas98ExtendAddCommand { get; }
		public ReactiveCommand<Canvas98ExtendItem> Canvas98ExtendRemoveCommand  { get; } = new ReactiveCommand<Canvas98ExtendItem>();
		public ReactiveProperty<Visibility> WebView2RuntimeVisiblity { get; }

		public ReactiveProperty<bool> OptoutAppCenterCrashes { get; }

		public ReactiveProperty<bool> IsEnabledOkButton { get; }


		public ReactiveCommand<ConfigListBoxItem> ConfigEditCommand { get; } = new ReactiveCommand<ConfigListBoxItem>();
		public ReactiveCommand<ConfigListBoxItem> ConfigRemoveCommand { get; } = new ReactiveCommand<ConfigListBoxItem>();


		public ReactiveCommand<RoutedEventArgs> OkButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CancelButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<Uri> LinkClickCommand { get; } = new ReactiveCommand<Uri>();


		public ConfigWindowViewModel() {
			CoreConfigThreadDataIncremental = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental);
			CoreConfigSavedResponse = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaResponseSave);

			Bords.AddRangeOnScheduler(
				Config.ConfigLoader.UserConfBord.Bords
					.OrderBy(x => x.SortIndex)
					.Select(x => new ConfigListBoxItem(Bords, x.Name, x, true)));

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

			Canvas98Bookmarklet = new ReactiveProperty<string>(Canvas98ConfigLoader.Bookmarklet.Value.Bookmarklet ?? "");
			Canvas98Extends.AddRangeOnScheduler(
				Canvas98.Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ExtendBookmarklet
					.Select(x => new Canvas98ExtendItem(x.Key, x.Value)));
			Canvas98ExtendAddCommand = new[] {
				Canvas98ExtendsName.Select(x => !string.IsNullOrWhiteSpace(x)),
				Canvas98ExtendsBookmarklet.Select(x => !string.IsNullOrWhiteSpace(x)),
			}.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand();
			Canvas98ExtendAddCommand.Subscribe(_ => OnCanvas98ExtendAdd());
			Canvas98ExtendRemoveCommand.Subscribe(x => OnCanvas98ExtendAdd(x));
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
			ConfigEditCommand.Subscribe(x => OnConfigEditButtonClick(x));
			ConfigRemoveCommand.Subscribe(x => OnConfigRemoveButtonClick(x));
			OkButtonClickCommand.Subscribe(x => OnOkButtonClick(x));
			CancelButtonClickCommand.Subscribe(x => OnCancelButtonClick(x));
			LinkClickCommand.Subscribe(x => OnLinkClick(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void OnConfigEditButtonClick(ConfigListBoxItem _) {
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
			Ng.NgConfig.NgConfigLoader.UpdateNgImageMethod((Ng.NgData.ImageNgMethod)NgConfigImageMethod.Value);
			Ng.NgConfig.NgConfigLoader.UpdateNgImageThreshold(NgConfigImageThreshold.Value);

			var s = PlatformData.WpfConfig.Create(
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
				isEnabledThreadCommandPalette: IsEnabledThreadCommandPalette.Value
			);
			WpfConfig.WpfConfigLoader.UpdateSystemConfig(s);
			Canvas98ConfigLoader.UpdateBookmarklet(
				Canvas98Bookmarklet.Value,
				Canvas98Extends
					.Select(x => (x.Name.Value, x.Bookmarklet.Value))
					.ToArray());
			// 今のところひとつしかないので不用意に設定ファイルを作らない
			if(Config.ConfigLoader.Optout.AppCenterCrashes != OptoutAppCenterCrashes.Value) {
				Config.ConfigLoader.UpdateOptout(Data.MakiMokiOptout.From(
					appcenterCrashes: OptoutAppCenterCrashes.Value
				));
			}
		}

		private void OnCancelButtonClick(RoutedEventArgs _) { }

		private void OnLinkClick(Uri e) {
			WpfUtil.PlatformUtil.StartBrowser(e);
		}

		private void OnCanvas98ExtendAdd() {
			if(!string.IsNullOrWhiteSpace(Canvas98ExtendsName.Value)
				&& !string.IsNullOrWhiteSpace(Canvas98ExtendsBookmarklet.Value)) {

				Canvas98Extends.AddOnScheduler(
					new Canvas98ExtendItem(
						Canvas98ExtendsName.Value, Canvas98ExtendsBookmarklet.Value));
				Canvas98ExtendsName.Value = Canvas98ExtendsBookmarklet.Value = "";
			}
		}

		private void OnCanvas98ExtendAdd(Canvas98ExtendItem x) {
			Canvas98Extends.Remove(x);
		}
	}
}
