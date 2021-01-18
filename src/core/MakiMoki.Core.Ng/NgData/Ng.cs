using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Ng.NgData {
	public class NgConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020102900;

		[JsonProperty("ng-catalog-id", Required = Required.DisallowNull)]
		public bool EnableCatalogIdNg { get; internal set; }

		[JsonProperty("ng-thread-id", Required = Required.DisallowNull)]
		public bool EnableThreadIdNg { get; internal set; }


		[JsonProperty("ng-catalog-words", Required = Required.DisallowNull)]
		public string[] CatalogWords { get; internal set; }

		[JsonProperty("ng-catalog-regex", Required = Required.DisallowNull)]
		public string[] CatalogRegex { get; internal set; }

		[JsonProperty("ng-thread-words", Required = Required.DisallowNull)]
		public string[] ThreadWords { get; internal set; }

		[JsonProperty("ng-thread-regex", Required = Required.DisallowNull)]
		public string[] ThreadRegex { get; internal set; }

		internal static NgConfig CreateDefault() {
			return new NgConfig() {
				Version = CurrentVersion,
				EnableCatalogIdNg = false,
				EnableThreadIdNg = false,
				CatalogWords = Array.Empty<string>(),
				CatalogRegex = Array.Empty<string>(),
				ThreadWords = Array.Empty<string>(),
				ThreadRegex = Array.Empty<string>(),
			};
		}
		
		internal static NgConfig Create(
			bool enableCatalogIdNg,
			bool enableThreadIdNg,
			string[] catalogWords,
			string[] catalogRegex,
			string[] threadWords,
			string[] threadRegex) {

			return new NgConfig() {
				Version = CurrentVersion,
				EnableCatalogIdNg = enableCatalogIdNg,
				EnableThreadIdNg = enableThreadIdNg,
				CatalogWords = catalogWords,
				CatalogRegex = catalogRegex,
				ThreadWords = threadWords,
				ThreadRegex = threadRegex,
			};
		}
	}

	public class NgImageConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;
		private static readonly int DefaultThreshold = 10;

		[JsonProperty("ng-hamming-threshold-value", Required = Required.DisallowNull)]
		public int Threshold { get; internal set; }

		[JsonProperty("ng-images", Required = Required.DisallowNull)]
		public NgImageData[] Images { get; internal set; }

		[JsonProperty("ng-method", Required = Required.DisallowNull)]
		public ImageNgMethod NgMethod { get; internal set; }

		internal static NgImageConfig CreateDefault() {
			return new NgImageConfig() {
				Version = CurrentVersion,
				Threshold = DefaultThreshold,
				Images = Array.Empty<NgImageData>(),
				NgMethod = ImageNgMethod.DummyImage,
			};
		}
	}

	public class WatchConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = -1;


		[JsonProperty("watch-catalog-words", Required = Required.DisallowNull)]
		public string[] CatalogWords { get; internal set; }

		[JsonProperty("watch-catalog-regex", Required = Required.DisallowNull)]
		public string[] CatalogRegex { get; internal set; }

		internal static WatchConfig CreateDefault() {
			return new WatchConfig() {
				Version = CurrentVersion,
				CatalogWords = Array.Empty<string>(),
				CatalogRegex = Array.Empty<string>(),
			};
		}

		internal static WatchConfig Create(
			string[] catalogWords,
			string[] catalogRegex) {

			return new WatchConfig() {
				Version = CurrentVersion,
				CatalogWords = catalogWords,
				CatalogRegex = catalogRegex,
			};
		}
	}

	public class WatchImageConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = -1;
		private static readonly int DefaultThreshold = 10;

		[JsonProperty("watch-hamming-threshold-value", Required = Required.DisallowNull)]
		public int Threshold { get; internal set; }

		[JsonProperty("watch-images", Required = Required.DisallowNull)]
		public NgImageData[] Images { get; internal set; }

		internal static WatchImageConfig CreateDefault() {
			return new WatchImageConfig() {
				Version = CurrentVersion,
				Threshold = DefaultThreshold,
				Images = Array.Empty<NgImageData>(),
			};
		}
	}

	public enum ImageNgMethod {
		Hidden,
		DummyImage,
	}

	public class NgImageData : Data.JsonObject {
		/* 元ファイル情報いるかな…
		[JsonProperty("md5", Required = Required.DisallowNull)]
		public string Md5 { get; private set; }

		// NULL可
		[JsonProperty("file", Required = Required.AllowNull)]
		public string RawFile { get; private set; }
		*/

		[JsonProperty("hash", Required = Required.DisallowNull)]
		public string Hash { get; private set; }

		[JsonProperty("algorithm", Required = Required.DisallowNull)]
		public NgHashAlgorithm HashAlgorithm { get; private set; }

		[JsonProperty("comment", Required = Required.DisallowNull)]
		public string Comment { get; private set; }

		public static NgImageData FromPerceptualHash(ulong hash, string commnet) {
			return new NgImageData() {
				HashAlgorithm = NgHashAlgorithm.PerceptualHash,
				Hash = hash.ToString(),
				Comment = commnet,
			};
		}
	}

	public enum NgHashAlgorithm {
		/// <summary>pHash</summary>
		PerceptualHash,
	}

	public class HiddenConfig : Data.ConfigObject {
		public static int CurrentVersion { get; } = 2020062900;
		private static int DefaultExpireDay = 30;

		[JsonProperty("expire-day", Required = Required.DisallowNull)]
		public int ExpireDay { get; internal set; }

		[JsonProperty("res", Required = Required.DisallowNull)]
		public HiddenData[] Res { get; internal set; }

		internal static HiddenConfig CreateDefault() {
			return new HiddenConfig() {
				Version = CurrentVersion,
				ExpireDay = DefaultExpireDay,
				Res = new HiddenData[0],
			};
		}
	}

	public class HiddenData : Data.JsonObject {
		[JsonProperty("url", Required = Required.DisallowNull)]
		public string BaseUrl { get; private set; }
		[JsonProperty("res", Required = Required.DisallowNull)]
		public Data.NumberedResItem Res { get; private set; }

		public static HiddenData FromResItem(string url, Data.NumberedResItem res) {
			return new HiddenData() {
				BaseUrl = url,
				Res = res,
			};
		}

		public override bool Equals(object obj) {
			if(obj is HiddenData hd) {
				return (this.BaseUrl == hd.BaseUrl) && (this.Res.No == hd.Res.No);
			}
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return (this.BaseUrl.ToString() + this.Res.ToString()).GetHashCode();
		}

		public static bool operator ==(HiddenData a, HiddenData b) { return a?.Equals(b) ?? false; }
		public static bool operator !=(HiddenData a, HiddenData b) { return !a?.Equals(b) ?? false; }
	}
}