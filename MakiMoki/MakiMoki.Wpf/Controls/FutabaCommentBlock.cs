﻿using System;
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
				typeof(Model.BindableFutabaResItem),
				typeof(FutabaCommentBlock),
				new PropertyMetadata(null, OnInlinePropertyChanged));
		public static readonly DependencyProperty MaxLinesProperty =
			DependencyProperty.RegisterAttached(
				nameof(MaxLines),
				typeof(int),
				typeof(FutabaCommentBlock),
				new PropertyMetadata(int.MaxValue, OnMaxLinesPropertyChanged));
		public static RoutedEvent LinkClickEvent
			= EventManager.RegisterRoutedEvent(
				nameof(LinkClick),
				RoutingStrategy.Tunnel,
				typeof(RoutedEventArgs),
				typeof(FutabaCommentBlock));

		public int MaxLines {
			get => (int)this.GetValue(MaxLinesProperty);
			set {
				this.SetValue(MaxLinesProperty, value);
			}
		}

		public event RoutedEventHandler LinkClick {
			add { AddHandler(LinkClickEvent, value); }
			remove { RemoveHandler(LinkClickEvent, value); }
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

			var msg = item?.CommentHtml.Value ?? "";
			var s1 = Regex.Replace(msg, @"<br>", Environment.NewLine,
				RegexOptions.IgnoreCase | RegexOptions.Multiline);
			//var s2 = Regex.Replace(s1, @"<[^>]*>", "",
			//	RegexOptions.IgnoreCase | RegexOptions.Multiline);
			//var s3 = System.Net.WebUtility.HtmlDecode(s2);
			msg = s1;
			var lines = msg.Replace("\r", "").Split('\n').Take(maxLines).ToArray();
			var regexOpt = RegexOptions.IgnoreCase | RegexOptions.Singleline;
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

				void EvalEmoji(string text, Color? color, ToolTip toolTip = null) {
					var fb = (color.HasValue) ? new SolidColorBrush(color.Value) : tb.Foreground;
					var pos = 0;
					foreach(Match m in Emoji.Wpf.EmojiData.MatchMultiple.Matches(text)) {
						tb.Inlines.Add(new Run(text.Substring(pos, m.Index - pos)) {
							Foreground = fb,
							ToolTip = toolTip,
						});
						tb.Inlines.Add(new Emoji.Wpf.EmojiInline() {
							FallbackBrush = tb.Foreground,
							Text = text.Substring(m.Index, m.Length),
							FontSize = tb.FontSize,
							ToolTip = toolTip,
						});

						pos = m.Index + m.Length;
					}
					tb.Inlines.Add(new Run(text.Substring(pos)) {
						Foreground = fb,
						ToolTip = toolTip,
					});
				}

				void EvalFont(StringBuilder inputVal, StringBuilder outputVal, ToolTip toolTip = null) {
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
							EvalEmoji(t3, color, toolTip);
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
									Foreground = new SolidColorBrush(Colors.Blue),
									NavigateUri = uri,
								};
								link.RequestNavigate += new RequestNavigateEventHandler(RequestNavigate);
								link.MouseEnter += new MouseEventHandler(link_MouseEnter);
								link.MouseLeave += new MouseEventHandler(link_MouseLeave);
								link.Inlines.Add(m.Value);
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
					if(i < item.Raw.Value.QuotLines.Length) {
						var q = item.Raw.Value.QuotLines[i];
						if(q.IsQuot) {
							if(q.IsHit) {
								var it = item.Parent.Value.ResItems
									.Where(x => x.Raw.Value.ResItem.No == q.ResNo)
									.FirstOrDefault();
								if(it != null) {
									toolTip = new ToolTip() {
										Content = new FutabaResBlock() {
											IsHitTestVisible = false,
											DataContext = it,
										}
									};
								}
							}
							if(toolTip == null) {
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
					EvalFont(input, output, toolTip);
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
			return s;
		}

		private static void RequestNavigate(object sender, RequestNavigateEventArgs e) {
			try {
				if(e.Source is Hyperlink hl) {
					FrameworkContentElement cl = hl;
					UIElement el = null;
					while(el == null && cl != null && cl.Parent != null) {
						if(cl.Parent is UIElement u) {
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
			if(link == null)
				return;

			// リンクにカーソルを当てたときは文字色を赤くする
			link.Foreground = Brushes.Red;
		}

		private static void link_MouseLeave(object sender, MouseEventArgs e) {
			var link = sender as Hyperlink;
			var parent = link.Parent as TextBlock;
			if(link == null || parent == null)
				return;

			// リンクからカーソルが離れたときは文字色をデフォルトカラーに戻す
			link.Foreground = new SolidColorBrush(Colors.Blue);
		}
	}
}
