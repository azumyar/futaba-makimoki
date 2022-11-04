using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Prism.Events;
using Prism.Mvvm;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Reactive;
using Yarukizero.Net.MakiMoki.Wpf.Controls;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaPostViewViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}
		internal class BaseCommandMessage {
			public string Token { get; }

			public BaseCommandMessage(string token) {
				this.Token = token;
			}
		}
		internal class PostCommandMessage : BaseCommandMessage {
			public PostCommandMessage(string token) : base(token) { }
		}
		internal class OpenImageCommandMessage : BaseCommandMessage {
			public OpenImageCommandMessage(string token) : base(token) { }
		}
		internal class OpenLoaderCommandMessage : BaseCommandMessage {
			public OpenLoaderCommandMessage(string token) : base(token) { }
		}
		internal class DeleteCommandMessage : BaseCommandMessage {
			public DeleteCommandMessage(string token) : base(token) { }
		}
		internal class CloseCommandMessage : BaseCommandMessage {
			public CloseCommandMessage(string token) : base(token) { }
		}
		internal class PasteImageCommandMessage : BaseCommandMessage {
			public PasteImageCommandMessage(string token) : base(token) { }
		}
		internal class PasteLoaderCommandMessage : BaseCommandMessage {
			public PasteLoaderCommandMessage(string token) : base(token) { }
		}


		internal class PostCloseMessage {
			public UrlContext Url { get; }

			public PostCloseMessage(UrlContext url) {
				this.Url = url;
			}
		}

		internal class ReplaceTextMessage {
			public UrlContext Url { get; }
			public string Text { get; }

			public ReplaceTextMessage(UrlContext url, string text) {
				this.Url = url;
				this.Text = text;
			}
		}

		internal class AppendTextMessage {
			public UrlContext Url { get; }
			public string Text { get; }

			public AppendTextMessage(UrlContext url, string text) {
				this.Url = url;
				this.Text = text;
			}
		}
		private ReactiveProperty<bool> IsUpplading2 { get; } = new ReactiveProperty<bool>(false);
		private ReactiveProperty<bool> IsPosting { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<KeyBinding[]> KeyGestures { get; } = new ReactiveProperty<KeyBinding[]>();
		public ReactiveProperty<Visibility> PostProgressVisibility { get; }

		public MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.PostHolder>> ContentsChangedCommand { get; }
			= new MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.PostHolder>>();

		public MakiMokiCommand<MouseButtonEventArgs> OpenImageCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> DeleteImageCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<PostHolder> DeletePostDataCommand { get; } = new MakiMokiCommand<PostHolder>();
		public MakiMokiCommand<PostHolder> MailSageClickCommand { get; } = new MakiMokiCommand<PostHolder>();
		public MakiMokiCommand<PostHolder> MailIdClickCommand { get; } = new MakiMokiCommand<PostHolder>();
		public MakiMokiCommand<PostHolder> MailIpClickCommand { get; } = new MakiMokiCommand<PostHolder>();


		public MakiMokiCommand<DragEventArgs> ImageDragOverCommand { get; } = new MakiMokiCommand<DragEventArgs>();
		public MakiMokiCommand<DragEventArgs> ImageDropCommand { get; } = new MakiMokiCommand<DragEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> OpenUploadCommand { get; } = new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<DragEventArgs> UploadDragOverpCommand { get; } = new MakiMokiCommand<DragEventArgs>();
		public MakiMokiCommand<DragEventArgs> UploadDroppCommand { get; } = new MakiMokiCommand<DragEventArgs>();

		public MakiMokiCommand<RoutedEventArgs> PostViewPostCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<PostHolder> MenuItemClickPastePostImageCommand { get; } = new MakiMokiCommand<PostHolder>();
		public MakiMokiCommand<PostHolder> MenuItemClickPastePostUpCommand { get; } = new MakiMokiCommand<PostHolder>();

		public MakiMokiCommand KeyBindingPostCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingOpenImageCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingOpenLoaderCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingDeleteCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingCloseCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingPasteImageCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand KeyBindingPaseteLoaderCommand { get; } = new MakiMokiCommand();

		public string Token { get; } = Guid.NewGuid().ToString();

		private Action<PlatformData.GestureConfig> onGestureConfigUpdateNotifyer;
		public FutabaPostViewViewModel() {
			PostProgressVisibility = this.IsPosting
				.Select(x => x ? Visibility.Visible : Visibility.Hidden)
				.ToReactiveProperty();

			UpdateKeyBindings();
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			OpenImageCommand.Subscribe(x => OnOpenImage(x));
			DeleteImageCommand.Subscribe(x => OnDeleteImage(x));
			MailSageClickCommand.Subscribe(x => OnMailSageClick(x));
			MailIdClickCommand.Subscribe(x => OnMailIdClick(x));
			MailIpClickCommand.Subscribe(x => OnMailIpClick(x));
			DeletePostDataCommand.Subscribe(x => OnDeletePostData(x));

			OpenUploadCommand.Subscribe(x => OnOpenUpload(x));
			ImageDragOverCommand.Subscribe(x => OnImageDragOver(x));
			ImageDropCommand.Subscribe(x => OnImageDrop(x));
			UploadDragOverpCommand.Subscribe(x => OnUploadDragOver(x));
			UploadDroppCommand.Subscribe(x => OnUploadDrop(x));
			PostViewPostCommand.Subscribe(x => OnPostViewPostClick((x.Source as FrameworkElement)?.DataContext as Model.PostHolder));

			MenuItemClickPastePostImageCommand.Subscribe(x => OnMenuItemClickPastePostImage(x));
			MenuItemClickPastePostUpCommand.Subscribe(x => OnMenuItemClickPastePostUp(x));

			KeyBindingPostCommand.Subscribe(_ => OnKeyBindingPost());
			KeyBindingOpenImageCommand.Subscribe(_ => OnKeyBindingOpenImage());
			KeyBindingOpenLoaderCommand.Subscribe(_ => OnKeyBindingOpenUpload());
			KeyBindingDeleteCommand.Subscribe(_ => OnKeyBindingDelete());
			KeyBindingCloseCommand.Subscribe(_ => OnKeyBindingClose());
			KeyBindingPasteImageCommand.Subscribe(_ => OnKeyBindingPasteImage());
			KeyBindingPaseteLoaderCommand.Subscribe(_ => OnKeyBindingPasteLoader());

			onGestureConfigUpdateNotifyer = (_) => UpdateKeyBindings();
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
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewPost
				.Select(x => GetKeyBinding(x, this.KeyBindingPostCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewOpenImage
				.Select(x => GetKeyBinding(x, this.KeyBindingOpenImageCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewOpenUploader
				.Select(x => GetKeyBinding(x, this.KeyBindingOpenLoaderCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewDelete
				.Select(x => GetKeyBinding(x, this.KeyBindingDeleteCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewClose
				.Select(x => GetKeyBinding(x, this.KeyBindingCloseCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewPasteImage
				.Select(x => GetKeyBinding(x, this.KeyBindingPasteImageCommand))
				.Where(x => x != null));
			kg.AddRange(WpfConfig.WpfConfigLoader.Gesture.KeyGesturePostViewPasteUploader
				.Select(x => GetKeyBinding(x, this.KeyBindingPaseteLoaderCommand))
				.Where(x => x != null));
			this.KeyGestures.Value = kg.ToArray();
		}

		private void UploadUp2(Model.PostHolder postData, string path) {
			if(this.IsPosting.Value || this.IsUpplading2.Value) {
				return;
			}

			this.IsUpplading2.Value = true;
			Util.Futaba.UploadUp2(path, postData.Password.Value)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Finally(() => { this.IsUpplading2.Value = false; })
				.Subscribe(x => {
					if(x.Successed) {
						Messenger.Instance.GetEvent<PubSubEvent<AppendTextMessage>>()
							.Publish(new AppendTextMessage(postData.Url, x.FileNameOrMessage));
					} else {
						Util.Futaba.PutInformation(new Information($"アップロード失敗：{ x.FileNameOrMessage }", postData));
					}
				});
		}

		public async Task OpenUpload(Model.PostHolder p) {
			if(this.IsPosting.Value) {
				return;
			}
			if(p == null) {
				return;
			}

			try {
				Application.Current.MainWindow.IsEnabled = false;
				var ext = Config.ConfigLoader.MimeUp2.Types.Select(x => x.Ext);
				var ofd = new Microsoft.Win32.OpenFileDialog() {
					Filter = "あぷ小ファイル|"
						+ string.Join(";", ext.Select(x => "*" + x).ToArray())
						+ "|すべてのファイル|*.*"
				};
				if(ofd.ShowDialog() ?? false) {
					this.UploadUp2(p, ofd.FileName);

					// ダイアログをダブルクリックで選択するとウィンドウに当たり判定がいくので
					// 一度待つ
					await Task.Delay(1);
				}
			}
			finally {
				Application.Current.MainWindow.IsEnabled = true;
			}
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<Model.PostHolder> e) { }

		private async void OnOpenImage(MouseButtonEventArgs e) {
			if((e.Source is FrameworkElement o) && (o.DataContext is PostHolder p)) {
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
								p.ImagePath.Value = ofd.FileName;
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
			if((e.Source is FrameworkElement o) && (o.DataContext is PostHolder p)) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						p.ImagePath.Value = "";
						e.Handled = true;
						break;
					}
				}
			}
		}

		private void OnMailSageClick(PostHolder p) {
			p.Mail.Value = "sage";
		}
		private void OnMailIdClick(PostHolder p) {
			p.Mail.Value = "id表示";
		}

		private void OnMailIpClick(PostHolder p) {
			p.Mail.Value = "ip表示";
		}

		private void OnDeletePostData(PostHolder p) {
			p.Reset();
		}

		private void OnImageDragOver(DragEventArgs e) {
			if(IsValidDragFile(e, Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext).ToArray())) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnImageDrop(DragEventArgs e) {
			if((e.Source as FrameworkElement)?.DataContext is Model.PostHolder p) {
				if(IsValidDragFile(e, Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext).ToArray())
					&& e.Data.GetData(DataFormats.FileDrop) is string[] files) {

					p.ImagePath.Value = files[0];
				} else {
					Util.Futaba.PutInformation(new Information("未対応のファイル"));
				}
			}
		}

		private async void OnOpenUpload(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement o && o.DataContext is Model.PostHolder p) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						e.Handled = true;
						await OpenUpload(p);
						break;
					}
				}
			}
		}

		public async Task OpenImage(Model.PostHolder p) {
			if(this.IsPosting.Value) {
				return;
			}
			if(p == null) {
				return;
			}

			try {
				Application.Current.MainWindow.IsEnabled = false;
				var ext = Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext);
				var ofd = new Microsoft.Win32.OpenFileDialog() {
					Filter = "ふたば画像ファイル|"
						+ string.Join(";", ext.Select(x => "*" + x).ToArray())
						+ "|すべてのファイル|*.*"
				};
				if(ofd.ShowDialog() ?? false) {
					p.ImagePath.Value = ofd.FileName;
					// ダイアログをダブルクリックで選択するとウィンドウに当たり判定がいくので
					// 一度待つ
					await Task.Delay(1);
				}
			}
			finally {
				Application.Current.MainWindow.IsEnabled = true;
			}
		}

		private void OnUploadDragOver(DragEventArgs e) {
			if(IsValidDragFile(e, Config.ConfigLoader.MimeUp2.Types.Select(x => x.Ext).ToArray())) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnUploadDrop(DragEventArgs e) {
			if(this.IsPosting.Value) {
				return;
			}

			if((e.Source as FrameworkElement)?.DataContext is Model.PostHolder postData) {
				if(IsValidDragFile(e, Config.ConfigLoader.MimeUp2.Types.Select(x => x.Ext).ToArray())
					&& e.Data.GetData(DataFormats.FileDrop) is string[] files) {

					this.UploadUp2(postData, files[0]);
				} else {
					Util.Futaba.PutInformation(new Information("未対応のファイル"));
				}
			}
		}

		private static bool IsValidDragFile(DragEventArgs e, string[] ext) {
			//var ext = Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext);
			return (e.Data.GetDataPresent(DataFormats.FileDrop, false)
				&& e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files
				&& (files.Length == 1)
				&& ext.Contains(Path.GetExtension(files[0]).ToLower()));
		}

		public void OnPostViewPostClick(Model.PostHolder x) {
			if(this.IsPosting.Value) {
				return;
			}

			if(x != null) {
				if(!x.PostButtonCommand.CanExecute()) {
					return;
				}

				this.IsPosting.Value = true;
				if(x.Url.IsCatalogUrl) {
					Util.Futaba.PostThread(x.Board,
						x.NameEncoded.Value,
						x.MailEncoded.Value,
						x.SubjectEncoded.Value,
						x.CommentEncoded.Value,
						x.ImagePath.Value,
						x.Password.Value)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Finally(() => { this.IsPosting.Value = false; })
					.Subscribe(y => {
						if(y.Successed) {
							Messenger.Instance.GetEvent<PubSubEvent<PostCloseMessage>>().Publish(new PostCloseMessage(x.Url));
							x.Reset();
							Task.Run(async () => {
								await Task.Delay(1000); // すぐにスレが作られないので1秒くらい待つ
								Util.Futaba.UpdateThreadRes(x.Board, y.NextOrMessage)
									.ObserveOn(UIDispatcherScheduler.Default)
									.Subscribe(z => {
										// TODO: utilでやる
										if((z.New != null) && (z.New.ResItems.Length != 0)) {
											Util.Futaba.PostItems.Value = Util.Futaba.PostItems.Value
												.Concat(new[] { new Data.PostedResItem(x.Url.BaseUrl, z.New.ResItems.FirstOrDefault().ResItem) })
												.ToArray();
										}
									});
							});
						} else {
							Util.Futaba.PutInformation(new Information(y.NextOrMessage));
						}
					});
				} else {
					Util.Futaba.PostRes(x.Board, x.Url.ThreadNo,
						x.NameEncoded.Value,
						x.MailEncoded.Value,
						x.SubjectEncoded.Value,
						x.CommentEncoded.Value,
						x.ImagePath.Value,
						x.Password.Value)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Finally(() => { this.IsPosting.Value = false; })
					.Subscribe(y => {
						if(y.Successed) {
							var com = x.CommentEncoded.Value;
							if(string.IsNullOrEmpty(com)) {
								var b = Config.ConfigLoader.Board.Boards
									.Where(z => z.Display && (z.Url == x.Url.BaseUrl))
									.FirstOrDefault();
								if(b != null) {
									com = b.DefaultComment;
								}
							}
							Messenger.Instance.GetEvent<PubSubEvent<PostCloseMessage>>().Publish(new PostCloseMessage(x.Url));
							x.Reset();
							Util.Futaba.UpdateThreadRes(x.Board, x.Url.ThreadNo, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental)
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(z => {
									// TODO: utilでやる
									if(z.New != null) {
										var resCount = z.Old?.ResItems.Length ?? 0;
										foreach(var res in z.New.ResItems.Skip(resCount).Reverse()) {
											var cLine = com.Replace("\r", "").Split('\n');
											var rLine = Regex.Split(res.ResItem.Res.Com, "<br>");
											if(cLine.Length == rLine.Length) {
												var success = 0;
												bool dice = false;
												for(var i = 0; i < cLine.Length; i++) {
													var c = cLine[i];
													var r = rLine[i];

													// ダイス判定(ダイスは1レスに1回だけ)
													if(!dice) {
														var m1 = Regex.Match(
															c,
															@"^dice(10|\d)d(10000|\d{1,4})([+-]\d+)?=?",
															RegexOptions.IgnoreCase);
														if(m1.Success) {
															dice = true;
															var m2 = Regex.Match(r, @"<font[^>]*>[^<]+</font>");
															if(m2.Success) {
																// ダイスの領域を削る
																c = c.Substring(m1.Length);
																r = r.Substring(
																	m1.Length + m2.Length
																		+ ((m1.Groups[0].Value.Last() == '=') ? 0 : 1));
															}
														}
													}

													if(System.Web.HttpUtility.HtmlDecode(Regex.Replace(r, @"<[^>]*>", "")) == c) {
														success++;
													} else {
														break;
													}
												}

												if(cLine.Length == success) {
													Util.Futaba.PostItems.Value = Util.Futaba.PostItems.Value
														.Concat(new[] { new Data.PostedResItem(x.Url.BaseUrl, res.ResItem) })
														.ToArray();
													break;
												}
											}
										}
									}
								});
						} else {
							Util.Futaba.PutInformation(new Information(y.Message, x.Url));
						}
					});
				}
			}
		}

		private async Task<string> Paste(Model.PostHolder postData, int maxFileSize) {
			var fileName = Util.TimeUtil.ToUnixTimeMilliseconds().ToString();
			if(Clipboard.ContainsImage()) {
				var path = Path.Combine(
					Config.ConfigLoader.InitializedSetting.CacheDirectory,
					$"{ fileName }.jpg");
				WpfUtil.ImageUtil.SaveJpeg(
					path,
					Clipboard.GetImage(),
					WpfConfig.WpfConfigLoader.SystemConfig.ClipbordJpegQuality);
				return path;
			} else if(Clipboard.ContainsText() && WpfConfig.WpfConfigLoader.SystemConfig.ClipbordIsEnabledUrl) {
				if(Uri.TryCreate(Clipboard.GetText(), UriKind.Absolute, out var uri)) {
					if((uri.Scheme != "http") && (uri.Scheme != "https")) {
						// BLOB URLは読めないのでreturn
						// 通知したほうがいいのかな…？
						// file://は読みに行ったほうがいいのか…？
						return "";
					}
					using(var client = new System.Net.Http.HttpClient() {
						Timeout = TimeSpan.FromMilliseconds(5000),
					}) {
						try {
							client.DefaultRequestHeaders.Add("User-Agent", WpfUtil.PlatformUtil.GetContentType());
							var ret1 = await client.SendAsync(new System.Net.Http.HttpRequestMessage(
								System.Net.Http.HttpMethod.Head, uri));
							if((ret1.StatusCode == System.Net.HttpStatusCode.OK) && (ret1.Content.Headers.ContentLength <= maxFileSize)) {
								var mime = Config.ConfigLoader.MimeFutaba.Types
									.Where(x => x.MimeType == ret1.Content.Headers.ContentType.MediaType)
									.FirstOrDefault();
								if(mime != null) {
									var path = Path.Combine(
										Config.ConfigLoader.InitializedSetting.CacheDirectory,
										$"{ fileName }{ mime.Ext }");
									var ret2 = await client.SendAsync(new System.Net.Http.HttpRequestMessage(
										System.Net.Http.HttpMethod.Get, uri));
									if(ret2.StatusCode == System.Net.HttpStatusCode.OK) {
										using(var fs = new FileStream(path, FileMode.OpenOrCreate)) {
											await ret2.Content.CopyToAsync(fs);
										}
										return path;
									} else {
										Util.Futaba.PutInformation(new Information("URLの画像取得に失敗", postData.Url));
									}
								}
							} else {
								Util.Futaba.PutInformation(new Information("URLの情報取得に失敗(HTTP失敗)", postData.Url));
							}
						}
						catch(TaskCanceledException) { // タイムアウト時くる
							Util.Futaba.PutInformation(new Information("URLの情報取得に失敗(タイムアウト)", postData.Url));
						}
					}
				}
			}
			return "";
		}

		private async void OnMenuItemClickPastePostImage(Model.PostHolder p) {
			await PasteImage(p);
		}

		private async void OnMenuItemClickPastePostUp(Model.PostHolder p) {
			await PaseteUploader2(p);
		}
		
		public async Task PasteImage(Model.PostHolder postData) {
			if(postData.Url.IsThreadUrl && !postData.Board.Extra.ResImage) {
				return;
			}

			var path = await Paste(postData, Config.ConfigLoader.Board.MaxFileSize);
			if(!string.IsNullOrEmpty(path) && File.Exists(path)) {
				postData.ImagePath.Value = path;
			}
		}
		public async Task PaseteUploader2(Model.PostHolder postData) {
			if(postData == null) {
				return;
			}

			var path = await Paste(postData, 3072000); // TODO: 設定ファイルに移動
			if(!string.IsNullOrEmpty(path) && File.Exists(path)) {
				this.UploadUp2(postData, path);
			}
		}

		private void OnKeyBindingPost()
			=> Messenger.Instance.GetEvent<PubSubEvent<PostCommandMessage>>()
				.Publish(new PostCommandMessage(this.Token));
		private void OnKeyBindingOpenImage()
			=> Messenger.Instance.GetEvent<PubSubEvent<OpenImageCommandMessage>>()
				.Publish(new OpenImageCommandMessage(this.Token));
		private void OnKeyBindingOpenUpload()
			=> Messenger.Instance.GetEvent<PubSubEvent<OpenLoaderCommandMessage>>()
				.Publish(new OpenLoaderCommandMessage(this.Token));
		private void OnKeyBindingDelete()
			=> Messenger.Instance.GetEvent<PubSubEvent<DeleteCommandMessage>>()
				.Publish(new DeleteCommandMessage(this.Token));
		private void OnKeyBindingClose()
			=> Messenger.Instance.GetEvent<PubSubEvent<CloseCommandMessage>>()
				.Publish(new CloseCommandMessage(this.Token));
		private void OnKeyBindingPasteImage()
			=> Messenger.Instance.GetEvent<PubSubEvent<PasteImageCommandMessage>>()
				.Publish(new PasteImageCommandMessage(this.Token));
		private void OnKeyBindingPasteLoader()
			=> Messenger.Instance.GetEvent<PubSubEvent<PasteLoaderCommandMessage>>()
				.Publish(new PasteLoaderCommandMessage(this.Token));

	}
}