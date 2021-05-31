using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Windows.UI.Xaml.Media;
using Yarukizero.Net.MakiMoki.Data;

namespace Yarukizero.Net.MakiMoki.Uno.Models {
	class PostHolder : Yarukizero.Net.MakiMoki.Shared.Bindings.BindingData.PostHolder {
		public ReactiveProperty<string> ImageName { get; }
		public ReactiveProperty<ImageSource> ImagePreview { get; }

		public PostHolder() : base() {
			this.ImageName = this.ImagePath.Select(x => {
				if(string.IsNullOrWhiteSpace(x)) {
					return "";
				} else {
					return Path.GetFileName(x);
				}
			}).ToReactiveProperty("");
			this.ImagePreview = this.ImagePath.Select<string, ImageSource>(x => {
				if(File.Exists(x)) {
					var ext = Path.GetExtension(x).ToLower();
					var imageExt = Config.ConfigLoader.MimeFutaba.Types
						.Where(y => y.MimeContents == MimeContents.Image)
						.Select(y => y.Ext)
						.ToArray();
					var movieExt = Config.ConfigLoader.MimeFutaba.Types
						.Where(y => y.MimeContents == MimeContents.Video)
						.Select(y => y.Ext)
						.ToArray();
					if(imageExt.Contains(ext)) {
						//return WpfUtil.ImageUtil.LoadImage(x);
					} else if(movieExt.Contains(ext)) {
						// 動画は今は何もしない
						// TODO: なんんか実装する
					}
				}
				return null;
			}).ToReactiveProperty();
		}
	}
}
