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
using Yarukizero.Net.MakiMoki.Wpf.Controls;
using Yarukizero.Net.MakiMoki.Wpf.Model;
using Yarukizero.Net.MakiMoki.Wpf.Reactive;

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
		public ReactiveProperty<KeyBinding[]> KeyGestures { get; } = new ReactiveProperty<KeyBinding[]>();

		public MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.BindableFutaba>> ContentsChangedCommand { get; }
			= new MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.BindableFutaba>>();

		public MakiMokiCommand<DragEventArgs> ImageDragOverCommand { get; } = new MakiMokiCommand<DragEventArgs>();
		public MakiMokiCommand<DragEventArgs> ImageDropCommand { get; } = new MakiMokiCommand<DragEventArgs>();
		public MakiMokiCommand<MouseButtonEventArgs> OpenUploadCommand { get; } = new MakiMokiCommand<MouseButtonEventArgs>();
		public MakiMokiCommand<DragEventArgs> UploadDragOverpCommand { get; } = new MakiMokiCommand<DragEventArgs>();
		public MakiMokiCommand<DragEventArgs> UploadDroppCommand { get; } = new MakiMokiCommand<DragEventArgs>();

		public MakiMokiCommand<RoutedEventArgs> PostViewPostCommand { get; } = new MakiMokiCommand<RoutedEventArgs>();

		public MakiMokiCommand<BindableFutaba> MenuItemClickPastePostImageCommand { get; } = new MakiMokiCommand<BindableFutaba>();
		public MakiMokiCommand<BindableFutaba> MenuItemClickPastePostUpCommand { get; } = new MakiMokiCommand<BindableFutaba>();

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
			UpdateKeyBindings();
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			OpenUploadCommand.Subscribe(x => OnOpenUpload(x));
			ImageDragOverCommand.Subscribe(x => OnImageDragOver(x));
			ImageDropCommand.Subscribe(x => OnImageDrop(x));
			UploadDragOverpCommand.Subscribe(x => OnUploadDragOver(x));
			UploadDroppCommand.Subscribe(x => OnUploadDrop(x));
			PostViewPostCommand.Subscribe(x => OnPostViewPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));

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

		private void UploadUp2(Model.BindableFutaba f, string path) {
			Util.Futaba.UploadUp2(path, f.PostData.Value.Password.Value)
				.Subscribe(x => {
					if(x.Successed) {
						Messenger.Instance.GetEvent<PubSubEvent<AppendTextMessage>>()
							.Publish(new AppendTextMessage(f.Url, x.FileNameOrMessage));
					} else {
						Util.Futaba.PutInformation(new Information($"アップロード失敗：{ x.FileNameOrMessage }"));
					}
				});
		}

		public async Task OpenUpload(Model.BindableFutaba f) {
			if(f == null) {
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
					this.UploadUp2(f, ofd.FileName);

					// ダイアログをダブルクリックで選択するとウィンドウに当たり判定がいくので
					// 一度待つ
					await Task.Delay(1);
				}
			}
			finally {
				Application.Current.MainWindow.IsEnabled = true;
			}
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<Model.BindableFutaba> e) { }

		private void OnImageDragOver(DragEventArgs e) {
			if(IsValidDragFile(e, Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext).ToArray())) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnImageDrop(DragEventArgs e) {
			if((e.Source as FrameworkElement)?.DataContext is Model.BindableFutaba f) {
				if(IsValidDragFile(e, Config.ConfigLoader.MimeFutaba.Types.Select(x => x.Ext).ToArray())
					&& e.Data.GetData(DataFormats.FileDrop) is string[] files) {

					f.PostData.Value.ImagePath.Value = files[0];
				} else {
					Util.Futaba.PutInformation(new Information("未対応のファイル"));
				}
			}
		}

		private async void OnOpenUpload(MouseButtonEventArgs e) {
			if(e.Source is FrameworkElement o && o.DataContext is Model.BindableFutaba f) {
				if((e.ClickCount == 1) && (VisualTreeHelper.HitTest(o, e.GetPosition(o)) != null)) {
					switch(e.ChangedButton) {
					case MouseButton.Left:
						e.Handled = true;
						await OpenUpload(f);
						break;
					}
				}
			}
		}

		public async Task OpenImage(Model.BindableFutaba f) {
			if(f == null) {
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
					f.PostData.Value.ImagePath.Value = ofd.FileName;
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
			if((e.Source as FrameworkElement)?.DataContext is Model.BindableFutaba f) {
				if(IsValidDragFile(e, Config.ConfigLoader.MimeUp2.Types.Select(x => x.Ext).ToArray())
					&& e.Data.GetData(DataFormats.FileDrop) is string[] files) {

					this.UploadUp2(f, files[0]);
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

		public void OnPostViewPostClick(Model.BindableFutaba x) {
			if(x != null) {
				if(!x.PostData.Value.PostButtonCommand.CanExecute()) {
					return;
				}

				if(x.Url.IsCatalogUrl) {
					Util.Futaba.PostThread(x.Raw.Bord,
						x.PostData.Value.NameEncoded.Value,
						x.PostData.Value.MailEncoded.Value,
						x.PostData.Value.SubjectEncoded.Value,
						x.PostData.Value.CommentEncoded.Value,
						x.PostData.Value.ImagePath.Value,
						x.PostData.Value.Password.Value)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(y => {
						if(y.Successed) {
							Messenger.Instance.GetEvent<PubSubEvent<PostCloseMessage>>().Publish(new PostCloseMessage(x.Url));
							x.PostData.Value.Reset();
							Task.Run(async () => {
								await Task.Delay(1000); // すぐにスレが作られないので1秒くらい待つ
								Util.Futaba.UpdateThreadRes(x.Raw.Bord, y.NextOrMessage)
									.ObserveOn(UIDispatcherScheduler.Default)
									.Subscribe(z => {
										// TODO: utilでやる
										if((z.New != null) && (z.New.ResItems.Length != 0)) {
											Util.Futaba.PostItems.Value = Util.Futaba.PostItems.Value
												.Concat(new[] { new Data.PostedResItem(x.Raw.Url.BaseUrl, z.New.ResItems.FirstOrDefault().ResItem) })
												.ToArray();
										}
									});
							});
						} else {
							Util.Futaba.PutInformation(new Information(y.NextOrMessage));
						}
					});
				} else {
					var resCount = x.ResCount.Value;
					Util.Futaba.PostRes(x.Raw.Bord, x.Url.ThreadNo,
						x.PostData.Value.NameEncoded.Value,
						x.PostData.Value.MailEncoded.Value,
						x.PostData.Value.SubjectEncoded.Value,
						x.PostData.Value.CommentEncoded.Value,
						x.PostData.Value.ImagePath.Value,
						x.PostData.Value.Password.Value)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(y => {
						if(y.Successed) {
							var com = x.PostData.Value.CommentEncoded.Value;
							/*
							if(string.IsNullOrEmpty(com)) {
								// TODO: cに板のデフォルトテキストを
							}
							*/
							Messenger.Instance.GetEvent<PubSubEvent<PostCloseMessage>>().Publish(new PostCloseMessage(x.Url));
							x.PostData.Value.Reset();
							Util.Futaba.UpdateThreadRes(x.Raw.Bord, x.Url.ThreadNo, Config.ConfigLoader.MakiMoki.FutabaThreadGetIncremental)
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(z => {
									// TODO: utilでやる
									if(z.New != null) {
										foreach(var res in z.New.ResItems.Skip(resCount + 1).Reverse()) {
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

													if(Regex.Replace(r, @"<[^>]*>", "") == c) {
														success++;
													} else {
														break;
													}
												}

												if(cLine.Length == success) {
													Util.Futaba.PostItems.Value = Util.Futaba.PostItems.Value
														.Concat(new[] { new Data.PostedResItem(x.Raw.Url.BaseUrl, res.ResItem) })
														.ToArray();
													break;
												}
											}
										}
									}
								});
						} else {
							Util.Futaba.PutInformation(new Information(y.Message));
						}
					});
				}
			}
		}

		private async Task<string> Paste(Model.BindableFutaba f, int maxFileSize) {
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
									Util.Futaba.PutInformation(new Information("URLの画像取得に失敗"));
								}
							}
						} else {
							Util.Futaba.PutInformation(new Information("URLの情報取得に失敗"));
						}
					}
				}
			}
			return "";
		}

		private async void OnMenuItemClickPastePostImage(Model.BindableFutaba f) {
			await PasteImage(f);
		}

		private async void OnMenuItemClickPastePostUp(Model.BindableFutaba f) {
			await PaseteUploader2(f);
		}
		
		public async Task PasteImage(Model.BindableFutaba f) {
			if(f.Raw.Url.IsThreadUrl && !f.Raw.Bord.Extra.ResImage) {
				return;
			}

			var path = await Paste(f, Config.ConfigLoader.Board.MaxFileSize);
			if(!string.IsNullOrEmpty(path) && File.Exists(path)) {
				f.PostData.Value.ImagePath.Value = path;
			}
		}
		public async Task PaseteUploader2(Model.BindableFutaba f) {
			if(f == null) {
				return;
			}

			var path = await Paste(f, 3072000); // TODO: 設定ファイルに移動
			if(!string.IsNullOrEmpty(path) && File.Exists(path)) {
				this.UploadUp2(f, path);
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