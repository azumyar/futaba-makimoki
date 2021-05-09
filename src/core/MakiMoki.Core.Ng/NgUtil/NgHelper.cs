using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yarukizero.Net.MakiMoki.Ng.NgConfig;

namespace Yarukizero.Net.MakiMoki.Ng.NgUtil {
	public static partial class NgHelper {
		
		private static bool CheckNg(Data.FutabaContext futaba, Data.FutabaContext.Item item, bool idNg, string[] word, string[] regex) {
			bool CheckId(Data.FutabaContext.Item it) {
				var m = it?.ResItem.Res.Email.ToLower() ?? "";
				return (m != "id表示") && (m != "ip表示");
			}
			var id = idNg;

			// ID表示スレはID NG機能は無効化
			if(id && futaba.Url.IsCatalogUrl) {
				id = CheckId(item);
			} else if(id && futaba.Url.IsThreadUrl) {
				id = CheckId(futaba.ResItems.FirstOrDefault());
			}

			if(id && !string.IsNullOrEmpty(item.ResItem.Res.Id)) {
				return true;
			}

			var com = Util.TextUtil.RowComment2Text(item.ResItem.Res.Com);
			if(word.Where(x => com.Contains(x)).Any()) {
				return true;
			}

			if(regex.Where(x => Regex.IsMatch(com, x, RegexOptions.IgnoreCase | RegexOptions.Multiline)).Any()) {
				return true;
			}

			return false;
		}

		public static bool CheckHidden(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			return NgConfig.NgConfigLoader.HiddenConfig.Res
				.Where(x => (x.BaseUrl == futaba.Url.BaseUrl) && (x.Res.No == item.ResItem.No))
				.FirstOrDefault() != null;
		}


		public static bool CheckCatalogNg(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			return CheckNg(futaba, item,
				NgConfig.NgConfigLoader.NgConfig.EnableCatalogIdNg,
				NgConfig.NgConfigLoader.NgConfig.CatalogWords,
				NgConfig.NgConfigLoader.NgConfig.CatalogRegex);
		}

		public static bool CheckThreadNg(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			return CheckNg(futaba, item,
				NgConfig.NgConfigLoader.NgConfig.EnableThreadIdNg,
				NgConfig.NgConfigLoader.NgConfig.ThreadWords,
				NgConfig.NgConfigLoader.NgConfig.ThreadRegex);
		}

		public static bool CheckCatalogWatch(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			if(futaba.Url.IsCatalogUrl) {
				var com = Util.TextUtil.RowComment2Text(item.ResItem.Res.Com);
				if(NgConfig.NgConfigLoader.WatchConfig.CatalogWords.Where(x => com.Contains(x)).Any()) {
					return true;
				}

				if(NgConfig.NgConfigLoader.WatchConfig.CatalogRegex.Where(x => Regex.IsMatch(com, x, RegexOptions.IgnoreCase | RegexOptions.Multiline)).Any()) {
					return true;
				}
			}
			return false;
		}

		public static bool CheckImageNg(ulong hash, int? threshold = null) {
			var tv = threshold ?? NgConfigLoader.NgImageConfig.Threshold;
			return NgConfigLoader.NgImageConfig.Images.Any(x => PerceptualHash.GetHammingDistance(hash, x) <= tv);
		}


		public static bool CheckImageWatch(ulong hash, int? threshold = null) {
			var tv = threshold ?? NgConfigLoader.WatchImageConfig.Threshold;
			return NgConfigLoader.WatchImageConfig.Images.Any(x => PerceptualHash.GetHammingDistance(hash, x) <= tv);
		}

		public static bool IsEnabledNgImage() {
			return NgConfigLoader.NgImageConfig.Images.Any()
				|| NgConfigLoader.WatchImageConfig.Images.Any();
		}
	}
}
