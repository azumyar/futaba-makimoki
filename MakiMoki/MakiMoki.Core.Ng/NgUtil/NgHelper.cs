using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yarukizero.Net.MakiMoki.Ng.NgConfig;

namespace Yarukizero.Net.MakiMoki.Ng.NgUtil {
	public static partial class NgHelper {
		
		private static bool CheckNg(Data.FutabaContext futaba, Data.FutabaContext.Item item, bool idNg, string[] word, string[] regex) {
			var id = idNg;

			// ID表示スレはID NG機能は無効化
			if(id) {
				var first = futaba.ResItems.FirstOrDefault();
				if(first != null) {
					if((first.ResItem.Res.Email == "id表示") || (first.ResItem.Res.Email == "ip表示")) {
						id = false;
					}
				}
			}

			if(id && !string.IsNullOrEmpty(item.ResItem.Res.Id)) {
				return true;
			}

			var com = Util.TextUtil.RowComment2Text(item.ResItem.Res.Com);
			if(word.Where(x => com.Contains(x)).Count() != 0) {
				return true;
			}

			if(regex.Where(x => Regex.IsMatch(com, x, RegexOptions.IgnoreCase | RegexOptions.Multiline)).Count() != 0) {
				return true;
			}

			return false;
		}

		public static bool CheckHidden(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			return NgConfig.NgConfigLoder.HiddenConfig.Res
				.Where(x => (x.BaseUrl == futaba.Url.BaseUrl) && (x.Res.No == item.ResItem.No))
				.FirstOrDefault() != null;
		}


		public static bool CheckCatalogNg(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			return CheckNg(futaba, item,
				NgConfig.NgConfigLoder.NgConfig.EnableIdNg,
				NgConfig.NgConfigLoder.NgConfig.CatalogWords,
				NgConfig.NgConfigLoder.NgConfig.CatalogRegex);
		}

		public static bool CheckThreadNg(Data.FutabaContext futaba, Data.FutabaContext.Item item) {
			return CheckNg(futaba, item,
				NgConfig.NgConfigLoder.NgConfig.EnableIdNg,
				NgConfig.NgConfigLoder.NgConfig.ThreadWords,
				NgConfig.NgConfigLoder.NgConfig.ThreadRegex);
		}

		public static bool IsEnabledNgImage() {
			return NgConfigLoder.NgImageConfig.Images.Length != 0;
		}
	}
}
