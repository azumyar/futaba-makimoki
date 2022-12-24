using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
using Yarukizero.Net.MakiMoki.Util;
using System.Runtime.CompilerServices;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	class FutabaCommentBlock : TextBlock {
		// インスタンスが同じだと更新が発火されないのでラップする
		public class CommentItem {
			public Model.BindableFutabaResItem Value { get; set; }
		}
		private static readonly Helpers.TimerCache<Uri, Uri> uriCache = new Helpers.TimerCache<Uri, Uri>();

		public static readonly DependencyProperty InlineProperty =
			DependencyProperty.Register(
				"Inline",
				typeof(CommentItem),
				typeof(FutabaCommentBlock),
				new PropertyMetadata(default(CommentItem), OnInlinePropertyChanged));
		public static readonly DependencyProperty MaxLinesProperty =
			DependencyProperty.Register(
				nameof(MaxLines),
				typeof(int),
				typeof(FutabaCommentBlock),
				new PropertyMetadata(int.MaxValue, OnMaxLinesPropertyChanged));
		public static RoutedEvent LinkClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(LinkClick),
				RoutingStrategy.Tunnel,
				typeof(PlatformData.HyperLinkEventHandler),
				typeof(FutabaCommentBlock));
		public static RoutedEvent QuotClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(QuotClick),
				RoutingStrategy.Tunnel,
				typeof(PlatformData.QuotClickEventHandler),
				typeof(FutabaCommentBlock));

		public int MaxLines {
			get => (int)this.GetValue(MaxLinesProperty);
			set {
				this.SetValue(MaxLinesProperty, value);
			}
		}

		public event PlatformData.HyperLinkEventHandler LinkClick {
			add { AddHandler(LinkClickEvent, value); }
			remove { RemoveHandler(LinkClickEvent, value); }
		}

		public event PlatformData.QuotClickEventHandler QuotClick {
			add { AddHandler(QuotClickEvent, value); }
			remove { RemoveHandler(QuotClickEvent, value); }
		}

		public static Model.BindableFutabaResItem GetInline(TextBlock element) {
			return (element != null) ? element.GetValue(InlineProperty) as Model.BindableFutabaResItem : null;
		}

		public static void SetInline(TextBlock element, Model.BindableFutabaResItem value) {
			if(element != null) {
				element.SetValue(InlineProperty, value);
			}
		}


		private static void OnInlinePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if((obj is TextBlock tb) && (e.NewValue is CommentItem item)) {
				ParseComment(tb, item.Value, (tb as FutabaCommentBlock)?.MaxLines ?? int.MaxValue);
			}
		}
		private static void OnMaxLinesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if((obj is TextBlock tb) && (e.NewValue is int maxLines)) {
				ParseComment(tb, GetInline(tb), maxLines);
			}
		}

		private static void ParseComment(TextBlock tb, Model.BindableFutabaResItem item, int maxLines) {
			var cmc = Application.Current?.TryFindResource("FutabaCommentColorMap") as PlatformData.ColorMapCollection;
			tb.Text = null;
			tb.Inlines.Clear();

			if(item == null) {
				return;
			}

			var regexOpt = /* RegexOptions.IgnoreCase | */ RegexOptions.Singleline;
			var regex = new Regex[] {
				new Regex(@"^(https?|ftp)(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)", regexOpt),
			}.Concat(Config.ConfigLoader.Uploder.Uploders.Select(x => new Regex(x.File, regexOpt))).ToArray();
			var regexFontStart = new Regex("^<font\\s+color=[\"']#([0-9a-fA-F]+)[\"'][^>]*>", regexOpt);
			var regexFontEnd = new Regex("</font>", regexOpt);
			void EvalEmoji(string text, Color? color, Model.BindableFutabaResItem quotRes = null, ToolTip toolTip = null) {
				var fb = (color.HasValue) ? new SolidColorBrush(color.Value) : null;
				var pos = 0;
				foreach(Match m in Emoji.Wpf.EmojiData.MatchOne.Matches(text)) {
					var run1 = new Run(text.Substring(pos, m.Index - pos)) {
						ToolTip = toolTip,
						Tag = quotRes,
					};
					if(fb != null) {
						// nullを入れると何も描画されなくなるのでnull以外の場合は代入
						run1.Foreground = fb;
					}
					var run2 = new Emoji.Wpf.EmojiInline() {
						Foreground = new SolidColorBrush(Colors.Black),
						Text = text.Substring(m.Index, m.Length),
						FontSize = tb.FontSize,
						ToolTip = toolTip,
						Tag = quotRes,
					};
					if(quotRes != null) {
						run1.Loaded += OnLoadedQuot;
						run2.Loaded += OnLoadedQuot;
					}

					tb.Inlines.Add(run1);
					tb.Inlines.Add(run2);
					pos = m.Index + m.Length;
				}
				var run3 = new Run(text.Substring(pos)) {
					ToolTip = toolTip,
					Tag = quotRes,
				};
				if(fb != null) {
					// nullを入れると何も描画されなくなるのでnull以外の場合は代入
					run3.Foreground = fb;
				}
				if(quotRes != null) {
					run3.Loaded += OnLoadedQuot;
				}
				tb.Inlines.Add(run3);
			}

			void EvalFont(StringBuilder inputVal, StringBuilder outputVal, Model.BindableFutabaResItem quotRes = null, ToolTip toolTip = null) {
				var fm = regexFontStart.Match(inputVal.ToString());
				if(fm.Success) {
					var rgb = fm.Groups[1].Value;
					var color = default(Color?);
					var ns = System.Globalization.NumberStyles.HexNumber;
					var fp = System.Globalization.CultureInfo.InvariantCulture;
					if(rgb.Length == 3) {
						if(int.TryParse(rgb[0].ToString(), ns, fp, out var r)
							&& int.TryParse(rgb[1].ToString(), ns, fp, out var g)
							&& int.TryParse(rgb[2].ToString(), ns, fp, out var b)) {
							color = Color.FromRgb((byte)r, (byte)g, (byte)b);
							color = cmc?.Where(x => x.Target == color).FirstOrDefault()?.Value ?? color;
						}
					} else {
						if(uint.TryParse(rgb, ns, fp, out var v)) {
							color = Color.FromRgb((byte)(v >> 16 & 0xff), (byte)(v >> 8 & 0xff), (byte)(v & 0xff));
							color = cmc?.Where(x => x.Target == color).FirstOrDefault()?.Value ?? color;
						}
					}
					inputVal.Remove(0, fm.Length);
					var fm2 = regexFontEnd.Match(inputVal.ToString());
					if(!fm.Success) {
						// 前提条件として想定していない形式
					} else {
						var text = inputVal.ToString().Substring(0, fm2.Index);
						var t2 = Regex.Replace(text, @"<[^>]*>", "",
							RegexOptions.IgnoreCase | RegexOptions.Multiline);
						var t3 = System.Net.WebUtility.HtmlDecode(t2);
						EvalEmoji(t3, color, quotRes, toolTip);
						inputVal.Remove(0, fm2.Index + fm2.Length);
					}
				}
			}

			void EvalLink(StringBuilder inputVal, StringBuilder outputVal) {
				foreach(var r in regex) {
					var m = r.Match(inputVal.ToString());
					if(m.Success) {
						try {
							var uri = new Uri(ToUrl(m.Value));
							var link = new Hyperlink() {
								TextDecorations = null,
								Foreground = tb.Foreground,
								NavigateUri = uri,
								Tag = item,
							};
							link.Loaded += OnLoadedLink;
							link.Unloaded += OnUnloadedLink;
							link.Inlines.Add(System.Net.WebUtility.HtmlDecode(m.Value));
							if(0 < outputVal.Length) {
								EvalEmoji(outputVal.ToString(), null);
							}
							tb.Inlines.Add(link);
						}
						catch(UriFormatException) {
							// URLが作れなかったのでべた書きする
							if(0 < outputVal.Length) {
								EvalEmoji(outputVal.ToString(), null);
							}
							EvalEmoji(m.Value, null);
						}
						outputVal.Clear();
						inputVal.Remove(0, m.Length);
						goto next;
					}
				}
				outputVal.Append(inputVal[0]);
				inputVal.Remove(0, 1);
			next:;
			}


			var zeroQuot = Array.Empty<Data.FutabaContext.Quot>();
			var disp = item.DisplayHtml.Value ?? "";
			var orig = item.OriginHtml.Value ?? "";
			var headLine = (disp == orig) ? item.HeadLineHtml.Value : "";
			var quotLine = (disp == orig) ? item.Raw.Value.QuotLines : zeroQuot;

			foreach(var m in new (string Msg, Data.FutabaContext.Quot[] Quot)[] { (headLine, zeroQuot), (disp, quotLine) }) {
				if(string.IsNullOrEmpty(m.Msg)) {
					continue;
				}
				var s1 = Regex.Replace(m.Msg, @"<br>", Environment.NewLine,
					RegexOptions.IgnoreCase | RegexOptions.Multiline);
				var lines = s1.Replace("\r", "").Split('\n').Take(maxLines).ToArray();
				var last = lines.LastOrDefault();
				for(var i = 0; i < lines.Length; i++) {
					var s = lines[i];
					var input = new StringBuilder(s);
					var output = new StringBuilder();

				start:
					var fontPos = input.ToString().ToLower().IndexOf("<font");
					if(fontPos < 0) {
						var text = input.ToString();
						var t2 = Regex.Replace(text, @"<[^>]*>", "",
							RegexOptions.IgnoreCase | RegexOptions.Multiline);
						var t3 = System.Net.WebUtility.HtmlDecode(t2);
						input = new StringBuilder(t3);
						while(input.Length != 0) {
							EvalLink(input, output);
						}
					} else if(fontPos == 0) {
						ToolTip toolTip = null;
						Model.BindableFutabaResItem quotRes = null;
						if(i < m.Quot.Length) {
							var q = m.Quot[i];
							if(q.IsQuot) {
								if(q.IsHit) {
									quotRes = item.Parent.Value.ResItems
										.Where(x => x.Raw.Value.ResItem.No == q.ResNo)
										.FirstOrDefault();
								} else {
									/* いったんコメントアウトで対応。要検討
									toolTip = new ToolTip() {
										Background = new SolidColorBrush((Color)App.Current.Resources["MakimokiBackgroundColor"]),
										Content = new TextBlock() {
											Foreground = new SolidColorBrush((Color)App.Current.Resources["MakimokiForegroundColor"]),
											Text = "見つかりませんでした",
											FontFamily = tb.FontFamily,
											FontSize = tb.FontSize,
										}
									};
									*/
								}
							}
						}
						EvalFont(input, output, quotRes, toolTip);
						goto start;
					} else {
						var text = input.ToString().Substring(0, fontPos);
						var t2 = Regex.Replace(text, @"<[^>]*>", "",
							RegexOptions.IgnoreCase | RegexOptions.Multiline);
						var t3 = System.Net.WebUtility.HtmlDecode(t2);
						var input2 = new StringBuilder(t3);
						while(input2.Length != 0) {
							EvalLink(input2, output);
						}
						if(output.Length != 0) {
							EvalEmoji(output.ToString(), null);
							output.Clear();
						}
						input.Remove(0, fontPos);
						EvalFont(input, output);
						goto start;
					}

					if(output.Length != 0) {
						EvalEmoji(output.ToString(), null);
						output.Clear();
					}
					if(!object.ReferenceEquals(s, last)) {
						tb.Inlines.Add(new LineBreak());
					}
				}
			}
		}

		private static string ToUrl(string s) {
			var ul = Config.ConfigLoader.Uploder.Uploders
				.Where(x => Regex.IsMatch(s, x.File, RegexOptions.IgnoreCase | RegexOptions.Singleline))
				.FirstOrDefault();
			if(ul != null) {
				return ul.Root + s;
			}

			return System.Net.WebUtility.HtmlDecode(s);
		}

		private static UIElement GetUIElement(Inline span) {
			FrameworkContentElement cl = span;
			UIElement el = null;
			while(el == null && cl != null && cl.Parent != null) {
				if(cl.Parent is UIElement u) {
					el = u;
					break;
				}
				cl = cl.Parent as FrameworkContentElement;
			}
			return el;
		}

		private static void OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
			try {
				if(e.Source is Hyperlink hl) {
					GetUIElement(hl)?.RaiseEvent(
						new PlatformData.HyperLinkEventArgs(
							LinkClickEvent, e.Source, hl.NavigateUri));
				}
			}
			catch {
				// 特に何もしない
			}
		}

		private static void OnLoadedQuot(object sender, RoutedEventArgs e) {
			if((e.Source is Run run) && (run.Tag is Model.BindableFutabaResItem ri)) {
				run.MouseLeftButtonUp += OnMouseLeftButtonUpQuot;
				run.MouseEnter += OnMouseEnterQuot;
				run.MouseLeave += OnMouseLeaveQuot;
			}
		}
		
		private static void OnMouseLeftButtonUpQuot(object sender, MouseButtonEventArgs e) {
			if((e.Source is Run run) && (run.Tag is Model.BindableFutabaResItem ri)) {
				if(e.ClickCount == 1) {
					Windows.Popups.QuotePopup.Show(
						ri,
						e.Source);
					/* いったんイベントの送出を抑止
					if(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledQuotLink) {
						GetUIElement(run)?.RaiseEvent(
							new PlatformData.QuotClickEventArgs(
								QuotClickEvent, e.Source, ri));
					}
					*/
				}
			}
		}

		private static void OnMouseEnterQuot(object sender, MouseEventArgs e) {
			if((e.Source is Run run) && (run.Tag is Model.BindableFutabaResItem ri)) {
				/* いったんコメントアウトで対応。需要次第で考える。
				if(run.ToolTip == null) {
					run.ToolTip = new ToolTip() {
						Background = new SolidColorBrush((Color)App.Current.Resources["MakimokiBackgroundColor"]),
						Content = new FutabaResBlock() {
							Foreground = new SolidColorBrush((Color)App.Current.Resources["MakimokiForegroundColor"]),
							Background = new SolidColorBrush((Color)App.Current.Resources["ThreadBackgroundColor"]),
							IsHitTestVisible = false,
							DataContext = ri,
						}
					};
				}
				*/

				if(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledQuotLink) {
					run.TextDecorations = System.Windows.TextDecorations.Underline;
				}
			}
		}

		private static void OnMouseLeaveQuot(object sender, MouseEventArgs e) {
			if(e.Source is Run run) {
				run.TextDecorations = null;
			}
		}
		
		private static void OnLoadedLink(object sender, RoutedEventArgs e) {
			static void setLink(Hyperlink targetLink, Uri targetUri = null) {
				targetLink.NavigateUri = targetUri ?? targetLink.NavigateUri;
				targetLink.Foreground = new SolidColorBrush(GetLinkColor());
				targetLink.RequestNavigate += OnRequestNavigate;
				targetLink.MouseEnter += OnMouseEnterLink;
				targetLink.MouseLeave += OnMouseLeaveLink;
			}

			if((e.Source is Hyperlink link) && (link.Tag is Model.BindableFutabaResItem ri)) {
				if(link.NavigateUri.Scheme == SharedConst.MkiMokiSchemeCompleteUrl) {
					if(uriCache.TryGetTarget(link.NavigateUri, out var uri1)) {
						if(uri1.Scheme != SharedConst.MkiMokiSchemeNull) {
							setLink(link, uri1);
						}
					} else {
						IObservable<(bool Successed, string UrlOrMessage, object Raw)> o = null;
						if(link.NavigateUri.Authority == SharedConst.MkiMokiCompleteUrlAuthorityUp) {
							o = Futaba.GetCompleteUrlUp(ri.Parent.Value.Url, link.NavigateUri.AbsolutePath.Substring(1));
						} else {
							Futaba.PutInformation(new Data.Information($"不明なURL{ link.NavigateUri }", ri.Parent.Value));
						}
						o?.Subscribe(x => {
							if(x.Successed) {
								System.Diagnostics.Debug.WriteLine($"{ link.NavigateUri } => { x.UrlOrMessage }");
								var newUri = new Uri(x.UrlOrMessage);
								uriCache.Add(link.NavigateUri, newUri);
								setLink(link, newUri);
							} else {
								if(x.Raw != null) {
									// エラーオブジェクトは返却されているので通信エラーではないと判断する
									uriCache.Add(link.NavigateUri, new Uri($"{ SharedConst.MkiMokiSchemeNull }://"));
								}
								Futaba.PutInformation(new Data.Information($"アップロードファイル補完エラー:{ x.UrlOrMessage }", ri.Parent.Value));
							}
						});
					}
				} else {
					setLink(link);
				}
			}
		}

		private static void OnUnloadedLink(object sender, RoutedEventArgs e) {
			if((e.Source is Hyperlink link) && (link.Tag is Model.BindableFutabaResItem)) {
				link.RequestNavigate -= OnRequestNavigate;
				link.MouseEnter -= OnMouseEnterLink;
				link.MouseLeave -= OnMouseLeaveLink;
			}
		}
		
		private static void OnMouseEnterLink(object sender, MouseEventArgs e) {
			if((sender is Hyperlink link) && (link.Tag is Model.BindableFutabaResItem ri)) {
				link.TextDecorations = System.Windows.TextDecorations.Underline;
				link.Foreground = new SolidColorBrush(GetLinkActiveColor());
				if(link.ToolTip == null) {
					// TODO: 設定ファイルに退避
					var up = new[] { @"f\d+(\.[a-zA-Z]+)$", @"fu\d+(\.[a-zA-Z]+)$" };
					var upExt = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".mp4", ".webm" };
					var url = link.NavigateUri.ToString();
					foreach(var r in up) {
						var m = Regex.Match(url, r, RegexOptions.IgnoreCase);
						if(m.Success && upExt.Contains(m.Groups[1].Value.ToLower())) {
							link.ToolTip = new ThumbToolTip(() => Futaba.GetThumbImageUp(ri.Raw.Value.Url, m.Value));
							goto end;
						}
					}
				end:;
				}
			}
		}

		private static void OnMouseLeaveLink(object sender, MouseEventArgs e) {
			if((sender is Hyperlink link) /* && (link.Parent is TextBlock parent) */) {
				link.TextDecorations = null;
				link.Foreground = new SolidColorBrush(GetLinkColor());
			}
		}

		private static Color GetLinkColor() {
			return App.Current.TryFindResource("ThreadLinkColor") switch {
				Color c => c,
				_ => Colors.Blue
			};
		}

		private static Color GetLinkActiveColor() {
			static Color dark(Color c ) {
				var hsv = WpfUtil.ImageUtil.ToHsv(c);
				return WpfUtil.ImageUtil.HsvToRgb(hsv.H, hsv.S, hsv.V * 0.8);
			}
			return App.Current.TryFindResource("ThreadLinkColor") switch {
				Color c => dark(c),
				_ => Colors.Red
			};
		}
	}
}
