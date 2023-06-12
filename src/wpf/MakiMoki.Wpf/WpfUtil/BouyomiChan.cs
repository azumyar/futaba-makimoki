using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	public static class BouyomiChan {
		private static System.Reactive.Concurrency.EventLoopScheduler BouyomiChanScheduler { get; }
			= new System.Reactive.Concurrency.EventLoopScheduler();


		public static void Speach(string text) {
			Observable.Return(text)
				.ObserveOn(BouyomiChanScheduler)
				.Subscribe(m => {
					foreach(var line in m.Replace("\r\n", "\n")
						.Split("\n")
						.Select(x => x.Replace('%', '％').Replace('&', '＆').Replace('?', '？'))) {

						try {
							if(Config.ConfigLoader.InitializedSetting.HttpClient == null) {
								return;
							}

							// awaitだとスレッドスタックが変わるのでちゃんとwaitする
							var r = Config.ConfigLoader.InitializedSetting.HttpClient.GetAsync(
								$"{WpfConfig.WpfConfigLoader.SystemConfig.BouyomiChanEndPoint}Talk?text={line}");
							r.Wait();
							if(r.Result.StatusCode != System.Net.HttpStatusCode.OK) {
								// エラー
							}
						}
						catch(AggregateException) {
							// エラー
						}
					}
			});
		}

	}
}
