using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	/// <summary>
	/// FutabaMediaViewer.xaml の相互作用ロジック
	/// </summary>
	public partial class FutabaPostView : UserControl {
		public static readonly DependencyProperty ContentsProperty
			= DependencyProperty.Register(
				nameof(Contents),
				typeof(Model.BindableFutaba),
				typeof(FutabaPostView),
				new PropertyMetadata(OnContentsChanged));
		public static RoutedEvent ContentsChangedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(ContentsChanged),
				RoutingStrategy.Tunnel,
				typeof(RoutedPropertyChangedEventHandler<Model.BindableFutaba>),
				typeof(FutabaPostView));

		public Model.BindableFutaba Contents {
			get => (Model.BindableFutaba)this.GetValue(ContentsProperty);
			set {
				this.SetValue(ContentsProperty, value);
			}
		}

		public event RoutedPropertyChangedEventHandler<Model.BindableFutaba> ContentsChanged {
			add { AddHandler(ContentsChangedEvent, value); }
			remove { RemoveHandler(ContentsChangedEvent, value); }
		}



		public FutabaPostView() {
			InitializeComponent();

			ViewModels.FutabaPostViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaPostViewViewModel.PostCloseMessage>>()
				.Subscribe(x => {
					if(x.Url == this.Contents?.Url) {
						this.Visibility = Visibility.Hidden;
					}
				});
			ViewModels.FutabaPostViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaPostViewViewModel.ReplaceTextMessage>>()
				.Subscribe(x => {
					if(x.Url == this.Contents?.Url) {
						this.PostCommentTextBox.Text = x.Text;
						this.PostCommentTextBox.SelectionStart = x.Text.Length;
						this.PostCommentTextBox.SelectionLength = 0;
						this.PostCommentTextBox.Focus();
					}
				});
			ViewModels.FutabaPostViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaPostViewViewModel.AppendTextMessage>>()
				.Subscribe(x => {
					if((x.Url == this.Contents?.Url) && !string.IsNullOrEmpty(x.Text)) {
						var s = x.Text + ((x.Text.Last() == '\n') ? "" : Environment.NewLine);
						var ss = this.PostCommentTextBox.SelectionStart;
						var sb = new StringBuilder(this.PostCommentTextBox.Text);
						sb.Insert(ss, s);
						this.PostCommentTextBox.Text = sb.ToString();
						this.PostCommentTextBox.SelectionStart = ss + s.Length;
						this.PostCommentTextBox.SelectionLength = 0;
						this.PostCommentTextBox.Focus();
					}
				});
		}

		private static void OnContentsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<Model.BindableFutaba>(
					e.OldValue as Model.BindableFutaba,
					e.NewValue as Model.BindableFutaba,
					ContentsChangedEvent));
			}
		}
	}
}