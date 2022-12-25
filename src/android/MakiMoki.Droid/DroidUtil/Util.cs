using Android.Content;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Droid.DroidUtil {
	internal static class Util {
		public static int Dp2Px(float dp, Context context) => (int)(dp * context.Resources.DisplayMetrics.Density);
		
		public static float Px2Dp(int px, Context context) => px / context.Resources.DisplayMetrics.Density;
	}
}
