using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	// 参考元: https://tnakamura.hatenablog.com/entry/20100929/textbox_placeholder
	static class PlaceHolderBehavior {
		// プレースホルダーとして表示するテキスト
		public static readonly DependencyProperty PlaceHolderTextProperty
			= DependencyProperty.RegisterAttached(
				"PlaceHolderText",
				typeof(string),
				typeof(PlaceHolderBehavior),
				new PropertyMetadata(null, OnPlaceHolderChanged));

		private static void OnPlaceHolderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			var textBox = sender as System.Windows.Controls.Primitives.TextBoxBase;
			if(textBox == null) {
				return;
			}

			var placeHolder = e.NewValue as string;
			var handler = CreateEventHandler(placeHolder);
			if(string.IsNullOrEmpty(placeHolder)) {
				textBox.TextChanged -= handler;
			} else {
				textBox.TextChanged += handler;
				if(IsTextEmpty(textBox)) {
					textBox.Background = CreateVisualBrush(placeHolder);
				}
			}
		}

		private static TextChangedEventHandler CreateEventHandler(string placeHolder) {
			// TextChanged イベントをハンドルし、TextBox が未入力のときだけ
			// プレースホルダーを表示するようにする。
			return (sender, e) => {
				// 背景に TextBlock を描画する VisualBrush を使って
				// プレースホルダーを実現
				var textBox = (System.Windows.Controls.Primitives.TextBoxBase)sender;
				if(IsTextEmpty(textBox)) {
					textBox.Background = CreateVisualBrush(placeHolder);
				} else {
					textBox.Background = new SolidColorBrush(Colors.Transparent);
				}
			};
		}

		private static VisualBrush CreateVisualBrush(string placeHolder) {
			var visual = new Label() {
				Content = placeHolder,
				Padding = new Thickness(5, 1, 1, 1),
				Foreground = new SolidColorBrush(Colors.LightGray),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
			};
			return new VisualBrush(visual) {
				Stretch = Stretch.None,
				TileMode = TileMode.None,
				AlignmentX = AlignmentX.Left,
				AlignmentY = AlignmentY.Center,
			};
		}


		public static void SetPlaceHolderText(System.Windows.Controls.Primitives.TextBoxBase textBox, string placeHolder) {
			textBox.SetValue(PlaceHolderTextProperty, placeHolder);
		}

		public static string GetPlaceHolderText(System.Windows.Controls.Primitives.TextBoxBase textBox) {
			return textBox.GetValue(PlaceHolderTextProperty) as string;
		}


		private static bool IsTextEmpty(System.Windows.Controls.Primitives.TextBoxBase textBox) {
			if(textBox is TextBox tb) {
				return string.IsNullOrEmpty(tb.Text);
			} else if(textBox is RichTextBox rb) {
				if(rb.Document.Blocks.Any()) {
					if(rb.Document.Blocks.SkipLast(1).Any()) {
						return false;
					} else {
						var b = rb.Document.Blocks.First();
						return string.IsNullOrEmpty(new System.Windows.Documents.TextRange(b.ContentStart, b.ContentEnd).Text);
					}
				} else {
					return true;
				}
			} else {
				return true;
			}
		}
	}

	public class PasswordBehavior : Behavior<PasswordBox> {
		private static readonly DependencyProperty IsAttachedProperty
			= DependencyProperty.RegisterAttached(
				"IsAttached",
				typeof(bool),
				typeof(PasswordBehavior),
				new FrameworkPropertyMetadata(
					false,
					FrameworkPropertyMetadataOptions.None,
					(s, e) => {
						if(s is PasswordBox passwordBox) {
							if((bool)e.OldValue) {
								passwordBox.PasswordChanged -= OnPasswordChanged;
							}

							if((bool)e.NewValue) {
								passwordBox.PasswordChanged += OnPasswordChanged;
							}
						}
					})
				);

		private static readonly DependencyProperty PasswordProperty =
			DependencyProperty.RegisterAttached(
				"Password",
				typeof(string),
				typeof(PasswordBehavior),
				new FrameworkPropertyMetadata(
					"",
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					(s, e) => {
						if(s is PasswordBox passwordBox) {
							var newPassword = (string)e.NewValue;

							if(GetIsAttached(passwordBox) == false) {
								SetIsAttached(passwordBox, true);
							}

							// 例外
							if(string.IsNullOrEmpty(passwordBox.Password) && string.IsNullOrEmpty(newPassword) ||
								passwordBox.Password == newPassword) {
								return;
							}

							passwordBox.PasswordChanged -= OnPasswordChanged;
							passwordBox.Password = newPassword;
							passwordBox.PasswordChanged += OnPasswordChanged;
						}
					})
				);
		public static readonly DependencyProperty PlaceHolderTextProperty
			= DependencyProperty.RegisterAttached(
				"PlaceHolderText",
				typeof(string),
				typeof(PasswordBehavior),
				new PropertyMetadata(null, OnPlaceHolderChanged));

		public static bool GetIsAttached(DependencyObject d) {
			return (bool)d.GetValue(IsAttachedProperty);
		}

		public static void SetIsAttached(DependencyObject d, bool value) {
			d.SetValue(IsAttachedProperty, value);
		}

		public static string GetPassword(DependencyObject d) {
			return (string)d.GetValue(PasswordProperty);
		}

		public static void SetPassword(DependencyObject d, string value) {
			d.SetValue(PasswordProperty, value);
		}

		public static void SetPlaceHolderText(System.Windows.Controls.Primitives.TextBoxBase textBox, string placeHolder) {
			textBox.SetValue(PlaceHolderTextProperty, placeHolder);
		}

		public static string GetPlaceHolderText(System.Windows.Controls.Primitives.TextBoxBase textBox) {
			return textBox.GetValue(PlaceHolderTextProperty) as string;
		}

		private static void OnPasswordChanged(object sender, RoutedEventArgs e) {
			if(sender is PasswordBox passwordBox) {
				SetPassword(passwordBox, passwordBox.Password);
			}
		}

		private static void OnPlaceHolderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			if((sender is PasswordBox textBox) && (e.NewValue is string placeHolder)) {
				var handler = CreateEventHandler(placeHolder);
				if(string.IsNullOrEmpty(placeHolder)) {
					textBox.PasswordChanged -= handler;
				} else {
					textBox.PasswordChanged += handler;
					if(string.IsNullOrEmpty(textBox.Password)) {
						textBox.Background = CreateVisualBrush(placeHolder);
					}
				}
			}
		}

		private static RoutedEventHandler CreateEventHandler(string placeHolder) {
			// TextChanged イベントをハンドルし、TextBox が未入力のときだけ
			// プレースホルダーを表示するようにする。
			return (sender, _) => {
				// 背景に TextBlock を描画する VisualBrush を使って
				// プレースホルダーを実現
				if(sender is PasswordBox passwordBox) {
					passwordBox.Background = string.IsNullOrEmpty(passwordBox.Password) switch {
						true => CreateVisualBrush(placeHolder),
						false => new SolidColorBrush(Colors.Transparent),
					};
				}
			};
		}

		private static VisualBrush CreateVisualBrush(string placeHolder) {
			var visual = new Label() {
				Content = placeHolder,
				Padding = new Thickness(5, 1, 1, 1),
				Foreground = new SolidColorBrush(Colors.LightGray),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Center,
			};
			return new VisualBrush(visual) {
				Stretch = Stretch.None,
				TileMode = TileMode.None,
				AlignmentX = AlignmentX.Left,
				AlignmentY = AlignmentY.Center,
			};
		}

		protected override void OnAttached() {
			base.OnAttached();

			this.AssociatedObject.PasswordChanged += OnPasswordChanged;
		}

		protected override void OnDetaching() {
			this.AssociatedObject.PasswordChanged -= OnPasswordChanged;

			base.OnDetaching();
		}
	}
}
