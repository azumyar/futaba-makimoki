using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Yarukizero.Net.MakiMoki.Wpf.Behaviors {

	internal class FluentContextMenuBehavior : Behavior<FrameworkElement> {
		public static readonly DependencyProperty ContextMenuProperty
			= DependencyProperty.Register(
				nameof(ContextMenu),
				typeof(ContextMenu),
				typeof(FluentContextMenuBehavior),
				new PropertyMetadata(null, OnPropertyChanged));

		public ContextMenu ContextMenu {
			get => (ContextMenu)this.GetValue(ContextMenuProperty);
			set { this.SetValue(ContextMenuProperty, value); }
		}

		protected override void OnAttached() {
			base.OnAttached();
			WpfHelpers.FluentHelper.ApplyPopupBackground(this.AssociatedObject);
		}

		private static void OnPropertyChanged(object _, DependencyPropertyChangedEventArgs e) {
			if(e.NewValue is ContextMenu nm) {
				if(nm.IsOpen) {
					WpfHelpers.FluentHelper.AttachAndApplyContextMenu(nm);
				}
				nm.Opened += OnOpend;
			}
			if(e.OldValue is ContextMenu om) {
				om.Opened -= OnOpend;
			}
		}

		private static void OnOpend(object _, RoutedEventArgs e) {
			if(e.Source is ContextMenu m) {
				WpfHelpers.FluentHelper.AttachAndApplyContextMenu(m);
			}
		}
	}

	internal class FluentSubMenuBehavior : Behavior<FrameworkElement> {
		public static readonly DependencyProperty PopupProperty
			= DependencyProperty.Register(
				nameof(Popup),
				typeof(Popup),
				typeof(FluentSubMenuBehavior),
				new PropertyMetadata(null, OnPropertyChanged));

		public Popup Popup {
			get => (Popup)this.GetValue(PopupProperty);
			set { this.SetValue(PopupProperty, value); }
		}

		protected override void OnAttached() {
			base.OnAttached();
			WpfHelpers.FluentHelper.ApplyPopupBackground(this.AssociatedObject);
		}

		private static void OnPropertyChanged(object _, DependencyPropertyChangedEventArgs e) {
			if(e.NewValue is Popup np) {
				if(np.IsOpen) {
					WpfHelpers.FluentHelper.AttachAndApplySubMenu(np);
				}
				np.Opened += OnOpend;
			}
			if(e.OldValue is Popup op) {
				op.Opened -= OnOpend;
			}
		}

		private static void OnOpend(object s, EventArgs _) {
			if(s is Popup p) {
				WpfHelpers.FluentHelper.AttachAndApplySubMenu(p);
			}
		}
	}
}