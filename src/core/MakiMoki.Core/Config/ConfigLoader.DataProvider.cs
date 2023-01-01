using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Yarukizero.Net.MakiMoki.Config {
	public static partial class ConfigLoader {
		public interface IDataProvider {
			T LoadSystem<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject;

			T LoadUser<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject;
			void SaveUser(string name, Data.JsonObject config);
			void RemoveUser(string name);

			T LoadWork<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject;
			void SaveWork(string name, Data.JsonObject config);
			void RemoveWork(string name);
		}

		public class FileSaveProvider : IDataProvider {
			private readonly Setting setting;

			public FileSaveProvider(Setting setting) {
				this.setting = setting;
			}

			private T Load<T>(string path, T defaultValue, Dictionary<int, Type>? migrateTable, Func<string, Exception, Exception>? exception) where T : Data.ConfigObject
				=> Util.FileUtil.LoadMigrate(path, defaultValue, migrateTable, exception);
			private void Save(string dir, string name, Data.JsonObject conf) {
				if(Directory.Exists(dir)) {
					Util.FileUtil.SaveJson(Path.Combine(dir, name), conf);
				}
			}
			private void Remove(string path) {
				if(System.IO.File.Exists(path)) {
					try {
						System.IO.File.Delete(path);
					}
					catch(IOException) { /* TODO: どうする？ */}
				}
			}

			public T LoadSystem<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject
				=> Load(Path.Combine(InitializedSetting.SystemDirectory, name), defaultValue, migrateTable, exception);


			public T LoadUser<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject
				=> Load(Path.Combine(InitializedSetting.UserDirectory, name), defaultValue, migrateTable, exception);
			public void SaveUser(string name, Data.JsonObject config) => Save(InitializedSetting.UserDirectory, name, config);
			public void RemoveUser(string name) => Remove(Path.Combine(InitializedSetting.WorkDirectory, name));

			public T LoadWork<T>(string name, T defaultValue, Dictionary<int, Type>? migrateTable = null, Func<string, Exception, Exception>? exception = null) where T : Data.ConfigObject
				=> Load(Path.Combine(InitializedSetting.WorkDirectory, name), defaultValue, migrateTable, exception);
			public void SaveWork(string name, Data.JsonObject config) => Save(InitializedSetting.WorkDirectory, name, config);
			public void RemoveWork(string name) => Remove(Path.Combine(InitializedSetting.WorkDirectory, name));
		}
	}
}
