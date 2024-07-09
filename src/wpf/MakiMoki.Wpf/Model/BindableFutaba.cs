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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Collections.ObjectModel;

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

		public ReactiveProperty<string> Name { get; }

		public Data.UrlContext Url { get; }

		public ReactiveProperty<Data.FutabaContext> Raw { get;}

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

		public ReactiveProperty<bool> IsDisableNg { get; } = new ReactiveProperty<bool>(false);
		public ReactiveProperty<bool> EnableSpeach { get; } = new ReactiveProperty<bool>(false);


		public ReactiveProperty<int> CatalogResCount { get; } = new ReactiveProperty<int>(0);
		public ReactiveProperty<object> UpdateToken { get; }
		public ReactiveProperty<object> NgUpdateToken { get; }


		private Action<Ng.NgData.NgConfig> ngUpdateAction;
		private Action<Ng.NgData.HiddenConfig> hiddenUpdateAction;
		private Action<Ng.NgData.NgImageConfig> imageUpdateAction;
		private Action<PlatformData.WpfConfig> systemUpdateAction;

		public BindableFutaba(Data.FutabaContext futaba) {
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

			this.UpdateToken = new ReactiveProperty<object>(DateTime.Now);
			this.NgUpdateToken = new ReactiveProperty<object>(DateTime.Now);

			ngUpdateAction = (_) => this.UpdateToken.Value = this.NgUpdateToken.Value = DateTime.Now;
			imageUpdateAction = (_) => this.UpdateToken.Value = this.NgUpdateToken.Value = DateTime.Now;
			hiddenUpdateAction = (_) => this.UpdateToken.Value = this.NgUpdateToken.Value = DateTime.Now;
			systemUpdateAction = (_) => this.UpdateToken.Value = DateTime.Now;
			Ng.NgConfig.NgConfigLoader.AddNgUpdateNotifyer(ngUpdateAction);
			Ng.NgConfig.NgConfigLoader.AddHiddenUpdateNotifyer(hiddenUpdateAction);
			Ng.NgConfig.NgConfigLoader.AddImageUpdateNotifyer(imageUpdateAction);
			WpfConfig.WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler(systemUpdateAction);


			this.Raw = new(initialValue: futaba);
			this.Name = this.Raw.Select(x => x.Name).ToReactiveProperty();
			this.ResCount = this.Raw.Select(x => x.ResItems?.LastOrDefault()?.ResItem.Res.Rsc ?? 0).ToReactiveProperty();
			this.Url = futaba.Url;

			var disps = new Helpers.AutoDisposable();
			this.IsDie = new ReactiveProperty<bool>(futaba.Raw?.IsDie ?? false);
			this.IsOld = new ReactiveProperty<bool>(futaba.Raw?.IsOld ?? false || this.IsDie.Value);
			this.IsMaxRes = new ReactiveProperty<bool>(futaba.Raw?.IsMaxRes ?? false);
			this.DieTextLong = this.Raw.Select(x => {
				if(x.Raw == null) {
					return "";
				} else if(x.Raw.IsDie) {
					return "スレッドは落ちました";
				} else {
					var t = x.Raw.DieDateTime;
					if(t.HasValue) {
						var ts = t.Value - (x.Raw.NowDateTime ?? DateTime.Now);
						var tt = DateTime.Now.Add(ts); // 消滅時間表示はPCの時計を使用
						return ts switch {
							TimeSpan y when y.TotalSeconds < 0 => $"スレ消滅：{Math.Abs(((futaba.Raw.NowDateTime ?? DateTime.Now) - t.Value).TotalSeconds):00}秒経過(消滅時間を過ぎました)",
							TimeSpan y when 0 < y.Days => $"スレ消滅：{tt.ToString("MM/dd")}(あと{ts.ToString(@"dd\日hh\時\間")})",
							TimeSpan y when 0 < y.Hours => $"スレ消滅：{tt.ToString("HH:mm")}(あと{ts.ToString(@"hh\時\間mm\分")})",
							TimeSpan y when 0 < y.Minutes => $"スレ消滅：{tt.ToString("HH:mm")}(あと{ts.ToString(@"mm\分ss\秒")})",
							_ => $"スレ消滅：{tt.ToString("HH:mm")}(あと{ts.ToString(@"ss\秒")})",
						};
					} else {
						return "スレ消滅：不明";
					}
				}
			}).ToReactiveProperty();

			this.ExportCommand.Subscribe(() => OnExport());

			this.FullScreenCatalogClickCommand.Subscribe(() => OnFullScreenCatalogClick());
			this.FullScreenThreadClickCommand.Subscribe(() => OnFullScreenThreadClick());

			{
				this.ResItems = new ReactiveCollection<BindableFutabaResItem>();
				foreach(var it in futaba.ResItems
						.Select((x, i) => new BindableFutabaResItem(i, x, futaba.Url.BaseUrl, this))
						.ToArray()) {

					var one = false;
					it.IsWatch.Subscribe(x => {
						if(one) {
							if(x) {
								this.UpdateToken.Value = DateTime.Now;
							}
						}
						one = true;
					});
					this.ResItems.Add(it);
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


		public void Update(FutabaContext futaba) {
			this.Raw.Value = futaba;
			if(futaba.Url.IsCatalogUrl) {
				using var d = new Helpers.AutoDisposable();
				foreach(var it in this.ResItems) {
					d.Add(it);
				}

				this.ResItems.Clear();
				this.ResItems.AddRange(
					futaba.ResItems.Select((x, i) => {
							var r = new BindableFutabaResItem(i, x, futaba.Url.BaseUrl, this);
							var one = false;
							r.IsWatch.Subscribe(y => {
								if(one) {
									if(y) {
										this.UpdateToken.Value = DateTime.Now;
									}
								}
								one = true;
							});
							return r;
						}));
			} else {
				var len = this.ResItems.Count;
				var list = futaba.ResItems.ToList();
				if(this.ResItems.Any()) {
					foreach(var it in this.ResItems) {
						FutabaContext.Item res;
						while(true) {
							res = list.First();
							list.RemoveAt(0);
							if(res.ResItem.No == it.Raw.Value.ResItem.No) {
								break;
							}
							if(!list.Any()) {
								goto end;
							}
						}
						it.Update(res);
					}
				end:;
				}
				foreach(var it in list.Select((x, i) => (Value: x, Index: i))) {
					this.ResItems.Add(
						new BindableFutabaResItem(
							len + it.Index,
							it.Value,
							futaba.Url.BaseUrl,
							this));
				}
			}
			this.UpdateToken.Value = DateTime.Now;
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
			if(this.Url.IsCatalogUrl) {
				return;
			}

			var sfd= new Microsoft.Win32.SaveFileDialog() {
				AddExtension = true,
				Filter = "HTML5ファイル|*.html;*.htm|HTML5ファイル(フルセット-試験中)|*.html;*.htm", 
				FileName = $"{this.Url.ThreadNo}.html",
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
						var futaba = this.Raw.Value;
						var resitems = this.Raw.Value.ResItems;
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
									this.Raw.Value.Board.Name, this.Raw.Value.Board.Extra.Name,
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

		public ReactiveProperty<int> Index { get; }
		public ReactiveProperty<string> ImageName { get; }


		private ReactiveProperty<object> ThumbToken { get; } = new ReactiveProperty<object>(initialValue: null);
		private WeakReference<Model.ImageObject> thumbSource = new WeakReference<Model.ImageObject>(default);
		public ReactiveProperty<bool?> ThumbDisplay { get; } = new ReactiveProperty<bool?>(); // NGではない場合true
		public Model.ImageObject ThumbSource {
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
				static Model.ImageObject apply(BindableFutabaResItem item, Model.ImageObject bitmap, bool? display) {
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
							this.Parent.Value.Url, this.Raw.Value.ResItem.Res)) is Model.ImageObject bmp) {
						this.ThumbToken.Value ??= new object();
						// NG判定を行う
						if(this.ThumbDisplay.Value.HasValue) {
							this.thumbSource.SetTarget(bmp);
							return bmp;
						} else {
							// 1フレーム進める
							Observable.Return(bmp)
								.Delay(TimeSpan.FromMilliseconds(1))
								.ObserveOn(UIDispatcherScheduler.Default)
								.Subscribe(x => {
									this.ThumbSource = x;
								});
							return null;
						}
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

		public ReactiveProperty<string> Sub { get; }
		public ReactiveProperty<string> Name { get; }
		public ReactiveProperty<string> Email { get; }
		public ReactiveProperty<string> Now { get; }
		public ReactiveProperty<int> Soudane { get; }
		public ReactiveProperty<string> No { get; }
		public ReactiveProperty<string> Id { get; }

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
			this.Sub = this.Raw.Select(x => x.ResItem.Res.Sub).ToReactiveProperty();
			this.Name = this.Raw.Select(x => x.ResItem.Res.Name).ToReactiveProperty();
			this.Email = this.Raw.Select(x => x.ResItem.Res.Email).ToReactiveProperty();
			this.Now = this.Raw.Select(x => x.ResItem.Res.Now).ToReactiveProperty();
			this.Soudane = this.Raw.Select(x => x.Soudane).ToReactiveProperty();
			this.No = this.Raw.Select(x => x.ResItem.No).ToReactiveProperty();
			this.Id = this.Raw.Select(x => x.ResItem.Res.Id).ToReactiveProperty();

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
					parent.Url.IsCatalogUrl ? Ng.NgUtil.NgHelper.CheckCatalogNg(parent.Raw.Value, item)
						: Ng.NgUtil.NgHelper.CheckThreadNg(parent.Raw.Value, item));
				this.IsWatchWord = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckCatalogWatch(parent.Raw.Value, item));
				this.IsWatchImage = this.ThumbHash
					.Select(x => x.HasValue ? Ng.NgUtil.NgHelper.CheckImageWatch(x.Value) : false)
					.ToReactiveProperty();
				this.IsWatch = new[] {
					this.IsWatchWord,
					this.IsWatchImage,
				}.CombineLatest(x => x.Any(y => y))
					.ToReactiveProperty();
				this.IsHidden = new ReactiveProperty<bool>(Ng.NgUtil.NgHelper.CheckHidden(parent.Raw.Value, item));
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
						this.IsNg, this.IsDel, parent.IsDisableNg,
						(x, y, z, a) => !(x || y || z) || a)
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
				watchImageUpdateAction = (x) => b();
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

		public void Update(FutabaContext.Item item) {
			if(item.HashText != this.Raw.Value.HashText) {
				this.Raw.Value = item;
				{
					var headLine = new StringBuilder();
					if(Raw.Value.ResItem.Res.IsDel) {
						headLine.Append("<font color=\"#ff0000\">スレッドを立てた人によって削除されました</font><br>");
					} else if(Raw.Value.ResItem.Res.IsDel2) {
						headLine.Append("<font color=\"#ff0000\">削除依頼によって隔離されました</font><br>");
					}
					if(!string.IsNullOrEmpty(Raw.Value.ResItem.Res.Host)) {
						headLine.Append($"[<font color=\"#ff0000\">{Raw.Value.ResItem.Res.Host}</font>]<br>");
					}
					this.IsNg.Value = this.Parent.Value.Url.IsCatalogUrl switch {
						true => Ng.NgUtil.NgHelper.CheckCatalogNg(this.Parent.Value.Raw.Value, item),
						false => Ng.NgUtil.NgHelper.CheckThreadNg(this.Parent.Value.Raw.Value, item),
					};
					this.IsWatchWord.Value = Ng.NgUtil.NgHelper.CheckCatalogWatch(this.Parent.Value.Raw.Value, item);
					this.IsHidden.Value = Ng.NgUtil.NgHelper.CheckHidden(this.Parent.Value.Raw.Value, item);
					this.HeadLineHtml.Value = headLine.ToString();
					this.OriginHtml.Value = Raw.Value.ResItem.Res.Com;
					this.SetCommentHtml();
				}

				if(item.ResItem.Res.Fsize != 0) {
					// 削除された場合srcの拡張子が消える
					var m = Regex.Match(item.ResItem.Res.Src, @"^.+/([^\.]+\..+)$");
					if(m.Success) {
						this.ImageName.Value = m.Groups[1].Value;
					} else {
						this.ImageName.Value = "";
						this.ThumbSource = null;
					}
				} else {
					this.ImageName.Value = "";
					this.ThumbSource = null;
				}
			}
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


		public void SetThumbSource(Model.ImageObject bmp) {
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
			var ng = this.Raw.Value.Url.IsCatalogUrl switch {
				true => Ng.NgUtil.NgHelper.CheckCatalogNg(this.Parent.Value.Raw.Value, this.Raw.Value),
				false => Ng.NgUtil.NgHelper.CheckThreadNg(this.Parent.Value.Raw.Value, this.Raw.Value),
			};
			var hidden = Ng.NgUtil.NgHelper.CheckHidden(this.Parent.Value.Raw.Value, this.Raw.Value);
			var isUpdate = (ng != this.IsNg.Value) || (hidden != this.IsHidden.Value);
			this.IsWatchWord.Value = Ng.NgUtil.NgHelper.CheckCatalogWatch(this.Parent.Value.Raw.Value, this.Raw.Value);
			this.IsCopyMode.Value = false;
			this.SetCommentHtml(ng, hidden);
			this.IsNg.Value = ng;
			this.IsHidden.Value = hidden;
			// 画像の再ロード
			if(isUpdate) {
				this.thumbSource.SetTarget(null);
				_ = this.ThumbSource;
			}
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

		private IObservable<ulong> StoreHash(Model.ImageObject bmp) {
			IObservable<(bool IsNew, ulong Value)> f() {
				if(HashCache.TryGetTarget(this.GetCacheKey(), out var hash)) {
					return Observable.Return((false, hash.Value));
				} else {
					var w = bmp.Image.PixelWidth;
					var h = bmp.Image.PixelHeight;
					var bytes = WpfUtil.ImageUtil.CreatePixelsBytes(bmp.Image);
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
					if(Ng.NgUtil.NgHelper.CheckImageWatch(x.Value)) {
						this.IsWatchImage.Value = true;
					}
					this.ThumbDisplay.Value = !Ng.NgUtil.NgHelper.CheckImageNg(x.Value);
					return x.Value;
				});

		}

		public IObservable<Model.ImageObject> LoadBitmapSource(bool forceLoad = false) {
			if((this.Raw.Value.ResItem.Res.Fsize == 0) || string.IsNullOrEmpty(this.Raw.Value.ResItem.Res.Thumb)) {
				return Observable.Return<Model.ImageObject>(null);
			}
			
			if(!forceLoad) {
				if(this.thumbSource.TryGetTarget(out var bitmapSource)) {
					this.ThumbToken.Value ??= new object();
					return Observable.Return<Model.ImageObject>(bitmapSource);
				}
				if(this.ThumbDisplay.Value.HasValue && !this.ThumbDisplay.Value.Value) {
					var bmp = WpfUtil.ImageUtil.GetNgImage();
					this.ThumbToken.Value ??= new object();
					this.thumbSource.SetTarget(bmp);
					return Observable.Return<Model.ImageObject>(bmp);
				} else {
					if(WpfUtil.ImageUtil.GetImageCache2(
						Util.Futaba.GetThumbImageLocalFilePath(
							this.Parent.Value.Url, this.Raw.Value.ResItem.Res)) is Model.ImageObject bmp) {
						this.ThumbToken.Value ??= new object();
						this.thumbSource.SetTarget(bmp);
						return Observable.Return<Model.ImageObject>(bmp);
					}
				}
			}

			return Util.Futaba.GetThumbImage(Raw.Value.Url, Raw.Value.ResItem.Res)
				.ObserveOn(UIDispatcherScheduler.Default)
				.Select(x => x switch {
					var v when v.Successed => WpfUtil.ImageUtil.CreateImage(x.LocalPath, x.FileBytes),
					_ => null,
				});
		}
	}
}
