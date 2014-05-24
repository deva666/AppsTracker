using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Security.AccessControl;
using System.ComponentModel;

namespace Task_Logger_Pro
{
    internal class WinAPI
    {
        #region custom struct
        [StructLayout( LayoutKind.Sequential )]
        internal struct KBDLLHOOKSTRUCT
        {
            internal int vkCode;
            internal int scanCode;
            internal int flags;
            int time;
            IntPtr dwExtraInfo;
        }

        [StructLayout( LayoutKind.Sequential )]
        internal struct MSLLHOOKSTRUCT
        {
            internal POINT pt;
            internal int mouseData;
            internal int flags;
            int time;
            IntPtr dwExtraInfo;
        }

       
        internal struct CURSORINFO
        {
            internal int cbSize;
            internal int flags;
            internal IntPtr hCursor;
            internal Point ptScreenPos;
        }

        internal struct ICONINFO
        {
            internal bool fIcon;
            internal int xHotspot;
            internal int yHotspot;
            internal IntPtr hbmMask;
            internal IntPtr hbmColor;
        }

        [StructLayout( LayoutKind.Sequential )]
        internal struct POINT
        {
            internal int X;
            internal int Y;

            internal POINT( int x, int y )
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point( POINT p )
            {
                return new System.Drawing.Point( p.X, p.Y );
            }

            public static implicit operator POINT( System.Drawing.Point p )
            {
                return new POINT( p.X, p.Y );
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        #region RECT Struct
        [StructLayout( LayoutKind.Sequential )]
        internal struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            internal RECT( RECT Rectangle )
                : this( Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom )
            {
            }
            internal RECT( int Left, int Top, int Right, int Bottom )
            {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            internal int X
            {
                get
                {
                    return _Left;
                }
                set
                {
                    _Left = value;
                }
            }
            internal int Y
            {
                get
                {
                    return _Top;
                }
                set
                {
                    _Top = value;
                }
            }
            internal int Left
            {
                get
                {
                    return _Left;
                }
                set
                {
                    _Left = value;
                }
            }
            internal int Top
            {
                get
                {
                    return _Top;
                }
                set
                {
                    _Top = value;
                }
            }
            internal int Right
            {
                get
                {
                    return _Right;
                }
                set
                {
                    _Right = value;
                }
            }
            internal int Bottom
            {
                get
                {
                    return _Bottom;
                }
                set
                {
                    _Bottom = value;
                }
            }
            internal int Height
            {
                get
                {
                    return _Bottom - _Top;
                }
                set
                {
                    _Bottom = value + _Top;
                }
            }
            internal int Width
            {
                get
                {
                    return _Right - _Left;
                }
                set
                {
                    _Right = value + _Left;
                }
            }
            internal Point Location
            {
                get
                {
                    return new Point( Left, Top );
                }
                set
                {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            internal Size Size
            {
                get
                {
                    return new Size( Width, Height );
                }
                set
                {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }

            public static implicit operator Rectangle( RECT Rectangle )
            {
                return new Rectangle( Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height );
            }
            public static implicit operator RECT( Rectangle Rectangle )
            {
                return new RECT( Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom );
            }
            public static bool operator ==( RECT Rectangle1, RECT Rectangle2 )
            {
                return Rectangle1.Equals( Rectangle2 );
            }
            public static bool operator !=( RECT Rectangle1, RECT Rectangle2 )
            {
                return !Rectangle1.Equals( Rectangle2 );
            }

            public override string ToString()
            {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            internal bool Equals( RECT Rectangle )
            {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals( object Object )
            {
                if ( Object is RECT )
                {
                    return Equals( ( RECT ) Object );
                }
                else if ( Object is Rectangle )
                {
                    return Equals( new RECT( ( Rectangle ) Object ) );
                }

                return false;
            }
        }
        #endregion

        internal enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }

        #endregion

        #region enums

        [Flags]
        internal enum ProcessAccessRights
        {
            PROCESS_CREATE_PROCESS = 0x0080, //  Required to create a process.
            PROCESS_CREATE_THREAD = 0x0002, //  Required to create a thread.
            PROCESS_DUP_HANDLE = 0x0040, // Required to duplicate a handle using DuplicateHandle.
            PROCESS_QUERY_INFORMATION = 0x0400, //  Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken, GetExitCodeProcess, GetPriorityClass, and IsProcessInJob).
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000, //  Required to retrieve certain information about a process (see QueryFullProcessImageName). A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION. Windows Server 2003 and Windows XP/2000:  This access right is not supported.
            PROCESS_SET_INFORMATION = 0x0200, //    Required to set certain information about a process, such as its priority class (see SetPriorityClass).
            PROCESS_SET_QUOTA = 0x0100, //  Required to set memory limits using SetProcessWorkingSetSize.
            PROCESS_SUSPEND_RESUME = 0x0800, // Required to suspend or resume a process.
            PROCESS_TERMINATE = 0x0001, //  Required to terminate a process using TerminateProcess.
            PROCESS_VM_OPERATION = 0x0008, //   Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
            PROCESS_VM_READ = 0x0010, //    Required to read memory in a process using ReadProcessMemory.
            PROCESS_VM_WRITE = 0x0020, //   Required to write to memory in a process using WriteProcessMemory.
            DELETE = 0x00010000, // Required to delete the object.
            READ_CONTROL = 0x00020000, //   Required to read information in the security descriptor for the object, not including the information in the SACL. To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
            SYNCHRONIZE = 0x00100000, //    The right to use the object for synchronization. This enables a thread to wait until the object is in the signaled state.
            WRITE_DAC = 0x00040000, //  Required to modify the DACL in the security descriptor for the object.
            WRITE_OWNER = 0x00080000, //    Required to change the owner in the security descriptor for the object.
            STANDARD_RIGHTS_REQUIRED = 0x000f0000,
            PROCESS_ALL_ACCESS = ( STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF ),//    All possible access rights for a process object.
        }

        internal enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUiAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        internal enum TokenElevationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        
        #endregion

        #region constants
        internal const int HWND_BROADCAST = 0xffff;
        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_LBUTTONUP = 0x0202;
        internal const int WM_LBUTTONDBLCLK = 0x0203;
        internal const int WM_RBUTTONDOWN = 0x0204;
        internal const int WM_RBUTTONUP = 0x0205;
        internal const int WM_RBUTTONDBLCLK = 0x0206;
        internal const int CSIDL_COMMON_STARTMENU = 0x16;
        internal const uint MAPVK_VK_TO_VSC = 0x00;
        internal const uint MAPVK_VSC_TO_VK = 0x01;
        internal const uint MAPVK_VK_TO_CHAR = 0x02;
        internal const uint MAPVK_VSC_TO_VK_EX = 0x03;
        internal const uint MAPVK_VK_TO_VSC_EX = 0x04;

        
        #endregion

        #region delegates
        public delegate IntPtr HookHandlerCallBack( int code, IntPtr wParam, IntPtr
 lParam );
       

        public delegate void WinEventCallBack( IntPtr hWinEventHook, uint eventType, IntPtr hWnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime );
        #endregion

        #region imported methods

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern bool GetWindowRect( IntPtr hWnd, ref RECT rect );

      
        [DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true )]
        internal static extern IntPtr GetForegroundWindow();


        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern IntPtr GetDC( IntPtr hwnd );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern IntPtr GetCursor();

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern bool GetCursorInfo( ref CURSORINFO cinfo );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern bool GetIconInfo( IntPtr hicon, ref ICONINFO iInfo );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern IntPtr CopyIcon( IntPtr hicon );

        [DllImport( "user32", CharSet = CharSet.Unicode, ThrowOnUnmappableChar= true )]
        internal static extern int GetWindowText( IntPtr hWnd, [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder lpString, int nMaxCount );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        internal static extern int GetWindowTextLength( IntPtr hWnd );

        [DllImport( "user32.dll", SetLastError = true )]
        internal static extern void GetWindowThreadProcessId( IntPtr hWnd,
            out uint lpdwProcessId );

        [DllImport( "user32.dll" )]
        internal static extern IntPtr SetWinEventHook( uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventCallBack lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags );

        [DllImport( "user32.dll" )]
        internal static extern bool UnhookWinEvent( IntPtr hWinEventHook );

        [DllImport( "kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
        internal static extern IntPtr GetModuleHandle( string lpModuleName );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        internal static extern IntPtr SetWindowsHookEx( int idHook, HookHandlerCallBack lpfn, IntPtr hMod, uint dwThreadId );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool UnhookWindowsHookEx( IntPtr hhk );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        internal static extern IntPtr CallNextHookEx( IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi )]
        internal static extern short GetKeyState( int keyCode );

        [DllImport( "user32.dll" )]
        internal static extern int ToAscii( uint uVirtKey, uint uScanCode, byte[] lpKeyState,
           byte[] lpwTransKey, uint fuState );

        [DllImport( "user32.dll", CharSet= CharSet.Unicode )]
        internal static extern int ToAscii( uint uVirtKey, uint uScanCode, byte[] lpKeyState,
           [Out] StringBuilder lpChar, uint uFlags );

        [DllImport( "user32.dll" )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool GetKeyboardState( byte[] lpKeyState );

        [DllImport( "user32.dll" )]
        internal static extern uint MapVirtualKeyEx( uint uCode, uint uMapType, IntPtr dwhkl );

        [DllImport( "user32.dll" )]
        internal static extern IntPtr GetKeyboardLayout( uint idThread );

        [DllImport( "user32.dll" )]
        internal static extern int ToUnicode( uint virtualKeyCode, uint scanCode, byte[] keyboardState,
            [Out, MarshalAs( UnmanagedType.LPWStr, SizeConst = 64 )]
StringBuilder receivingBuffer, int bufferSize, uint flags );

        [DllImport( "user32.dll" )]
        internal static extern int ToUnicodeEx( uint wVirtKey, uint wScanCode, byte[]
           lpKeyState, [Out, MarshalAs( UnmanagedType.LPWStr )] StringBuilder pwszBuff,
           int cchBuff, uint wFlags, IntPtr dwhkl );

        [DllImport( "advapi32.dll", SetLastError = true )]
        internal static extern bool GetKernelObjectSecurity( IntPtr Handle, int securityInformation,
            [Out] byte[] pSecurityDescriptor, uint nLength, out uint lpnLengthNeeded );

        [DllImport( "advapi32.dll", SetLastError = true )]
        internal static extern bool SetKernelObjectSecurity( IntPtr Handle, int securityInformation, [In] byte[] pSecurityDescriptor );

        [DllImport( "advapi32.dll", SetLastError = true )]
        internal static extern bool GetTokenInformation( IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength );

        [DllImport( "kernel32.dll" )]
        internal static extern IntPtr GetCurrentProcess();
        
        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        #endregion

        #region class methods

        internal static RawSecurityDescriptor GetProcessSecurityDescriptor( IntPtr processHandle )
        {
            const int DACL_SECURITY_INFORMATION = 0x00000004;
            byte[] psd = new byte[0];
            uint bufSizeNeeded;

            GetKernelObjectSecurity( processHandle, DACL_SECURITY_INFORMATION, psd, 0, out bufSizeNeeded );
            if ( bufSizeNeeded < 0 || bufSizeNeeded > short.MaxValue ) throw new Win32Exception();
            if ( !GetKernelObjectSecurity( processHandle, DACL_SECURITY_INFORMATION, psd = new byte[bufSizeNeeded], bufSizeNeeded, out bufSizeNeeded ) ) throw new Win32Exception();

            return new RawSecurityDescriptor( psd, 0 );
        }

        internal static void SetProcessSecurityDescriptor( IntPtr processHandle, RawSecurityDescriptor dacl )
        {
            const int DACL_SECURITY_INFORMATION = 0x00000004;
            byte[] rawsd = new byte[dacl.BinaryLength];
            dacl.GetBinaryForm( rawsd, 0 );
            if ( !SetKernelObjectSecurity( processHandle, DACL_SECURITY_INFORMATION, rawsd ) ) throw new Win32Exception();
        }


        #endregion

    }
}
