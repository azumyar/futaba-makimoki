using System;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Data {
	public interface IMigrateCompatObject {
		ConfigObject Migrate();
	}
}
