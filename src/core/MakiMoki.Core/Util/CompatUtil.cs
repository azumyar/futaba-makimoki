using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class CompatUtil {
		public static T Migrate<T>(string json, Dictionary<int, Type> migrateDic) where T : Data.ConfigObject {
			var targetConf = JsonConvert.DeserializeObject<Data.ConfigObject>(json);
			var p = typeof(T).GetProperty("CurrentVersion", BindingFlags.Public | BindingFlags.Static);
			if(p == null) {
				throw new ArgumentException($"{ typeof(T).FullName }はCurrentVersionを持っていません");
			}
			var ver = (int)p.GetValue(null);
			if(targetConf.Version == ver) {
				return JsonConvert.DeserializeObject<T>(json);
			}

			var convert = typeof(JsonConvert)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(x =>
					(x.Name == nameof(JsonConvert.DeserializeObject))
						&& (x.GetParameters().Length == 1)
						&& (x.GetParameters()[0].ParameterType == typeof(string))
						&& x.IsGenericMethodDefinition)
				.FirstOrDefault();
			System.Diagnostics.Debug.Assert(convert != null);
			if(migrateDic.TryGetValue(targetConf.Version, out var type)) {
				var o = convert.MakeGenericMethod(type).Invoke(null, new object[] { json });
				if(o is Data.ConfigObject co) {
					if(co.Version == ver) {
						if(co is T r) {
							return r;
						} else {
							throw new ArgumentException($"{ type.FullName }は{ typeof(T).Name }とVersionが一致しますが異なるオブジェクトです。");
						}
					} else if(co is Data.IMigrateCompatObject mo) {
						do {
							var mco = mo.Migrate();
							if(mco.Version == ver) {
								if(mco is T r) {
									return r;
								} else {
									throw new ArgumentException($"{ type.FullName }は{ typeof(T).Name }とVersionが一致しますが異なるオブジェクトです。");
								}
							}
							mo = mco as Data.IMigrateCompatObject;
						} while(mo != null);
					}
				} else {
					throw new ArgumentException($"{ type.FullName }は{ typeof(Data.ConfigObject).Name }ではありません。");
				}
			}
			throw new Exceptions.MigrateFailedException($"{ typeof(T).Name }(Ver={ targetConf.Version })の更新に失敗しました。");
		}
	}
}
