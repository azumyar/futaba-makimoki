using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class RichTextBoxBehavior : Behavior<RichTextBox> {
		public static readonly DependencyProperty PlaneTextProperty =
			DependencyProperty.Register(
				nameof(PlaneText),
				typeof(string),
				typeof(RichTextBoxBehavior),
				new PropertyMetadata(null, OnPropertyChanged));

		public string PlaneText {
			get => (string)this.GetValue(PlaneTextProperty);
			set {
				this.SetValue(PlaneTextProperty, value);
			}
		}

		protected override void OnAttached() {
			base.OnAttached();
			this.AssociatedObject.TextChanged += OnTextChanged;
		}

		protected override void OnDetaching() {
			base.OnDetaching();
			this.AssociatedObject.TextChanged -= OnTextChanged;
		}


		private void OnTextChanged(object sender, TextChangedEventArgs e) {
			this.PlaneText = GetText(this.AssociatedObject);
		}

		private static string GetText(RichTextBox rb) {
			if(rb == null) {
				return "";
			} else {
				var sb = new StringBuilder();
				foreach(var b in rb.Document.Blocks.SkipLast(1)) {
					sb.AppendLine(new TextRange(b.ContentStart, b.ContentEnd).Text);
				}
				{
					var b = rb.Document.Blocks.LastOrDefault();
					if(b != null) {
						sb.Append(new TextRange(b.ContentStart, b.ContentEnd).Text);
					}
				}

				return sb.ToString();
			}
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			if(sender is RichTextBoxBehavior rb && e.NewValue is string s) {
				if((rb.AssociatedObject != null) && (s != GetText(rb.AssociatedObject))) {
					rb.AssociatedObject.Document ??= new FlowDocument();
					rb.AssociatedObject.Document.Blocks.Clear();
					rb.AssociatedObject.Document.Blocks.AddRange(
						s.Replace("\r\n", "\n")
							.Split("\n")
							.Select(x => new Paragraph(new Run(x))));
				}
			}
		}
	}
}