using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Util {
	public static class TaskUtil {
		private static volatile object lockObj = new object();
		private static List<Action> tasks = new List<Action>();
		private static Queue<Action> imageTasks = new Queue<Action>();
		private static Task task;
		private static Task imageTask;

		public static void Initialize() {
			task = Task.Run(async () => {
				while(true) {
					Task[] t = null;
					lock(lockObj) {
						if(tasks.Count != 0) {
							t = tasks.Select(x => Task.Run(x)).ToArray();
							tasks.Clear();
						}
					}
					if (t != null) {
						Task.WaitAll(t);
					} else {
						await Task.Delay(1000);
					}
				}
			});
			imageTask = Task.Run(async () => {
				var t = new List<Task>();
				while (true) {
					lock (lockObj) {
						for (var i = 0; i < 5; i++) {
							if (imageTasks.Count != 0) {
								t.Add(Task.Run(imageTasks.Dequeue()));
							}
						}
					}
					if (0 < t.Count) {
						Task.WaitAll(t.ToArray());
						t.Clear();
					} else {
						await Task.Delay(1000);
					}
				}
			});
		}

		public static void Push(params Action[] action) {
			lock (lockObj) {
				tasks.AddRange(action);
			}
		}

		public static void PushImage(params Action[] action) {
			lock (lockObj) {
				foreach (var a in action) {
					imageTasks.Enqueue(a);
				}
			}
		}

		public static void Exit() { 
		}
	}
}
