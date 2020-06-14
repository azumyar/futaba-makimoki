using Prism.Events;
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

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class ConfigWindowViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		internal class DialogCloseMessage {
			public DialogCloseMessage() {}
		}

		private CompositeDisposable Disposable { get; } = new CompositeDisposable();


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


		public ReactiveProperty<bool> IsEnabledOkButton { get; }


		public ReactiveCommand<RoutedEventArgs> OkButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<RoutedEventArgs> CancelButtonClickCommand { get; } = new ReactiveCommand<RoutedEventArgs>();
		public ReactiveCommand<Uri> LinkClickCommand { get; } = new ReactiveCommand<Uri>();


		public ConfigWindowViewModel() {
			NgConfigIdNg = new ReactiveProperty<bool>(Ng.NgConfig.NgConfigLoder.NgConfig.EnableIdNg);
			NgConfigCatalogNgWord = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoder.NgConfig.CatalogWords));
			NgConfigCatalogNgWordRegex = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoder.NgConfig.CatalogRegex));
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
			NgConfigThreadNgWord = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoder.NgConfig.ThreadWords));
			NgConfigThreadNgWordRegex = new ReactiveProperty<string>(string.Join(Environment.NewLine, Ng.NgConfig.NgConfigLoder.NgConfig.ThreadRegex));
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
			NgConfigImageMethod = new ReactiveProperty<int>((int)Ng.NgConfig.NgConfigLoder.NgImageConfig.NgMethod);
			NgConfigImageThreshold = new ReactiveProperty<int>(Ng.NgConfig.NgConfigLoder.NgImageConfig.Threshold);

			IsEnabledOkButton = new[] { NgConfigCatalogNgWordRegexValid, NgConfigThreadNgWordRegexValid }.CombineLatest(x => x.All(y => y)).ToReactiveProperty();
			OkButtonClickCommand.Subscribe(x => OnOkButtonClick(x));
			CancelButtonClickCommand.Subscribe(x => OnCancelButtonClick(x));
			LinkClickCommand.Subscribe(x => OnLinkClick(x));
		}

		public void Dispose() {
			Disposable.Dispose();
		}

		private void OnOkButtonClick(RoutedEventArgs e) {
			Ng.NgConfig.NgConfigLoder.UpdateIdNg(NgConfigIdNg.Value);
			Ng.NgConfig.NgConfigLoder.ReplaceCatalogNgWord(
				NgConfigCatalogNgWord.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoder.ReplaceCatalogNgRegex(
				NgConfigCatalogNgWordRegex.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoder.ReplaceThreadNgWord(
				NgConfigThreadNgWord.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoder.ReplaceThreadNgRegex(
				NgConfigThreadNgWordRegex.Value
					.Replace("\r", "")
					.Split('\n')
					.Where(x => !string.IsNullOrEmpty(x))
					.ToArray());
			Ng.NgConfig.NgConfigLoder.UpdateNgImageMethod((Ng.NgData.ImageNgMethod)NgConfigImageMethod.Value);
			Ng.NgConfig.NgConfigLoder.UpdateNgImageThreshold(NgConfigImageThreshold.Value);

			Messenger.Instance.GetEvent<PubSubEvent<DialogCloseMessage>>()
				.Publish(new DialogCloseMessage());
		}

		private void OnCancelButtonClick(RoutedEventArgs e) {
			Messenger.Instance.GetEvent<PubSubEvent<DialogCloseMessage>>()
				.Publish(new DialogCloseMessage());
		}

		private void OnLinkClick(Uri e) {
			try {
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.AbsoluteUri));
			}
			catch(System.ComponentModel.Win32Exception) {
				// 関連付け実行に失敗
			}
		}

	}
}
