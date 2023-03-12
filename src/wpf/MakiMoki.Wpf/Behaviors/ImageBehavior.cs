using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {
	class ImageBehavior : Behavior<Image> {
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register(
				nameof(Source),
				typeof(Model.ImageObject),
				typeof(ImageBehavior),
				new PropertyMetadata(null));

		public Model.ImageObject Source {
			get => (Model.ImageObject)this.GetValue(SourceProperty);
			set {
				this.SetValue(SourceProperty, value);
			}
		}

		private Storyboard storyboard = new Storyboard();

		protected override void OnAttached() {
			base.OnAttached();
			this.AssociatedObject.Loaded += OnLoaded; ;
			this.AssociatedObject.Unloaded += OnUnloaded;
		}

		protected override void OnDetaching() {
			base.OnDetaching();
			this.AssociatedObject.Loaded -= OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			if(this.Source?.AnimationSource is Timeline tl) {
				Storyboard.SetTarget(this.AssociatedObject, tl);
				Storyboard.SetTargetProperty(tl, new PropertyPath(Image.SourceProperty));

				storyboard.Children.Add(tl);
				storyboard.Begin(this.AssociatedObject);
			}
		}

		private void OnUnloaded(object sender, RoutedEventArgs e) {
			if(sender is Image img) {
				img.Unloaded -= OnUnloaded;
				this.storyboard.Stop();
			}
		}
	}
}
