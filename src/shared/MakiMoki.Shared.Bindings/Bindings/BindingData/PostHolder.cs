using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Yarukizero.Net.MakiMoki.Shared.Bindings.BindingData {
	class PostHolder : INotifyPropertyChanged, IDisposable {
		private static readonly string FallbackUnicodeString = "\a";
		private static readonly Encoding FutabaEncoding = Encoding.GetEncoding(
			"Shift_JIS",
			new EncoderReplacementFallback(FallbackUnicodeString),
			DecoderFallback.ReplacementFallback);
		private static string GetDefaultSubject(string defaultValue = "") => Config.ConfigLoader.MakiMoki.FutabaPostSavedSubject ? Config.ConfigLoader.FutabaApi.SavedSubject : defaultValue;
		private static string GetDefaultName(string defaultValue = "") => Config.ConfigLoader.MakiMoki.FutabaPostSavedName ? Config.ConfigLoader.FutabaApi.SavedName : defaultValue;
		private static string GetDefaultMail(string defaultValue = "") => Config.ConfigLoader.MakiMoki.FutabaPostSavedMail ? Config.ConfigLoader.FutabaApi.SavedMail : defaultValue;

#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
		public ReactiveProperty<string> Comment { get; } = new ReactiveProperty<string>("");
		public ReadOnlyReactiveProperty<string> CommentEncoded { get; }
		public ReadOnlyReactiveProperty<int> CommentBytes { get; }
		public ReadOnlyReactiveProperty<int> CommentLines { get; }

		public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>(GetDefaultName());
		public ReadOnlyReactiveProperty<string> NameEncoded { get; }
		public ReactiveProperty<string> Mail { get; } = new ReactiveProperty<string>(GetDefaultMail());
		public ReadOnlyReactiveProperty<string> MailEncoded { get; }
		public ReactiveProperty<string> Subject { get; } = new ReactiveProperty<string>(GetDefaultSubject());
		public ReadOnlyReactiveProperty<string> SubjectEncoded { get; }
		public ReactiveProperty<string> Password { get; } = new ReactiveProperty<string>(
			Config.ConfigLoader.FutabaApi.SavedPassword);
		public ReactiveProperty<string> ImagePath { get; } = new ReactiveProperty<string>("");

		public ReactiveProperty<bool> CommentValidFlag { get; }
		public ReactiveProperty<bool> ImageValidFlag { get; }
		public ReactiveProperty<bool> CommentImageValidFlag { get; }
		public ReactiveProperty<bool> PasswordValidFlag { get; }

		public MakiMokiCommand PostButtonCommand { get; }

		public PostHolder() {

			this.CommentEncoded = this.Comment
				.Select(x => Util.TextUtil.ConvertUnicodeTextToFutabaComment(x))
				.ToReadOnlyReactiveProperty();
			this.CommentBytes = this.CommentEncoded
				.Select(x => Util.TextUtil.GetTextFutabaByteCount(x))
				.ToReadOnlyReactiveProperty();
			this.CommentLines = this.Comment
				.Select(x => (x.Length == 0) ? 0 : (x.Where(y => y == '\n').Count() + 1))
				.ToReadOnlyReactiveProperty();
			this.NameEncoded = this.Name
				.Select(x => Util.TextUtil.ConvertUnicodeTextToFutabaComment(x))
				.ToReadOnlyReactiveProperty();
			this.MailEncoded = this.Mail
				.Select(x => Util.TextUtil.ConvertUnicodeTextToFutabaComment(x))
				.ToReadOnlyReactiveProperty();
			this.SubjectEncoded = this.Subject
				.Select(x => Util.TextUtil.ConvertUnicodeTextToFutabaComment(x))
				.ToReadOnlyReactiveProperty();

			this.CommentValidFlag = this.Comment.Select(x => x.Length != 0).ToReactiveProperty();
			this.ImageValidFlag = this.ImagePath.Select(x => x.Length != 0).ToReactiveProperty();
			this.CommentImageValidFlag = new[] { this.CommentValidFlag, this.ImageValidFlag }
				.CombineLatest(x => x.Any(y => y))
				.ToReactiveProperty();
			this.PasswordValidFlag = this.Password.Select(x => x.Length != 0).ToReactiveProperty();
			this.PostButtonCommand = new[] { CommentImageValidFlag, PasswordValidFlag }
				.CombineLatestValuesAreAllTrue()
				.ToMakiMokiCommand();

			Config.ConfigLoader.PostConfigUpdateNotifyer.AddHandler(this.UpdateFromConfig);
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public void Reset() {
			Comment.Value = "";
			Name.Value = GetDefaultName();
			Mail.Value = GetDefaultMail();
			Subject.Value = GetDefaultSubject();
			Password.Value = Config.ConfigLoader.FutabaApi.SavedPassword;
			ImagePath.Value = "";
		}

		public void UpdateFromConfig() {
			Name.Value = GetDefaultName(Name.Value);
			Mail.Value = GetDefaultMail(Mail.Value);
			Subject.Value = GetDefaultSubject(Subject.Value);
			Password.Value = Config.ConfigLoader.FutabaApi.SavedPassword;
		}
	}
}
