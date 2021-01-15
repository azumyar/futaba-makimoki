using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Ng.NgUtil {
	public static class PerceptualHash {
		// 参考元：https://hexadrive.jp/hexablog/program/28091/
		private const int lanczosDefaultN = 3;
		private static byte[] ResizeLanczos(byte[] src, int srcWidth, int srcHeight, int width, int height, int range = lanczosDefaultN) {
			var bitmap = new byte[width * height];

			// スケール
			var scaleX = (double)width / srcWidth;
			var scaleY = (double)height / srcHeight;

			// 単位を原画像の座標系に変換
			var unitX = 1.0 / scaleX;
			var unitY = 1.0 / scaleY;

			// 原画像から参照する範囲
			var n = (double)range;
			var rangeX = n;
			var rangeY = n;

			// 1ピクセル幅を考慮した座標の補正
			var correctionX = unitX * 0.5;
			var correctionY = unitY * 0.5;

			// 縮小のときは参照範囲を拡大
			if(1.0 < unitX) {
				rangeX *= unitX;
			}
			if(1.0 < unitY) {
				rangeY *= unitY;
			}

			Parallel.For(0, height, y => {
				for(int x = 0; x < width; x++) {
					// 原画像の座標系に変換
					var srcX = unitX * x + correctionX;
					var srcY = unitY * y + correctionY;

					bitmap[y * width + x]= InterpolateLanczos(
						src, srcWidth, srcHeight, 
						srcX, srcY, rangeX, rangeY, n);
				}
			});

			return bitmap;
		}

		private static byte InterpolateLanczos(
			byte[] src, int srcWidth, int srcHeight,
			double srcX, double srcY, double rangeX, double rangeY, double n) {

			double clamp(double v, double min, double max) => Math.Min(Math.Max(v, min), max);
			double lanczos(double x, double N) => (N <= Math.Abs(x)) ? 0.0f : sinc(x) * sinc(x / N);
			double sinc(double x) => (x == 0.0f) ? 1.0f : (Math.Sin(Math.PI * x) / (Math.PI * x));
			double y = 0;

			// 距離のスケール修正
			var scaleCorrectionX = n / rangeX;
			var scaleCorrectionY = n / rangeY;

			var totalWeight = 0.0;

			// 基準位置(srcX, srcY)から距離n未満のピクセル範囲を求める
			var firstX = (int)clamp(srcX - rangeX + 0.5f, 0.0f, srcWidth);
			var firstY = (int)clamp(srcY - rangeY + 0.5f, 0.0f, srcHeight);
			var lastX = (int)clamp(srcX + rangeX - 0.5f, 0.0f, srcWidth);
			var lastY = (int)clamp(srcY + rangeY - 0.5f, 0.0f, srcHeight);

			// 1ピクセルずつ参照して距離パターンが限られるため重みをキャッシュ
			var weightX = new double[lastX - firstX + 1];
			var weightY = new double[lastY - firstY + 1];

			// X方向の重みを事前計算
			for(var intX = firstX; intX <= lastX; intX++) {
				// 着目ピクセルの中心と基準位置との距離から重み算出
				var diff = (intX + 0.5 - srcX) * scaleCorrectionX;
				weightX[intX - firstX] = lanczos(diff, n);
			}

			// Y方向の重みを事前計算
			for(int intY = firstY; intY <= lastY; intY++) {
				// 着目ピクセルの中心と基準位置との距離から重み算出
				var diff = (intY + 0.5 - srcY) * scaleCorrectionY;
				weightY[intY - firstY] = lanczos(diff, n);
			}

			for(int intY = firstY; intY < lastY; intY++) {
				for(int intX = firstX; intX < lastX; intX++) {
					var weight = weightX[intX - firstX] * weightY[intY - firstY];

					if(weight == 0.0) {
						continue;
					}
					totalWeight += weight;

					y += src[intY * srcWidth + intX] * weight;
				}
			}

			var result = y;

			// 積分なので全体で重み1となるよう補正
			if(totalWeight != 0.0f) {
				result = result / totalWeight;
			}

			return (byte)result;
		}

		private static byte[] ToGray(byte[] input, int width, int height, int bit) {
			System.Diagnostics.Debug.Assert((bit == 8) || (bit == 24) || (bit == 32));
			System.Diagnostics.Debug.Assert(input != null);
			System.Diagnostics.Debug.Assert(input.Length == width * height * (bit / 8));

			var s = bit / 8;
			if(s == 1) {
				return input.ToArray();
			}

			var ret = new byte[width * height];
			for(var i = 0; i < ret.Length; i++) {
				var b = input[i * s + 0];
				var g = input[i * s + 1];
				var r = input[i * s + 2];
				//ret[i] = (byte)Math.Round(0.2126 * r + 0.7152 * g + 0.0722 * b);
				ret[i] = (byte)Math.Round(0.299 * r + 0.587 * g + 0.114 * b);
			}
			return ret;
		}

		private static double[] Dct2d(byte[] input, int width, int height) {
			System.Diagnostics.Debug.Assert(input != null);
			System.Diagnostics.Debug.Assert(input.Length == width * height);
			var r = new double[input.Length];

			var cp = 1.0 / Math.Sqrt(2);
			double c(int x) => (x == 0) ? cp : 1.0;
			Parallel.For(0, input.Length, i => {
				var x1 = i % width;
				var y1 = i / width;
				for(var j = 0; j < input.Length; j++) {
					var x2 = j % width;
					var y2 = j / width;

					r[i] += input[j]
						* Math.Cos(((2 * x2 + 1) * x1 * Math.PI) / (2 * width))
						* Math.Cos(((2 * y2 + 1) * y1 * Math.PI) / (2 * height));
				}
				r[i] = c(x1) * c(y1) * r[i];
			});
			/*
			for(var i=0; i<input.Length;i++) {
				var x1 = i % width;
				var y1 = i / width;
				for(var j=0;j<input.Length;j++) {
					var x2 = j % width;
					var y2 = j / width;

					var a = (x2 == 0) ? 0.5 : Math.Cos(Math.PI / width * x2 * (x1 + 0.5));
					var b = (y2 == 0) ? 0.5 : Math.Cos(Math.PI / height * y2 * (y1 + 0.5));

					r[j] += input[i] * a * b;
				}
			}
			*/
			return r;
		}

		private static double Median(double[] d) {
			var d2 = d.OrderBy(x => x).ToArray();
			if(d2.Length % 2 == 0) {
				return (d2[d2.Length / 2] + d2[d2.Length / 2 + 1]) / 2;
			} else {
				return d2[d2.Length / 2];
			}
		}

		//static Random rnd = new Random();

		public static ulong CalculateHash(byte[] image, int width, int height, int bit) {
			int size = 32;
			var gray = ToGray(image, width, height, bit);
			var input = ResizeLanczos(gray, width, height, size, size);
			var dct = Dct2d(input, size, size);
			var low = new double[8 * 8];
			for(var x=0; x<8; x++) {
				for(var y=0; y<8; y++) {
					low[y * 8 + x] = dct[y * size + x];
				}
			}
			var m = Median(low);
			ulong r = 0;
			for(var i = 0; i < low.Length; i++) {
				r |= ((m < low[i]) ? 1ul : 0) << i;
			}

			//ExportPgm("y:\\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + $"-{ rnd.Next() }.pgm", gray, width, height);
			//ExportPgm("y:\\" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + $"-{ rnd.Next() }.pgm", input, size, size);

			return r;
		}

		public static int GetHammingDistance(ulong a, ulong b) {
			/*
			var d = a ^ b;
			var i = 0;
			while(d != 0) {
				d &= d - 1;
				i++;
			}
			return i;
			*/
			int count(ulong bits) {
				bits = (bits & 0x5555555555555555) + (bits >> 1 & 0x5555555555555555);
				bits = (bits & 0x3333333333333333) + (bits >> 2 & 0x3333333333333333);
				bits = (bits & 0x0f0f0f0f0f0f0f0f) + (bits >> 4 & 0x0f0f0f0f0f0f0f0f);
				bits = (bits & 0x00ff00ff00ff00ff) + (bits >> 8 & 0x00ff00ff00ff00ff);
				bits = (bits & 0x0000ffff0000ffff) + (bits >> 16 & 0x0000ffff0000ffff);
				bits = (bits & 0x00000000ffffffff) + (bits >> 32 & 0x00000000ffffffff);
				return (int)bits;
			}
			return count(a ^ b);
		}

		public static int GetHammingDistance(ulong a, NgData.NgImageData b) {
			if(ulong.TryParse(b.Hash, out var v)) {
				return GetHammingDistance(a, v);
			} else {
				return int.MaxValue;
			}
		}

		/// <summary>PGM形式で出力デバッグ用</summary>
		private static void ExportPgm(string file, byte[] bytes, int width, int height) {
			var sb = new StringBuilder()
				.AppendLine("P2")
				.AppendLine($"{ width } { height }");
			foreach(var b in bytes) {
				sb.Append($"{ b } ");
			}

			var fm = File.Exists(file) ? FileMode.Truncate : FileMode.OpenOrCreate;
			using(FileStream fs = new FileStream(file, fm, FileAccess.Write)) {
				var b = ASCIIEncoding.Default.GetBytes(sb.ToString());
				fs.Write(b, 0, b.Length);
				fs.Flush();
				fs.Close();
			}
		}

		/// <summary>PPM形式で出力デバッグ用</summary>
		private static void ExportPpm(string file, byte[] bytes, int width, int height) {
			var sb = new StringBuilder()
				.AppendLine("P3")
				.AppendLine($"{ width } { height }")
				.AppendLine("255");
			for(int i = 0; i < bytes.Length; i += 4) {
				sb.AppendLine($"{bytes[i + 2]} {bytes[i + 1]} {bytes[i + 0]}");

			}
			var fm = File.Exists(file) ? FileMode.Truncate : FileMode.OpenOrCreate;
			using(FileStream fs = new FileStream(file, fm, FileAccess.Write)) {
				var b = ASCIIEncoding.Default.GetBytes(sb.ToString());
				fs.Write(b, 0, b.Length);
				fs.Flush();
				fs.Close();
			}
		}
	}
}
