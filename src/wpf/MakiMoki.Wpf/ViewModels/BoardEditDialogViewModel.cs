using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows;
using Newtonsoft.Json;
using Yarukizero.Net.MakiMoki.Reactive;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class BoardEditDialogViewModel : BindableBase, IDialogAware, IDisposable {
		public string Title { get { return "板設定"; } }

		public event Action<IDialogResult> RequestClose;

		public ReactiveProperty<string> Name { get; }
		public ReactiveProperty<string> Url { get; }
		public ReactiveProperty<string> DefaultComment { get; }
		public ReactiveProperty<string> SortIndex { get; }
		public ReactiveProperty<string> MaxThreadCount { get; }
		public ReactiveProperty<string> MaxThreadTime { get; }
		public ReactiveProperty<bool> IsNameValid { get; }
		public ReactiveProperty<bool> IsUrlValid { get; }
		public ReactiveProperty<bool> IsDefaultCommentValid { get; }
		public ReactiveProperty<bool> IsSortIndexValid { get; }
		public ReactiveProperty<bool> IsMaxThreadCountValid { get; }
		public ReactiveProperty<bool> IsMaxThreadTimeValid { get; }
		public ReactiveProperty<Visibility> NameErrorVisibility { get; }
		public ReactiveProperty<Visibility> UrlErrorVisibility { get; }
		public ReactiveProperty<Visibility> DefaultCommentErrorVisibility { get; }
		public ReactiveProperty<Visibility> SortIndexErrorVisibility { get; }
		public ReactiveProperty<Visibility> MaxThreadCountErrorVisibility { get; }
		public ReactiveProperty<Visibility> NMaxThreadTimeErrorVisibility { get; }

		public ReactiveProperty<bool> IsEnabledName { get; }
		public ReactiveProperty<bool> IsEnabledResImage { get; }
		public ReactiveProperty<bool> IsEnabledTegaki { get; }
		public ReactiveProperty<bool> IsEnabledMailIp { get; }
		public ReactiveProperty<bool> IsEnabledMailId { get; }
		public ReactiveProperty<bool> IsAlwaysIp { get; }
		public ReactiveProperty<bool> IsAlwaysId { get; }

		public ReactiveProperty<bool> IsEnabledPassiveReload { get; }

		public MakiMokiCommand OkButtonCommand { get; }
		public MakiMokiCommand CancelButtonCommand { get; } = new MakiMokiCommand();

		public BoardEditDialogViewModel() {
			static Visibility ToErrorVisibility(bool v) { return v ? Visibility.Hidden : Visibility.Visible; }

			var defaultExtra = JsonConvert.DeserializeObject<Data.BoardDataExtra>("{}");
			var defaultMakiMokiExtra = JsonConvert.DeserializeObject<Data.MakiMokiBoardDataExtra>("{}");
			Name = new ReactiveProperty<string>("");
			Url = new ReactiveProperty<string>("");
			DefaultComment = new ReactiveProperty<string>("本文無し");
			SortIndex = new ReactiveProperty<string>("1");
			MaxThreadCount = new ReactiveProperty<string>("");
			MaxThreadTime = new ReactiveProperty<string>("");
			IsNameValid = Name.Select(x => !string.IsNullOrWhiteSpace(x)).ToReactiveProperty();
			IsUrlValid = Url.Select(x => Regex.IsMatch(x, @"^https?://[^\.]+\.2chan\.net/[^/]+/$")).ToReactiveProperty();
			IsDefaultCommentValid = DefaultComment.Select(x => !string.IsNullOrWhiteSpace(x)).ToReactiveProperty();
			IsSortIndexValid = SortIndex.Select(x => ushort.TryParse(x, out var _)).ToReactiveProperty();
			IsMaxThreadCountValid = MaxThreadCount.Select(x => (x == "") || ushort.TryParse(x, out var _)).ToReactiveProperty();
			IsMaxThreadTimeValid = MaxThreadTime.Select(x => (x == "") || ushort.TryParse(x, out var _)).ToReactiveProperty();
			NameErrorVisibility = IsNameValid.Select(x => ToErrorVisibility(x)).ToReactiveProperty();
			UrlErrorVisibility = IsUrlValid.Select(x => ToErrorVisibility(x)).ToReactiveProperty();
			DefaultCommentErrorVisibility = IsDefaultCommentValid.Select(x => ToErrorVisibility(x)).ToReactiveProperty();
			SortIndexErrorVisibility = IsSortIndexValid.Select(x => ToErrorVisibility(x)).ToReactiveProperty();
			MaxThreadCountErrorVisibility = IsMaxThreadCountValid.Select(x => ToErrorVisibility(x)).ToReactiveProperty();
			NMaxThreadTimeErrorVisibility = IsMaxThreadTimeValid.Select(x => ToErrorVisibility(x)).ToReactiveProperty();

			IsEnabledName = new ReactiveProperty<bool>(defaultExtra.Name);
			IsEnabledResImage = new ReactiveProperty<bool>(defaultExtra.ResImage);
			IsEnabledTegaki = new ReactiveProperty<bool>(defaultExtra.ResTegaki);
			IsEnabledMailIp = new ReactiveProperty<bool>(defaultExtra.MailIp);
			IsEnabledMailId = new ReactiveProperty<bool>(defaultExtra.MailId);
			IsAlwaysIp = new ReactiveProperty<bool>(defaultExtra.AlwaysIp);
			IsAlwaysId = new ReactiveProperty<bool>(defaultExtra.AlwaysId);

			IsEnabledPassiveReload = new ReactiveProperty<bool>(defaultMakiMokiExtra.IsEnabledPassiveReload);

			OkButtonCommand = new[] {
				IsNameValid,
				IsUrlValid,
				IsDefaultCommentValid,
				IsSortIndexValid,
				IsMaxThreadCountValid,
				IsMaxThreadTimeValid,
			}.CombineLatestValuesAreAllTrue()
				.ToMakiMokiCommand();
			OkButtonCommand.Subscribe(_ => OnOkButtonClick());
			CancelButtonCommand.Subscribe(_ => OnCancelButtonClick());
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public bool CanCloseDialog() {
			return true;
		}

		public void OnDialogOpened(IDialogParameters parameters) {
			if(parameters.TryGetValue<Data.BoardData>(
				typeof(Data.BoardData).FullName,
				out var bd)) {

				Name.Value = bd.Name;
				Url.Value = bd.Url;
				DefaultComment.Value = bd.DefaultComment;
				SortIndex.Value = bd.SortIndex.ToString();
				MaxThreadCount.Value = bd.Extra.MaxStoredRes.ToString();
				MaxThreadTime.Value = bd.Extra.MaxStoredTime.ToString();

				IsEnabledName.Value = bd.Extra.Name;
				IsEnabledResImage.Value = bd.Extra.ResImage;
				IsEnabledTegaki.Value = bd.Extra.ResTegaki;
				IsEnabledMailIp.Value = bd.Extra.MailIp;
				IsEnabledMailId.Value = bd.Extra.MailId;
				IsAlwaysIp.Value = bd.Extra.AlwaysIp;
				IsAlwaysId.Value = bd.Extra.AlwaysId;

				IsEnabledPassiveReload.Value = bd.MakiMokiExtra.IsEnabledPassiveReload;
			}
		}

		public void OnDialogClosed() {}

		private void OnOkButtonClick() {
			var sortIndex = 0;
			var maxStoredRes = 0;
			var maxStoredTime = 0;
			if(!int.TryParse(SortIndex.Value, out sortIndex)) {
				sortIndex = 0;
			}
			if(!int.TryParse(MaxThreadCount.Value, out maxStoredRes)) {
				maxStoredRes = 0;
			}
			if(!int.TryParse(MaxThreadTime.Value, out maxStoredRes)) {
				maxStoredTime = 0;
			}

			var bd = Data.BoardData.From(
				name: Name.Value,
				url: Url.Value,
				defaultComment: DefaultComment.Value,
				sortIndex: sortIndex,
				extra: Data.BoardDataExtra.From(
					name: IsEnabledName.Value,
					resImage: IsEnabledResImage.Value,
					mailIp: IsEnabledMailIp.Value,
					mailId: IsEnabledMailId.Value,
					alwaysIp: IsAlwaysIp.Value,
					alwaysId: IsAlwaysId.Value,
					maxStoredRes: maxStoredRes,
					maxStoredTime: maxStoredTime,
					resTegaki: IsEnabledTegaki.Value),
				makimokiExtra: Data.MakiMokiBoardDataExtra.From(
					isEnabledPassiveReload: IsEnabledPassiveReload.Value));
			var param = new DialogParameters();
			param.Add(typeof(Data.BoardData).FullName, bd);

			RequestClose?.Invoke(new DialogResult(ButtonResult.OK, param));
		}

		private void OnCancelButtonClick() {
			RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
		}
	}
}
