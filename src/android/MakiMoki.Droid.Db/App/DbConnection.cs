using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Yarukizero.Net.MakiMoki.Droid.App {
	public class DbConnection {
		private readonly string dbPath;
		private SQLiteConnection? Connection { get; set; }
		public System.Reactive.Concurrency.EventLoopScheduler DbScheduler { get; } = new System.Reactive.Concurrency.EventLoopScheduler();

		public DbConnection(string dbPath) {
			this.dbPath = dbPath;
		}

		public IObservable<SQLiteConnection> Connect() {
			return Observable.Return(this.Connection)
				.ObserveOn(this.DbScheduler)
				.Select(x => {
					if(x == null) {
						this.Connection = new SQLiteConnection(dbPath);
						//this.Connection.CreateTable<DroidData.Sql.FutabaResponseTable>();
						this.Connection.CreateTable<DroidData.Db.ImageTable>();
						return this.Connection;
					} else {
						return x;
					}
				});
		}

	}
}
