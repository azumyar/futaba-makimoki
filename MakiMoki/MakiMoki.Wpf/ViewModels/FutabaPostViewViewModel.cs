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
		internal class PostEndedMessage {
			public UrlContext Url { get; }

			public PostEndedMessage(UrlContext url) {
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

		private CompositeDisposable Disposable { get; } = new CompositeDisposable();


		public ReactiveCommand<RoutedPropertyChangedEventArgs<Model.BindableFutaba>> ContentsChangedCommand { get; } 
			= new ReactiveCommand<RoutedPropertyChangedEventArgs<Model.BindableFutaba>>();

		public ReactiveCommand<DragEventArgs> ImageDragOverCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<DragEventArgs> ImageDropCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<MouseButtonEventArgs> OpenUploadCommand { get; } = new ReactiveCommand<MouseButtonEventArgs>();
		public ReactiveCommand<DragEventArgs> UploadDragOverpCommand { get; } = new ReactiveCommand<DragEventArgs>();
		public ReactiveCommand<DragEventArgs> UploadDroppCommand { get; } = new ReactiveCommand<DragEventArgs>();

		public ReactiveCommand<RoutedEventArgs> PostViewPostCommand { get; } = new ReactiveCommand<RoutedEventArgs>();


		public FutabaPostViewViewModel() {
			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));

			OpenUploadCommand.Subscribe(x => OnOpenUpload(x));
			ImageDragOverCommand.Subscribe(x => OnImageDragOver(x));
			ImageDropCommand.Subscribe(x => OnImageDrop(x));
			UploadDragOverpCommand.Subscribe(x => OnUploadDragOver(x));
			UploadDroppCommand.Subscribe(x => OnUploadDrop(x));
			PostViewPostCommand.Subscribe(x => OnPostViewPostClick((x.Source as FrameworkElement)?.DataContext as Model.BindableFutaba));
		}

		public void Dispose() {
			Disposable.Dispose();
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
						break;
					}
				}
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
							Messenger.Instance.GetEvent<PubSubEvent<PostEndedMessage>>().Publish(new PostEndedMessage(x.Url));
							x.PostData.Value = new Model.BindableFutaba.PostHolder();
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
							Messenger.Instance.GetEvent<PubSubEvent<PostEndedMessage>>().Publish(new PostEndedMessage(x.Url));
							x.PostData.Value = new Model.BindableFutaba.PostHolder();
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
	}
}