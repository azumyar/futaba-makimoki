using System;
using System.Collections.Generic;
using System.Text;
#if __ANDROID__
using Android;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
#endif
using Yarukizero.Net.MakiMoki.Uno.Views.Extensions;

namespace Yarukizero.Net.MakiMoki.Uno.Views {
	namespace Extensions {
#if __ANDROID__
		static class MatrixExtension {
			public static Matrix Identify(this Matrix @this) {
				@this.Reset();
				return @this;
			}
			public static Matrix Translate(this Matrix @this, float x, float y) {
				@this.PreTranslate(x, y);
				return @this;
			}
			public static Matrix Scale(this Matrix @this, float x, float y) {
				@this.PreScale(x, y);
				return @this;
			}
		}
#endif
	}

	// UnoのImageが遅いので自前で処理する
	partial class UnoImage : Windows.UI.Xaml.Controls.UserControl {
		public static readonly Windows.UI.Xaml.DependencyProperty SourceProperty
			= Windows.UI.Xaml.DependencyProperty.Register(
				nameof(Source),
				typeof(Windows.UI.Xaml.Media.ImageSource),
				typeof(UnoImage),
				new Windows.UI.Xaml.PropertyMetadata(null, OnChangedelectedSource));

		public Windows.UI.Xaml.Media.ImageSource Source {
			get => (Windows.UI.Xaml.Media.ImageSource)this.GetValue(SourceProperty);
			set => this.SetValue(SourceProperty, value);
		}

		private static void OnChangedelectedSource(Windows.UI.Xaml.DependencyObject d, Windows.UI.Xaml.DependencyPropertyChangedEventArgs e) {
#if __ANDROID__
			if(d is View v) {
				v.Invalidate();
			}
#endif
		}

		public UnoImage() : base() {
			this.InitDroid();
		}
		partial void InitDroid();


#if __ANDROID__
		public UnoImage(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
			this.InitDroid();
		}

		partial void InitDroid() {
			this.SetWillNotDraw(false);
		}

		protected override void OnDraw(Canvas? canvas) {
			var bmp = (this.Source as UnoModels.UnoImageSource)?.NativeImage;
			if(bmp is null) {
				base.OnDraw(canvas);
				return;
			}

			var droidView = (View)this; // Width, HeightがnewでUWPになっているのでキャストしてネイティブを使う
			using var m = (
				View: this,
				Bitmap: bmp,
				Sx: ((float)droidView.Width / bmp.Width),
				Sy: ((float)droidView.Height / bmp.Height)) switch {
					var x when(x.Bitmap.Height < x.Bitmap.Width) => new Matrix()
						.Identify()
						.Translate(0f, (droidView.Height - (x.Bitmap.Height * x.Sx)) / 2f)
						.Scale(x.Sx, x.Sx),
					var x => new Matrix()
						.Identify()
						.Translate((droidView.Width - (x.Bitmap.Width * x.Sy)) / 2f, 0f)
						.Scale(x.Sy, x.Sy),
				};
			canvas.DrawBitmap(bmp, m, null);
		}
#endif
	}
}
