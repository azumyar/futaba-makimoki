using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Yarukizero.Net.MakiMoki.Wpf.WpfUtil {
	static class MediaFoundationUtil {
		class InternalException : Exception {
			public InternalException(int code) : base($"erro code is {code:x}") {}
		}

		private const int VT_I8 = 20;
		private const int MF_SDK_VERSION = 0x0002;
		private const int MF_API_VERSION = 0x0070;
		private const int MF_VERSION = (MF_SDK_VERSION << 16 | MF_API_VERSION);
		private const int MFSTARTUP_NOSOCKET = 0x1;
		private const int MFSTARTUP_LITE = MFSTARTUP_NOSOCKET;
		private const int MFSTARTUP_FULL = 0;
		private const int MF_SOURCE_READER_FIRST_VIDEO_STREAM = unchecked((int)0xFFFFFFFC);
		private const int MF_SOURCE_READER_MEDIASOURCE = unchecked((int)0xFFFFFFFF);
		private const int MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED = 0x20;
		private const int D3DFMT_X8R8G8B8 = 22;
		private static readonly Guid GUID_NULL = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		private static readonly Guid MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING = new Guid(0xfb394f3d, 0xccf1, 0x42ee, 0xbb, 0xb3, 0xf9, 0xb8, 0x45, 0xd5, 0x68, 0x1d);
		private static readonly Guid MF_MT_MAJOR_TYPE = new Guid(0x48eba18e, 0xf8c9, 0x4687, 0xbf, 0x11, 0x0a, 0x74, 0xc9, 0xf9, 0x6a, 0x8f);
		private static readonly Guid MF_MT_SUBTYPE = new Guid(0xf7e34c9a, 0x42e8, 0x4714, 0xb7, 0x4b, 0xcb, 0x29, 0xd7, 0x2c, 0x35, 0xe5);
		private static readonly Guid MF_MT_FRAME_SIZE = new Guid(0x1652c33d, 0xd6b2, 0x4012, 0xb8, 0x34, 0x72, 0x03, 0x08, 0x49, 0xa3, 0x7d);
		private static readonly Guid MF_MT_DEFAULT_STRIDE = new Guid(0x644b4e48, 0x1e02, 0x4516, 0xb0, 0xeb, 0xc0, 0x1c, 0xa9, 0xd4, 0x9a, 0xc6);
		private static readonly Guid MF_PD_DURATION = new Guid(0x6c990d33, 0xbb8e, 0x477a, 0x85, 0x98, 0xd, 0x5d, 0x96, 0xfc, 0xd8, 0x8a);
		private static readonly Guid MFMediaType_Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);
		private static readonly Guid MFImageFormat_RGB32 = new Guid(0x00000016, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		private static readonly Guid MFVideoFormat_RGB32 = new Guid(D3DFMT_X8R8G8B8, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		[DllImport("Mfplat.dll")]
		private static extern int MFStartup(int Version, int dwFlags);
		[DllImport("Mfplat.dll")]
		private static extern int MFShutdown();

		[DllImport("Mfplat.dll")]
		private static extern int MFCreateAttributes(out IMFAttributes ppMFAttributes, int cInitialSize);

		[DllImport("Mfplat.dll")]
		private static extern int MFCreateMediaType(out IMFMediaType ppMFType);

		[DllImport("Mfreadwrite.dll")]
		private static extern int MFCreateSourceReaderFromURL(
			[MarshalAs(UnmanagedType.LPWStr)]
			string pwszURL,
			[In] IMFAttributes pAttributes,
			[Out] out IMFSourceReader ppSourceReader);

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		struct PROPVARIANT {
			[FieldOffset(0)]
			public short vt;
			[FieldOffset(2)]
			public short wReserved1;
			[FieldOffset(4)]
			public short wReserved2;
			[FieldOffset(6)]
			public short wReserved3;

			[FieldOffset(8)]
			public int intVal;
			[FieldOffset(8)]
			public long longVal;

			[FieldOffset(8)]
			public uint i0;
			[FieldOffset(12)]
			public uint i1;
			[FieldOffset(16)]
			public uint i2;
		}

		// COMインタフェース定義はめどいのでつかうの以外は名前だけ
		[ComImport]
		[Guid("2cd2d921-c447-44a7-a13c-4adabfc247e3")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IMFAttributes {
			[PreserveSig]
			int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [In][Out] ref PROPVARIANT pValue);
			[PreserveSig]
			int GetItemType(/* __RPC__in REFGUID guidKey, __RPC__out MF_ATTRIBUTE_TYPE *pType */);
			[PreserveSig]
			int CompareItem(/*__RPC__in REFGUID guidKey, __RPC__in REFPROPVARIANT Value, __RPC__out BOOL *pbResult*/);
			[PreserveSig]
			int Compare(/*__RPC__in_opt IMFAttributes *pTheirs, MF_ATTRIBUTES_MATCH_TYPE MatchType, __RPC__out BOOL *pbResult*/);
			[PreserveSig]
			int GetUINT32(/*__RPC__in REFGUID guidKey, __RPC__out UINT32 *punValue*/);
			[PreserveSig]
			int GetUINT64(/*__RPC__in REFGUID guidKey, __RPC__out UINT64 *punValue*/);
			[PreserveSig]
			int GetDouble(/*__RPC__in REFGUID guidKey, __RPC__out double* pfValue*/);
			[PreserveSig]
			int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [Out][MarshalAs(UnmanagedType.LPArray)] byte[] pguidValue);
			[PreserveSig]
			int GetStringLength(/*__RPC__in REFGUID guidKey, __RPC__out UINT32 *pcchLength*/);
			[PreserveSig]
			int GetString(/*__RPC__in REFGUID guidKey, [size_is][out]  __RPC__out_ecount_full(cchBufSize) LPWSTR pwszValue, UINT32 cchBufSize, __RPC__inout_opt UINT32 *pcchLength */);
			[PreserveSig]
			int GetAllocatedString(/*__RPC__in REFGUID guidKey, [size_is][size_is][out]  __RPC__deref_out_ecount_full_opt(( * pcchLength + 1 ) ) LPWSTR* ppwszValue, __RPC__out UINT32* pcchLength */);
			[PreserveSig]
			int GetBlobSize(/*__RPC__in REFGUID guidKey,  __RPC__out UINT32 *pcbBlobSize */);
			[PreserveSig]
			int GetBlob(/*__RPC__in REFGUID guidKey, [size_is][out]  __RPC__out_ecount_full(cbBufSize) UINT8 *pBuf, UINT32 cbBufSize, __RPC__inout_opt UINT32 *pcbBlobSize*/);
			[PreserveSig]
			int GetAllocatedBlob(/*__RPC__in REFGUID guidKey, [size_is][size_is][out] __RPC__deref_out_ecount_full_opt(* pcbSize) UINT8 **ppBuf, __RPC__out UINT32 *pcbSize */);
			[PreserveSig]
			int GetUnknown(/*__RPC__in REFGUID guidKey, __RPC__in REFIID riid, [iid_is][out]  __RPC__deref_out_opt LPVOID *ppv */);
			[PreserveSig]
			int SetItem(/* __RPC__in REFGUID guidKey, __RPC__in REFPROPVARIANT Value */);
			[PreserveSig]
			int DeleteItem(/*__RPC__in REFGUID guidKey */);
			[PreserveSig]
			int DeleteAllItems();
			[PreserveSig]
			int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, int unValue);
			[PreserveSig]
			int SetUINT64(/*__RPC__in REFGUID guidKey, UINT64 unValue*/);
			[PreserveSig]
			int SetDouble(/*__RPC__in REFGUID guidKey, double fValue */);
			[PreserveSig]
			int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [In][MarshalAs(UnmanagedType.LPStruct)] Guid guidValue);
			[PreserveSig]
			int SetString(/*__RPC__in REFGUID guidKey, __RPC__in_string LPCWSTR wszValue*/);
			[PreserveSig]
			int SetBlob(/*__RPC__in REFGUID guidKey, [size_is][in] __RPC__in_ecount_full(cbBufSize) const UINT8* pBuf, UINT32 cbBufSize*/);
			[PreserveSig]
			int SetUnknown(/*__RPC__in REFGUID guidKey, __RPC__in_opt IUnknown *pUnknown*/);
			[PreserveSig]
			int LockStore();
			[PreserveSig]
			int UnlockStore();
			[PreserveSig]
			int GetCount(/* __RPC__out UINT32 *pcItems */);
			[PreserveSig]
			int GetItemByIndex(/*UINT32 unIndex, __RPC__out GUID *pguidKey, __RPC__inout_opt PROPVARIANT *pValue */);
			[PreserveSig]
			int CopyAllItems(/*__RPC__in_opt IMFAttributes *pDest*/);
		}

		[ComImport]
		[Guid("44ae0fa8-ea31-4109-8d2e-4cae4997c555")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IMFMediaType /* : IMFAttributes */ {
			#region IMFAttributes メソッド
			[PreserveSig]
			int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [In][Out] ref PROPVARIANT pValue);
			[PreserveSig]
			int GetItemType(/* __RPC__in REFGUID guidKey, __RPC__out MF_ATTRIBUTE_TYPE *pType */);
			[PreserveSig]
			int CompareItem(/*__RPC__in REFGUID guidKey, __RPC__in REFPROPVARIANT Value, __RPC__out BOOL *pbResult*/);
			[PreserveSig]
			int Compare(/*__RPC__in_opt IMFAttributes *pTheirs, MF_ATTRIBUTES_MATCH_TYPE MatchType, __RPC__out BOOL *pbResult*/);
			[PreserveSig]
			int GetUINT32(/*__RPC__in REFGUID guidKey, __RPC__out UINT32 *punValue*/);
			[PreserveSig]
			int GetUINT64(/*__RPC__in REFGUID guidKey, __RPC__out UINT64 *punValue*/);
			[PreserveSig]
			int GetDouble(/*__RPC__in REFGUID guidKey, __RPC__out double* pfValue*/);
			[PreserveSig]
			int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [Out][MarshalAs(UnmanagedType.LPArray)] byte[] pguidValue);
			[PreserveSig]
			int GetStringLength(/*__RPC__in REFGUID guidKey, __RPC__out UINT32 *pcchLength*/);
			[PreserveSig]
			int GetString(/*__RPC__in REFGUID guidKey, [size_is][out]  __RPC__out_ecount_full(cchBufSize) LPWSTR pwszValue, UINT32 cchBufSize, __RPC__inout_opt UINT32 *pcchLength */);
			[PreserveSig]
			int GetAllocatedString(/*__RPC__in REFGUID guidKey, [size_is][size_is][out]  __RPC__deref_out_ecount_full_opt(( * pcchLength + 1 ) ) LPWSTR* ppwszValue, __RPC__out UINT32* pcchLength */);
			[PreserveSig]
			int GetBlobSize(/*__RPC__in REFGUID guidKey,  __RPC__out UINT32 *pcbBlobSize */);
			[PreserveSig]
			int GetBlob(/*__RPC__in REFGUID guidKey, [size_is][out]  __RPC__out_ecount_full(cbBufSize) UINT8 *pBuf, UINT32 cbBufSize, __RPC__inout_opt UINT32 *pcbBlobSize*/);
			[PreserveSig]
			int GetAllocatedBlob(/*__RPC__in REFGUID guidKey, [size_is][size_is][out] __RPC__deref_out_ecount_full_opt(* pcbSize) UINT8 **ppBuf, __RPC__out UINT32 *pcbSize */);
			[PreserveSig]
			int GetUnknown(/*__RPC__in REFGUID guidKey, __RPC__in REFIID riid, [iid_is][out]  __RPC__deref_out_opt LPVOID *ppv */);
			[PreserveSig]
			int SetItem(/* __RPC__in REFGUID guidKey, __RPC__in REFPROPVARIANT Value */);
			[PreserveSig]
			int DeleteItem(/*__RPC__in REFGUID guidKey */);
			[PreserveSig]
			int DeleteAllItems();
			[PreserveSig]
			int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, int unValue);
			[PreserveSig]
			int SetUINT64(/*__RPC__in REFGUID guidKey, UINT64 unValue*/);
			[PreserveSig]
			int SetDouble(/*__RPC__in REFGUID guidKey, double fValue */);
			[PreserveSig]
			int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [In][MarshalAs(UnmanagedType.LPStruct)] Guid guidValue);
			[PreserveSig]
			int SetString(/*__RPC__in REFGUID guidKey, __RPC__in_string LPCWSTR wszValue*/);
			[PreserveSig]
			int SetBlob(/*__RPC__in REFGUID guidKey, [size_is][in] __RPC__in_ecount_full(cbBufSize) const UINT8* pBuf, UINT32 cbBufSize*/);
			[PreserveSig]
			int SetUnknown(/*__RPC__in REFGUID guidKey, __RPC__in_opt IUnknown *pUnknown*/);
			[PreserveSig]
			int LockStore();
			[PreserveSig]
			int UnlockStore();
			[PreserveSig]
			int GetCount(/* __RPC__out UINT32 *pcItems */);
			[PreserveSig]
			int GetItemByIndex(/*UINT32 unIndex, __RPC__out GUID *pguidKey, __RPC__inout_opt PROPVARIANT *pValue */);
			[PreserveSig]
			int CopyAllItems(/*__RPC__in_opt IMFAttributes *pDest*/);
			#endregion

			[PreserveSig]
			int GetMajorType(/* out__RPC__out GUID *pguidMajorType */);
			[PreserveSig]
			int IsCompressedFormat(/* out __RPC__out BOOL *pfCompressed */);
			[PreserveSig]
			int IsEqual(/*__RPC__in_opt IMFMediaType *pIMediaType, out __RPC__out DWORD *pdwFlags */);
			[PreserveSig]
			int GetRepresentation(/* GUID guidRepresentation, out _Out_ LPVOID *ppvRepresentation */);
			[PreserveSig]
			int FreeRepresentation(/*GUID guidRepresentation, IntPtr pvRepresentation*/);
		}

		[ComImport]
		[Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IMFSourceReader {
			[PreserveSig]
			int GetStreamSelection(/*_In_ DWORD dwStreamIndex, _Out_ BOOL *pfSelected */);
			[PreserveSig]
			int SetStreamSelection(int dwStreamIndex, bool fSelected);
			[PreserveSig]
			int GetNativeMediaType(/*_In_ DWORD dwStreamIndex, _In_ DWORD dwMediaTypeIndex, _Out_ IMFMediaType **ppMediaType*/);
			[PreserveSig]
			int GetCurrentMediaType(int dwStreamIndex, [Out] out IMFMediaType ppMediaType);
			[PreserveSig]
			int SetCurrentMediaType(int dwStreamIndex, IntPtr pdwReserved, [In] IMFMediaType pMediaType);
			[PreserveSig]
			int SetCurrentPosition([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat, [In] ref PROPVARIANT varPosition);
			[PreserveSig]
			int ReadSample(int dwStreamIndex, int dwControlFlags, out int pdwActualStreamIndex, out int pdwStreamFlags, out long pllTimestamp, [Out] out IMFSample ppSample);
			[PreserveSig]
			int Flush(/*_In_ DWORD dwStreamIndex */);
			[PreserveSig]
			int GetServiceForStream(/*_In_ DWORD dwStreamIndex, _In_ REFGUID guidService, _In_ REFIID riid, _Out_ LPVOID *ppvObject*/);
			[PreserveSig]
			int GetPresentationAttribute(int dwStreamIndex, [In][MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute, [Out] out PROPVARIANT pvarAttribute);
		}

		[ComImport]
		[Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IMFSample /* : IMFAttributes */ {
			#region IMFAttributesメソッド
			[PreserveSig]
			int GetItem([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [In][Out] ref PROPVARIANT pValue);
			[PreserveSig]
			int GetItemType(/* __RPC__in REFGUID guidKey, __RPC__out MF_ATTRIBUTE_TYPE *pType */);
			[PreserveSig]
			int CompareItem(/*__RPC__in REFGUID guidKey, __RPC__in REFPROPVARIANT Value, __RPC__out BOOL *pbResult*/);
			[PreserveSig]
			int Compare(/*__RPC__in_opt IMFAttributes *pTheirs, MF_ATTRIBUTES_MATCH_TYPE MatchType, __RPC__out BOOL *pbResult*/);
			[PreserveSig]
			int GetUINT32(/*__RPC__in REFGUID guidKey, __RPC__out UINT32 *punValue*/);
			[PreserveSig]
			int GetUINT64(/*__RPC__in REFGUID guidKey, __RPC__out UINT64 *punValue*/);
			[PreserveSig]
			int GetDouble(/*__RPC__in REFGUID guidKey, __RPC__out double* pfValue*/);
			[PreserveSig]
			int GetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [Out][MarshalAs(UnmanagedType.LPArray)] byte[] pguidValue);
			[PreserveSig]
			int GetStringLength(/*__RPC__in REFGUID guidKey, __RPC__out UINT32 *pcchLength*/);
			[PreserveSig]
			int GetString(/*__RPC__in REFGUID guidKey, [size_is][out]  __RPC__out_ecount_full(cchBufSize) LPWSTR pwszValue, UINT32 cchBufSize, __RPC__inout_opt UINT32 *pcchLength */);
			[PreserveSig]
			int GetAllocatedString(/*__RPC__in REFGUID guidKey, [size_is][size_is][out]  __RPC__deref_out_ecount_full_opt(( * pcchLength + 1 ) ) LPWSTR* ppwszValue, __RPC__out UINT32* pcchLength */);
			[PreserveSig]
			int GetBlobSize(/*__RPC__in REFGUID guidKey,  __RPC__out UINT32 *pcbBlobSize */);
			[PreserveSig]
			int GetBlob(/*__RPC__in REFGUID guidKey, [size_is][out]  __RPC__out_ecount_full(cbBufSize) UINT8 *pBuf, UINT32 cbBufSize, __RPC__inout_opt UINT32 *pcbBlobSize*/);
			[PreserveSig]
			int GetAllocatedBlob(/*__RPC__in REFGUID guidKey, [size_is][size_is][out] __RPC__deref_out_ecount_full_opt(* pcbSize) UINT8 **ppBuf, __RPC__out UINT32 *pcbSize */);
			[PreserveSig]
			int GetUnknown(/*__RPC__in REFGUID guidKey, __RPC__in REFIID riid, [iid_is][out]  __RPC__deref_out_opt LPVOID *ppv */);
			[PreserveSig]
			int SetItem(/* __RPC__in REFGUID guidKey, __RPC__in REFPROPVARIANT Value */);
			[PreserveSig]
			int DeleteItem(/*__RPC__in REFGUID guidKey */);
			[PreserveSig]
			int DeleteAllItems();
			[PreserveSig]
			int SetUINT32([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, int unValue);
			[PreserveSig]
			int SetUINT64(/*__RPC__in REFGUID guidKey, UINT64 unValue*/);
			[PreserveSig]
			int SetDouble(/*__RPC__in REFGUID guidKey, double fValue */);
			[PreserveSig]
			int SetGUID([In][MarshalAs(UnmanagedType.LPStruct)] Guid guidKey, [In][MarshalAs(UnmanagedType.LPStruct)] Guid guidValue);
			[PreserveSig]
			int SetString(/*__RPC__in REFGUID guidKey, __RPC__in_string LPCWSTR wszValue*/);
			[PreserveSig]
			int SetBlob(/*__RPC__in REFGUID guidKey, [size_is][in] __RPC__in_ecount_full(cbBufSize) const UINT8* pBuf, UINT32 cbBufSize*/);
			[PreserveSig]
			int SetUnknown(/*__RPC__in REFGUID guidKey, __RPC__in_opt IUnknown *pUnknown*/);
			[PreserveSig]
			int LockStore();
			[PreserveSig]
			int UnlockStore();
			[PreserveSig]
			int GetCount(/* __RPC__out UINT32 *pcItems */);
			[PreserveSig]
			int GetItemByIndex(/*UINT32 unIndex, __RPC__out GUID *pguidKey, __RPC__inout_opt PROPVARIANT *pValue */);
			[PreserveSig]
			int CopyAllItems(/*__RPC__in_opt IMFAttributes *pDest*/);
			#endregion

			[PreserveSig]
			int GetSampleFlags(/*_Out_ DWORD *pdwSampleFlags*/);
			[PreserveSig]
			int SetSampleFlags(/*DWORD dwSampleFlags*/);
			[PreserveSig]
			int GetSampleTime(/*_Out_ LONGLONG *phnsSampleTime */);
			[PreserveSig]
			int SetSampleTime(/*LONGLONG hnsSampleTime*/);
			[PreserveSig]
			int GetSampleDuration(/*_Out_ LONGLONG *phnsSampleDuration*/);
			[PreserveSig]
			int SetSampleDuration(/*LONGLONG hnsSampleDuration*/);
			[PreserveSig]
			int GetBufferCount(/*_Out_ DWORD *pdwBufferCount*/);
			[PreserveSig]
			int GetBufferByIndex(/*DWORD dwIndex, _Out_ IMFMediaBuffer **ppBuffer*/);
			[PreserveSig]
			int ConvertToContiguousBuffer([Out]out IMFMediaBuffer ppBuffer);
			[PreserveSig]
			int AddBuffer(/* IMFMediaBuffer* pBuffer*/);
			[PreserveSig]
			int RemoveBufferByIndex(/*DWORD dwIndex*/);
			[PreserveSig]
			int RemoveAllBuffers();
			[PreserveSig]
			int GetTotalLength(/*_Out_ DWORD *pcbTotalLength*/);
			[PreserveSig]
			int CopyToBuffer(/*IMFMediaBuffer* pBuffer*/);
		}

		[ComImport]
		[Guid("045FA593-8799-42b8-BC8D-8968C6453507")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface IMFMediaBuffer {
			[PreserveSig]
			int Lock([Out]out IntPtr ppbBuffer, [Out]out int pcbMaxLength, [Out]out int pcbCurrentLength);
			[PreserveSig]
			int Unlock();
			[PreserveSig]
			int GetCurrentLength(/*_Out_ DWORD *pcbCurrentLength*/);
			[PreserveSig]
			int SetCurrentLength(/*DWORD cbCurrentLength*/);
			[PreserveSig]
			int GetMaxLength(/*_Out_ DWORD *pcbMaxLength*/);
        }

		public static void StratUp() {
			try {
				IsSucess(MFStartup(MF_VERSION, MFSTARTUP_NOSOCKET));
			}
			catch(InternalException ex) {
				System.Diagnostics.Debug.WriteLine(ex.ToString());
				throw new InvalidOperationException();
			}
		}

		public static void Shutdown() {
			try {
				IsSucess(MFShutdown());
			}
			catch(InternalException ex) {
				System.Diagnostics.Debug.WriteLine(ex.ToString());
				throw new InvalidOperationException();
			}
		}

		public static System.Windows.Media.Imaging.BitmapSource CreateThumbnail(string file) {
			IMFAttributes attr = null;
			IMFMediaType mediaType = null;
			IMFSourceReader sourceReader = null;
			IMFSample smp = null;
			IMFMediaBuffer buf = null;
			try {
				// ファイル読み込み
				IsSucess(MFCreateAttributes(out attr, 1));
				IsSucess(attr.SetUINT32(MF_SOURCE_READER_ENABLE_VIDEO_PROCESSING, 1));
				IsSucess(MFCreateSourceReaderFromURL(file, attr, out sourceReader));

				// 読み取りをRGB32に設定
				IsSucess(MFCreateMediaType(out mediaType));
				IsSucess(mediaType.SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video));
				IsSucess(mediaType.SetGUID(MF_MT_SUBTYPE, MFVideoFormat_RGB32));
				IsSucess(sourceReader.SetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, IntPtr.Zero, mediaType));
				{
					// 情報取得
					IsSucess(sourceReader.SetStreamSelection(MF_SOURCE_READER_FIRST_VIDEO_STREAM, true));
					IsSucess(sourceReader.GetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, out var mt));
					var guid = new byte[16];
					IsSucess(mt.GetGUID(MF_MT_SUBTYPE, guid));
					{
						var st = new Guid(guid);
						if(st != MFVideoFormat_RGB32) {
							return null;
						}
					}
					PROPVARIANT frameSize = new PROPVARIANT();
					PROPVARIANT stride = new PROPVARIANT();
					IsSucess(mt.GetItem(MF_MT_FRAME_SIZE, ref frameSize));
					IsSucess(mt.GetItem(MF_MT_DEFAULT_STRIDE, ref stride));

					// 0ナノ秒目のサンプル取得
					var p = new PROPVARIANT() {
						vt = VT_I8,
						longVal = 0,
					};
					IsSucess(sourceReader.SetCurrentPosition(GUID_NULL, ref p));
					IsSucess(sourceReader.ReadSample(MF_SOURCE_READER_FIRST_VIDEO_STREAM, 0, out var _, out var f, out var _, out smp));
					// メディアタイプが違うので念のため再取得
					if((f & MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED) == MF_SOURCE_READERF_CURRENTMEDIATYPECHANGED) {
						IsSucess(sourceReader.GetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, out var mt2));
						IsSucess(mt2.GetGUID(MF_MT_SUBTYPE, guid));
						{
							var st = new Guid(guid);
							if(st != MFVideoFormat_RGB32) {
								return null;
							}
						}

						IsSucess(mt2.GetItem(MF_MT_FRAME_SIZE, ref frameSize));
						IsSucess(mt2.GetItem(MF_MT_DEFAULT_STRIDE, ref stride));
					}
					// バッファ取得ビットマップに変換
					IsSucess(smp.ConvertToContiguousBuffer(out buf));
					IsSucess(buf.Lock(out var ptr, out var max, out var current));
					try {
						var dst = new byte[current];
						Marshal.Copy(ptr, dst, 0, current);

						var width = (int)(frameSize.longVal >> 32);
						var height = (int)(frameSize.longVal & 0xffffffff); 
						var r = new System.Windows.Media.Imaging.WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Bgr32, null);
						r.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), dst, stride.intVal, 0);
						return r;
					}
					finally {
						IsSucess(buf.Unlock());
					}
				}
			}
			catch(InternalException ex) {
				System.Diagnostics.Debug.WriteLine("Media Foundation Error");
				System.Diagnostics.Debug.WriteLine(ex.ToString());
#if DEBUG
				// MF_E_INVALIDSTREAMNUMBER (0xC00D36B3);
				// MF_E_ATTRIBUTENOTFOUND (0xC00D36E6)
				if(System.Diagnostics.Debugger.IsAttached) {
					System.Diagnostics.Debugger.Break();
				}
#endif
				return null;
			}
			finally {
				static void release(object? o) {
					if(o != null) {
						Marshal.ReleaseComObject(o);
					}
				}

				release(buf);
				release(smp);
				release(sourceReader);
				release(mediaType);
				release(attr);
			}
		}


		private static void IsSucess(int r) {
			if(r != 0) {
				throw new InternalException(r);
			}
		}
	}
}
