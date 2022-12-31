using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using Yarukizero.Net.MakiMoki.Data;
using Yarukizero.Net.MakiMoki.Ng.NgData;
using Yarukizero.Net.MakiMoki.Util;
using Yarukizero.Net.MakiMoki.Reactive;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	public class BindableFutaba : Bindable.CommonBindableFutaba {
		[Helpers.AutoDisposable.IgonoreDispose]
		[Helpers.AutoDisposable.IgonoreDisposeBindingsValue]
		public ReactiveCollection<BindableFutabaResItem> ResItems { get; }
		public ReactiveProperty<FutabaContext[]> OpenedThreads { get; }
		public ReactiveProperty<int> ResCount { get; }
		public ReactiveProperty<string> DieTextLong { get; }


		public MakiMokiCommand ExportCommand { get; } = new MakiMokiCommand();

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

		public MakiMokiCommand FullScreenThreadClickCommand { get; } = new MakiMokiCommand();
		public MakiMokiCommand FullScreenCatalogClickCommand { get; } = new MakiMokiCommand();
		public ReactiveProperty<bool> IsFullScreenThreadMode { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<bool> IsFullScreenCatalogMode { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<int> FullScreenSpan { get; }
		public ReactiveProperty<Visibility> FullScreenVisibility { get; }
		public ReactiveProperty<Visibility> FullScreenCatalogVisibility { get; }
		public ReactiveProperty<Visibility> FullScreenThreadButtonVisibility { get; }
		public ReactiveProperty<Visibility> FullScreenThreadCatalogVisibility { get; }
		public ReactiveProperty<double> FullScreenThreadContainerWidth { get; }
		public ReactiveProperty<int> FullScreenThreadColumn { get; }
		public ReactiveProperty<int> FullScreenThreadColumnSpan { get; }
		public ReactiveProperty<Visibility> FullScreenBorderVisibility { get; }

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
			FullScreenSpan = IsFullScreenCatalogMode.Select(x => x ? 3 : 1).ToReactiveProperty();
			FullScreenVisibility = IsFullScreenCatalogMode.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			FullScreenCatalogVisibility = IsFullScreenCatalogMode
				.Select(x => x ? Visibility.Hidden : Visibility.Visible)
				.ToReactiveProperty();
			FullScreenThreadButtonVisibility = IsFullScreenCatalogMode
				.Select(x => x ? Visibility.Collapsed : Visibility.Visible)
				.ToReactiveProperty();
			FullScreenThreadContainerWidth = IsFullScreenThreadMode
				.Select(x => x ? 32d : 0d)
				.ToReactiveProperty();
			FullScreenThreadCatalogVisibility = IsFullScreenThreadMode
				.Select(x => x ? Visibility.Hidden : Visibility.Visible)
				.ToReactiveProperty();
			FullScreenThreadColumn = IsFullScreenThreadMode
				.Select(x => x ? 1 : 3)
				.ToReactiveProperty();
			FullScreenThreadColumnSpan = IsFullScreenThreadMode
				.Select(x => x ? 3 : 1)
				.ToReactiveProperty();
			FullScreenBorderVisibility = new[] {
				IsFullScreenCatalogMode,
				IsFullScreenThreadMode,
			}.CombineLatest(x => x.Any(y => y))
				.Select(x => x ? Visibility.Collapsed : Visibility.Visible)
				.ToReactiveProperty();
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
				IsFullScreenCatalogMode.Value = old.IsFullScreenCatalogMode.Value;
				IsFullScreenThreadMode.Value = old.IsFullScreenThreadMode.Value;
			}


			this.Raw = futaba;
			this.Name = futaba.Name;
			this.Url = futaba.Url;

			var disps = new Helpers.AutoDisposable();
			this.IsDie = new ReactiveProperty<bool>(futaba.Raw?.IsDie ?? false);
			this.IsOld = new ReactiveProperty<bool>(futaba.Raw?.IsOld ?? false || this.IsDie.Value);
			this.IsMaxRes = new ReactiveProperty<bool>(futaba.Raw?.IsMaxRes ?? false);
			if(futaba.Raw == null) {
				this.DieTextLong = new ReactiveProperty<string>("");
			} else if(futaba.Raw.IsDie) {
				this.DieTextLong = new ReactiveProperty<string>("スレッドは落ちました");
			} else {
				var t = futaba.Raw.DieDateTime;
				if(t.HasValue) {
					var ts = t.Value - (futaba.Raw.NowDateTime ?? DateTime.Now);
					var tt = DateTime.Now.Add(ts); // 消滅時間表示はPCの時計を使用
					this.DieTextLong = new ReactiveProperty<string>(ts switch {
						TimeSpan x when x.TotalSeconds < 0 => $"スレ消滅：{Math.Abs(((futaba.Raw.NowDateTime ?? DateTime.Now) - t.Value).TotalSeconds):00}秒経過(消滅時間を過ぎました)",
						TimeSpan x when 0 < x.Days => $"スレ消滅：{ tt.ToString("MM/dd") }(あと{ ts.ToString(@"dd\日hh\時\間") })",
						TimeSpan x when 0 < x.Hours => $"スレ消滅：{ tt.ToString("HH:mm") }(あと{ ts.ToString(@"hh\時\間mm\分") })",
						TimeSpan x when 0 < x.Minutes => $"スレ消滅：{ tt.ToString("HH:mm") }(あと{ ts.ToString(@"mm\分ss\秒") })",
						_ => $"スレ消滅：{ tt.ToString("HH:mm") }(あと{ ts.ToString(@"ss\秒") })",
					});
				} else {
					this.DieTextLong = new ReactiveProperty<string>("スレ消滅：不明");
				}
			}
			this.ExportCommand.Subscribe(() => OnExport());

			this.FullScreenCatalogClickCommand.Subscribe(() => OnFullScreenCatalogClick());
			this.FullScreenThreadClickCommand.Subscribe(() => OnFullScreenThreadClick());
			this.ResCount = new ReactiveProperty<int>(futaba.ResItems?.LastOrDefault()?.ResItem.Res.Rsc ?? 0);

			if((old == null) || old.Raw.Url.IsCatalogUrl) {
				var rm = old?.ResItems.ToArray() ?? Array.Empty<BindableFutabaResItem>();
				/* 一旦保留
				this.ResItems = old switch {
					var x when x != null => x.ResItems,
					_ => new ReactiveCollection<BindableFutabaResItem>(),
				};
				*/
				this.ResItems = new ReactiveCollection<BindableFutabaResItem>();

				int c = 0;
				foreach(var it in futaba.ResItems
						.Select(x => new BindableFutabaResItem(c++, x, futaba.Url.BaseUrl, this))
						.ToArray()) {

					it.IsWatch.Subscribe(x => {
						if(x) {
							this.UpdateToken.Value = DateTime.Now;
						}
					});
					this.ResItems.Add(it);
				}

				foreach(var it in rm) {
					it.Dispose();
				}
				/* 一旦保留
				if(rm.Any()) {
					// 1フレームスキップする必要ある？
					Observable.Return(rm)
						.Delay(TimeSpan.FromMilliseconds(1))
						.ObserveOn(UIDispatcherScheduler.Default)
						.Subscribe(x => {
							foreach(var it in x) {
								this.ResItems.RemoveAt(0);
								it.Dispose();
							}
							this.UpdateToken.Value = DateTime.Now;
						});
				}
				*/
			} else {
				this.ResItems = old.ResItems;
				var i = 0;
				var prevId = this.ResItems.Select(x => x.Raw.Value.ResItem.No).ToArray();
				var newId = futaba.ResItems.Select(x => x.ResItem.No).ToArray();
				var ep = prevId.Except(newId).ToArray();
				foreach(var it in ep) {
					for(var ii = 0; ii < this.ResItems.Count; ii++) {
						if(it == this.ResItems[ii].Raw.Value.ResItem.No) {
							disps.Add(this.ResItems[ii]);
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
						disps.Add(this.ResItems[i]);
						this.ResItems.RemoveAt(i);
						continue;
					}

					if(a.Raw.Value.HashText != b.HashText) {
						// 画面から参照されているので this の初期化が終わっていないこのタイミングで書き換えてはいけない
						//this.ResItems[i] = new BindableFutabaResItem(i, b, futaba.Url.BaseUrl, this);
						var bf = new BindableFutabaResItem(i, b, futaba.Url.BaseUrl, this);
						if(b.ResItem.Res.IsHavedImage) {
							BindableFutabaResItem.CopyImage(a, bf);
						}
						this.ResItems[i] = bf;
						disps.Add(a);
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

			if(futaba.Url.IsThreadUrl) {
				var d = new Dictionary<string, (int Count, List<BindableFutabaResItem> Res)>();
				foreach(var it in this.ResItems) {
					if(!d.TryGetValue(it.ThreadResNo.Value, out _)) {
						d.Add(it.ThreadResNo.Value, (0, new List<BindableFutabaResItem>()));
					}

					foreach(var q in it.Raw.Value.QuotLines
						.Where(x => x.IsHit)
						.Select(x => x.ResNo)
						.Distinct()) {

						if(this.ResItems.Where(x => x.ThreadResNo.Value == q).Any()) {
							if(d.TryGetValue(q, out var t)) {
								t.Res.Add(it);

								d[q] = (t.Count + 1, t.Res);
							} else {
								d.Add(q, (1, new List<BindableFutabaResItem>() { it }));
							}
						}
					}
				}

				foreach(var it in this.ResItems) {
					var t = default((int Count, List<BindableFutabaResItem> Res));
					if(!d.TryGetValue(it.ThreadResNo.Value, out t)) {
						t = (0, null);
					}
					it.SetResCount(t.Count, t.Res?.Distinct().ToArray() ?? Array.Empty<BindableFutabaResItem>());
				}
			}

			// 初期化がすべて終わったタイミングで書き換える
			foreach(var it in this.ResItems) {
				it.Parent.Value = this;
			}

			// 古いインスタンスを削除
			disps.Dispose();
		}

		private void OnFullScreenCatalogClick() {
			this.IsFullScreenCatalogMode.Value = !this.IsFullScreenCatalogMode.Value;
		}

		private void OnFullScreenThreadClick() {
			this.IsFullScreenThreadMode.Value = !this.IsFullScreenThreadMode.Value;
		}

		private void OnExport() {
			if(this.Raw == null) {
				return;
			}

			var sfd= new Microsoft.Win32.SaveFileDialog() {
				AddExtension = true,
				Filter = "HTML5ファイル|*.html;*.htm|HTML5ファイル(フルセット-試験中)|*.html;*.htm", 
			};
			if(sfd.ShowDialog() ?? false) {
				string getImageBase64(
					BindableFutabaResItem bfi,
					Func<Data.UrlContext, Data.ResItem, IObservable<(bool Successed, string LocalPath, byte[] FileBytes)>> getImageFunc = null
				) {
					getImageFunc ??= ((url, item) => Util.Futaba.GetThumbImage(url, item));

					var url = bfi.Raw.Value.Url;
					var item = bfi.Raw.Value.ResItem.Res;
					if(string.IsNullOrEmpty(item.Ext)) {
						return "";
					}

					var ngImage = bfi.ThumbDisplay.Value ?? false;
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
					return getImageFunc(url, item)
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
				string getImageBase64TestImpl(
					FutabaContext.Item it,
					Func<Data.UrlContext, Data.ResItem, IObservable<(bool Successed, string LocalPath, byte[] FileBytes)>> getImageFunc = null
				) {
					var url = it.Url;
					var item = it.ResItem.Res;
					if(string.IsNullOrEmpty(item.Ext)) {
						return "";
					}
					/*
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
					*/
					// TODO: 基本的にキャッシュされているはずだけど良くないのでメソッドの変更を考える
					return getImageFunc(url, item)
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


				if(sfd.FilterIndex == 2) {
					Task.Run(async () => {
						var futaba = this.Raw;
						var resitems = this.Raw.ResItems;
						var img = resitems
							.Where(x => (x.ResItem.Res.Fsize != 0) && !string.IsNullOrEmpty(x.ResItem.Res.Ext))
							.ToArray();
						foreach(var it in img) {
							if(Util.Futaba.GetThumbImage(it.Url, it.ResItem.Res)
								.Wait()
								.FileBytes != null) {

								// ダウンロードした場合は待つ
								await Task.Delay(100);
							}
						}
						foreach(var it in img) {
							if(Util.Futaba.GetThreadResImage(it.Url, it.ResItem.Res)
								.Wait()
								.FileBytes != null) {

								// ダウンロードした場合は待つ
								await Task.Delay(100);
							}
						}

						var fm = File.Exists(sfd.FileName) ? FileMode.Truncate : FileMode.OpenOrCreate;
						using(var sf = new FileStream(sfd.FileName, fm)) {
							Util.ExportUtil.ExportHtmlTestImpl(sf,
								new Data.ExportHolder(
									futaba.Board.Name, futaba.Board.Extra.Name,
									resitems.Select(x => {
										var ngRes = false; // x.IsHidden.Value || x.IsNg.Value;
										var ngImage = false;
										/*
										if((WpfConfig.WpfConfigLoader.SystemConfig.ExportNgImage == PlatformData.ExportNgImage.Hidden)
											&& Ng.NgUtil.NgHelper.IsEnabledNgImage()) {

											var h = WpfUtil.ImageUtil.CalculatePerceptualHash(WpfUtil.ImageUtil.LoadImage(x.LocalPath, x.FileBytes));
											if(Ng.NgUtil.NgHelper.CheckImageNg(h)) {
												ngImage = true;
											}
										}
										*/
										if(ngRes && (WpfConfig.WpfConfigLoader.SystemConfig.ExportNgRes == PlatformData.ExportNgRes.Hidden)) {
											return null;
										}
										var imageName = "";
										var m = Regex.Match(x.ResItem.Res.Src, @"^.+/([^\.]+\..+)$");
										if(m.Success) {
											imageName = m.Groups[1].Value;
										}

										return new Data.ExportData() {
											Subject = x.ResItem.Res.Sub,
											Name = x.ResItem.Res.Name,
											Email = x.ResItem.Res.Email,
											//Comment = (ngRes && (WpfConfig.WpfConfigLoader.SystemConfig.ExportNgRes == PlatformData.ExportNgRes.Mask))
											//	? x.CommentHtml.Value : x.OriginHtml.Value,
											Comment = x.ResItem.Res.Com,
											No = x.ResItem.No,
											Date = string.Format("{0}{1}", x.ResItem.Res.Now, string.IsNullOrEmpty(x.ResItem.Res.Id) ? "" : (" " + x.ResItem.Res.Id)),
											Soudane = x.Soudane,
											Host = x.ResItem.Res.Host,
											OriginalImageName = (ngImage || string.IsNullOrEmpty(x.ResItem.Res.Ext)) ? "" : imageName,
											OriginalImageData = getImageBase64TestImpl(x, (url, item) => Util.Futaba.GetThreadResImage(url, item)),
											ThumbnailImageData = getImageBase64TestImpl(x, (url, item) => Util.Futaba.GetThumbImage(url, item)),
										};
									}).Where(x => x != null).ToArray()
								));
							sf.Flush();
							Futaba.PutInformation(new Information("HTML保存が完了しました", futaba));
						}
					});
				} else {
					try {
						var fm = File.Exists(sfd.FileName) ? FileMode.Truncate : FileMode.OpenOrCreate;
						using(var sf = new FileStream(sfd.FileName, fm)) {
							Util.ExportUtil.ExportHtml(sf,
								new Data.ExportHolder(
									this.Raw.Board.Name, this.Raw.Board.Extra.Name,
									this.ResItems.Select(x => {
										var ngRes = x.IsHidden.Value || x.IsNg.Value;
										var ngImage = !(x.ThumbDisplay.Value ?? true)
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
	}

	public class BindableFutabaResItem : Bindable.CommonBindableFutabaResItem {
		private class RefValue<T> where T:struct {
			public T Value { get; }

			public RefValue(T val) {
				this.Value = val;
			}
		}
		private static Helpers.WeakCache<string, RefValue<ulong>> HashCache { get; }
			= new Helpers.WeakCache<string, RefValue<ulong>>();
		private static Helpers.ConnectionQueue<ulong?> HashQueue = new Helpers.ConnectionQueue<ulong?>(
			name: "NG/Watchハッシュ計算キュー",
			maxConcurrency: 4,
			waitTime: 100);

		public static void CopyImage(BindableFutabaResItem src, BindableFutabaResItem dst) {
			dst.ThumbHash.Value = src.ThumbHash.Value;
			dst.hashValue = src.hashValue;

			src.ThumbHash.Value = null;
		}

		public string Id { get; }
		public ReactiveProperty<int> Index { get; }
		public ReactiveProperty<string> ImageName { get; }


		private ReactiveProperty<object> ThumbToken { get; } = new ReactiveProperty<object>(initialValue: null);
		private WeakReference<BitmapSource> thumbSource = new WeakReference<BitmapSource>(default);
		public ReactiveProperty<bool?> ThumbDisplay { get; } = new ReactiveProperty<bool?>(); // NGではない場合true
		public BitmapSource ThumbSource {
			set {
				if((value != null) && Ng.NgUtil.NgHelper.IsEnabledNgImage()) {
					this.StoreHash(value)
						.Subscribe(y => {
							this.thumbSource.SetTarget(this.ThumbDisplay.Value.Value switch {
								true => value,
								false => (Ng.NgConfig.NgConfigLoader.NgImageConfig.NgMethod == ImageNgMethod.Hidden) switch {
									true => null,
									false => WpfUtil.ImageUtil.GetNgImage()
								}
							});
							this._propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThumbSource)));
						});
				} else {
					this.ThumbDisplay.Value = true;
					this.thumbSource.SetTarget(value);
					this._propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ThumbSource)));
				}
			}
			get {
				static BitmapSource apply(BindableFutabaResItem item, BitmapSource bitmap, bool? display) {
					return (display ?? true) switch {
						true => bitmap,
						false => (Ng.NgConfig.NgConfigLoader.NgImageConfig.NgMethod == ImageNgMethod.Hidden) switch {
							true => null,
							false => WpfUtil.ImageUtil.GetNgImage()
						}
					};
				}

				if(this.Raw.Value.ResItem.Res.Fsize == 0) {
					return null;
				}

				if(this.thumbSource.TryGetTarget(out var bitmapSource)) {
					return bitmapSource;
				}
				if(this.ThumbDisplay.Value.HasValue && !this.ThumbDisplay.Value.Value) {
					var bmp = WpfUtil.ImageUtil.GetNgImage();
					this.ThumbToken.Value ??= new object();
					this.thumbSource.SetTarget(bmp);
					return bmp;
				}
				{
					if(WpfUtil.ImageUtil.GetImageCache2(
						Util.Futaba.GetThumbImageLocalFilePath(
							this.Parent.Value.Url, this.Raw.Value.ResItem.Res)) is BitmapSource bmp) {
						this.ThumbToken.Value ??= new object();
						this.thumbSource.SetTarget(bmp);
						return bmp;
					}
				}

				this.LoadBitmapSource(true)
					.Subscribe(b => {
						if(b != null) {
							this.ThumbToken.Value ??= new object();
							this.ThumbSource = apply(this, b, this.ThumbDisplay.Value);
						} else {
							// エラー
						}
					});
				return null;
			}
		}
		public ReactiveProperty<ulong?> ThumbHash { get; }

		public ReactiveProperty<Visibility> NameVisibility { get; }
		public ReactiveProperty<Visibility> ResImageVisibility { get; }

		public ReactiveProperty<Data.BoardData> Bord { get; }
		public ReactiveProperty<string> ThreadResNo { get; }
		public ReactiveProperty<Data.FutabaContext.Item> Raw { get; }

		public ReactiveProperty<int> ResCount { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<BindableFutabaResItem[]> ResCitedSource { get; } = new ReactiveProperty<BindableFutabaResItem[]>(Array.Empty<BindableFutabaResItem>());
		public ReactiveProperty<string> ResCountText { get; }


		public ReactiveProperty<string> HeadLineHtml { get; }
		public ReactiveProperty<string> DisplayHtml { get; }
		public ReactiveProperty<string> CommentHtml { get; }
		public ReactiveProperty<string> OriginHtml { get; }
		public ReactiveProperty<bool> IsNg { get; }
		public ReactiveProperty<bool> IsWatch { get; }
		public ReactiveProperty<bool> IsWatchWord { get; }
		public ReactiveProperty<bool> IsWatchImage { get; }
		public ReactiveProperty<bool> IsHidden { get; }
		public ReactiveProperty<bool> IsDel { get; }
		public ReactiveProperty<bool> IsVisibleOriginComment { get; }
		public ReactiveProperty<bool> IsNgImageHidden { get; }

		public ReactiveProperty<string> CommentCopy { get; }
		public ReactiveProperty<Visibility> CommandPaletteVisibility { get; }
		public ReactiveProperty<HorizontalAlignment> CommandPaletteAlignment { get; }


		public ReactiveProperty<Visibility> ReleaseHiddenResBuutonVisibility { get; }
		public ReactiveProperty<Visibility> ShowNgResButtonVisibility { get; }
		public ReactiveProperty<Visibility> CopyModeButtonVisibility { get; }
		public ReactiveProperty<string> ShowNgResButtonText { get; }

		public ReactiveProperty<bool> IsCopyMode { get; } = new ReactiveProperty<bool>(false);

		public ReactiveProperty<Visibility> FutabaTextBlockVisibility { get; }
		public ReactiveProperty<Visibility> CopyBlockVisibility { get; }

		public ReactiveProperty<Visibility> IsVisibleMenuItemWatchImage { get; }
		public ReactiveProperty<Visibility> IsVisibleMenuItemNgImage { get; }
		
		public ReactiveProperty<bool> IsVisibleCatalogIdMarker{ get; }

		public ReactiveProperty<Visibility> MenuItemRegisterHiddenVisibility { get; }
		public ReactiveProperty<Visibility> MenuItemUnregisterHiddenVisibility { get; }

		[Helpers.AutoDisposable.IgonoreDisposeBindingsValue]
		public ReactiveProperty<BindableFutaba> Parent { get; }

		public MakiMokiCommand<MouseButtonEventArgs> FutabaTextBlockMouseDownCommand { get; }
			= new MakiMokiCommand<MouseButtonEventArgs>();

		private RefValue<ulong> hashValue;
		private Action<Ng.NgData.NgConfig> ngUpdateAction;
		private Action<Ng.NgData.HiddenConfig> hiddenUpdateAction;
		private Action<Ng.NgData.NgImageConfig> imageUpdateAction;
		private Action<Ng.NgData.WatchConfig> watchUpdateAction;
		private Action<Ng.NgData.WatchImageConfig> watchImageUpdateAction;
		private Action<PlatformData.WpfConfig> systemUpdateAction;

		public BindableFutabaResItem(int index, Data.FutabaContext.Item item, string baseUrl, BindableFutaba parent) {
			System.Diagnostics.Debug.Assert(item != null);
			System.Diagnostics.Debug.Assert(baseUrl != null);
			System.Diagnostics.Debug.Assert(parent != null);
			var bord = Config.ConfigLoader.Board.Boards.Where(x => x.Url == baseUrl).FirstOrDefault();
			System.Diagnostics.Debug.Assert(bord != null);
			this.Index = new ReactiveProperty<int>(index);
			this.Bord = new ReactiveProperty<Data.BoardData>(bord);
			this.Parent = new ReactiveProperty<BindableFutaba>(parent);
			this.ThreadResNo = new ReactiveProperty<string>(item.ResItem.No);
			this.Raw = new ReactiveProperty<Data.FutabaContext.Item>(item);
			this.NameVisibility = new ReactiveProperty<Visibility>(
				(bord.Extra ?? new Data.BoardDataExtra()).Name ? Visibility.Visible : Visibility.Collapsed);
			this.ThumbHash = new ReactiveProperty<ulong?>();
			this.CommandPaletteVisibility = new ReactiveProperty<Visibility>(
				WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledThreadCommandPalette ? Visibility.Visible : Visibility.Collapsed);
			this.CommandPaletteAlignment = new ReactiveProperty<HorizontalAlignment>(UiPotionToHorizontalAlignment(
				WpfConfig.WpfConfigLoader.SystemConfig.CommandPalettePosition));
			this.IsVisibleCatalogIdMarker = new ReactiveProperty<bool>(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledIdMarker);

			this.ResCountText = this.ResCount.Select(x => (0 < x) ? $"{ x }レス" : "").ToReactiveProperty();

			// delとhostの処理
			{
				var headLine = new StringBuilder();
				if(Raw.Value.ResItem.Res.IsDel) {
					headLine.Append("<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font><br>");
				} else if(Raw.Value.ResItem.Res.IsDel2) {
					headLine.Append("<font color=\"#ff0000\">削除依頼によって隔離されました</font><br>");
				}
				if(!string.IsNullOrEmpty(Raw.Value.ResItem.Res.Host)) {
					headLine.Append($"[<font color=\"#ff0000\">{ Raw.Value.ResItem.Res.Host }</font>]<br>");
				}
				this.IsNg = new ReactiveProperty<bool>(
					parent.Url.IsCatalogUrl ? Ng.NgUtil.NgHelper.CheckCatalogNg(parent.Raw, item)
						: Ng.NgUtil.NgHelper.CheckThreadNg(parent.Raw, item));
				this.IsWatchWord = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckCatalogWatch(parent.Raw, item));
				this.IsWatchImage = this.ThumbHash
					.Select(x => x.HasValue ? Ng.NgUtil.NgHelper.CheckImageWatch(x.Value) : false)
					.ToReactiveProperty();
				this.IsWatch = new[] {
					this.IsWatchWord,
					this.IsWatchImage,
				}.CombineLatest(x => x.Any(y => y))
					.ToReactiveProperty();
				this.IsHidden = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckHidden(parent.Raw, item));
				this.IsDel = new ReactiveProperty<bool>(
					(item.ResItem.Res.IsDel || item.ResItem.Res.IsDel2)
						&& (WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden));
				this.IsNgImageHidden = new ReactiveProperty<bool>(false);
				this.HeadLineHtml = new ReactiveProperty<string>(headLine.ToString());
				this.CommentHtml = new ReactiveProperty<string>("");
				this.OriginHtml = new ReactiveProperty<string>(Raw.Value.ResItem.Res.Com);
				this.SetCommentHtml();
				this.IsVisibleOriginComment = this.IsHidden
					.CombineLatest(
						this.IsNg, this.IsDel,
						(x, y, z) => !(x || y || z))
					.ToReactiveProperty();
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
				watchUpdateAction = (x) => a();
				imageUpdateAction = (x) => b();
				watchImageUpdateAction = (x) => this.ThumbHash.ForceNotify();
				systemUpdateAction = (x) => c();
				Ng.NgConfig.NgConfigLoader.AddNgUpdateNotifyer(ngUpdateAction);
				Ng.NgConfig.NgConfigLoader.AddHiddenUpdateNotifyer(hiddenUpdateAction);
				Ng.NgConfig.NgConfigLoader.AddImageUpdateNotifyer(imageUpdateAction);
				Ng.NgConfig.NgConfigLoader.WatchUpdateNotifyer.AddHandler(watchUpdateAction);
				Ng.NgConfig.NgConfigLoader.WatchImageUpdateNotifyer.AddHandler(watchImageUpdateAction);
				WpfConfig.WpfConfigLoader.AddSystemConfigUpdateNotifyer(systemUpdateAction);
			}

			// コピー用コメント生成
			{
				var sb = new StringBuilder()
					.Append(Index.Value);
				if(Bord.Value.Extra?.Name ?? true) {
					sb.Append($" {Raw.Value.ResItem.Res.Sub} {Raw.Value.ResItem.Res.Name}");
				}
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Email)) {
					sb.Append($" [{Raw.Value.ResItem.Res.Email}]");
				}
				sb.Append($" {Raw.Value.ResItem.Res.Now}");
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Host)) {
					sb.Append($" {Raw.Value.ResItem.Res.Host}");
				}
				if(!string.IsNullOrWhiteSpace(Raw.Value.ResItem.Res.Id)) {
					sb.Append($" {Raw.Value.ResItem.Res.Id}");
				}
				if(0 < Raw.Value.Soudane) {
					sb.Append($" そうだね×{Raw.Value.Soudane}");
				}
				sb.Append($" No.{Raw.Value.ResItem.No}")
					.AppendLine()
					.Append(WpfUtil.TextUtil.RawComment2Text(Raw.Value.ResItem.Res.Com));
				this.CommentCopy = new ReactiveProperty<string>(sb.ToString());
			}

			if(item.ResItem.Res.Fsize != 0) {
				// 削除された場合srcの拡張子が消える
				var m = Regex.Match(item.ResItem.Res.Src, @"^.+/([^\.]+\..+)$");
				if(m.Success) {
					this.ImageName = new ReactiveProperty<string>(m.Groups[1].Value);
					if(Ng.NgUtil.NgHelper.IsEnabledNgImage() && HashCache.TryGetTarget(this.GetCacheKey(), out var hash)) {
						if(this.Raw.Value.Url.IsCatalogUrl
							&& WpfConfig.WpfConfigLoader.SystemConfig.CatalogNgImage == PlatformData.CatalogNgImage.Hidden
							&& Ng.NgUtil.NgHelper.CheckImageNg(hash.Value)) {

							this.IsNgImageHidden.Value = true;
						}
					}
				} else {
					this.ImageName = new ReactiveProperty<string>("");
				}
			} else {
				this.ImageName = new ReactiveProperty<string>("");
			}
			this.FutabaTextBlockVisibility = this.IsCopyMode.Select(x => x ? Visibility.Hidden : Visibility.Visible).ToReactiveProperty();
			this.CopyBlockVisibility = this.IsCopyMode
				.CombineLatest(IsVisibleOriginComment, (x, y) => x ? Visibility.Visible : (y ? Visibility.Hidden : Visibility.Collapsed))
				.ToReactiveProperty();
			this.ResImageVisibility = this.ThumbToken
				.CombineLatest(IsVisibleOriginComment, (x, y) => ((x != null) && y) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
			this.IsVisibleMenuItemNgImage = this.ImageName
				.Select(x => !string.IsNullOrEmpty(x))
				.CombineLatest(this.ThumbDisplay, (x, y) => x && (y ?? true))
				.Select(x => x ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
			this.IsVisibleMenuItemWatchImage = this.ImageName
				.Select(x => !string.IsNullOrEmpty(x))
				.CombineLatest(this.ThumbHash, (x, y) => {
					if(x) {
						if(y.HasValue) {
							return !Ng.NgUtil.NgHelper.CheckImageWatch(y.Value, 0);
						} else {
							return true;
						}
					} else {
						return false;
					}
				}).Select(x => x ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
			this.MenuItemRegisterHiddenVisibility = this.IsHidden.Select(x => x ? Visibility.Collapsed : Visibility.Visible).ToReactiveProperty();
			this.MenuItemUnregisterHiddenVisibility = this.IsHidden.Select(x => x ? Visibility.Visible : Visibility.Collapsed).ToReactiveProperty();

			this.FutabaTextBlockMouseDownCommand.Subscribe(x => OnFutabaTextBlockMouseDown(x));
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
					e.Handled = true;
				}
			}
		}


		public void SetThumbSource(BitmapSource bmp) {
			// Watch画像から送られてくる
			this.ThumbSource = bmp;
		}

		public void SetResCount(int count, BindableFutabaResItem[] res) {
			this.ResCount.Value = count;
			this.ResCitedSource.Value = res;
		}

		private void SetCommentHtml(bool? ng = null, bool? hidden = null) {
			var del = WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden;
			if(ng ?? this.IsNg.Value) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">NG設定に抵触しています</font>";
			} else if(hidden ?? this.IsHidden.Value) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">非表示に設定されています</font>";
			} else if(this.Raw.Value.ResItem.Res.IsDel && del) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font>";
			} else if(this.Raw.Value.ResItem.Res.IsDel2 && del) {
				this.CommentHtml.Value = "<font color=\"#ff0000\">削除依頼によって隔離されました</font>";
			} else {
				this.CommentHtml.Value = this.OriginHtml.Value;
			}

		}

		private string GetCacheKey() {
			return Util.Futaba.GetThumbImageLocalFilePath(
				this.Parent.Value.Url,
				this.Raw.Value.ResItem.Res);
		}

		// TODO: 名前変える
		private void a() {
			// NGとHiddenの設定前にコメントを更新する
			var ng = this.Raw.Value.Url.IsCatalogUrl
				? Ng.NgUtil.NgHelper.CheckCatalogNg(this.Parent.Value.Raw, this.Raw.Value)
					: Ng.NgUtil.NgHelper.CheckThreadNg(this.Parent.Value.Raw, this.Raw.Value);
			var hidden = Ng.NgUtil.NgHelper.CheckHidden(this.Parent.Value.Raw, this.Raw.Value);
			this.IsWatchWord.Value = Ng.NgUtil.NgHelper.CheckCatalogWatch(this.Parent.Value.Raw, this.Raw.Value);
			this.IsCopyMode.Value = false;
			this.SetCommentHtml(ng, hidden);
			this.IsNg.Value = ng;
			this.IsHidden.Value = hidden;
		}

		// TODO: 名前変える
		private void b() {
			var b = WpfUtil.ImageUtil.GetImageCache2(this.GetCacheKey());
			if(b == null) {
				// キャッシュに残っていないので再ロードさせる
				this.ThumbToken.Value = null;
				this.ThumbDisplay.Value = null;
				this.thumbSource.SetTarget(null); 
				_ = this.ThumbSource;
			} else {
				// NG設定が変わったので上書きする
				this.ThumbSource = b;
			}
		}

		// TODO:名前考える
		private void c() {
			this.IsDel.Value = (this.Raw.Value.ResItem.Res.IsDel || this.Raw.Value.ResItem.Res.IsDel2)
				&& (WpfConfig.WpfConfigLoader.SystemConfig.ThreadDelResVisibility == PlatformData.ThreadDelResVisibility.Hidden);
			this.SetCommentHtml();
			this.CommandPaletteVisibility.Value = WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledThreadCommandPalette ? Visibility.Visible : Visibility.Collapsed;
			this.CommandPaletteAlignment.Value = UiPotionToHorizontalAlignment(WpfConfig.WpfConfigLoader.SystemConfig.CommandPalettePosition);
			this.IsVisibleCatalogIdMarker.Value = WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledIdMarker;
		}

		private HorizontalAlignment UiPotionToHorizontalAlignment(PlatformData.UiPosition position) {
			switch(position) {
			case PlatformData.UiPosition.Left: return HorizontalAlignment.Left;
			case PlatformData.UiPosition.Right: return HorizontalAlignment.Right;
			default: return HorizontalAlignment.Right;
			}
		}

		private IObservable<ulong> StoreHash(BitmapSource bmp) {
			IObservable<(bool IsNew, ulong Value)> f() {
				if(HashCache.TryGetTarget(this.GetCacheKey(), out var hash)) {
					return Observable.Return((false, hash.Value));
				} else {
					var w = bmp.PixelWidth;
					var h = bmp.PixelHeight;
					var bytes = WpfUtil.ImageUtil.CreatePixelsBytes(bmp);
					return Observable.Create<(bool, ulong)>(o => {
						o.OnNext((true, Ng.NgUtil.PerceptualHash.CalculateHash(bytes, w, h, 32)));
						o.OnCompleted();
						return System.Reactive.Disposables.Disposable.Empty;
					}).SubscribeOn(System.Reactive.Concurrency.ThreadPoolScheduler.Instance);
				}
			}
			return f()
				.ObserveOn(UIDispatcherScheduler.Default)
				.Select(x => {
					if(x.IsNew) {
						this.hashValue = new RefValue<ulong>(x.Value);
						HashCache.Add(this.GetCacheKey(), this.hashValue);
					}
					this.ThumbHash.Value = x.Value;
					this.ThumbDisplay.Value = !Ng.NgUtil.NgHelper.CheckImageNg(x.Value);
					return x.Value;
				});

		}

		public IObservable<BitmapSource> LoadBitmapSource(bool forceLoad = false) {
			if(!forceLoad) {
				if(this.Raw.Value.ResItem.Res.Fsize == 0) {
					return Observable.Return<BitmapSource>(null);
				}

				if(this.thumbSource.TryGetTarget(out var bitmapSource)) {
					this.ThumbToken.Value ??= new object();
					return Observable.Return<BitmapSource>(bitmapSource);
				}
				if(this.ThumbDisplay.Value.HasValue && !this.ThumbDisplay.Value.Value) {
					var bmp = WpfUtil.ImageUtil.GetNgImage();
					this.ThumbToken.Value ??= new object();
					this.thumbSource.SetTarget(bmp);
					return Observable.Return<BitmapSource>(bmp);
				} else {
					if(WpfUtil.ImageUtil.GetImageCache2(
						Util.Futaba.GetThumbImageLocalFilePath(
							this.Parent.Value.Url, this.Raw.Value.ResItem.Res)) is BitmapSource bmp) {
						this.ThumbToken.Value ??= new object();
						this.thumbSource.SetTarget(bmp);
						return Observable.Return<BitmapSource>(bmp);
					}
				}
			}

			return Util.Futaba.GetThumbImage(Raw.Value.Url, Raw.Value.ResItem.Res)
				.Select(x => {
					if(x.Successed) {
						return (Path: x.LocalPath, Stream: WpfUtil.ImageUtil.LoadStream(x.LocalPath, x.FileBytes));
					} else {
						return (null, null);
					}
				}).ObserveOn(UIDispatcherScheduler.Default)
				.Select(x => WpfUtil.ImageUtil.CreateImage(x.Path, x.Stream));
		}
	}
}
