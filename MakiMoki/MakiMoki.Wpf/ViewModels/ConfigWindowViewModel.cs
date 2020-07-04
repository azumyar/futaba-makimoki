﻿using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using Yarukizero.Net.MakiMoki.Wpf.PlatformData;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class ConfigWindowViewModel : BindableBase, IDisposable {
		public ReactiveProperty<bool> CoreConfigThreadDataIncremental { get; }
		public ReactiveProperty<bool> CoreConfigSavedResponse { get; }


		public ReactiveProperty<bool> NgConfigIdNg { get; }
		public ReactiveProperty<string> NgConfigCatalogNgWord{ get; }
		public ReactiveProperty<string> NgConfigCatalogNgWordRegex { get; }
		public ReactiveProperty<bool> NgConfigCatalogNgWordRegexValid { get; }
		public ReactiveProperty<Visibility> NgConfigCatalogNgWordRegexVisibility { get; }
		public ReactiveProperty<string> NgConfigThreadNgWord { get; }
		public ReactiveProperty<string> NgConfigThreadNgWordRegex { get; }
		public ReactiveProperty<bool> NgConfigThreadNgWordRegexValid { get; }
		public ReactiveProperty<Visibility> NgConfigThreadNgWordRegexVisibility { get; }
		public ReactiveProperty<int> NgConfigImageMethod { get; }
		public ReactiveProperty<int> NgConfigImageThreshold { get; }

		public ReactiveProperty<string> PostItemExpireDay { get; }
		public ReactiveProperty<bool> PostItemExpireDayValid { get; }

		public ReactiveProperty<bool> CatalogIsEnabledMovieMarker { get; }
		public ReactiveProperty<bool> CatalogIsEnabledOldMarker { get; }
		public ReactiveProperty<int> CatalogNgImageAction { get; }
		public ReactiveProperty<bool> CatalogIsVisibleIsolateThread { get; }
		public ReactiveProperty<int> CatalogSearchResult { get; }
		public ReactiveProperty<int> ThreadDelResVisibility { get; }
		public ReactiveProperty<string> ClipbordJpegQuality { get; }
		public ReactiveProperty<bool> ClipbordJpegQualityValid { get; }
		public ReactiveProperty<bool> ClipbordIsEnabledUrl { get; }

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
		public ReactiveProperty<string> BrowserPath { get; }


		public ReactiveProperty<bool> IsEnabledOkButton { get; }


		public ReactiveCommand<RoutedEventArgs> OkButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CancelButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<Uri> LinkClickCommand { get; } = new ReactiveCommand<Uri>();


		public ConfigWindowViewModel() {
			CoreConfigThreadDataIncremental = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental);
			CoreConfigSavedResponse = new ReactiveProperty<bool>(Config.ConfigLoader.MakiMoki.FutabaResponseSave);

			NgConfigIdNg = new ReactiveProperty<bool>(Ng.NgConfig.NgConfigLoader.NgConfig.EnableIdNg);
			NgConfigCatalogNgWord = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.NgConfig.CatalogWords));
			NgConfigCatalogNgWordRegex = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoader.NgConfig.CatalogRegex));
			NgConfigCatalogNgWordRegexValid = NgConfigCatalogNgWordRegex.Select(x => {
				foreach(var r in x.Replace("\r", "").Split('\n')) {
					try {
						new Regex(r);
					}
					catch(ArgumentException e) {
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
						new Regex(r);
					}
					catch(ArgumentException e) {
						return false;
					}
				}
				return true;
			}).ToReactiveProperty();
			NgConfigThreadNgWordRegexVisibility = NgConfigThreadNgWordRegexValid.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			NgConfigImageMethod = new ReactiveProperty<int>((int)Ng.NgConfig.NgConfigLoader.NgImageConfig.NgMethod);
			NgConfigImageThreshold = new ReactiveProperty<int>(Ng.NgConfig.NgConfigLoader.NgImageConfig.Threshold);

			PostItemExpireDay = new ReactiveProperty<string>(Config.ConfigLoader.MakiMoki.FutabaPostDataExpireDay.ToString());
			PostItemExpireDayValid = PostItemExpireDay.Select(x => {
				if(int.TryParse(x, out var v)) {
					return 0 <= v;
				} else {
					return false;
				}
			}).ToReactiveProperty();

			CatalogIsEnabledMovieMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledMovieMarker);
			CatalogIsEnabledOldMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledOldMarker);
			CatalogNgImageAction = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.CatalogNgImage);
			CatalogIsVisibleIsolateThread = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsVisibleCatalogIsolateThread);
			CatalogSearchResult = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.CatalogSearchResult);
			ThreadDelResVisibility = new ReactiveProperty<int>((int)WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility);
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
			PostViewMaxWidth = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.MinWidthPostView.ToString());
			PostViewMaxWidthValid = PostViewMaxWidth.Select(x => {
				if(int.TryParse(x, out var v)) {
					if((v == 0) || (360 <= v)) {
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
			BrowserPath = new ReactiveProperty<string>(WpfConfig.WpfConfigLoader.SystemConfig.BrowserPath);

			IsEnabledOkButton = new[] {
				NgConfigCatalogNgWordRegexValid,
				NgConfigThreadNgWordRegexValid,
				PostItemExpireDayValid,
				ClipbordJpegQualityValid,
				MediaCacheExpireDayValid,
				PostViewMaxWidthValid,
				PostViewMinWidthValid,
				PostViewOpacityValid,
			}.CombineLatest(x => x.All(y => y)).ToReactiveProperty();
			OkButtonClickCommand.Subscribe(x => OnOkButtonClick(x));
			CancelButtonClickCommand.Subscribe(x => OnCancelButtonClick(x));
			LinkClickCommand.Subscribe(x => OnLinkClick(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private void OnOkButtonClick(RoutedEventArgs e) {
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

			Ng.NgConfig.NgConfigLoader.UpdateIdNg(NgConfigIdNg.Value);
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
				opacityPostView: int.Parse(PostViewOpacity.Value)
			);
			WpfConfig.WpfConfigLoader.UpdateSystemConfig(s);
		}

		private void OnCancelButtonClick(RoutedEventArgs e) { }

		private void OnLinkClick(Uri e) {
			WpfUtil.PlatformUtil.StartBrowser(e);
		}
	}
}
