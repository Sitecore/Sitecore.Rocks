// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[ComVisible(false), Guid("B61FC35B-EEBF-4dec-BFF1-28A2DD43C38F")]
public interface SVsUIShell
{
}

[StructLayout(LayoutKind.Sequential, Pack = 4), ComConversionLoss]
public struct VSOPENFILENAMEW
{
    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint lStructSize;

    [ComConversionLoss]
    public IntPtr hwndOwner;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCWSTR"), MarshalAs(UnmanagedType.LPWStr)]
    public string pwzDlgTitle;

    public IntPtr pwzFileName;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint nMaxFileName;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCWSTR"), MarshalAs(UnmanagedType.LPWStr)]
    public string pwzInitialDir;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCWSTR"), MarshalAs(UnmanagedType.LPWStr)]
    public string pwzFilter;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint nFilterIndex;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint nFileOffset;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint nFileExtension;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint dwHelpTopic;

    [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")]
    public uint dwFlags;
}

[ComImport, Guid("BEC804F7-F5DE-4F3E-8EBB-DAB26649F33F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IVsEnumGuids
{
    [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    int Next([In, ComAliasName("OLE.ULONG")] uint celt, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] Guid[] rgelt, [ComAliasName("OLE.ULONG")] out uint pceltFetched);

    [PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    int Skip([In, ComAliasName("OLE.ULONG")] uint celt);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Reset();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Clone([MarshalAs(UnmanagedType.Interface)] out IVsEnumGuids ppEnum);
}

[ComImport, Guid("2B70EA30-51F2-48BB-ABA8-051946A37283"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IVsUIShell5
{
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetOpenFileNameViaDlgEx2([In, Out, ComAliasName("VsShell.VSOPENFILENAMEW"), MarshalAs(UnmanagedType.LPArray)] VSOPENFILENAMEW[] openFileName, [In, ComAliasName("OLE.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string HelpTopic, [In, ComAliasName("OLE.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string openButtonLabel);

    [return: ComAliasName("Microsoft.VisualStudio.Shell.Interop.VS_RGBA")]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    uint GetThemedColor([In, ComAliasName("OLE.REFGUID")] ref Guid colorCategory, [In, ComAliasName("OLE.LPCOLESTR"), MarshalAs(UnmanagedType.LPWStr)] string colorName, [In, ComAliasName("Microsoft.VisualStudio.Shell.Interop.THEMEDCOLORTYPE")] uint colorType);

    [return: MarshalAs(UnmanagedType.BStr)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    string GetKeyBindingScope([In, ComAliasName("OLE.REFGUID")] ref Guid keyBindingScope);

    [return: MarshalAs(UnmanagedType.Interface)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    IVsEnumGuids EnumKeyBindingScopes();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    bool ThemeWindow([In] IntPtr hwnd);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    IntPtr CreateThemedImageList([In] IntPtr hImageList, [In, ComAliasName("VsShell.COLORREF")] uint crBackground);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ThemeDIBits([In, ComAliasName("OLE.DWORD")] uint dwBitmapLength, [Out, ComAliasName("TextManager.BYTE"), MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] pBitmap, [In, ComAliasName("OLE.DWORD")] uint dwPixelWidth, [In, ComAliasName("OLE.DWORD")] uint dwPixelHeight, [In] bool fIsTopDownBitmap, [In, ComAliasName("VsShell.COLORREF")] uint crBackground);
}
