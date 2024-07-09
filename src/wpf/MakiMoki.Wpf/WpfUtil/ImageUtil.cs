using LibAPNG;
using LibAPNG.WPF;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using WpfAnimatedGif.Decoding;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class ImageUtil {
		class ImageStream : Stream {
			private byte[] bytes;

			public ImageStream(byte[] imageBytes) {
				this.bytes = imageBytes;
			}
			protected override void Dispose(bool disposing) {
				base.Dispose(disposing);
				if(disposing) {
					this.bytes = null;
				}
			}


			public override bool CanRead => true;

			public override bool CanSeek => true;

			public override bool CanWrite => false;

			public override long Length => this.bytes.Length;

			public override long Position { get; set; }

			public override void Flush() { }

			public override int Read(byte[] buffer, int offset, int count) {
				var i = 0;
				for(; (i < count) && (this.Position < this.Length); i++) {
					buffer[offset + i] = this.bytes[this.Position++];
				}
				return i;
			}

			public override long Seek(long offset, SeekOrigin origin) {
				switch(origin) {
				case SeekOrigin.Begin:
					this.Position = offset;
					break;
				case SeekOrigin.Current:
					this.Position += offset;
					break;
				case SeekOrigin.End:
					this.Position = this.Length - 1 - offset;
					break;
				}
				if(this.Position < 0) {
					this.Position = 0;
				}
				if(this.Length < this.Position) {
					this.Position = this.Length;
				}
				return this.Position;
			}

			public override void SetLength(long value) { }

			public override void Write(byte[] buffer, int offset, int count) { }
		}

		[DllImport(
			"libwebp",
			EntryPoint = "WebPGetInfo",
			CallingConvention = CallingConvention.Cdecl)]
		private static extern int WebPGetInfo(
			byte[] data,
			IntPtr data_size,
			ref int width,
			ref int height);
		[DllImport(
			"libwebp",
			EntryPoint = "WebPDecodeBGRAInto",
			CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr WebPDecodeBGRAInto(
			byte[] data,
			IntPtr data_size,
			byte[] output_buffer,
			IntPtr output_buffer_size,
			int output_stride);

		class PhantomCache<T> where T : class {
			private readonly object @lock = new();
			private List<(DateTime Time, T Value)> cache = new();

			private int keepSize, maxSize, keepTimeSec;

			public PhantomCache(int keep = 300, int max = 500, int keepTimeSec = 2 * 3600) {
				this.keepSize = keep;
				this.maxSize = max;
				this.keepTimeSec = keepTimeSec;
			}

			public PhantomCache<T> Add(T instance) {
				lock(@lock) {
					var now = DateTime.Now;
					var keep = now.AddSeconds(-this.keepTimeSec);
					this.cache.Add((now, instance));
					if(this.maxSize < this.cache.Count) {
						this.cache = this.cache
							.TakeLast(this.keepSize)
							.Where(x => keep <  x.Time)
							.ToList();
					}
					return this;
				}
			}
		}

		private volatile static Dictionary<string, WeakReference<Model.ImageObject>> bitmapBytesDic2
			= new Dictionary<string, WeakReference<Model.ImageObject>>();
		private volatile static PhantomCache<Model.ImageObject> phantomCache = new();
		private static Model.ImageObject ErrorImage { get; set; } = null;
		private static Model.ImageObject NgImage { get; set; } = null;

		private static bool TryGetImage2(string file, out Model.ImageObject image) {
			lock(bitmapBytesDic2) {
				if(bitmapBytesDic2.TryGetValue(file, out var v)) {
					if(v.TryGetTarget(out var b)) {
						image = b;
						return true;
					}
				}
				image = null;
				return false;
			}
		}

		private static Model.ImageObject SetImage2(string file, Model.ImageObject image) {
			lock(bitmapBytesDic2) {
				var r = new WeakReference<Model.ImageObject>(image);
				if(bitmapBytesDic2.ContainsKey(file)) {
					bitmapBytesDic2[file] = r;
				} else {
					bitmapBytesDic2.Add(file, r);
					phantomCache.Add(image);
				}

				foreach(var k in bitmapBytesDic2
					.Select(x => (Key: x.Key, Value: x.Value.TryGetTarget(out _)))
					.Where(x => !x.Value)
					.ToArray()) {

					bitmapBytesDic2.Remove(k.Key);
				}
				System.Diagnostics.Debug.WriteLine($"キャッシュサイズ= {bitmapBytesDic2.Count}");
				return image;
			}
		}


		public static Model.ImageObject GetErrorImage() {
			if(ErrorImage == null) {
				var asm = typeof(App).Assembly;
				ErrorImage = new Model.ImageObject(new WriteableBitmap(
					BitmapFrame.Create(asm.GetManifestResourceStream(
						$"{typeof(App).Namespace}.Resources.Images.ErrorImage.png"))));
			}
			return ErrorImage;
		}

		public static Model.ImageObject GetNgImage() {
			if(NgImage == null) {
				var asm = typeof(App).Assembly;
				NgImage = new Model.ImageObject(new WriteableBitmap(
					BitmapFrame.Create(asm.GetManifestResourceStream(
						$"{typeof(App).Namespace}.Resources.Images.NgImage.png"))));
			}
			return NgImage;
		}

		public static string GetNgImageBase64() {
			var asm = typeof(App).Assembly;
			return Util.FileUtil.ToBase64(asm.GetManifestResourceStream(
				$"{ typeof(App).Namespace }.Resources.Images.NgImage.png"));
		}

		public static Model.ImageObject GetImageCache2(string path) {
			if(TryGetImage2(path, out var b)) {
				return b;
			}
			return null;
		}

#if false
		public static BitmapSource CreateImage(string file, Stream stream) {
			if(stream == null) {
				return null;
			}
			if(System.Windows.Threading.Dispatcher.CurrentDispatcher != System.Windows.Application.Current?.Dispatcher) {
				System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!! UIスレッド外でのCreateImage !!!!!!!!!!!!!!!!!!!");
				return null;
			}

			if(TryGetImage2(file, out var b)) {
				return b;
			} else {
				return SetImage2(file, new WriteableBitmap(BitmapFrame.Create(stream)));
			}
			/*
			var bitmapImage = new BitmapImage();
			try {
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = stream;
				bitmapImage.EndInit();
				bitmapImage.Freeze();

				return bitmapImage;
			}
			catch(ArgumentException) { // 画像によってはエラー出てクラッシュするので対策
				return null;
			}
			*/
		}
#endif

		public static Model.ImageObject CreateImage(string file, byte[] imageBytes = null) {
			if(System.Windows.Threading.Dispatcher.CurrentDispatcher != System.Windows.Application.Current?.Dispatcher) {
				System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!! UIスレッド外でのCreateImage !!!!!!!!!!!!!!!!!!!");
				return null;
			}

			if(TryGetImage2(file, out var b)) {
				return b;
			} else {
				var ex = Path.GetExtension(file).ToLower();
				try {
					return SetImage2(file, ex switch {
						".png" => LoadPng(file, imageBytes),
						".gif" => LoadGif(file, imageBytes),
						_ => new Model.ImageObject(BitmapFrame.Create(LoadStream(file, imageBytes))),
					});
				}
				catch(Exception e) when ((e is COMException) || (e is Exceptions.ImageLoadFailedException)) {
					return GetErrorImage();
				}
			}
		}

		public static Stream LoadStream(string path, byte[] imageBytes = null) {
			if(Path.GetExtension(path).ToLower() == ".webp") {
				return LoadWebP(path, imageBytes);
			}
				
			if(imageBytes != null) {
				return new ImageStream(imageBytes);
				//return new MemoryStream(imageBytes);
			}


			var s = CreateStream(path);
			if(!s.Sucessed) {
				return null;
			}
			
			var l = new List<byte>();
			{
				var b = new byte[1024];
				while(true) {
					var r = s.Stream.Read(b);
					if(0 < r) {
						l.AddRange(b.Take(r));
					} else {
						break;
					}
				}
			}
			s.Stream.Dispose();

			return new ImageStream(l.ToArray());
			//return new MemoryStream(l.ToArray());
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

		// とりあえず用意しておく
		public static IObservable<Stream> LoadStreamObservable(string path, byte[] imageBytes = null) {
			return Observable.Create<Stream>(async o => {
				static byte[] readStream(Stream stream) {
					var l = new List<byte>(512);
					var b = new byte[512];
					while(true) {
						var r = stream.Read(b);
						if(0 < r) {
							l.AddRange(b.Take(r));
						} else {
							break;
						}
					}
					return l.ToArray();
				}

				try {
					if(Path.GetExtension(path).ToLower() == ".webp") {
						try {
							static byte[] readData(string path, byte[] imageBytes) {
								if(imageBytes != null) {
									return imageBytes;
								}
								using var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
								return readStream(s);
							}

							var data = readData(path, imageBytes);
							{
								var w = 0;
								var h = 0;
								if(WebPGetInfo(data, (IntPtr)data.Length, ref w, ref h) == 0) {
									throw new Exceptions.ImageLoadFailedException(GetErrorMessage(path));
								}
								var stride = w * 4;
								var output = new byte[w * h * 4];
								if(WebPDecodeBGRAInto(
									data, (IntPtr)data.Length,
									output, (IntPtr)output.Length,
									stride) == IntPtr.Zero) {

									o.OnError(new Exceptions.ImageLoadFailedException(GetErrorMessage(path)));
								}

								var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
								wb.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), output, stride, 0);

								var ms = new MemoryStream();
								var encoder = new PngBitmapEncoder();
								encoder.Frames.Add(BitmapFrame.Create(wb));
								encoder.Save(ms);
								o.OnNext(ms);
							}
						}
						catch(IOException e) {
							await Task.Delay(500);
							o.OnError(new Exceptions.ImageLoadFailedException(GetErrorMessage(path), e));
						}
					} else {
						if(imageBytes != null) {
							o.OnNext(new ImageStream(imageBytes));
						}

						try {
							using var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
							o.OnNext(new ImageStream(readStream(s)));
						}
						catch(IOException e) {
							await Task.Delay(500);
							o.OnError(new Exceptions.ImageLoadFailedException(GetErrorMessage(path), e));
						}
					}
				}
				finally {
					o.OnCompleted();
				}
				return System.Reactive.Disposables.Disposable.Empty;
			}).Retry(5);
		}

		private static Stream LoadWebP(string path, byte[] imageBytes) {
			var data = imageBytes;
			if(data == null) {
				var s = CreateStream(path);
				try {
					if(!s.Sucessed) {
						throw new Exceptions.ImageLoadFailedException(GetErrorMessage(path));
					}
					var l = new List<byte>();
					while(s.Stream.CanRead) {
						var bb = new byte[1024];
						var c = s.Stream.Read(bb, 0, bb.Length);
						if(c == 0) {
							break;
						}
						l.AddRange(bb.Take(c));
					}
					data = l.ToArray();
				}
				finally {
					s.Stream?.Dispose();
				}
			}

			var w = 0;
			var h = 0;
			if(WebPGetInfo(data, (IntPtr)data.Length, ref w, ref h) == 0) {
				throw new Exceptions.ImageLoadFailedException(GetErrorMessage(path));
			}
			var stride = w * 4;
			var output = new byte[w * h * 4];
			if(WebPDecodeBGRAInto(
				data, (IntPtr)data.Length,
				output, (IntPtr)output.Length,
				stride) == IntPtr.Zero) {

				throw new Exceptions.ImageLoadFailedException(GetErrorMessage(path));
			}

			var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Pbgra32, null);
			wb.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), output, stride, 0);

			var ms = new MemoryStream();
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(wb));
			encoder.Save(ms);
			//ms.Position = 0;			
			return ms;
		}

		private static Model.ImageObject? LoadPng(string file, byte[]? imageBytes) {
			var p = imageBytes switch {
				byte[] v => new LibAPNG.APNG(imageBytes),
				_ => new LibAPNG.APNG(file),
			};

			// 8bitインデックスカラーPNGが読めないので対策する
			static Stream fixIndexdPng(string file, byte[]? imageBytes) {
				var stream = new MemoryStream();
				if(imageBytes is byte[]) {
					using var ms = new MemoryStream(imageBytes);
					using var bmp = new System.Drawing.Bitmap(ms);
					bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				} else {
					using var bmp = new System.Drawing.Bitmap(file);
					bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				}
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}

			var img = BitmapFrame.Create(
				((p.IHDRChunk.ColorType == 3) && p.IsSimplePNG) switch {
					true => fixIndexdPng(file, imageBytes),
					false => p.DefaultImage.GetStream(),
				},
				BitmapCreateOptions.None,
				BitmapCacheOption.OnLoad);
			if(p.IsSimplePNG) {
				return new Model.ImageObject(img);
			} else {
				var animation = p.ToAnimation();
				var storyboard = new Storyboard();
				//Storyboard.SetTargetName(animation, Path.GetFileNameWithoutExtension());
				Storyboard.SetTargetProperty(
					animation,
					new PropertyPath(System.Windows.Controls.Image.SourceProperty));
				storyboard.Children.Add(animation);
				return new Model.ImageObject(img, storyboard);
			}
		}

		private static Model.ImageObject? LoadGif(string file, byte[] imageBytes) {
			Stream stream;
			if(imageBytes != null) {
				stream = new MemoryStream(imageBytes);
			} else {
				var s = CreateStream(file);
				if(!s.Sucessed) {
					return GetErrorImage();
				}
				stream = s.Stream;
			}
			if(BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad) is GifBitmapDecoder decoder) {
				try {
					if(decoder?.Metadata != null) {
						if(!decoder.Frames.Skip(1).Any()) {
							return new Model.ImageObject(decoder.Frames.First());
						}

						stream.Seek(0, SeekOrigin.Begin);
						var gifFile = GifFile.ReadGifFile(stream, true);
						var width = gifFile.Header.LogicalScreenDescriptor.Width;
						var height = gifFile.Header.LogicalScreenDescriptor.Height;
						int index = 0;
						var keyFrames = new ObjectKeyFrameCollection();
						var totalDuration = TimeSpan.Zero;
						var baseFrame = default(BitmapSource);
						foreach(var rawFrame in decoder.Frames) {
							static bool isFull(GifImageDescriptor metadata, int w, int h) {
								return metadata.Left == 0
									   && metadata.Top == 0
									   && metadata.Width == w
									   && metadata.Height == h;
							}
							static BitmapSource clear(BitmapSource frame, GifImageDescriptor metadata) {
								DrawingVisual visual = new DrawingVisual();
								using(var context = visual.RenderOpen()) {
									var fullRect = new Rect(0, 0, frame.PixelWidth, frame.PixelHeight);
									var clearRect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
									var clip = Geometry.Combine(
										new RectangleGeometry(fullRect),
										new RectangleGeometry(clearRect),
										GeometryCombineMode.Exclude,
										null);
									context.PushClip(clip);
									context.DrawImage(frame, fullRect);
								}

								var bitmap = new RenderTargetBitmap(
										frame.PixelWidth, frame.PixelHeight,
										frame.DpiX, frame.DpiY,
										PixelFormats.Pbgra32);
								bitmap.Render(visual);

								var result = new WriteableBitmap(bitmap);

								if(result.CanFreeze && !result.IsFrozen)
									result.Freeze();
								return result;
							}

							var ggce = gifFile.Frames[index].Extensions.OfType<GifGraphicControlExtension>().FirstOrDefault();
							var metadata = new {
								Descriptor = gifFile.Frames[index].Descriptor,
								Left = gifFile.Frames[index].Descriptor.Left,
								Top = gifFile.Frames[index].Descriptor.Top,
								Width = gifFile.Frames[index].Descriptor.Width,
								Height = gifFile.Frames[index].Descriptor.Height,
								Delay = ggce switch {
									GifGraphicControlExtension v when v.Delay != 0 => TimeSpan.FromMilliseconds(v.Delay),
									_ => TimeSpan.FromMilliseconds(100),
								},
								DisposalMethod = ggce switch {
									GifGraphicControlExtension v => (ImageBehavior.FrameDisposalMethod)v.DisposalMethod,
									_ => ImageBehavior.FrameDisposalMethod.None
								}
							};
							BitmapSource frame;
							if((baseFrame == null) && isFull(metadata.Descriptor, width, height)) {
								frame = rawFrame;
							} else {
								var visual = new DrawingVisual();
								using(var context = visual.RenderOpen()) {
									if(baseFrame != null) {
										context.DrawImage(baseFrame, new Rect(0, 0, width, height));
									}

									var rect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
									context.DrawImage(rawFrame, rect);
								}
								var bitmap = new RenderTargetBitmap(
									width, height,
									96, 96,
									PixelFormats.Pbgra32);
								bitmap.Render(visual);

								frame = new WriteableBitmap(bitmap);
								if(frame.CanFreeze && !frame.IsFrozen) {
									frame.Freeze();
								}
							}

							keyFrames.Add(new DiscreteObjectKeyFrame(frame, totalDuration));
							totalDuration += metadata.Delay;
							baseFrame = metadata switch {
								var v when (v.DisposalMethod == ImageBehavior.FrameDisposalMethod.None) => frame,
								var v when (v.DisposalMethod == ImageBehavior.FrameDisposalMethod.DoNotDispose) => frame,
								var v when (v.DisposalMethod == ImageBehavior.FrameDisposalMethod.RestoreBackground) && isFull(v.Descriptor, width, height) => null,
								var v when (v.DisposalMethod == ImageBehavior.FrameDisposalMethod.RestoreBackground) => clear(frame, v.Descriptor),
								var v when (v.DisposalMethod == ImageBehavior.FrameDisposalMethod.RestorePrevious) => baseFrame,
								var v when ((4 <= (int)v.DisposalMethod) && ((int)v.DisposalMethod <= 7)) => frame, // 4-7は仕様で未定義なのでとりあえずnoneとしてふるまう
								_ => throw new ArgumentException(),
							};
							index++;
						}
						// 無限に固定
						//var repeatCount = gifFile.RepeatCount;
						return new Model.ImageObject(decoder.Frames.First(), new ObjectAnimationUsingKeyFrames {
							KeyFrames = keyFrames,
							Duration = totalDuration,
							RepeatBehavior = RepeatBehavior.Forever,
							SpeedRatio = 1.0
						});
					}
				}
				catch { }
			}
			return null;
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
			return $"{Path.GetFileName(path)}の読み込みに失敗しました";
		}

		public static byte[] CreatePixelsBytes(BitmapSource bitmapImage) {
			if(System.Windows.Threading.Dispatcher.CurrentDispatcher != System.Windows.Application.Current?.Dispatcher) {
				System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!! UIスレッド外でのCreateImage !!!!!!!!!!!!!!!!!!!");
				return null;
			}

			var fcb = new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgr32, null, 0);
			var bytes = new byte[bitmapImage.PixelWidth * bitmapImage.PixelHeight * 4];
			var stride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
			fcb.CopyPixels(bytes, stride, 0);
			return bytes;
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

		public static Color GetMaterialSubColor(Color baseColor) {
			var hsl = ToHsl(baseColor);
			var l = Math.Max(hsl.Lightness - (0.05 * 2), 0);
			var rgb = HslToRgb(hsl.Hue, hsl.Saturation, l);
			return Color.FromArgb(baseColor.A, rgb.R, rgb.G, rgb.B);
		}

		public static Color GetTextColor(Color background, Color white, Color black, double threshold = 0.5) {
			return (ToHsl(background).Lightness < threshold) ? white : black;
		}

		public static (double H, double S, double V) ToHsv(Color c) {
			var r = c.R / 255.0;
			var g = c.G / 255.0;
			var b = c.B / 255.0;

			var list = new[] { r, g, b };
			var max = list.Max();
			var min = list.Min();

			double h;
			if(max == min) {
				h = 0;
			} else if(max == r) {
				h = (60 * (g - b) / (max - min) + 360) % 360;
			} else if(max == g) {
				h = 60 * (b - r) / (max - min) + 120;
			} else {
				h = 60 * (r - g) / (max - min) + 240;
			}
			var s = ((max == 0) ? 0 : ((max - min) / max)) * 100;
			var v = max * 100;

			return (h, s, v);
		}

		public static Color HsvToRgb(double Hue, double Saturation, double Brightness) {
			// https://sites.google.com/site/bknobiboroku/programming-tips/wpf/Csharp_wpf_HSB_to_RGB
			List<double> colors = new List<double> { 0, 0, 0 };
			int CC = colors.Count;

			double RelativeInDegrees = Hue / (360 / CC);
			int WholeNoPart = (int)Math.Truncate(RelativeInDegrees);
			double FractionalPart = RelativeInDegrees % 1.0;

			int StartIndex = WholeNoPart % CC;
			int SecondIndex = (WholeNoPart + 1) % CC;

			double val = FractionalPart * 2;
			double color1 = Math.Min(2 - val, 1);
			double color2 = Math.Min(val, 1);
			colors[StartIndex] = 255 * color1;
			colors[SecondIndex] = 255 * color2;
			double satAs0to1 = Saturation / 100.0;
			colors = colors.Select(item => item += (255 - item) * (1.0 - satAs0to1)).ToList();

			double brightAs0to1 = Brightness / 100.0;
			colors = colors.Select(item => item *= brightAs0to1).ToList();

			var col = Color.FromArgb(255, (byte)colors[0], (byte)colors[1], (byte)colors[2]);
			return col;
		}

		public static (double Hue, double Saturation, double Lightness) ToHsl(Color rgb) {
			float r = (float)rgb.R / 255f;
			float g = (float)rgb.G / 255f;
			float b = (float)rgb.B / 255f;

			float max = Math.Max(r, Math.Max(g, b));
			float min = Math.Min(r, Math.Min(g, b));

			float lightness = (max + min) / 2f;

			float hue, saturation;
			if(max == min) {
				//undefined
				hue = 0f;
				saturation = 0f;
			} else {
				float c = max - min;

				if(max == r) {
					hue = (g - b) / c;
				} else if(max == g) {
					hue = (b - r) / c + 2f;
				} else {
					hue = (r - g) / c + 4f;
				}
				hue *= 60f;
				if(hue < 0f) {
					hue += 360f;
				}

				//saturation = c / (1f - Math.Abs(2f * lightness - 1f));
				if(lightness < 0.5f) {
					saturation = c / (max + min);
				} else {
					saturation = c / (2f - max - min);
				}
			}

			return (hue, saturation, lightness);
		}

		public static Color HslToRgb(double hue, double saturation, double lightness) {
			var s = saturation;
			var l = lightness;

			double r1, g1, b1;
			if(s == 0) {
				r1 = l;
				g1 = l;
				b1 = l;
			} else {
				var h = hue / 60.0;
				var i = (int)Math.Floor(h);
				var f = h - i;
				//float c = (1f - Math.Abs(2f * l - 1f)) * s;
				var c = (l < 0.5) ? (2.0 * s * l) : (2.0 * s * (1.0 - l));
				var m = l - c / 2.0;
				var p = c + m;
				//float x = c * (1f - Math.Abs(h % 2f - 1f));
				var q = (i % 2 == 0) ? (l + c * (f - 0.5)) : (l - c * (f - 0.5));

				switch(i) {
				case 0:
					r1 = p;
					g1 = q;
					b1 = m;
					break;
				case 1:
					r1 = q;
					g1 = p;
					b1 = m;
					break;
				case 2:
					r1 = m;
					g1 = p;
					b1 = q;
					break;
				case 3:
					r1 = m;
					g1 = q;
					b1 = p;
					break;
				case 4:
					r1 = q;
					g1 = m;
					b1 = p;
					break;
				case 5:
					r1 = p;
					g1 = m;
					b1 = q;
					break;
				default:
					throw new ArgumentException(
						"色相の値が不正です。", "hsl");
				}
			}

			return Color.FromRgb(
				(byte)Math.Round(r1 * 255.0),
				(byte)Math.Round(g1 * 255.0),
				(byte)Math.Round(b1 * 255.0));
		}
	}
}
