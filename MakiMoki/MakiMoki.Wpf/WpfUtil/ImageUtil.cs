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
		private volatile static Dictionary<string, WeakReference<BitmapImage>> bitmapDic
			= new Dictionary<string, WeakReference<BitmapImage>>();
		private static BitmapImage NgImage { get; set; } = null;

		private static bool TryGetImage(string file, out BitmapImage image) {
			lock(bitmapDic) {
				if(bitmapDic.TryGetValue(file, out var v)) {
					if(v.TryGetTarget(out var b)) {
						image = b;
						return true;
					}
				}
				image = null;
				return false;
			}
		}

		private static void SetImage(string file, BitmapImage image) {
			lock(bitmapDic) {
				var r = new WeakReference<BitmapImage>(image);
				if(bitmapDic.ContainsKey(file)) {
					bitmapDic[file] = r;
				} else {
					bitmapDic.Add(file, r);
				}
			}
		}

		public static BitmapImage GetNgImage() {
			if(NgImage == null) {
				var asm = typeof(App).Assembly;
				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = asm.GetManifestResourceStream(
					$"{ typeof(App).Namespace }.Resources.Images.NgImage.png");
				bitmapImage.EndInit();
				bitmapImage.Freeze();

				NgImage = bitmapImage;
			}
			return NgImage;
		}

		public static string GetNgImageBase64() {
			var asm = typeof(App).Assembly;

			return Util.FileUtil.ToBase64(asm.GetManifestResourceStream(
				$"{ typeof(App).Namespace }.Resources.Images.NgImage.png"));
		}

		public static BitmapImage GetImageCache(string path) {
			if(TryGetImage(path, out var b)) {
				return b;
			}
			return null;
		}

		public static BitmapImage LoadImage(string path) {
			try {
				if(TryGetImage(path, out var b)) {
					return b;
				}

				if(Path.GetExtension(path).ToLower() == ".webp") {
					System.Drawing.Image bitmap = null;
					try {
						Stream stream = null;
						try {
							var decoder = new Imazen.WebP.SimpleDecoder();
							var s = CreateStream(path);
							if(s.Sucessed) {
								stream = s.Stream;
								var l = new List<byte>();
								while(s.Stream.CanRead) {
									var bb = new byte[1024];
									var c = s.Stream.Read(bb, 0, bb.Length);
									if(c == 0) {
										break;
									}
									l.AddRange(bb.Take(c));
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
						finally {
							stream?.Dispose();
						}

						var bitmapImage = new BitmapImage();
						var ms = new MemoryStream();
						bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
						ms.Position = 0;

						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = ms;
						bitmapImage.EndInit();
						bitmapImage.Freeze();

						SetImage(path, bitmapImage);
						return bitmapImage;
					}
					finally {
						bitmap?.Dispose();
					}
				} else {
					var s = CreateStream(path);
					try {
						if(s.Sucessed) {
							var bitmapImage = new BitmapImage();
							bitmapImage.BeginInit();
							bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
							bitmapImage.StreamSource = s.Stream;
							bitmapImage.EndInit();
							bitmapImage.Freeze();

							SetImage(path, bitmapImage);
							return bitmapImage;
						} else {
							System.Diagnostics.Debug.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
							return null; /* TODO: エラー画像を用意する */
						}
					}
					finally {
						// AnimationGifで再利用されるのでDisposeしてはいけない
						// stream?.Dispose();
					}
				}
			}
			finally {
				var d = System.Windows.Threading.Dispatcher.CurrentDispatcher;
				if(d != System.Windows.Application.Current?.Dispatcher) {
					d.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.SystemIdle);
					//System.Windows.Threading.Dispatcher.Run();
				}
			}
		}

		public static BitmapImage LoadImage(string path, byte[] imageBytes) {
			try {
				if(TryGetImage(path, out var b)) {
					return b;
				}

				if(Path.GetExtension(path).ToLower() == ".webp") {
					System.Drawing.Image bitmap = null;
					try {
						var decoder = new Imazen.WebP.SimpleDecoder();
						bitmap = decoder.DecodeFromBytes(imageBytes, imageBytes.Length);

						var bitmapImage = new BitmapImage();
						var stream = new MemoryStream();
						bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
						stream.Position = 0;

						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = stream;
						bitmapImage.EndInit();
						bitmapImage.Freeze();

						SetImage(path, bitmapImage);
						return bitmapImage;
					}
					finally {
						bitmap?.Dispose();
					}
				} else {
					Stream stream = null;
					try {
						/*
						if(Path.GetExtension(path).ToLower() == ".png") {
							stream = LoadPng(path, imageBytes);
						}
						*/
						if(stream == null) {
							stream = new MemoryStream(imageBytes);
						}
						var bitmapImage = new BitmapImage();
						bitmapImage.BeginInit();
						bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
						bitmapImage.StreamSource = stream;
						bitmapImage.EndInit();
						bitmapImage.Freeze();

						SetImage(path, bitmapImage);
						return bitmapImage;
					}
					finally {
						// AnimationGifで再利用されるのでDisposeしてはいけない
						// stream?.Dispose();
					}
				}
			}
			finally {
				var d = System.Windows.Threading.Dispatcher.CurrentDispatcher;
				if(d != System.Windows.Application.Current?.Dispatcher) {
					d.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.SystemIdle);
					//System.Windows.Threading.Dispatcher.Run();
				}
			}
		}

		private static (bool Sucessed, FileStream Stream) CreateStream(string path) {
			var sucessed = false;
			FileStream stream = null;
			Observable.Create<FileStream>(async (o) => {
				try {
					var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					o.OnNext(s);
				}
				catch(IOException e) {
					System.Diagnostics.Debug.WriteLine("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
					await Task.Delay(500);
					o.OnError(e);
				}
			}).Retry(5)
			.Subscribe(
				s => { stream = s; sucessed = true; },
				ex => { });
			return (sucessed, stream);
		}

#if false
		private static Stream LoadPng(string path, byte[] imageBytes) {
			var list = new List<byte>();
			if(imageBytes == null) {
				using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					while(fs.CanRead) {
						var bb = new byte[1024];
						var c = fs.Read(bb, 0, bb.Length);
						if(c == 0) {
							break;
						}
						list.AddRange(bb.Take(c));
					}
					/*
					if((8 + 0x10) <= l.Count) {
						width = BitConverter.ToInt32(list.Skip(8 + 0x08).Take(4).Reverse().ToArray(), 0);
						height = BitConverter.ToInt32(list.Skip(8 + 0x0c).Take(4).Reverse().ToArray(), 0);
					}
					*/
				}
			} else {
				list.AddRange(imageBytes);
			}
			var i = 8 + 25;
			while(i < list.Count) {
				const int chankDataLength = 4;
				const int chankType = 4;
				const int crc = 4;
				var sz = BitConverter.ToInt32(list.Skip(i).Take(4).Reverse().ToArray(), 0);
				var tp = list.Skip(i).Skip(0x04).Take(4).ToArray();
				// pHYsチャンク判定
				if((tp[0] == 0x70) && (tp[1] == 0x48) && (tp[2] == 0x59) && (tp[3] == 0x73)) {
					/*
					var isM = list.Skip(i).Skip(0x10).Take(1).First();
					if(isM == 1) {
						const double inch = 39.3700787d;
						var dpix = BitConverter.ToInt32(list.Skip(i).Skip(0x08).Take(4).Reverse().ToArray(), 0) / inch;
						var dpiy = BitConverter.ToInt32(list.Skip(i).Skip(0x0c).Take(4).Reverse().ToArray(), 0) / inch;
						System.Diagnostics.Debug.WriteLine($"{dpix}/{dpiy}");
					}
					*/
					// pHYsチャンクを削除する
					list.RemoveRange(i, sz + chankDataLength + chankType + crc);
					break;
				} else {
					i += sz + chankDataLength + chankType + crc;
				}
			}
			return new MemoryStream(list.ToArray());
		}
#endif

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
						return await Util.Futaba.GetThumbImageAsync(x.Url, x.ResItem.Res);
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

		public static ulong CalculatePerceptualHash(BitmapSource bitmapImage) {
#if true
			var fcb = new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgr32, null, 0);
			var bytes = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight * 4];
			var stride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
			fcb.CopyPixels(bytes, stride, 0);
			return Ng.NgUtil.PerceptualHash.CalculateHash(bytes, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 32);
#else
			var fcb = new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray8, null, 0);
			var bytes = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight];
			var stride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
			var tmp = new byte[bitmapImage.PixelHeight * stride];
			fcb.CopyPixels(tmp, stride, 0);
			for(var yy = 0; yy < bitmapImage.PixelHeight; yy++) {
				for(var xx = 0; xx < bitmapImage.PixelWidth; xx++) {
					bytes[yy * bitmapImage.PixelWidth + xx] = tmp[yy * stride + xx];
				}
			}
			return Ng.NgUtil.PerceptualHash.CalculateHash(bytes, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 8);
#endif
		}

		public static void SaveJpeg(string path, System.Drawing.Image image, int quality) {
			var jpegEncoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
				.Where(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
				.FirstOrDefault();
			if(jpegEncoder != null) {
				var encParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
				var encParams = new System.Drawing.Imaging.EncoderParameters(1);
				encParams.Param[0] = encParam;
				image.Save(path, jpegEncoder, encParams);
			}
		}

		public static void SaveJpeg(string path, BitmapSource image, int quality) {
			var jpegEncoder = new JpegBitmapEncoder() {
				QualityLevel = quality,
			};
			jpegEncoder.Frames.Add(BitmapFrame.Create(image));
			var fm = File.Exists(path) ? FileMode.Truncate : FileMode.OpenOrCreate;
			using(FileStream fs = new FileStream(path, fm)) {
				jpegEncoder.Save(fs);
			}
		}
	}
}
