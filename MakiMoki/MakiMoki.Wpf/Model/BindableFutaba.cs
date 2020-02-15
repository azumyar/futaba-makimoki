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

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	public class BindableFutaba : INotifyPropertyChanged, IDisposable {
		public class PostHolder : INotifyPropertyChanged, IDisposable {

			private static readonly string FallbackUnicodeString = "�";
			private static readonly Encoding FutabaEncoding = Encoding.GetEncoding(
				"Shift_JIS",
				new EncoderReplacementFallback(FallbackUnicodeString),
				DecoderFallback.ReplacementFallback);

#pragma warning disable CS0067
			public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
			private CompositeDisposable Disposable { get; } = new CompositeDisposable();

			public ReactiveProperty<string> Comment { get; } = new ReactiveProperty<string>("");
			public ReadOnlyReactiveProperty<string> CommentEncoded { get; }
			public ReadOnlyReactiveProperty<int> CommentBytes { get; }
			public ReadOnlyReactiveProperty<int> CommentLines { get; }

			public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>("");
			public ReactiveProperty<string> Mail { get; } = new ReactiveProperty<string>("");
			public ReactiveProperty<string> Subject { get; } = new ReactiveProperty<string>("");
			public ReactiveProperty<string> Password { get; } = new ReactiveProperty<string>(
				Config.ConfigLoader.Password.FutabaValue);
			public ReactiveProperty<string> ImagePath { get; } = new ReactiveProperty<string>("");

			public ReactiveProperty<string> ImageName { get; }
			public ReactiveProperty<ImageSource> ImagePreview { get; }

			public ReactiveProperty<bool> CommentImageValidFlag { get; } = new ReactiveProperty<bool>(false);
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
						// TODO: この辺拡張子設定ファイルに移動
						var ext = Path.GetExtension(x).ToLower();
						var imageExt = new string[] {
							".jpg",
							".jpeg",
							".png",
							".gif",
							".webp",
						};
						var movieExt = new string[] {
							".mp4",
							".webm",
						};
						if(imageExt.Contains(ext)) {
							return WpfUtil.ImageUtil.LoadImage(x);
						} else if(movieExt.Contains(ext)) {
							// 動画は今は何もしない
							// TODO: なんんか実装する
						}
					}
					return null;
				}).ToReactiveProperty();

				this.CommentEncoded = this.Comment.Select(x => {
					// System.Net.WebUtility.HtmlEncode(x) ♡などをスルーするので自前で解析もする
					var sb = new StringBuilder(System.Net.WebUtility.HtmlEncode(x));
					for(var i = 0; i < sb.Length; i++) {
						var c = sb[i];

						var b = FutabaEncoding.GetBytes(c.ToString());
						var s = FutabaEncoding.GetString(b);
						if(s == FallbackUnicodeString) {
							var ss = string.Format("&#{0};", (int)c);
							sb.Remove(i, 1);
							sb.Insert(i, ss);
							i += ss.Length;
						}
					}
					return sb.ToString();
				}).ToReadOnlyReactiveProperty();
				this.CommentBytes = this.CommentEncoded.Select(x => {
					return FutabaEncoding.GetByteCount(x);
				}).ToReadOnlyReactiveProperty();
				this.CommentLines = this.Comment
					.Select(x => (x.Length == 0) ? 0 : (x.Replace(@"\r", "").Where(y => y == '\n').Count() + 1))
					.ToReadOnlyReactiveProperty();
				this.Comment.Subscribe(x => {
					if(x.Length != 0) {
						this.CommentImageValidFlag.Value = true;
					} else {
						this.CommentImageValidFlag.Value = this.ImagePath.Value.Length != 0;
					}
				});
				this.ImagePath.Subscribe(x => {
					if(x.Length != 0) {
						this.CommentImageValidFlag.Value = true;
					} else {
						this.CommentImageValidFlag.Value = this.Comment.Value.Length != 0;
					}
				});
				this.PasswordValidFlag = this.Password.Select(x => x.Length != 0).ToReactiveProperty();
				this.PostButtonCommand = new[] { CommentImageValidFlag, PasswordValidFlag }
					.CombineLatestValuesAreAllTrue()
					.ToReactiveCommand();
			}

			public void Dispose() {
				Disposable.Dispose();
			}
		}
#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();
		public ReactiveProperty<BindableFutabaResItem[]> ResItems { get; }
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


		public string Name { get; }

		public Data.UrlContext Url { get; }

		public Data.FutabaContext Raw { get; }

		public ReactiveProperty<bool> IsOld { get; }
		public ReactiveProperty<bool> IsDie { get; }

		public BindableFutaba(Data.FutabaContext futaba, BindableFutaba old = null) {
			int c = 0;
			this.Raw = futaba;
			this.Name = futaba.Name;
			this.Url = futaba.Url;
			this.ResItems = new ReactiveProperty<BindableFutabaResItem[]>(
				futaba.ResItems
					.Select(x => new BindableFutabaResItem(c++, x, futaba.Url.BaseUrl, this))
					.ToArray());
			this.ResCount = this.ResItems.Select(x => futaba.Url.IsCatalogUrl ? x.Length : (x.Length - 1)).ToReactiveProperty();
			this.IsDie = new ReactiveProperty<bool>(futaba.Raw?.IsDie ?? false);
			this.IsOld = new ReactiveProperty<bool>(futaba.Raw?.IsOld ?? false || this.IsDie.Value);
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
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("hh:mm"), ts.ToString(@"hh\時\間mm\分")));
				} else if(0 < ts.Minutes) {
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("hh:mm"), ts.ToString(@"mm\分ss\秒")));
				} else {
					this.DieTextLong = new ReactiveProperty<string>(
						string.Format("スレ消滅：{0}(あと{1})", t.ToString("hh:mm"), ts.ToString(@"ss\秒")));
				}
			}

			var bord = Config.ConfigLoader.Bord.Where(x => x.Url == futaba.Url.BaseUrl).FirstOrDefault();
			this.PostTitle = new ReactiveProperty<string>(futaba.Url.IsCatalogUrl ? "スレ立て" : "レス");
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
		}

		public void Dispose() {
			Disposable.Dispose();
		}

		private async void OnOpenImage(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement o) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						try {
							Application.Current.MainWindow.IsEnabled = false;
							var ext = Config.ConfigLoader.Mime.Types.Select(x => x.Ext);
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

		private void OnExport() {
			if(this.Raw == null) {
				return;
			}

			var sfd= new Microsoft.Win32.SaveFileDialog() {
				AddExtension = true,
				Filter = "HTML5ファイル|*.html;*.htm", 
			};
			if(sfd.ShowDialog() ?? false) {
				string getImageBase64(Data.UrlContext url, Data.ResItem item) {
					if(item.Fsize == 0) {
						return "";
					}

					return Util.Futaba.GetThumbImage(url, item)
						.Select(x => {
							if(x.Successed) {
								if(Config.ConfigLoader.Mime.MimeTypes.TryGetValue(Path.GetExtension(x.LocalPath).ToLower(), out var mime)) {
									using(var stream = new FileStream(x.LocalPath, FileMode.Open)) {
										var list = new List<byte>();
										var b = new byte[1024];
										var r = 0;
										while(0 < (r = stream.Read(b, 0, b.Length))) {
											for(var i = 0; i < r; i++) {
												list.Add(b[i]);
											}
										}
										return "data:" + mime + ";base64," + Convert.ToBase64String(list.ToArray(), Base64FormattingOptions.None);
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
								this.ResItems.Value.Select(x => new Data.ExportData() {
									Subject = x.Raw.Value.ResItem.Res.Sub,
									Name = x.Raw.Value.ResItem.Res.Name,
									Email = x.Raw.Value.ResItem.Res.Email,
									Comment = x.CommentHtml.Value,
									No = x.Raw.Value.ResItem.No,
									Date = string.Format("{0}{1}", x.Raw.Value.ResItem.Res.Now, string.IsNullOrEmpty(x.Raw.Value.ResItem.Res.Id) ? "" : (" " + x.Raw.Value.ResItem.Res.Id)),
									Soudane = x.Raw.Value.Soudane,
									OriginalImageName = x.ImageName.Value,
									ThumbnailImageData = getImageBase64(x.Raw.Value.Url, x.Raw.Value.ResItem.Res),
								}).ToArray()
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
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();

		public string Id { get; }
		public ReactiveProperty<int> Index { get; }
		public ReactiveProperty<string> ImageName { get; }

		public ReactiveProperty<ImageSource> ThumbSource { get; }

		public ReactiveProperty<Visibility> NameVisibility { get; }
		public ReactiveProperty<Visibility> ResImageVisibility { get; }

		public ReactiveProperty<Data.BordConfig> Bord { get; }
		public ReactiveProperty<string> ThreadResNo { get; }
		public ReactiveProperty<Data.FutabaContext.Item> Raw { get; }

		public ReactiveProperty<string> CommentHtml { get; }
		public ReactiveProperty<string> CommentCopy { get; }

		public ReactiveProperty<BindableFutaba> Parent { get; }

		public BindableFutabaResItem(int index, Data.FutabaContext.Item item, string baseUrl, BindableFutaba parent) {
			System.Diagnostics.Debug.Assert(item != null);
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(parent != null);
			var bord = Config.ConfigLoader.Bord.Where(x => x.Url == baseUrl).FirstOrDefault();
			System.Diagnostics.Debug.Assert(bord != null);
			this.Index = new ReactiveProperty<int>(index);
			this.Bord = new ReactiveProperty<Data.BordConfig>(bord);
			this.Parent = new ReactiveProperty<BindableFutaba>(parent);
			this.ThreadResNo = new ReactiveProperty<string>(item.ResItem.No);
			this.Raw = new ReactiveProperty<Data.FutabaContext.Item>(item);
			this.NameVisibility = new ReactiveProperty<Visibility>(
				(bord.Extra ?? new Data.BordConfigExtra()).NameValue ? Visibility.Visible : Visibility.Collapsed);
			this.ThumbSource = new ReactiveProperty<ImageSource>();
			//this.ThumbSource = WpfUtil.ImageUtil.ToThumbProperty(this.Row);
			this.ResImageVisibility = this.ThumbSource
				.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();

			if(Raw.Value.ResItem.Res.IsDel2) {
				this.CommentHtml = new ReactiveProperty<string>(string.Format(
					"<font color=\"#ff0000\">削除依頼によって隔離されました</font><br>{0}",
					Raw.Value.ResItem.Res.Com));
			} else {
				this.CommentHtml = new ReactiveProperty<string>(Raw.Value.ResItem.Res.Com);
			}
			var sb = new StringBuilder()
				.Append(Index.Value)
				.Append(" ")
				.Append("No.")
				.Append(Raw.Value.ResItem.No);
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
			sb.AppendLine();
			sb.Append(WpfUtil.TextUtil.RawComment2Text(Raw.Value.ResItem.Res.Com));
			this.CommentCopy = new ReactiveProperty<string>(sb.ToString());

			if(item.ResItem.Res.Fsize != 0) {
				Util.Futaba.GetThumbImage(item.Url, item.ResItem.Res)
					.Select(x => {
						if(x.Successed) {
							return WpfUtil.ImageUtil.LoadImage(x.LocalPath);
						} else {
							return null;
						}
					})
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(x => {
						if(x != null) {
							ThumbSource.Value = x;
						} else {
							// TODO: エラー画像表示
						}
					});
				this.ImageName = new ReactiveProperty<string>(Regex.Replace(
					item.ResItem.Res.Src, @"^.+/([^\.]+\..+)$", "$1"));
			} else {
				this.ImageName = new ReactiveProperty<string>("");
			}
		}

		public void Dispose() {
			Disposable.Dispose();
		}
	}
}
