using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.PlatformData {
	public class FutabaMedia {
		public bool IsExternalUrl { get; private set; }
		public Data.UrlContext BaseUrl { get; private set; }
		public Data.ResItem Res { get; private set; }

		public string ExternalUrl { get; private set; }

		private FutabaMedia() { }

		public static FutabaMedia FromFutabaUrl(Data.UrlContext baseUrl, Data.ResItem res) {
			return new FutabaMedia() {
				IsExternalUrl = false,
				BaseUrl = baseUrl,
				Res = res,
			};
		}

		public static FutabaMedia FromExternalUrl(string url) {
			return new FutabaMedia() {
				IsExternalUrl = true,
				ExternalUrl = url,
			};
		}
	}

	public class MediaQuickSaveItem : IDisposable {

		public ReactiveProperty<bool> IsEnabled { get; }
		public ReactiveProperty<string> Name { get; }
		public ReactiveProperty<string> Path { get; }

		public ReactiveProperty<PlatformData.FutabaMedia> Media { get; }

		public MediaQuickSaveItem() {
			IsEnabled = new ReactiveProperty<bool>(false);
			Name = new ReactiveProperty<string>("なし");
		}

		public MediaQuickSaveItem(PlatformData.FutabaMedia media, string path) {
			var b = Directory.Exists(path);
			IsEnabled = new ReactiveProperty<bool>(b);
			Name = new ReactiveProperty<string>(b ? a(path) : $"[存在しません]{ a(path) }");
			Path = new ReactiveProperty<string>(path);
			Media = new ReactiveProperty<FutabaMedia>(media);
		}

		// TODO: なまえ考える
		private string a(string path) {
			var max = 32;
			if(path.Length <= max) {
				return path;
			}
			var a = path.Substring(0, 3);
			var b = path.Substring(path.Length - (max - 6));
			return $"{ a }...{ b }";
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}
	}
}
