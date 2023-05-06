using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.Activity.Result.Contract;
using AndroidX.Activity.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Droid.Extensions;
using AndroidX.Fragment.App;
using AndroidX.Core.App;
using AndroidX.AppCompat.App;
using static AndroidX.Core.Text.PrecomputedTextCompat;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;
using System.Text.RegularExpressions;
using static Android.Provider.ContactsContract.CommonDataKinds;
using Android.OS;
using Android.Provider;

namespace Yarukizero.Net.MakiMoki.Droid.DroidUtil {
	internal static class Util {
		private class Java2CsObject<T> : Java.Lang.Object {
			public T? Value { get; }

			public Java2CsObject(T value) : base() {
				this.Value = value;
			}
			protected Java2CsObject(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
		}

		public interface IActivityResultLauncher {
			void Launch();
			void Unregister();
		}

		public interface IActivityResultLauncher<TParam> : IActivityResultLauncher {
			void Launch(TParam value);
		}

		private class ActivityResultLauncherImpl<TParam> : IActivityResultLauncher<TParam> {
			private ActivityResultLauncher launcher;

			public ActivityResultLauncherImpl(ActivityResultLauncher launcher) {
				this.launcher = launcher;
			}

			public void Launch() => this.launcher.Launch(default);
			public void Launch(TParam value) => this.launcher.Launch(new Java2CsObject<TParam>(value));
			public void Unregister() => this.launcher.Unregister();
		}


		private class Contract<TParam, TResult> : ActivityResultContract {
			private readonly Func<Android.Content.Context, TParam?, Android.Content.Intent> creater;
			private readonly Func<int, Android.Content.Intent, TResult> parser;

			public Contract(
				Func<Android.Content.Context, TParam?, Android.Content.Intent> creater,
				Func<int, Android.Content.Intent, TResult?> parser)
				: base() {

				this.creater = creater;
				this.parser = parser;
			}
			protected Contract(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override Android.Content.Intent CreateIntent(Android.Content.Context context, Java.Lang.Object? input) {
				if(input == null) {
					return this.creater(context, default);
				} else if(input is Java2CsObject<TParam> p) {
					return this.creater(context, p.Value);
				}
				throw new ArgumentException();
			}

			public override Java.Lang.Object? ParseResult(int resultCode, Android.Content.Intent? intent)
				=> new Java2CsObject<TResult>(this.parser(resultCode, intent));
		}

		private class ActivityResultCallback<TResult> : Java.Lang.Object, IActivityResultCallback {
			private readonly Action<TResult> onResult;

			public ActivityResultCallback(Action<TResult> onResult) : base() {
				this.onResult = onResult;
			}
			protected ActivityResultCallback(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public void OnActivityResult(Java.Lang.Object? p) {
				if(p is Java2CsObject<TResult> o) {
					this.onResult.Invoke(o.Value);
				}
			}
		}

		public static int Dp2Px(float dp, Context context) => (int)(dp * context.Resources.DisplayMetrics.Density);
		
		public static float Px2Dp(int px, Context context) => px / context.Resources.DisplayMetrics.Density;

		public static IActivityResultLauncher RegisterForActivityResult<TResult>(
			AppCompatActivity activity,
			Func<Android.Content.Context, object?, Android.Content.Intent> creater,
			Func<int, Android.Content.Intent, TResult?> parser,
			Action<TResult> onResult) => RegisterForActivityResult<object, TResult>(activity, creater, parser, onResult);

		public static IActivityResultLauncher<TParam> RegisterForActivityResult<TParam, TResult>(
			AppCompatActivity activity,
			Func<Android.Content.Context, TParam?, Android.Content.Intent> creater,
			Func<int, Android.Content.Intent, TResult?> parser,
			Action<TResult> onResult) 
				=> new ActivityResultLauncherImpl<TParam>(
						activity.RegisterForActivityResult(
							new Contract<TParam, TResult>(creater, parser),
							new ActivityResultCallback<TResult>(onResult)));

		public static IActivityResultLauncher RegisterForActivityResult<TResult>(
			Fragment fragment,
			Func<Android.Content.Context, object?, Android.Content.Intent> creater,
			Func<int, Android.Content.Intent, TResult?> parser,
			Action<TResult> onResult) => RegisterForActivityResult<object, TResult>(fragment, creater, parser, onResult);

		public static IActivityResultLauncher<TParam> RegisterForActivityResult<TParam, TResult>(
			Fragment fragment,
			Func<Android.Content.Context, TParam?, Android.Content.Intent> creater,
			Func<int, Android.Content.Intent, TResult?> parser,
			Action<TResult> onResult)
				=> new ActivityResultLauncherImpl<TParam>(
						fragment.RegisterForActivityResult(
							new Contract<TParam, TResult>(creater, parser),
							new ActivityResultCallback<TResult>(onResult)));

		private class __URLSpan : URLSpan {
			public __URLSpan(Parcel src) : base(src) { }
			public __URLSpan(string? url) : base(url) { }
			protected __URLSpan(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

			public override void UpdateDrawState(TextPaint ds) {
				base.UpdateDrawState(ds);
				ds.UnderlineText = false;
			}
		}

		public static SpannableStringBuilder ParseFutabaComment(Data.FutabaContext.Item item) {
			var regexOpt = /* RegexOptions.IgnoreCase | */ RegexOptions.Singleline;
			var regex = new Regex[] {
				new Regex(@"^(https?|ftp)(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)", regexOpt),
			}.Concat(Config.ConfigLoader.Uploder.Uploders.Select(x => new Regex(x.File, regexOpt))).ToArray();

			var com = MakiMoki.Util.TextUtil.RowComment2Text(item.ResItem.Res.Com.Replace("\r\n", "\n"));
			var r = new SpannableStringBuilder(com);
			var c = 0;
			foreach(var line in com.Split('\n').Select((x, i) => (Value: x, Index: i))) {
				if(line.Value.FirstOrDefault() == '>') {
					r.SetSpan(new __URLSpan($"x-makimoki-android://q?line={line.Index}"), c, c + line.Value.Length, SpanTypes.ExclusiveExclusive);
					r.SetSpan(new ForegroundColorSpan(Color.DarkGreen), c, c + line.Value.Length, SpanTypes.ExclusiveExclusive);
				} else {
					for(var i=0; i<line.Value.Length; i++) {
						foreach(var reg in regex) {
							var m = reg.Match(line.Value, i);
							if(m.Success) {
								var ul = Config.ConfigLoader.Uploder.Uploders
									.Where(x => Regex.IsMatch(m.Value, x.File, RegexOptions.IgnoreCase | RegexOptions.Singleline))
									.FirstOrDefault() switch {
										Data.UploderData ud => $"{ud.Root}{m.Value}",
										_ => m.Value
									};
								r.SetSpan(new __URLSpan(ul), c + i, c + i + m.Value.Length, SpanTypes.ExclusiveExclusive);
								i += m.Value.Length;
								break;
							}
						}
					}
				}
				c += line.Value.Length;
			}
			return r;
		}


		public static string? Uri2Path(Android.Content.Context context, Android.Net.Uri uri) {
			var c = context.ContentResolver.Query(uri, new[] { MediaStore.IMediaColumns.DisplayName }, null, null, null);
			try {
				if(c?.MoveToFirst() ?? false) {
					return c.GetString(c.GetColumnIndex(MediaStore.IMediaColumns.DisplayName));
				}
				return default;
			}
			finally {
				c?.Close();
			}
		}
	}
}
