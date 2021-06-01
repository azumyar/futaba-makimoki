using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Prism.Regions;
using Prism.Events;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Data;
using Windows.UI.Xaml;
using System.IO;
using Windows.UI.Input;
using Android.Provider;

#if __ANDROID__
using Android.Content;
#endif

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	abstract class PostPageViewModel : ViewModelBase {
		internal class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

#if __ANDROID__
		public class DroidPostChoseResultMessage {
			public int RequestCode { get; }
			public global::Android.App.Result ResultCode { get; }
			public global::Android.Content.Intent Intent { get; }

			public DroidPostChoseResultMessage(
				int requestCode,
				global::Android.App.Result resultCode,
				global::Android.Content.Intent intent) {

				this.RequestCode = requestCode;
				this.ResultCode = resultCode;
				this.Intent = intent;
			}
		}
#endif

		public class Navigation {
			public bool IsThreadMode { get; }
			public bool IsResMode { get; }
			public BoardData Board { get; }
			public UrlContext Url { get; }
			public string Commnet { get; }

			private Navigation(BoardData board, UrlContext url = null, string comment="") {
				this.IsThreadMode = url == null;
				this.IsResMode = url != null;
				this.Board = board;
				this.Url = url;
				this.Commnet = comment;
			}

			public static NavigationParameters FromParamater(BoardData board) {
				return new NavigationParameters() {
					{
						typeof(Navigation).FullName,
						new Navigation(board: board)
					}
				};
			}

			public static NavigationParameters FromParamater(BoardData board, UrlContext url, string comment="") {
				return new NavigationParameters() {
					{
						typeof(Navigation).FullName,
						new Navigation(
							board: board,
							url: url,
							comment: comment)
					}
				};
			}

			public static Navigation FromContext(NavigationContext navigationContext) {
				if(navigationContext.Parameters.TryGetValue<object>(typeof(Navigation).FullName, out var p)
					&& (p is Navigation n)) {

					return n;
				}
				throw new InvalidOperationException();
			}
		}

		private readonly IRegionManager regionManager;
		private readonly IDialogService dialogService;
		public ReactiveProperty<Models.PostHolder> PostHolder { get; } = new ReactiveProperty<Models.PostHolder>(new Models.PostHolder());
		public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>("");
		public ReactiveProperty<Visibility> ImageButtonVisibirity { get; } = new ReactiveProperty<Visibility>(Visibility.Visible);
		public ReactiveProperty<Visibility> IdButtonVisibirity { get; } = new ReactiveProperty<Visibility>(Visibility.Visible);
		public ReactiveProperty<Visibility> IpButtonVisibirity { get; } = new ReactiveProperty<Visibility>(Visibility.Visible);
		public ReactiveProperty<Visibility> ImageDeleteButtonVisibirity { get; }

		public ReactiveCommand BackClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand ImageClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand UploadClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand UploadRightTappedCommand { get; } = new ReactiveCommand();
		public ReactiveCommand DeleteClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand CancelImageClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand SageClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand IdClickCommand { get; } = new ReactiveCommand();
		public ReactiveCommand IpClickCommand { get; } = new ReactiveCommand();

		protected ReactiveProperty<bool> IsUpplading2 { get; } = new ReactiveProperty<bool>(false);
		protected ReactiveProperty<bool> IsPosting { get; } = new ReactiveProperty<bool>(false);
		public IDisposable DroidPostChoseResultEvent { get; }

		public PostPageViewModel(IRegionManager regionManager, IDialogService dialogService) {
			this.regionManager = regionManager;
			this.dialogService = dialogService;

			this.ImageDeleteButtonVisibirity = this.PostHolder.Value.ImagePath
				.Select(x => string.IsNullOrEmpty(x) ? Visibility.Collapsed : Visibility.Visible)
				.ToReactiveProperty();

			this.BackClickCommand.Subscribe(() => this.OnBackClick());
			this.PostHolder.Value.PostButtonCommand.Subscribe(() => this.OnPostClick());
			this.ImageClickCommand.Subscribe(() => this.OnImageClick());
			this.UploadClickCommand.Subscribe(() => this.OnUploadClick());
			this.UploadRightTappedCommand.Subscribe(() => this.OnUploadRightTapped());
			this.DeleteClickCommand.Subscribe(() => this.OnDeleteClick());
			this.CancelImageClickCommand.Subscribe(() => this.OnCancelImageClick());

			this.SageClickCommand.Subscribe(() => this.OnSageClick());
			this.IdClickCommand.Subscribe(() => this.OnIdClick());
			this.IpClickCommand.Subscribe(() => this.OnIpClick());

#if __ANDROID__
			this.DroidPostChoseResultEvent = Messenger.Instance.GetEvent<PubSubEvent<DroidPostChoseResultMessage>>()
				.Subscribe(x => {
					string GetPath(global::Android.Net.Uri uri) {
						if(uri.Scheme?.ToLower() == "content") { 
						    var projection = new string[] {
								global::Android.Provider.MediaStore.MediaColumns.DisplayName
							}; 
							try { 
								using(var cursor = Droid.MainActivity.ActivityContext.ContentResolver.Query(
									uri, projection, null, null, null)) {
									if((cursor != null) && cursor.MoveToFirst()) {
										return cursor.GetString(
											cursor.GetColumnIndexOrThrow(
												global::Android.Provider.MediaStore.MediaColumns.DisplayName));
									}
								}
							}
							catch (Exception) { } 
						} else if (uri.Scheme?.ToLower() == "file") { 
							return uri.Path; 
						}
						return "";
					}

					void Copy(global::Android.Net.Uri input, string output) {
						var b = new byte[1024];
						using(var ins = Droid.MainActivity.ActivityContext.ContentResolver.OpenInputStream(input))
						using(var ous = new FileStream(output, FileMode.Create)) {
							var i = 0;
							while(0 < (i = ins.Read(b, 0, b.Length))) {
								ous.Write(b, 0, i);
							}
							ous.Flush();
						}
					}

					if(x.ResultCode == global::Android.App.Result.Ok) {
						MessageDialogViewModel.ShowDialog(
							this.dialogService,
							(r) => {
								if(r.Result == ButtonResult.OK) {
									var uri = x.Intent.Data;
									var filePath = GetPath(uri);
									var fileName = Util.TimeUtil.ToUnixTimeMilliseconds().ToString();
									var path = Path.Combine(
										Config.ConfigLoader.InitializedSetting.CacheDirectory,
										$"{ fileName }.{ Path.GetExtension(filePath) }");
									try {
										Copy(uri, path);
										if(x.RequestCode == Droid.MainActivity.RequestCodeChoosePostImage) {
											this.PostHolder.Value.ImagePath.Value = path;
										} else if(x.RequestCode == Droid.MainActivity.RequestCodeChooseUploadItem) {
											this.Upload(path);
										}
									}
									catch(IOException e) {
										UnoHelpers.Toast.Show($"ファイルコピー失敗");
									}
								}
							});
					}
				});
#endif
		}

		private void OnBackClick() {
			if(this.BackClickCommand.CanExecute()) {
				this.BackClickCommand.Execute();
			}
		}
		protected abstract void OnPostClick();

		private void OnImageClick() {
#if __ANDROID__
			Droid.MainActivity.ActivityContext.StartActivityForResult(
				Intent.CreateChooser(
					new Intent()
						.SetAction(Intent.ActionOpenDocument)
						.SetType("image/* video/*")
						.AddCategory(Intent.CategoryOpenable),
					"添付画像の選択"),
				Droid.MainActivity.RequestCodeChoosePostImage);
#endif

		}
		private void OnUploadClick() {
#if __ANDROID__
			Droid.MainActivity.ActivityContext.StartActivityForResult(
				Intent.CreateChooser(
					new Intent()
						.SetAction(Intent.ActionOpenDocument)
						.SetType("image/* video/*")
						.AddCategory(Intent.CategoryOpenable),
					//.SetData(MediaStore.Images.Media.ExternalContentUri),
					"添付画像の選択"),
				Droid.MainActivity.RequestCodeChooseUploadItem);
#endif
		}

		private void OnUploadRightTapped() {
#if __ANDROID__
			Droid.MainActivity.ActivityContext.StartActivityForResult(
				Intent.CreateChooser(
					new Intent()
						.SetAction(Intent.ActionOpenDocument)
						.SetType("file/*"),
					"添付ファイルの選択"),
				Droid.MainActivity.RequestCodeChooseUploadItem);
#endif
		}

		private void OnDeleteClick() {
			this.PostHolder.Value.Reset();
		}

		private void OnCancelImageClick() {
			this.PostHolder.Value.ImagePath.Value = "";
		}

		private void OnSageClick() {
			this.PostHolder.Value.Mail.Value = "sage";
		}
		private void OnIdClick() {
			this.PostHolder.Value.Mail.Value = "id表示";
		}
		private void OnIpClick() {
			this.PostHolder.Value.Mail.Value = "ip表示";
		}

		private void Upload(string path) {
			if(this.IsPosting.Value || this.IsUpplading2.Value) {
				return;
			}

			this.IsUpplading2.Value = true;
			Util.Futaba.UploadUp2(path, this.PostHolder.Value.Password.Value)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Finally(() => { this.IsUpplading2.Value = false; })
				.Subscribe(x => {
					if(x.Successed) {
						//Messenger.Instance.GetEvent<PubSubEvent<AppendTextMessage>>()
						//	.Publish(new AppendTextMessage(f.Url, x.FileNameOrMessage));
						if(string.IsNullOrEmpty(this.PostHolder.Value.Comment.Value)) {
							this.PostHolder.Value.Comment.Value = x.FileNameOrMessage;
						} else {
							this.PostHolder.Value.Comment.Value += $"{ Environment.NewLine }{ x.FileNameOrMessage }";
						}
					} else {
						//Util.Futaba.PutInformation(new Information($"アップロード失敗：{ x.FileNameOrMessage }", f));
						UnoHelpers.Toast.Show($"アップロード失敗：{ x.FileNameOrMessage }");
					}
				});
		}

	}
}
