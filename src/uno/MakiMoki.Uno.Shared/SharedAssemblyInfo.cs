using System;
using System.Collections.Generic;
using System.Text;

#if __ANDROID__
[assembly: System.Runtime.Versioning.SupportedOSPlatform("android31.0")]
#endif

#if CANARY
#warning カナリアビルド設定です
#endif

