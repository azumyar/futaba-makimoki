using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	public class ImageObject : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public BitmapSource Image { get; }
		public Timeline AnimationSource { get; }

		public ImageObject(BitmapSource image, Timeline animation = null) {
			this.Image = image;
			this.AnimationSource = animation;
		}
	}
}
