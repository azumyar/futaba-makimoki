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

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaPostViewViewModel : BindableBase, IDisposable {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
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

		public ReactiveCommand<RoutedPropertyChangedEventArgs<Model.BindableFutaba>> ContentsChangedCommand { get; }
			= new ReactiveCommand<RoutedPropertyChangedEventArgs<Model.BindableFutaba>>();

		public ReactiveCommand<DragEventArgs> ImageDragOverCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<DragEventArgs> ImageDropCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> OpenUploadCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<DragEventArgs> UploadDragOverpCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<DragEventArgs> UploadDroppCommand { get; } = new ReactiveCommand<DragEventArgs>();

		public ReactiveCommand<RoutedEventArgs> PostViewPostCommand { get; } = new ReactiveCommand<RoutedEventArgs>();


		public ReactiveCommand<Model.BindableFutaba> KeyBindingPostCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingOpenImageCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingOpenUploadCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingDeleteCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingCloseCommand { get; } = new ReactiveCommand<Model.BindableFutaba>();
		public ReactiveCommand<Model.BindableFutaba> KeyBindingClipbordCommand { get; } = new ReactiveCommand<BindableFutaba>();

		public FutabaPostViewViewModel() {
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			OpenUploadCommand.Subscribe(x => OnOpenUpload(x));
			ImageDragOverCommand.Subscribe(x => OnImageDragOver(x));
			ImageDropCommand.Subscribe(x => OnImageDrop(x));
			UploadDragOverpCommand.Subscribe(x => OnUploadDragOver(x));
			UploadDroppCommand.Subscribe(x => OnUploadDrop(x));
			PostViewPostCommand.Subscribe(x => OnPostViewPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));

			KeyBindingPostCommand.Subscribe(x => OnKeyBindingPost(x));
			KeyBindingOpenImageCommand.Subscribe(x => OnKeyBindingOpenImage(x));
			KeyBindingOpenUploadCommand.Subscribe(x => OnKeyBindingOpenUpload(x));
			KeyBindingDeleteCommand.Subscribe(x => OnKeyBindingDelete(x));
			KeyBindingCloseCommand.Subscribe(x => OnKeyBindingClose(x));
			KeyBindingClipbordCommand.Subscribe(x => OnKeyBindingClipbord(x));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		private async Task OpenUpload(Model.BindableFutaba f) {
			try {
				Application.Current.MainWindow.IsEnabled = false;
				var ext = Config.ConfigLoader.Mime.Types.Select(x => x.Ext);
				var ofd = new Microsoft.Win32.OpenFileDialog() {
					Filter = "ふたば画像ファイル|"
						+ string.Join(";", ext.Select(x => "*" + x).ToArray())
						+ "|すべてのファイル|*.*"
				};
				if(ofd.ShowDialog() ?? false) {
					Util.Futaba.UploadUp2(ofd.FileName, f.PostData.Value.Password.Value)
						.Subscribe(x => {
							if(x.Successed) {
								Messenger.Instance.GetEvent<PubSubEvent<AppendTextMessage>>()
									.Publish(new AppendTextMessage(f.Url, x.FileNameOrMessage));
							} else {
								MessageBox.Show(x.FileNameOrMessage);
							}
						});
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
			if(IsValidDragFile(e)) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnImageDrop(DragEventArgs e) {
			if((e.Source as FrameworkElement)?.DataContext is Model.BindableFutaba f) {
				if(IsValidDragFile(e) && e.Data.GetData(DataFormats.FileDrop) is string[] files) {
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

		private async Task OpenImage(Model.BindableFutaba f) {
			try {
				Application.Current.MainWindow.IsEnabled = false;
				var ext = Config.ConfigLoader.Mime.Types.Select(x => x.Ext);
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
			if(IsValidDragFile(e)) {
				e.Effects = DragDropEffects.Copy;
			} else {
				e.Effects = DragDropEffects.None; // 機能していないけど今度調べる
			}

			e.Handled = true;
		}

		private void OnUploadDrop(DragEventArgs e) {
			if((e.Source as FrameworkElement)?.DataContext is Model.BindableFutaba f) {
				if(IsValidDragFile(e) && e.Data.GetData(DataFormats.FileDrop) is string[] files) {
					Util.Futaba.UploadUp2(files[0], f.PostData.Value.Password.Value)
						.Subscribe(x => {
							if(x.Successed) {
								Messenger.Instance.GetEvent<PubSubEvent<AppendTextMessage>>()
									.Publish(new AppendTextMessage(f.Url, x.FileNameOrMessage));
							} else {
								MessageBox.Show(x.FileNameOrMessage);
							}
						});
				} else {
					Util.Futaba.PutInformation(new Information("未対応のファイル"));
				}
			}
		}

		private bool IsValidDragFile(DragEventArgs e) {
			var ext = Config.ConfigLoader.Mime.Types.Select(x => x.Ext);
			return (e.Data.GetDataPresent(DataFormats.FileDrop, false)
				&& e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files
				&& (files.Length == 1)
				&& ext.Contains(Path.GetExtension(files[0]).ToLower()));
		}

		private void OnPostViewPostClick(Model.BindableFutaba x) {
			if(x != null) {
				if(!x.PostData.Value.PostButtonCommand.CanExecute()) {
					return;
				}

				if(x.Url.IsCatalogUrl) {
					Util.Futaba.PostThread(x.Raw.Bord,
						x.PostData.Value.Name.Value,
						x.PostData.Value.Mail.Value,
						x.PostData.Value.Subject.Value,
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
						x.PostData.Value.Name.Value,
						x.PostData.Value.Mail.Value,
						x.PostData.Value.Subject.Value,
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

		private void OnKeyBindingPost(Model.BindableFutaba f) {
			this.OnPostViewPostClick(f);
		}

		private async void OnKeyBindingOpenImage(Model.BindableFutaba f) {
			await this.OpenImage(f);
		}

		private async void OnKeyBindingOpenUpload(Model.BindableFutaba f) {
			await this.OpenUpload(f);
		}

		private void OnKeyBindingDelete(Model.BindableFutaba f) {
			f.PostData.Value.Reset();
		}

		private void OnKeyBindingClose(Model.BindableFutaba f) {
			Messenger.Instance.GetEvent<PubSubEvent<PostCloseMessage>>().Publish(new PostCloseMessage(f.Url));
		}

		private async void OnKeyBindingClipbord(Model.BindableFutaba f) {
			if(!f.Raw.Bord.Extra.ResImageValue) {
				return;
			}

			var path = "";
			var fileName = new DateTimeOffset(DateTime.Now, new TimeSpan(+09, 00, 00)).ToUnixTimeMilliseconds().ToString();
			if(Clipboard.ContainsImage()) {
				path = Path.Combine(
					Config.ConfigLoader.InitializedSetting.CacheDirectory,
					$"{ fileName }.jpeg");
				WpfUtil.ImageUtil.SaveJpeg(
					path,
					Clipboard.GetImage(),
					WpfConfig.WpfConfigLoader.SystemConfig.ClipbordJpegQuality);
			} else if(Clipboard.ContainsText() && WpfConfig.WpfConfigLoader.SystemConfig.ClipbordIsEnabledUrl) {
				if(Uri.TryCreate(Clipboard.GetText(), UriKind.Absolute, out var uri)) {
					using(var client = new System.Net.Http.HttpClient() {
						Timeout = TimeSpan.FromMilliseconds(5000),
					}) {
						var ret1 = await client.SendAsync(new System.Net.Http.HttpRequestMessage(
							System.Net.Http.HttpMethod.Head, uri));
						if((ret1.StatusCode == System.Net.HttpStatusCode.OK) && (ret1.Content.Headers.ContentLength <= 3072000)) { // TODO: 設定ファイルに移動
							var mime = Config.ConfigLoader.Mime.Types
								.Where(x => x.MimeType == ret1.Content.Headers.ContentType.MediaType)
								.FirstOrDefault();
							if(mime != null) {
								path = Path.Combine(
									Config.ConfigLoader.InitializedSetting.CacheDirectory,
									$"{ fileName }{ mime.Ext }");
								var ret2 = await client.SendAsync(new System.Net.Http.HttpRequestMessage(
									System.Net.Http.HttpMethod.Get, uri));
								if(ret2.StatusCode == System.Net.HttpStatusCode.OK) {
									using(var fs = new FileStream(path, FileMode.OpenOrCreate)) {
										await ret2.Content.CopyToAsync(fs);
									}
									goto end;
								} else {
									Util.Futaba.PutInformation(new Information("URLの画像取得に失敗"));
								}
							}
						} else {
							Util.Futaba.PutInformation(new Information("URLの情報取得に失敗"));
						}
					}
				end:;
				}
			}

			if(!string.IsNullOrEmpty(path) && File.Exists(path)) {
				f.PostData.Value.ImagePath.Value = path;
			}
		}
	}
}