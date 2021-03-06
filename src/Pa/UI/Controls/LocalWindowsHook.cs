// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2005-2015, SIL International.
// <copyright from='2005' to='2015' company='SIL International'>
//		Copyright (c) 2005-2015, SIL International.
//    
//		This software is distributed under the MIT License, as specified in the LICENSE.txt file.
// </copyright> 
#endregion
// 
// ***********************************************************************
//  LocalWindowsHook class
//  Dino Esposito, summer 2002 
// 
//  Provide a general infrastructure for using Win32 
//  hooks in .NET applications
// 
// ***********************************************************************

//
// I took this class from the example at http://msdn.microsoft.com/msdnmag/issues/02/10/cuttingedge
// and made a couple of minor tweaks to it - dpk
//

using System;
using System.Runtime.InteropServices;

namespace SIL.Pa.UI.Controls
{
	#region Class HookEventArgs
	public class HookEventArgs : EventArgs
	{
		public int HookCode;	// Hook code
		public IntPtr wParam;	// WPARAM argument
		public IntPtr lParam;	// LPARAM argument
	}
	#endregion

	#region Enum HookType
	// Hook Types
	public enum HookType : int
	{
		WH_JOURNALRECORD = 0,
		WH_JOURNALPLAYBACK = 1,
		WH_KEYBOARD = 2,
		WH_GETMESSAGE = 3,
		WH_CALLWNDPROC = 4,
		WH_CBT = 5,
		WH_SYSMSGFILTER = 6,
		WH_MOUSE = 7,
		WH_HARDWARE = 8,
		WH_DEBUG = 9,
		WH_SHELL = 10,
		WH_FOREGROUNDIDLE = 11,
		WH_CALLWNDPROCRET = 12,		
		WH_KEYBOARD_LL = 13,
		WH_MOUSE_LL = 14
	}
	#endregion

	#region Class LocalWindowsHook
	public class LocalWindowsHook
	{
		// ************************************************************************
		// Filter function delegate
		// ************************************************************************
		public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);
		
		// ************************************************************************
		// Internal properties
		// ************************************************************************
		protected IntPtr m_hhook = IntPtr.Zero;
		protected HookProc m_filterFunc = null;
		protected HookType m_hookType;

		// ************************************************************************
		// Event delegate
		// ************************************************************************
		public delegate void HookEventHandler(object sender, HookEventArgs e);

		// ************************************************************************
		// Event: HookInvoked 
		// ************************************************************************
		public event HookEventHandler HookInvoked;
		protected void OnHookInvoked(HookEventArgs e)
		{
			if (HookInvoked != null)
				HookInvoked(this, e);
		}

		// ************************************************************************
		// Class constructor(s)
		// ************************************************************************
		public LocalWindowsHook(HookType hook)
		{
			m_hookType = hook;
			m_filterFunc = this.CoreHookProc; 
		}

		public LocalWindowsHook(HookType hook, HookProc func)
		{
			m_hookType = hook;
			m_filterFunc = func; 
		}		

		// ************************************************************************
		// Default filter function
		// ************************************************************************
		protected int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code < 0)
				return CallNextHookEx(m_hhook, code, wParam, lParam);

			// Let clients determine what to do
			HookEventArgs e = new HookEventArgs();
			e.HookCode = code;
			e.wParam = wParam;
			e.lParam = lParam;
			OnHookInvoked(e);

			// Yield to the next hook in the chain
			return CallNextHookEx(m_hhook, code, wParam, lParam);
		}

		// ************************************************************************
		// Install the hook
		// ************************************************************************
		public void Install()
		{
			m_hhook = SetWindowsHookEx(m_hookType, m_filterFunc,
				IntPtr.Zero, GetCurrentThreadId());
		}

		// ************************************************************************
		// Uninstall the hook
		// ************************************************************************
		public void Uninstall()
		{
			UnhookWindowsHookEx(m_hhook); 
			m_hhook = IntPtr.Zero;
		}
		
		// ************************************************************************
		public bool IsInstalled
		{
			get{ return m_hhook != IntPtr.Zero; }
		}

		#endregion
		
		#region OS-specific stuff
#if !__MonoCS__
		[DllImport("user32.dll")]
		protected static extern IntPtr SetWindowsHookEx(HookType code, 
			HookProc func,
			IntPtr hInstance,
			int threadID);
#else
		protected static IntPtr SetWindowsHookEx(HookType code, 
			HookProc func,
			IntPtr hInstance,
			int threadID)
		{
			Console.WriteLine("Warning--using unimplemented method SetWindowsHookEx"); // FIXME Linux
			return(IntPtr.Zero);
		}
#endif

#if !__MonoCS__
		[DllImport("user32.dll")]
		protected static extern int UnhookWindowsHookEx(IntPtr hhook); 
#else
		protected static int UnhookWindowsHookEx(IntPtr hhook)
		{
			Console.WriteLine("Warning--using unimplemented method UnhookWindowsHookEx"); // FIXME Linux
			return(0);
		}
#endif

#if !__MonoCS__
		[DllImport("user32.dll")]
		protected static extern int CallNextHookEx(IntPtr hhook, 
			int code, IntPtr wParam, IntPtr lParam);
#else
		protected static int CallNextHookEx(IntPtr hhook, 
			int code, IntPtr wParam, IntPtr lParam)
		{
			Console.WriteLine("Warning--using unimplemented method CallNextHookEx"); // FIXME Linux
			return(0);
		}
#endif
	
#if !__MonoCS__
		[DllImport("kernel32.dll")]
		public static extern int GetCurrentThreadId();
#else
		public static int GetCurrentThreadId()
		{
			Console.WriteLine("Warning--using unimplemented method GetCurrentThreadId"); // FIXME Linux
			return(0);
		}
#endif
	}
		#endregion
}
