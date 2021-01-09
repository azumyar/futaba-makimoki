using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.ComponentModel;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using System.Reactive.Concurrency;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Ng.NgData;
using Yarukizero.Net.MakiMoki.Util;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	public class BindableFutaba : INotifyPropertyChanged, IDisposable {
		public class PostHolder : INotifyPropertyChanged, IDisposable {

			private static readonly string FallbackUnicodeString = "\a";
			private static readonly Encoding FutabaEncoding = Encoding.GetEncoding(
				"Shift_JIS",
				new EncoderReplacementFallback(FallbackUnicodeString),
				DecoderFallback.ReplacementFallback);
			private static string GetDefaultSubject() => Config.ConfigLoader.MakiMoki.FutabaPostSavedSubject ? Config.ConfigLoader.FutabaApi.SavedSubject : "";
			private static string GetDefaultName() => Config.ConfigLoader.MakiMoki.FutabaPostSavedName ? Config.ConfigLoader.FutabaApi.SavedName : "";
			private static string GetDefaultMail() => Config.ConfigLoader.MakiMoki.FutabaPostSavedMail ? Config.ConfigLoader.FutabaApi.SavedMail : "";

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

			public ReactiveProperty<string> ImageName { get; }
			public ReactiveProperty<ImageSource> ImagePreview { get; }

			public ReactiveProperty<bool> CommentValidFlag { get; }
			public ReactiveProperty<bool> ImageValidFlag { get; }
			public ReactiveProperty<bool> CommentImageValidFlag { get; }
			public ReactiveProperty<bool> PasswordValidFlag { get; }

			public ReactiveCommand PostButtonCommand { get; }

			public PostHolder() {
				this.ImageName = this.ImagePath.Select(x => {
					if(string.IsNullOrWhiteSpace(x)) {
						return "";
					} else {
						return Path.GetFileName(x);
					}
				}).ToReactiveProperty("");
				this.ImagePreview = this.ImagePath.Select<string, ImageSource>(x => {
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
							return WpfUtil.ImageUtil.LoadImage(x);
						} else if(movieExt.Contains(ext)) {
							// 動画は今は何もしない
							// TODO: なんんか実装する
						}
					}
					return null;
				}).ToReactiveProperty();

				this.CommentEncoded = this.Comment
					.Select(x =>Util.TextUtil.ConvertUnicodeTextToFutabaComment(x))
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
					.ToReactiveCommand();
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
		}
#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
		[Helpers.AutoDisposable.IgonoreDispose]
		public ReactiveCollection<BindableFutabaResItem> ResItems { get; }
		public ReactiveProperty<FutabaContext[]> OpenedThreads { get; }
		public ReactiveProperty<int> ResCount { get; }
		public ReactiveProperty<string> DieTextLong { get; }

		public ReactiveProperty<string> PostTitle { get; }
		public ReactiveProperty<PostHolder> PostData { get; }

		public ReactiveProperty<Visibility> PostNameVisibility { get; }
		public ReactiveProperty<Visibility> PostImageVisibility { get; }
		public ReactiveProperty<Visibility> PostIpOptionVisibility { get; }
		public ReactiveProperty<Visibility> PostIdOptionVisibility { get; }

		public ReactiveCommand<MouseButtonEventArgs> OpenImageCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> DeleteImageCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand DeletePostDataCommand { get; } = new ReactiveCommand();
		public ReactiveCommand MailSageClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand MailIdClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand MailIpClickCommand { get; } = new ReactiveCommand();

		public ReactiveCommand ExportCommand { get; } = new ReactiveCommand();

		public ReactiveProperty<Data.CatalogSortItem> CatalogSortItem { get; } = new ReactiveProperty<Data.CatalogSortItem>(Data.CatalogSort.Catalog);

		public ReactiveProperty<bool> CatalogListMode { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<Visibility> CatalogListVisibility { get; }
		public ReactiveProperty<Visibility> CatalogWrapVisibility { get; }

		public ReactiveProperty<string> FilterText { get; } = new ReactiveProperty<string>("");

		public string Name { get; }

		public Data.UrlContext Url { get; }

		public Data.FutabaContext Raw { get; }

		public ReactiveProperty<bool> IsOld { get; }
		public ReactiveProperty<bool> IsDie { get; }
		public ReactiveProperty<bool> IsMaxRes { get; }

		public ReactiveCommand FullScreenClickCommand { get; } = new ReactiveCommand();
		public ReactiveProperty<bool> IsFullScreenMode { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<int> FullScreenSpan { get; }
		public ReactiveProperty<Visibility> FullScreenVisibility { get; }

		public ReactiveProperty<int> CatalogResCount { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<object> UpdateToken { get; } = new ReactiveProperty<object>(DateTime.Now);


		private Action<Ng.NgData.NgConfig> ngUpdateAction;
		private Action<Ng.NgData.HiddenConfig> hiddenUpdateAction;
		//private Action<Ng.NgData.NgImageConfig> imageUpdateAction;
		private Action<PlatformData.WpfConfig> systemUpdateAction;

		public BindableFutaba(Data.FutabaContext futaba, BindableFutaba old = null) {
			OpenedThreads = Util.Futaba.Threads.Select(x => x.Where(y => y.Url.BaseUrl == futaba.Url.BaseUrl).ToArray()).ToReactiveProperty();
			CatalogListVisibility = CatalogListMode.Select(x => futaba.Url.IsCatalogUrl ? (x ? Visibility.Visible : Visibility.Hidden) :  Visibility.Hidden).ToReactiveProperty();
			FullScreenVisibility = CatalogListMode.Select(x => futaba.Url.IsCatalogUrl ? (x ? Visibility.Hidden : Visibility.Visible) :  Visibility.Hidden).ToReactiveProperty();
			FullScreenSpan = IsFullScreenMode.Select(x => x ? 3 : 1).ToReactiveProperty();
			FullScreenVisibility = IsFullScreenMode.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			ngUpdateAction = (_) => UpdateToken.Value = DateTime.Now;
			hiddenUpdateAction = (_) => UpdateToken.Value = DateTime.Now;
			systemUpdateAction = (_) => UpdateToken.Value = DateTime.Now;
			Ng.NgConfig.NgConfigLoader.AddNgUpdateNotifyer(ngUpdateAction);
			Ng.NgConfig.NgConfigLoader.AddHiddenUpdateNotifyer(hiddenUpdateAction);
			WpfConfig.WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler(systemUpdateAction);
			if(old != null) {
				FilterText.Value = old.FilterText.Value;
				CatalogSortItem.Value = old.CatalogSortItem.Value;
				CatalogListMode.Value = old.CatalogListMode.Value;
				CatalogResCount.Value = old.CatalogResCount.Value;
				IsFullScreenMode.Value = old.IsFullScreenMode.Value;
			}


			this.Raw = futaba;
			this.Name = futaba.Name;
			this.Url = futaba.Url;

			var updateItems = new List<(int Index, BindableFutabaResItem Item)>();
			if((old == null) || old.Raw.Url.IsCatalogUrl) { // カタログはそうとっかえする
				this.ResItems = new ReactiveCollection<BindableFutabaResItem>();
				int c = 0;
				foreach(var it in futaba.ResItems
						.Select(x => new BindableFutabaResItem(c++, x, futaba.Url.BaseUrl, this))
						.ToArray()) {

					this.ResItems.Add(it);
				}
			} else {
				this.ResItems = old.ResItems;
				var i = 0;
				var prevId = this.ResItems.Select(x => x.Raw.Value.ResItem.No).ToArray();
				var newId = futaba.ResItems.Select(x => x.ResItem.No).ToArray();
				var ep = prevId.Except(newId).ToArray();
				foreach(var it in ep) {
					for(var ii=0; ii<this.ResItems.Count;ii++) {
						if(it == this.ResItems[ii].Raw.Value.ResItem.No) {
							this.ResItems.RemoveAt(ii);
							break;
						}
					}
				}

				while((i < this.ResItems.Count) && (i < futaba.ResItems.Length)) {
					var a = this.ResItems[i];
					var b = futaba.ResItems[i];
					if(a.Raw.Value.ResItem.No != b.ResItem.No) {
						// 普通は来ない
						System.Diagnostics.Debug.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%% this.ResItems.RemoveAt(i) %%%%%%%%%%%%%%%%%%%%%%%");
						this.ResItems.RemoveAt(i);
						continue;
					}

					if(a.Raw.Value.HashText != b.HashText) {
						// 画面から参照されているので this の初期化が終わっていないこのタイミングで書き換えてはいけない
						//this.ResItems[i] = new BindableFutabaResItem(i, b, futaba.Url.BaseUrl, this);
						var bf = new BindableFutabaResItem(i, b, futaba.Url.BaseUrl, this);
						if(b.ResItem.Res.Fsize != 0) {
							bf.ThumbSource.Value = a.ThumbSource.Value;
							bf.ThumbHash.Value = a.ThumbHash.Value;
							bf.OriginSource.Value = a.OriginSource.Value;
						}
						updateItems.Add((i, bf));
					}
					i++;
				} 

				if(i < futaba.ResItems.Length) {
					foreach(var it in futaba.ResItems
							.Skip(i)
							.Select(x => new BindableFutabaResItem(i++, x, futaba.Url.BaseUrl, this))
							.ToArray()) {

						this.ResItems.Add(it);
					}
				}
			}

			this.ResCount = new ReactiveProperty<int>(futaba.Url.IsCatalogUrl ? this.ResItems.Count : (this.ResItems.Count - 1));
			this.IsDie = new ReactiveProperty<bool>(futaba.Raw?.IsDie ?? false);
			this.IsOld = new ReactiveProperty<bool>(futaba.Raw?.IsOld ?? false || this.IsDie.Value);
			this.IsMaxRes = new ReactiveProperty<bool>(futaba.Raw?.IsMaxRes ?? false);
			if(futaba.Raw == null) {
				this.DieTextLong = new ReactiveProperty<string>("");
			} else if(futaba.Raw.IsDie) {
				this.DieTextLong = new ReactiveProperty<string>("スレッドは落ちました");
			} else {
				var t = futaba.Raw.DieDateTime ?? DateTime.Now;
				var ts = t - DateTime.Now;
				if(ts.TotalSeconds < 0){
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(消滅時間を過ぎました)", t.ToString("mm:ss")));
				} else if(0 < ts.Days) {
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("MM/dd"), ts.ToString(@"dd\日hh\時\間")));
				} else if(0 < ts.Hours) {
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("HH:mm"), ts.ToString(@"hh\時\間mm\分")));
				} else if(0 < ts.Minutes) {
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("HH:mm"), ts.ToString(@"mm\分ss\秒")));
				} else {
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("HH:mm"), ts.ToString(@"ss\秒")));
				}
			}

			var bord = Config.ConfigLoader.Bord.Bords.Where(x => x.Url == futaba.Url.BaseUrl).FirstOrDefault();
			this.PostTitle = new ReactiveProperty<string>(futaba.Url.IsCatalogUrl ? "スレッド作成" : "レス投稿");
			if(bord == null) {
				this.PostNameVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
				this.PostImageVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
				this.PostIpOptionVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
				this.PostIdOptionVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
			} else {
				this.PostNameVisibility = new ReactiveProperty<Visibility>(
					(bord.Extra.NameValue) ? Visibility.Visible : Visibility.Collapsed);
				if(futaba.Url.IsCatalogUrl) {
					this.PostImageVisibility = new ReactiveProperty<Visibility>(Visibility.Visible);
					this.PostIpOptionVisibility = new ReactiveProperty<Visibility>(
						bord.Extra.MailIpValue ? Visibility.Visible : Visibility.Collapsed);
					this.PostIdOptionVisibility = new ReactiveProperty<Visibility>(
						bord.Extra.MailIdValue ? Visibility.Visible : Visibility.Collapsed);
				} else {
					this.PostImageVisibility = new ReactiveProperty<Visibility>(
						bord.Extra.ResImageValue ? Visibility.Visible : Visibility.Collapsed);
					this.PostIpOptionVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
					this.PostIdOptionVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
				}
			}
			this.PostData = old?.PostData ?? new ReactiveProperty<PostHolder>(new PostHolder());
			this.OpenImageCommand.Subscribe(x => OnOpenImage(x));
			this.DeleteImageCommand.Subscribe(x => OnDeleteImage(x));
			this.MailSageClickCommand.Subscribe(x => OnMailSageClick());
			this.MailIdClickCommand.Subscribe(x => OnMailIdClick());
			this.MailIpClickCommand.Subscribe(x => OnMailIpClick());
			this.DeletePostDataCommand.Subscribe(() => OnDeletePostData());
			this.ExportCommand.Subscribe(() => OnExport());

			this.FullScreenClickCommand.Subscribe(() => OnFullScreenClick());

			// 初期化がすべて終わったタイミングで書き換える
			foreach(var it in this.ResItems) {
				it.Parent.Value = this;
			}
			foreach(var it in updateItems) {
				this.ResItems[it.Index] = it.Item;
			}
		}

		public void Dispose() {
			if(this.Raw.Url.IsCatalogUrl) {
				this.ResItems.Dispose();
			}
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private async void OnOpenImage(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement o) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						try {
							Application.Current.MainWindow.IsEnabled = false;
							var ext = Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext);
							var ofd = new Microsoft.Win32.OpenFileDialog() {
								Filter = "ふたば画像ファイル|"
									+ string.Join(";", ext.Select(x => "*" + x).ToArray())
									+ "|すべてのファイル|*.*"
							};
							e.Handled = true;
							if(ofd.ShowDialog() ?? false) {
								this.PostData.Value.ImagePath.Value = ofd.FileName;
								// ダイアログをダブルクリックで選択するとウィンドウに当たり判定がいくので
								// 一度待つ
								await Task.Delay(1);
							}
						}
						finally {
							Application.Current.MainWindow.IsEnabled = true;
						}
						break;
					}
				}
			}
		}

		private void OnDeleteImage(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement o) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						this.PostData.Value.ImagePath.Value = "";
						e.Handled = true;
						break;
					}
				}
			}
		}

		private void OnMailSageClick() {
			this.PostData.Value.Mail.Value = "sage";
		}
		private void OnMailIdClick() {
			this.PostData.Value.Mail.Value = "id表示";
		}

		private void OnMailIpClick() {
			this.PostData.Value.Mail.Value = "ip表示";
		}

		private void OnDeletePostData() {
			this.PostData.Value = new PostHolder();
		}

		private void OnFullScreenClick() {
			this.IsFullScreenMode.Value = !this.IsFullScreenMode.Value;
		}

		private void OnExport() {
			if(this.Raw == null) {
				return;
			}

			var sfd= new Microsoft.Win32.SaveFileDialog() {
				AddExtension = true,
				Filter = "HTML5ファイル|*.html;*.htm", 
			};
			if(sfd.ShowDialog() ?? false) {
				string getImageBase64(BindableFutabaResItem bfi) {
					var url = bfi.Raw.Value.Url;
					var item = bfi.Raw.Value.ResItem.Res;
					if(string.IsNullOrEmpty(item.Ext)) {
						return "";
					}

					var ngImage = !object.ReferenceEquals(bfi.ThumbSource.Value, bfi.OriginSource.Value);
					if(ngImage && WpfConfig.WpfConfigLoader.SystemConfig.ExportNgImage == PlatformData.ExportNgImage.Hidden) {
						return "";
					} else if(ngImage && WpfConfig.WpfConfigLoader.SystemConfig.ExportNgImage == PlatformData.ExportNgImage.Dummy) {
						if(Config.ConfigLoader.MimeFutaba.MimeTypes.TryGetValue(".png", out var mime)) {
							return "data:" + mime + ";base64," + WpfUtil.ImageUtil.GetNgImageBase64();
						} else {
							return "";
						}
					}

					// TODO: 基本的にキャッシュされているはずだけど良くないのでメソッドの変更を考える
					return Util.Futaba.GetThumbImage(url, item)
						.Select(x => {
							if(x.Successed) {
								if(Config.ConfigLoader.MimeFutaba.MimeTypes.TryGetValue(Path.GetExtension(x.LocalPath).ToLower(), out var mime)) {
									using(var stream = new FileStream(x.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
										return "data:" + mime + ";base64," + FileUtil.ToBase64(stream);
									}
								}
							}
							return "";
						}).Wait();
				}

				try {
					var fm = File.Exists(sfd.FileName) ? FileMode.Truncate : FileMode.OpenOrCreate;
					using(var sf = new FileStream(sfd.FileName, fm)) {
						Util.ExportUtil.ExportHtml(sf,
							new Data.ExportHolder(
								this.Raw.Bord.Name, this.Raw.Bord.Extra.NameValue,
								this.ResItems.Select(x => {
									var ngRes = x.IsHidden.Value || x.IsNg.Value;
									var ngImage = !object.ReferenceEquals(x.ThumbSource.Value, x.OriginSource.Value)
										&& (WpfConfig.WpfConfigLoader.SystemConfig.ExportNgImage == PlatformData.ExportNgImage.Hidden);
									if(ngRes && (WpfConfig.WpfConfigLoader.SystemConfig.ExportNgRes == PlatformData.ExportNgRes.Hidden)) {
										return null;
									}
									return new Data.ExportData() {
										Subject = x.Raw.Value.ResItem.Res.Sub,
										Name = x.Raw.Value.ResItem.Res.Name,
										Email = x.Raw.Value.ResItem.Res.Email,
										Comment = (ngRes && (WpfConfig.WpfConfigLoader.SystemConfig.ExportNgRes == PlatformData.ExportNgRes.Mask))
											? x.CommentHtml.Value : x.OriginHtml.Value,
										No = x.Raw.Value.ResItem.No,
										Date = string.Format("{0}{1}", x.Raw.Value.ResItem.Res.Now, string.IsNullOrEmpty(x.Raw.Value.ResItem.Res.Id) ? "" : (" " + x.Raw.Value.ResItem.Res.Id)),
										Soudane = x.Raw.Value.Soudane,
										Host = x.Raw.Value.ResItem.Res.Host,
										OriginalImageName = (ngImage || string.IsNullOrEmpty(x.Raw.Value.ResItem.Res.Ext)) ? "" : x.ImageName.Value,
										ThumbnailImageData = getImageBase64(x),
									};
								}).Where(x => x != null).ToArray()
							));
						sf.Flush();
					}
				}
				catch(IOException) {
					// TODO: いい感じにする
					MessageBox.Show("保存に失敗");
				}
			}
		}
	}

	public class BindableFutabaResItem : INotifyPropertyChanged, IDisposable {
#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
		public string Id { get; }
		public ReactiveProperty<int> Index { get; }
		public ReactiveProperty<string> ImageName { get; }

		public ReactiveProperty<BitmapSource> ThumbSource { get; }
		public ReactiveProperty<BitmapSource> OriginSource { get; }
		public ReactiveProperty<ulong?> ThumbHash { get; }

		public ReactiveProperty<Visibility> NameVisibility { get; }
		public ReactiveProperty<Visibility> ResImageVisibility { get; }

		public ReactiveProperty<Data.BordData> Bord { get; }
		public ReactiveProperty<string> ThreadResNo { get; }
		public ReactiveProperty<Data.FutabaContext.Item> Raw { get; }

		public ReactiveProperty<string> HeadLineHtml { get; }
		public ReactiveProperty<string> DisplayHtml { get; }
		public ReactiveProperty<string> CommentHtml { get; }
		public ReactiveProperty<string> OriginHtml { get; }
		public ReactiveProperty<bool> IsNg { get; }
		public ReactiveProperty<bool> IsHidden { get; }
		public ReactiveProperty<bool> IsDel { get; }
		public ReactiveProperty<bool> IsVisibleOriginComment { get; }
		public ReactiveProperty<bool> IsNgImageHidden { get; }

		public ReactiveProperty<string> CommentCopy { get; }
		public ReactiveProperty<Visibility> CommandPaletteVisibility { get; }


		public ReactiveProperty<Visibility> ReleaseHiddenResBuutonVisibility { get; }
		public ReactiveProperty<Visibility> ShowNgResButtonVisibility { get; }
		public ReactiveProperty<Visibility> CopyModeButtonVisibility { get; }
		public ReactiveProperty<string> ShowNgResButtonText { get; }

		public ReactiveProperty<bool> IsCopyMode { get; } = new ReactiveProperty<bool>(false);

		public ReactiveProperty<Visibility> FutabaTextBlockVisibility { get; }
		public ReactiveProperty<Visibility> CopyBlockVisibility { get; }

		public ReactiveProperty<Visibility> IsVisibleMenuItemNgImage { get; }

		public ReactiveProperty<bool> IsVisibleCatalogIdMarker{ get; }

		public ReactiveProperty<string> MenuItemTextHidden { get; }

		public ReactiveProperty<BindableFutaba> Parent { get; }

		public ReactiveCommand<MouseButtonEventArgs> FutabaTextBlockMouseDownCommand { get; }
			= new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand ThumbLoadCommand { get; }
			= new ReactiveCommand();

		private Action<Ng.NgData.NgConfig> ngUpdateAction;
		private Action<Ng.NgData.HiddenConfig> hiddenUpdateAction;
		private Action<Ng.NgData.NgImageConfig> imageUpdateAction;
		private Action<PlatformData.WpfConfig> systemUpdateAction;

		public BindableFutabaResItem(int index, Data.FutabaContext.Item item, string baseUrl, BindableFutaba parent) {
			System.Diagnostics.Debug.Assert(item != null);
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(parent != null);
			var bord = Config.ConfigLoader.Bord.Bords.Where(x => x.Url == baseUrl).FirstOrDefault();
			System.Diagnostics.Debug.Assert(bord != null);
			this.Index = new ReactiveProperty<int>(index);
			this.Bord = new ReactiveProperty<Data.BordData>(bord);
			this.Parent = new ReactiveProperty<BindableFutaba>(parent);
			this.ThreadResNo = new ReactiveProperty<string>(item.ResItem.No);
			this.Raw = new ReactiveProperty<Data.FutabaContext.Item>(item);
			this.NameVisibility = new ReactiveProperty<Visibility>(
				(bord.Extra ?? new Data.BordDataExtra()).NameValue ? Visibility.Visible : Visibility.Collapsed);
			this.ThumbSource = new ReactiveProperty<BitmapSource>();
			this.OriginSource = new ReactiveProperty<BitmapSource>();
			this.ThumbHash = new ReactiveProperty<ulong?>();
			this.CommandPaletteVisibility = new ReactiveProperty<Visibility>(
				WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledThreadCommandPalette ? Visibility.Visible : Visibility.Collapsed);
			this.IsVisibleCatalogIdMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledIdMarker);

			// delとhostの処理
			{
				var headLine = new StringBuilder();
				if(Raw.Value.ResItem.Res.IsDel) {
					headLine.Append("<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font><br>");
				} else if(Raw.Value.ResItem.Res.IsDel2) {
					headLine.Append("<font color=\"#ff0000\">削除依頼によって隔離されました</font><br>");
				}
				if(!string.IsNullOrEmpty(Raw.Value.ResItem.Res.Host)) {
					headLine.AppendFormat("[<font color=\"#ff0000\">{0}</font>]<br>", Raw.Value.ResItem.Res.Host);
				}
				this.IsNg = new ReactiveProperty<bool>(
					parent.Url.IsCatalogUrl ? Ng.NgUtil.NgHelper.CheckCatalogNg(parent.Raw, item)
						: Ng.NgUtil.NgHelper.CheckThreadNg(parent.Raw, item));
				this.IsHidden = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckHidden(parent.Raw, item));
				this.IsDel = new ReactiveProperty<bool>(
					(item.ResItem.Res.IsDel || item.ResItem.Res.IsDel2)
						&& (WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden));
				this.IsNgImageHidden = new ReactiveProperty<bool>(false);
				this.HeadLineHtml = new ReactiveProperty<string>(headLine.ToString());
				this.CommentHtml = new ReactiveProperty<string>("");
				this.OriginHtml = new ReactiveProperty<string>(Raw.Value.ResItem.Res.Com);
				this.SetCommentHtml();
				this.IsVisibleOriginComment = new ReactiveProperty<bool>((this.IsHidden.Value || this.IsNg.Value || this.IsDel.Value) ? false : true);
				this.DisplayHtml = IsVisibleOriginComment
					.Select(x => x ? this.OriginHtml.Value : this.CommentHtml.Value)
					.ToReactiveProperty();
				this.ReleaseHiddenResBuutonVisibility = this.IsHidden
					.Select(x => x ? Visibility.Visible : Visibility.Collapsed)
					.ToReactiveProperty();
				this.ShowNgResButtonVisibility = new[] { this.IsNg, this.IsDel }
					.CombineLatest(x => x.Any(y => y))
					.Select(x => x ? Visibility.Visible : Visibility.Collapsed)
					.ToReactiveProperty();
				this.CopyModeButtonVisibility = new[] { this.IsHidden, this.IsNg, this.IsDel }
					.CombineLatest(x => x.All(y => !y))
					.CombineLatest(this.IsVisibleOriginComment, (x, y) => (x | y) ? Visibility.Visible : Visibility.Collapsed)
					.ToReactiveProperty();
				this.ShowNgResButtonText = this.IsVisibleOriginComment
						.Select(x => x ? "非表示" : "レスを表示")
						.ToReactiveProperty();

				ngUpdateAction = (x) => a();
				hiddenUpdateAction = (x) => a();
				imageUpdateAction = (x) => b();
				systemUpdateAction = (x) => c();
				Ng.NgConfig.NgConfigLoader.AddNgUpdateNotifyer(ngUpdateAction);
				Ng.NgConfig.NgConfigLoader.AddHiddenUpdateNotifyer(hiddenUpdateAction);
				Ng.NgConfig.NgConfigLoader.AddImageUpdateNotifyer(imageUpdateAction);
				WpfConfig.WpfConfigLoader.AddSystemConfigUpdateNotifyer(systemUpdateAction);
			}

			// コピー用コメント生成
			{
				var sb = new StringBuilder()
					.Append(Index.Value);
				if(Bord.Value.Extra?.NameValue ?? true) {
					sb.Append(" ")
						.Append(Raw.Value.ResItem.Res.Sub)
						.Append(" ")
						.Append(Raw.Value.ResItem.Res.Name);
				}
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Email)) {
					sb.Append(" [").Append(Raw.Value.ResItem.Res.Email).Append("]");
				}
				sb.Append(" ").Append(Raw.Value.ResItem.Res.Now);
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Host)) {
					sb.Append(" ").Append(Raw.Value.ResItem.Res.Host);
				}
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Id)) {
					sb.Append(" ").Append(Raw.Value.ResItem.Res.Id);
				}
				if(0 < Raw.Value.Soudane) {
					sb.Append(" そうだね×").Append(Raw.Value.Soudane);
				}
				sb.Append(" No.").Append(Raw.Value.ResItem.No);
				sb.AppendLine();
				sb.Append(WpfUtil.TextUtil.RawComment2Text(Raw.Value.ResItem.Res.Com));
				this.CommentCopy = new ReactiveProperty<string>(sb.ToString());
			}

			if(item.ResItem.Res.Fsize != 0) {
				this.ImageName = new ReactiveProperty<string>(Regex.Replace(
					item.ResItem.Res.Src, @"^.+/([^\.]+\..+)$", "$1"));
				var bmp = WpfUtil.ImageUtil.GetImageCache(
					Util.Futaba.GetThumbImageLocalFilePath(item.Url, item.ResItem.Res));
				if(bmp != null) {
					this.SetThumbSource(bmp);
					if(this.Raw.Value.Url.IsCatalogUrl
						&& WpfConfig.WpfConfigLoader.SystemConfig.CatalogNgImage == PlatformData.CatalogNgImage.Hidden
						&& !object.ReferenceEquals(this.ThumbSource.Value, this.OriginSource.Value)) {

						this.IsNgImageHidden.Value = true;
					}
				}
			} else {
				this.ImageName = new ReactiveProperty<string>("");
			}
			this.FutabaTextBlockVisibility = this.IsCopyMode.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			this.CopyBlockVisibility = this.IsCopyMode
				.CombineLatest(IsVisibleOriginComment, (x, y) => x ? Visibility.Visible : (y ? Visibility.Hidden : Visibility.Collapsed))
				.ToReactiveProperty();
			this.ResImageVisibility = this.ThumbSource
				.CombineLatest(IsVisibleOriginComment, (x, y) => ((x != null) && y) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
			this.IsVisibleMenuItemNgImage = this.ImageName.Select(x => !string.IsNullOrEmpty(x))
				.CombineLatest(this.OriginSource, this.ThumbSource, (x, y, z) => x && (y == null || object.ReferenceEquals(y, z)))
				.Select(x => x ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
			this.MenuItemTextHidden = this.IsHidden.Select(x => x ? "非表示を解除" : "非表示").ToReactiveProperty();
			this.FutabaTextBlockMouseDownCommand.Subscribe(x => OnFutabaTextBlockMouseDown(x));
			this.ThumbLoadCommand.Subscribe(_ => OnThumbLoad());
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public void ReleaseHiddenRes() {
			Ng.NgConfig.NgConfigLoader.RemoveHiddenRes(
				Ng.NgData.HiddenData.FromResItem(this.Raw.Value.Url.BaseUrl, this.Raw.Value.ResItem));
		}

		public void ToggleDisplayNgRes() {
			if(this.IsHidden.Value || this.IsNg.Value || this.IsDel.Value) {
				this.IsVisibleOriginComment.Value = !this.IsVisibleOriginComment.Value;
			}
		}

		public void StartCopyMode() {
			this.IsCopyMode.Value = true;
		}

		public void EndCopyMode() {
			this.IsCopyMode.Value = false;
		}

		private void OnFutabaTextBlockMouseDown(MouseButtonEventArgs e) {
			if((e.ClickCount == 2) && (e.ChangedButton == MouseButton.Left)) {
				var b1 = (this.IsHidden.Value || this.IsNg.Value) && this.IsVisibleOriginComment.Value;
				var b2 = !this.IsHidden.Value && !this.IsNg.Value;
				if(b1 || b2) {
					this.StartCopyMode();
				}
			}
		}

		private void OnThumbLoad() {
			if((Raw.Value.ResItem.Res.Fsize != 0) && (ThumbSource.Value == null)) {
				Util.Futaba.GetThumbImage(Raw.Value.Url, Raw.Value.ResItem.Res)
					.Select(x => {
						if(x.Successed) {
							if(x.FileBytes != null) {
								return WpfUtil.ImageUtil.LoadImage(x.LocalPath, x.FileBytes);
							} else {
								return WpfUtil.ImageUtil.LoadImage(x.LocalPath);
							}
						} else {
							return null;
						}
					})
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						if(x != null) {
							SetThumbSource(x);
						} else {
							// TODO: エラー画像表示
						}
					});
			}
		}

		private void SetThumbSource(BitmapImage bmp) {
			var h = Ng.NgUtil.NgHelper.IsEnabledNgImage()
				? WpfUtil.ImageUtil.CalculatePerceptualHash(bmp) : default(ulong?);
			this.ThumbHash.Value = h;
			this.OriginSource.Value = bmp;
			if(h.HasValue && Ng.NgConfig.NgConfigLoader.NgImageConfig.Images.Any(
				y => Ng.NgUtil.PerceptualHash.GetHammingDistance(h.Value, y) <= Ng.NgConfig.NgConfigLoader.NgImageConfig.Threshold)) {

				ThumbSource.Value = (Ng.NgConfig.NgConfigLoader.NgImageConfig.NgMethod == ImageNgMethod.Hidden)
					? null : WpfUtil.ImageUtil.GetNgImage();
			} else {
				ThumbSource.Value = bmp;
			}
		}

		private void SetCommentHtml() {
			var del = WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden;
			if(this.IsNg.Value) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">NG設定に抵触しています</font>";
			} else if(this.IsHidden.Value) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">非表示に設定されています</font>";
			} else if(this.Raw.Value.ResItem.Res.IsDel && del) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font>";
			} else if(this.Raw.Value.ResItem.Res.IsDel2 && del) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">削除依頼によって隔離されました</font>";
			} else {
				this.CommentHtml.Value = this.OriginHtml.Value;
			}

		}

		// TODO: 名前変える
		private void a() {
			this.IsNg.Value = this.Raw.Value.Url.IsCatalogUrl
				? Ng.NgUtil.NgHelper.CheckCatalogNg(this.Parent.Value.Raw, this.Raw.Value)
					: Ng.NgUtil.NgHelper.CheckThreadNg(this.Parent.Value.Raw, this.Raw.Value);
			this.IsHidden.Value = Ng.NgUtil.NgHelper.CheckHidden(this.Parent.Value.Raw, this.Raw.Value);
			this.IsCopyMode.Value = false;
			this.SetCommentHtml();
			this.IsVisibleOriginComment.Value = (this.IsHidden.Value || this.IsNg.Value || this.IsDel.Value) ? false : true;
		}

		// TODO: 名前変える
		private void b() {
			if(OriginSource.Value == null) {
				return;
			}

			if(!Ng.NgUtil.NgHelper.IsEnabledNgImage()) {
				return;
			}

			if(!ThumbHash.Value.HasValue) {
				ThumbHash.Value = WpfUtil.ImageUtil.CalculatePerceptualHash(OriginSource.Value);
			}

			if(ThumbHash.Value.HasValue) {
				if(Ng.NgConfig.NgConfigLoader.NgImageConfig.Images.Any(
					x => Ng.NgUtil.PerceptualHash.GetHammingDistance(ThumbHash.Value.Value, x) <= Ng.NgConfig.NgConfigLoader.NgImageConfig.Threshold)) {

					// NG画像
					ThumbSource.Value = (Ng.NgConfig.NgConfigLoader.NgImageConfig.NgMethod == ImageNgMethod.Hidden) 
						? null :  WpfUtil.ImageUtil.GetNgImage();
				} else {
					ThumbSource.Value = OriginSource.Value;
				}
			}
		}

		// TODO:名前考える
		private void c() {
			this.IsDel.Value = (this.Raw.Value.ResItem.Res.IsDel || this.Raw.Value.ResItem.Res.IsDel2)
				&& (WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden);
			this.SetCommentHtml();
			this.IsVisibleOriginComment.Value =(this.IsHidden.Value || this.IsNg.Value || this.IsDel.Value) ? false : true;
			this.CommandPaletteVisibility.Value = WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledThreadCommandPalette ? Visibility.Visible : Visibility.Collapsed;
			this.IsVisibleCatalogIdMarker.Value = WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledIdMarker;
		}
	}
}