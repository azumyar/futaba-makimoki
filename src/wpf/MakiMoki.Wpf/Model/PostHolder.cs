using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Windows.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Yarukizero.Net.MakiMoki.Reactive;
using Yarukizero.Net.MakiMoki.Data;
using System.IO;
using System.Windows;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	public class PostHolder : Bindable.CommonPostHolder {
		private static readonly string FallbackUnicodeString = "\a";
		private static readonly Encoding FutabaEncoding = Encoding.GetEncoding(
			"Shift_JIS",
			new EncoderReplacementFallback(FallbackUnicodeString),
			DecoderFallback.ReplacementFallback);
		private static string GetDefaultSubject(string defaultValue = "") => Config.ConfigLoader.MakiMoki.FutabaPostSavedSubject ? Config.ConfigLoader.FutabaApi.SavedSubject : defaultValue;
		private static string GetDefaultName(string defaultValue = "") => Config.ConfigLoader.MakiMoki.FutabaPostSavedName ? Config.ConfigLoader.FutabaApi.SavedName : defaultValue;
		private static string GetDefaultMail(string defaultValue = "") => Config.ConfigLoader.MakiMoki.FutabaPostSavedMail ? Config.ConfigLoader.FutabaApi.SavedMail : defaultValue;

		public BoardData Board { get; }
		public UrlContext Url { get; }

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

		public ReactiveProperty<string> ImageName { get; }
		public ReactiveProperty<ImageSource> ImagePreview { get; }

		public ReactiveProperty<bool> CommentValidFlag { get; }
		public ReactiveProperty<bool> ImageValidFlag { get; }
		public ReactiveProperty<bool> CommentImageValidFlag { get; }
		public ReactiveProperty<bool> PasswordValidFlag { get; }

		public MakiMokiCommand PostButtonCommand { get; }
		public ReactiveProperty<string> PostTitle { get; }
		public ReactiveProperty<Visibility> PostNameVisibility { get; }
		public ReactiveProperty<Visibility> PostImageVisibility { get; }
		public ReactiveProperty<Visibility> PostIpOptionVisibility { get; }
		public ReactiveProperty<Visibility> PostIdOptionVisibility { get; }

		public ReactiveProperty<object> UpdateToken { get; } = new ReactiveProperty<object>(DateTime.Now);
		public PostHolder(BoardData board, UrlContext url) {
			this.Board = board;
			this.Url = url;
			this.ImageName = this.ImagePath.Select(x => {
				if(string.IsNullOrWhiteSpace(x)) {
					return "";
				} else {
					return Path.GetFileName(x);
				}
			}).ToReactiveProperty("");
			this.ImagePreview = this.ImagePath
				.ObserveOn(UIDispatcherScheduler.Default)
				.Select<string, ImageSource>(x => {

				if(File.Exists(x)) {
					var ext = Path.GetExtension(x).ToLower();
					var imageExt = Config.ConfigLoader.MimeFutaba.Types
						.Where(y => y.MimeContents == MimeContents.Image)
						.Select(y => y.Ext)
						.ToArray();
					var movieExt = Config.ConfigLoader.MimeFutaba.Types
						.Where(y => y.MimeContents == MimeContents.Video)
						.Select(y => y.Ext)
						.ToArray();
					if(imageExt.Contains(ext)) {
						return WpfUtil.ImageUtil.CreateImage(
							x,
							WpfUtil.ImageUtil.LoadStream(x));
					} else if(movieExt.Contains(ext)) {
						return WpfUtil.MediaFoundationUtil.CreateThumbnail(x);
					}
				}
				return null;
			}).ToReactiveProperty();
			this.PostTitle = new ReactiveProperty<string>(url.IsCatalogUrl ? "スレッド作成" : "レス投稿");
			this.PostNameVisibility = new ReactiveProperty<Visibility>(
				(board.Extra.Name) ? Visibility.Visible : Visibility.Collapsed);
			if(url.IsCatalogUrl) {
				this.PostImageVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
				this.PostIpOptionVisibility = new ReactiveProperty<Visibility>(
					board.Extra.MailIp ? Visibility.Visible : Visibility.Collapsed);
				this.PostIdOptionVisibility = new ReactiveProperty<Visibility>(
					board.Extra.MailId ? Visibility.Visible : Visibility.Collapsed);
			} else {
				this.PostImageVisibility = new ReactiveProperty<Visibility>(
					board.Extra.ResImage ? Visibility.Visible : Visibility.Collapsed);
				this.PostIpOptionVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
				this.PostIdOptionVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
			}

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
			WpfConfig.WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler(this.UpdateFromConfig);
		}

		public void Reset() {
			Comment.Value = "";
			Name.Value = GetDefaultName();
			Mail.Value = GetDefaultMail();
			Subject.Value = GetDefaultSubject();
			Password.Value = Config.ConfigLoader.FutabaApi.SavedPassword;
			ImagePath.Value = "";
		}

		private void UpdateFromConfig() {
			Name.Value = GetDefaultName(Name.Value);
			Mail.Value = GetDefaultMail(Mail.Value);
			Subject.Value = GetDefaultSubject(Subject.Value);
			Password.Value = Config.ConfigLoader.FutabaApi.SavedPassword;

			this.UpdateFromConfig(default(PlatformData.WpfConfig));
		}

		private void UpdateFromConfig(PlatformData.WpfConfig _) {
			UpdateToken.Value = DateTime.Now;
		}
	}
}