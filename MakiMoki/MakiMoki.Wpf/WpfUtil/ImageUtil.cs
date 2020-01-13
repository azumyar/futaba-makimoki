using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class ImageUtil {
		private static Dictionary<string, WeakReference<BitmapImage>> bitmapDic
			= new Dictionary<string, WeakReference<BitmapImage>>();

		private static bool TryGetImage(string file, out BitmapImage image) {
			if(bitmapDic.TryGetValue(file, out var v)) {
				if(v.TryGetTarget(out var b)) {
					image = b;
					return true;
				}
			}
			image = null;
			return false;
		}

		private static void SetImage(string file, BitmapImage image) {
			var r = new WeakReference<BitmapImage>(image);
			if(bitmapDic.ContainsKey(file)) {
				bitmapDic[file] = r;
			} else {
				bitmapDic.Add(file, r);
			}
		}

		public static BitmapImage LoadImage(string path) {
			if(TryGetImage(path, out var b)) {
				return b;
			}

			if(Path.GetExtension(path).ToLower() == "webp") {
				System.Drawing.Image bitmap = null;
				try {
					try {
						var decoder = new Imazen.WebP.SimpleDecoder();
						using(var fs = new FileStream(path, FileMode.Open)) {
							var l = new List<byte>();
							while(fs.CanRead) {
								var bb = new byte[1024];
								var c = fs.Read(bb, 0, bb.Length);
								for(var i = 0; i < c; i++) {
									l.Add(bb[i]);
								}
							}
							bitmap = decoder.DecodeFromBytes(l.ToArray(), l.Count);
						}
					}
					catch(IOException e) {
						throw new Exceptions.ImageLoadFailedException(GetErrorMessage(path), e);
					}
					catch(ArgumentException e) {
						throw new Exceptions.ImageLoadFailedException(GetErrorMessage(path), e);
					}

					var bitmapImage = new BitmapImage();
					using(var stream = new MemoryStream()) {
						bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
						stream.Position = 0;

						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = stream;
						bitmapImage.EndInit();
					}

					SetImage(path, bitmapImage);
					return bitmapImage;
				}
				finally {
					bitmap?.Dispose();
				}
			} else {
				var bitmapImage = new BitmapImage(new Uri(path));

				SetImage(path, bitmapImage);
				return bitmapImage;
			}
		}

		private static string GetErrorMessage(string path) {
			return string.Format("{0}の読み込みに失敗しました", Path.GetFileName(path));
		}

		public static ReactiveProperty<ImageSource> ToThumbProperty(ReactiveProperty<Data.FutabaContext> futaba) {
			return ToThumbProperty(futaba
				.Select(x => {
					var it = x?.ResItems.FirstOrDefault();
					if((it != null) && it.Url.IsThreadUrl) {
						if(it.ResItem.Res.Fsize != 0) {
							return it;
						}
					}
					return null;
				}).ToReactiveProperty());
		}

		public static ReactiveProperty<ImageSource> ToThumbProperty(ReactiveProperty<Model.BindableFutaba> futaba) {
			return ToThumbProperty(futaba
				.Select(x => {
					var it = x?.Raw.ResItems.FirstOrDefault();
					if((it != null) && it.Url.IsThreadUrl) {
						if(it.ResItem.Res.Fsize != 0) {
							return it;
						}
					}
					return null;
				}).ToReactiveProperty());
		}


		public static ReactiveProperty<ImageSource> ToThumbProperty(ReactiveProperty<Data.FutabaContext.Item> futaba) {
			return futaba
				.SelectMany(async x => {
					if(x == null || (x.ResItem.Res.Fsize == 0)) {
						return null;
					} else {
						/*
						var t = Util.Futaba.GetThumbImage(x.Url, x.ResItem.Res);
						t.Wait();
						return t.Result;
						*/
						return await Util.Futaba.GetThumbImage(x.Url, x.ResItem.Res);
					}
				}).ObserveOnDispatcher()
				.Select(x => (x != null) ? (ImageSource)WpfUtil.ImageUtil.LoadImage(x) : null)
				.ToReactiveProperty();
			/*
			return futaba
				.ObserveOn(ThreadPoolScheduler.Instance)
				.Select(x => {
					System.Diagnostics.Debug.WriteLine(x?.Url);
					if (x == null || (x.ResItem.Res.Fsize == 0)) {
						return null;
					} else {
						/*
						var t = Util.Futaba.GetThumbImage(x.Url, x.ResItem.Res);
						t.Wait();
						return t.Result;
						* /
						return Util.Futaba.GetThumbImageSync(x.Url, x.ResItem.Res);
					}
				}).ObserveOnDispatcher()
				.Select(x => (x != null) ? (ImageSource)WpfUtil.ImageUtil.LoadImage(x) : null)
				.ToReactiveProperty();
			*/
		}
	}
}
