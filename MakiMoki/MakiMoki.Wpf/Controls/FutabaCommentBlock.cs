using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	public class FutabaCommentBlock : TextBlock {
		//private static Encoding m_Enc = Encoding.GetEncoding("Shift_JIS");

		public static readonly DependencyProperty ArticleContentProperty =
			DependencyProperty.RegisterAttached(
				"Inline",
				typeof(string),
				typeof(FutabaCommentBlock),
				new PropertyMetadata(null, OnInlinePropertyChanged));
		public static RoutedEvent LinkClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(LinkClick),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventArgs),
				typeof(FutabaCommentBlock));

		public event RoutedEventHandler LinkClick {
			add { AddHandler(LinkClickEvent, value); }
			remove { RemoveHandler(LinkClickEvent, value); }
		}

		public static string GetInline(TextBlock element) {
			return (element != null) ? element.GetValue(ArticleContentProperty) as string : string.Empty;
		}

		public static void SetInline(TextBlock element, string value) {
			if (element != null)
				element.SetValue(ArticleContentProperty, value);
		}

		private static void OnInlinePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			var tb = obj as TextBlock;
			var msg = e.NewValue as string;
			if (tb == null || msg == null) {
				return;
			}

			tb.Text = null;
			tb.Inlines.Clear();

			var s1 = Regex.Replace(msg, @"<br>", Environment.NewLine,
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			//var s2 = Regex.Replace(s1, @"<[^>]*>", "",
			//	RegexOptions.IgnoreCase | RegexOptions.Multiline);
			//var s3 = System.Net.WebUtility.HtmlDecode(s2);
			msg = s1;
			var lines = msg.Replace("\r", "").Split('\n');
			var regexOpt = RegexOptions.IgnoreCase | RegexOptions.Singleline;
			var regex = new Regex[] {
				new Regex(@"^(https?|ftp)(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)", regexOpt),
				new Regex(@"^su[0-9]+\.[a-z]+", regexOpt),
				new Regex(@"^ss[0-9]+\.[a-z]+", regexOpt),
				new Regex(@"^f[0-9]+\.[a-z]+", regexOpt),
				new Regex(@"^fu[0-9]+\.[a-z]+", regexOpt),
			};
			var regexFontStart = new Regex("^<font\\s+color=[\"']#([0-9a-fA-F]+)[\"'][^>]*>", regexOpt);
			var regexFontEnd = new Regex("</font>", regexOpt);
			var last = lines.LastOrDefault();
			foreach (var s in lines) {
				var input = new StringBuilder(s);
				var output = new StringBuilder();

				void EvalFont(StringBuilder inputVal, StringBuilder outputVal) {
					var fm = regexFontStart.Match(inputVal.ToString());
					if (fm.Success) {
						var rgb = fm.Groups[1].Value;
						var color = default(Color?);
						var ns = System.Globalization.NumberStyles.HexNumber;
						var fp = System.Globalization.CultureInfo.InvariantCulture;
						if (rgb.Length == 3) {
							if (int.TryParse(rgb[0].ToString(), ns, fp, out var r)
								&& int.TryParse(rgb[1].ToString(), ns, fp, out var g)
								&& int.TryParse(rgb[2].ToString(), ns, fp, out var b)) {
								color = Color.FromRgb((byte)r, (byte)g, (byte)b);
							}
						} else {
							if (uint.TryParse(rgb, ns, fp, out var i)) {
								color = Color.FromRgb((byte)(i >> 16 & 0xff), (byte)(i >> 8 & 0xff), (byte)(i & 0xff));
							}
						}
						inputVal.Remove(0, fm.Length);
						var fm2 = regexFontEnd.Match(inputVal.ToString());
						if (!fm.Success) {
							// 前提条件として想定していない形式
						} else {
							var text = inputVal.ToString().Substring(0, fm2.Index);
							var t2 = Regex.Replace(text, @"<[^>]*>", "",
								RegexOptions.IgnoreCase | RegexOptions.Multiline);
							var t3 = System.Net.WebUtility.HtmlDecode(t2);
							tb.Inlines.Add(new Run(t3) {
								Foreground = (color.HasValue) ? new SolidColorBrush(color.Value) : null,
							});
							inputVal.Remove(0, fm2.Index + fm2.Length);
						}
					}
				}

				void EvalLink(StringBuilder inputVal, StringBuilder outputVal) {
					foreach (var r in regex) {
						var m = r.Match(inputVal.ToString());
						if (m.Success) {
							var link = new Hyperlink() {
								TextDecorations = null,
								Foreground = new SolidColorBrush(Colors.Blue),
								NavigateUri = new Uri(ToUrl(m.Value)),
							};
							link.RequestNavigate += new RequestNavigateEventHandler(RequestNavigate);
							link.MouseEnter += new MouseEventHandler(link_MouseEnter);
							link.MouseLeave += new MouseEventHandler(link_MouseLeave);
							link.Inlines.Add(m.Value);
							tb.Inlines.Add(link);
							tb.Inlines.Add(new Run(outputVal.ToString()));
							outputVal.Clear();
							inputVal.Remove(0, m.Length);
							goto next;
						}
					}
					outputVal.Append(inputVal[0]);
					inputVal.Remove(0, 1);
				next:;
				}
			start:
				var fontPos = input.ToString().ToLower().IndexOf("<font");
				if (fontPos < 0) {
					var text = input.ToString();
					var t2 = Regex.Replace(text, @"<[^>]*>", "",
						RegexOptions.IgnoreCase | RegexOptions.Multiline);
					var t3 = System.Net.WebUtility.HtmlDecode(t2);
					input = new StringBuilder(t3);
					while (input.Length != 0) {
						EvalLink(input, output);
					}
				} else if (fontPos == 0) {
					EvalFont(input, output);
					goto start;
				} else {
					var text = input.ToString().Substring(0, fontPos);
					var t2 = Regex.Replace(text, @"<[^>]*>", "",
						RegexOptions.IgnoreCase | RegexOptions.Multiline);
					var t3 = System.Net.WebUtility.HtmlDecode(t2);
					input = new StringBuilder(t3);
					while (input.Length != 0) {
						EvalLink(input, output);
					}
					if (output.Length != 0) {
						tb.Inlines.Add(new Run(output.ToString()));
						output.Clear();
					}
					EvalFont(input, output);
					goto start;
				}

				if (output.Length != 0) {
					tb.Inlines.Add(new Run(output.ToString()));
					output.Clear();
				}
				if (!object.ReferenceEquals(lines, last)) {
					tb.Inlines.Add(new LineBreak());
				}
			}
		}

		private static string ToUrl(string s) {
			// TODO: 設定に定義
			if (s.StartsWith("su")) {
				return "http://www.nijibox5.com/futabafiles/tubu/src/" + s;
			} else if (s.StartsWith("ss")) {
				return "http://www.nijibox5.com/futabafiles/kobin/src/" + s;
			} else if (s.StartsWith("fu")) {
				return "https://dec.2chan.net/up2/src/" + s;
			} else if (s.StartsWith("f")) {
				return "https://dec.2chan.net/up/src/" + s;
			}
			return s;
		}

		private static void RequestNavigate(object sender, RequestNavigateEventArgs e) {
			try {
				if(e.Source is Hyperlink hl) {
					FrameworkContentElement cl = hl;
					UIElement el = null;
					while(el == null && cl != null && cl.Parent != null) {
						if (cl.Parent is UIElement u) {
							el = u;
							break;
						}
						cl = cl.Parent as FrameworkContentElement;
					}
					el?.RaiseEvent(new RoutedEventArgs(LinkClickEvent, e.Source));
				}
			}
			catch {
				// 特に何もしない
			}
		}

		private static void link_MouseEnter(object sender, MouseEventArgs e) {
			var link = sender as Hyperlink;
			if (link == null)
				return;

			// リンクにカーソルを当てたときは文字色を赤くする
			link.Foreground = Brushes.Red;
		}

		private static void link_MouseLeave(object sender, MouseEventArgs e) {
			var link = sender as Hyperlink;
			var parent = link.Parent as TextBlock;
			if (link == null || parent == null)
				return;

			// リンクからカーソルが離れたときは文字色をデフォルトカラーに戻す
			link.Foreground = new SolidColorBrush(Colors.Blue);
		}
	}
}
