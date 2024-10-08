using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarukizero.Net.MakiMoki.Ng.NgData;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfConfig {
	internal static class ConfigNotifyer {
		private static ReactivePropertySlim<object> WpfSystemToken_ { get; } = new(initialValue: DateTime.MinValue);
		private static ReactivePropertySlim<object> WpfGestureToken_ { get; } = new(initialValue: DateTime.MinValue);
		private static ReactivePropertySlim<object> NgModuleWordToken_ { get; } = new(initialValue: DateTime.MinValue);
		private static ReactivePropertySlim<object> NgModuleImageToken_ { get; } = new(initialValue: DateTime.MinValue);
		private static ReactivePropertySlim<object> NgModuleHiddenToken_ { get; } = new(initialValue: DateTime.MinValue);
		private static ReactivePropertySlim<object> NgModuleWatchWordToken_ { get; } = new(initialValue: DateTime.MinValue);
		private static ReactivePropertySlim<object> NgModuleWatchImageToken_ { get; } = new(initialValue: DateTime.MinValue);


		public static ReadOnlyReactivePropertySlim<object> WpfSystemToken { get; }
		public static ReadOnlyReactivePropertySlim<object> WpfGestureToken { get; }
		public static ReadOnlyReactivePropertySlim<object> NgToken { get; }
		public static ReadOnlyReactivePropertySlim<object> WatchToken { get; }

		public static ReadOnlyReactivePropertySlim<object> NgModuleWordToken { get; }
		public static ReadOnlyReactivePropertySlim<object> NgModuleImageToken { get; }
		public static ReadOnlyReactivePropertySlim<object> NgModuleHiddenToken { get; }
		public static ReadOnlyReactivePropertySlim<object> NgModuleWatchWordToken { get; }
		public static ReadOnlyReactivePropertySlim<object> NgModuleWatchImageToken { get; }

		static ConfigNotifyer() {
			WpfSystemToken = WpfSystemToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();
			WpfGestureToken = WpfGestureToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();

			NgModuleWordToken = NgModuleWordToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();
			NgModuleImageToken = NgModuleImageToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();
			NgModuleHiddenToken = NgModuleHiddenToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();
			NgModuleWatchWordToken = NgModuleWatchWordToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();
			NgModuleWatchImageToken = NgModuleWatchImageToken_
				.ObserveOn(UIDispatcherScheduler.Default)
				.ToReadOnlyReactivePropertySlim();

			NgToken = NgModuleWordToken.CombineLatest(
				NgModuleImageToken,
				NgModuleHiddenToken,
				(_, _, _) => (object)DateTime.Now).ToReadOnlyReactivePropertySlim();
			WatchToken = NgModuleWatchWordToken.CombineLatest(
				NgModuleWatchImageToken,
				(_, _) => (object)DateTime.Now).ToReadOnlyReactivePropertySlim();

			WpfConfigLoader.SystemConfigUpdateNotifyer.AddHandler((_) => WpfSystemToken_.Value = DateTime.Now);
			WpfConfigLoader.GestureConfigUpdateNotifyer.AddHandler((_) => WpfGestureToken_.Value = DateTime.Now);
			Ng.NgConfig.NgConfigLoader.AddNgUpdateNotifyer((_) => NgModuleWordToken_.Value = DateTime.Now);
			Ng.NgConfig.NgConfigLoader.AddImageUpdateNotifyer((_) => NgModuleImageToken_.Value = DateTime.Now);
			Ng.NgConfig.NgConfigLoader.AddHiddenUpdateNotifyer((_) => NgModuleHiddenToken_.Value = DateTime.Now);
			Ng.NgConfig.NgConfigLoader.WatchUpdateNotifyer.AddHandler((_) => NgModuleWatchWordToken_.Value = DateTime.Now);
			Ng.NgConfig.NgConfigLoader.WatchImageUpdateNotifyer.AddHandler((_) => NgModuleWatchImageToken_.Value = DateTime.Now);
		}
	}
}
