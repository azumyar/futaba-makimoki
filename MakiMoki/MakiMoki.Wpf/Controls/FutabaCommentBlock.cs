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
using Yarukizero.Net.MakiMoki.Util;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	class FutabaCommentBlock : TextBlock {
		private static readonly Helpers.TimerCache<Uri, Uri> uriCache = new Helpers.TimerCache<Uri, Uri>();

		public static readonly DependencyProperty ArticleContentProperty =
			DependencyProperty.Register(
				"Inline",
				typeof(Model.BindableFutabaResItem),
				typeof(FutabaCommentBlock),
				new PropertyMetadata(null, OnInlinePropertyChanged));
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
			return (element != null) ? element.GetValue(ArticleContentProperty) as Model.BindableFutabaResItem : null;
		}

		public static void SetInline(TextBlock element, Model.BindableFutabaResItem value) {
			if(element != null) {
				element.SetValue(ArticleContentProperty, value);
			}
		}


		private static void OnInlinePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if((obj is TextBlock tb) && (e.NewValue is Model.BindableFutabaResItem item)) {
				a(tb, item, (tb as FutabaCommentBlock)?.MaxLines ?? int.MaxValue);
				// メモリリークする気がする
				item?.DisplayHtml.Subscribe(x => a(
					tb,
					GetInline(tb),
					(tb as FutabaCommentBlock)?.MaxLines ?? int.MaxValue));
			}
		}
		private static void OnMaxLinesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if((obj is TextBlock tb) && (e.NewValue is int maxLines)) {
				a(tb, GetInline(tb), maxLines);
			}
		}

		private static void a(TextBlock tb, Model.BindableFutabaResItem item, int maxLines) {
			tb.Text = null;
			tb.Inlines.Clear();

			if(item == null) {
				return;
			}

			var msg = item.DisplayHtml.Value ?? "";
			var s1 = Regex.Replace(msg, @"<br>", Environment.NewLine,
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			//var s2 = Regex.Replace(s1, @"<[^>]*>", "",
			//	RegexOptions.IgnoreCase | RegexOptions.Multiline);
			//var s3 = System.Net.WebUtility.HtmlDecode(s2);
			msg = s1;
			var lines = msg.Replace("\r", "").Split('\n').Take(maxLines).ToArray();
			var regexOpt = /* RegexOptions.IgnoreCase | */ RegexOptions.Singleline;
			var regex = new Regex[] {
				new Regex(@"^(https?|ftp)(:\/\/[-_.!~*\'()a-zA-Z0-9;\/?:\@&=+\$,%#]+)", regexOpt),
			}.Concat(Config.ConfigLoader.Uploder.Uploders.Select(x => new Regex(x.File, regexOpt))).ToArray();
			var regexFontStart = new Regex("^<font\\s+color=[\"']#([0-9a-fA-F]+)[\"'][^>]*>", regexOpt);
			var regexFontEnd = new Regex("</font>", regexOpt);
			var last = lines.LastOrDefault();
			for(var i = 0; i < lines.Length; i++) {
				var s = lines[i];
				var input = new StringBuilder(s);
				var output = new StringBuilder();

				void EvalEmoji(string text, Color? color, Model.BindableFutabaResItem quotRes = null, ToolTip toolTip = null) {
					var fb = (color.HasValue) ? new SolidColorBrush(color.Value) : tb.Foreground;
					var pos = 0;
					foreach(Match m in Emoji.Wpf.EmojiData.MatchMultiple.Matches(text)) {
						var run1 = new Run(text.Substring(pos, m.Index - pos)) {
							Foreground = fb,
							ToolTip = toolTip,
							Tag = quotRes,
						};
						var run2 = new Emoji.Wpf.EmojiInline() {
							FallbackBrush = tb.Foreground,
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
						Foreground = fb,
						ToolTip = toolTip,
						Tag = quotRes,
					};
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
							}
						} else {
							if(uint.TryParse(rgb, ns, fp, out var v)) {
								color = Color.FromRgb((byte)(v >> 16 & 0xff), (byte)(v >> 8 & 0xff), (byte)(v & 0xff));
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
					if(i < item.Raw.Value.QuotLines.Length) {
						var q = item.Raw.Value.QuotLines[i];
						if(q.IsQuot) {
							if(q.IsHit) {
								quotRes = item.Parent.Value.ResItems
									.Where(x => x.Raw.Value.ResItem.No == q.ResNo)
									.FirstOrDefault();
							} else {
								toolTip = new ToolTip() {
									Content = new TextBlock() {
										Text = "見つかりませんでした",
										FontFamily = tb.FontFamily,
										FontSize = tb.FontSize,
									}
								};
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
					if(WpfConfig.WpfConfigLoader.SystemConfig.IsEnabledQuotLink) {
						GetUIElement(run)?.RaiseEvent(
							new PlatformData.QuotClickEventArgs(
								QuotClickEvent, e.Source, ri));
					}
				}
			}
		}

		private static void OnMouseEnterQuot(object sender, MouseEventArgs e) {
			if((e.Source is Run run) && (run.Tag is Model.BindableFutabaResItem ri)) {
				if(run.ToolTip == null) {
					run.ToolTip = new ToolTip() {
						Content = new FutabaResBlock() {
							IsHitTestVisible = false,
							DataContext = ri,
						}
					};
				}

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
			void setLink(Hyperlink targetLink, Uri targetUri = null) {
				targetLink.NavigateUri = targetUri ?? targetLink.NavigateUri;
				targetLink.Foreground = new SolidColorBrush(GetLinkColor());
				targetLink.RequestNavigate += OnRequestNavigate;
				targetLink.MouseEnter += OnMouseEnterLink;
				targetLink.MouseLeave += OnMouseLeaveLink;
			}

			if((e.Source is Hyperlink link) && (link.Tag is Model.BindableFutabaResItem ri)) {
				if(link.NavigateUri.Scheme == SharadConst.MkiMokiSchemeCompleteUrl) {
					if(uriCache.TryGetTarget(link.NavigateUri, out var uri1)) {
						if(uri1.Scheme != SharadConst.MkiMokiSchemeNull) {
							setLink(link, uri1);
						}
					} else {
						IObservable<(bool Successed, string UrlOrMessage, object Raw)> o = null;
						if(link.NavigateUri.Authority == SharadConst.MkiMokiCompleteUrlAuthorityUp) {
							o = Futaba.GetCompleteUrlUp(ri.Parent.Value.Url, link.NavigateUri.AbsolutePath.Substring(1));
						} else if(link.NavigateUri.Authority == SharadConst.MkiMokiCompleteUrlAuthorityShiokara) {
							o = Futaba.GetCompleteUrlShiokara(ri.Parent.Value.Url, link.NavigateUri.AbsolutePath.Substring(1));
						} else {
							Futaba.PutInformation(new Data.Information($"不明なURL{ link.NavigateUri.ToString() }"));
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
									uriCache.Add(link.NavigateUri, new Uri($"{ SharadConst.MkiMokiSchemeNull }://"));
								}
								Futaba.PutInformation(new Data.Information($"アップロードファイル補完エラー:{ x.UrlOrMessage }"));
							}
						});
					}
				} else {
					setLink(link);
				}
			}
		}

		private static void OnMouseEnterLink(object sender, MouseEventArgs e) {
			var link = sender as Hyperlink;
			if(link == null)
				return;

			link.TextDecorations = System.Windows.TextDecorations.Underline;
			link.Foreground = new SolidColorBrush(GetLinkActiveColor());
		}

		private static void OnMouseLeaveLink(object sender, MouseEventArgs e) {
			var link = sender as Hyperlink;
			var parent = link.Parent as TextBlock;
			if(link == null || parent == null)
				return;

			link.TextDecorations = null;
			link.Foreground = new SolidColorBrush(GetLinkColor());
		}

		private static Color GetLinkColor() {
			return (App.Current.TryFindResource("ViewerSecondaryColor") is Color c) ? c : Colors.Blue;
		}

		private static Color GetLinkActiveColor() {
			return (App.Current.TryFindResource("ViewerSecondaryDarkColor") is Color c) ? c : Colors.Red;
		}
	}
}
