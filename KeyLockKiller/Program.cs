// Copyright (C) 2021 BComes-DigiPen
//
// Redistribution and use in source and binary forms, with or without modification, are permitted
// provided that the following conditions are met:
//
// 1.	Redistributions of source code must retain the above copyright notice, this list of
// conditions and the following disclaimer.
//
// 2.	Redistributions in binary form must reproduce the above copyright notice, this list of
// conditions and the following disclaimer in the documentation and/or other materials provided with
// the distribution.
//
// 3.	Neither the name of the copyright holder nor the names of its contributors may be used to
// endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyLockKiller
{
	internal class Program
	{
		#region Main Program Stuff

		// Capture state of lock keys on program start
		private static readonly bool[] initKeys = { CLOn(), NLOn(), SLOn() };

		private static void Init()
		{
			Console.Title = "KeyLockKiller";
#if (DEBUG)
			Console.Title += " [DEBUG | " + Convert.ToInt32(initKeys[0]) + ", " + Convert.ToInt32(initKeys[1]) + ", " + Convert.ToInt32(initKeys[2]) + ']';
#else
			Console.Title += " (Press ESC to exit.)";
#endif
			Console.CursorVisible = false;

			// This should fix issues where the program wouldn't work (change key states) when other
			// more resource-intensive applications or processes were active.
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

			// Call this twice because calling it once doesn't make the console show one line
			ResizeConsole();
			ResizeConsole();

			NoResize();
		}

		[STAThread]
		private static void Main()
		{
			Init();

			// Loop until ESC is pressed while the window is in focus.
			while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
			{
				CallGC();

				bool[] currKeys = { CLOn(), NLOn(), SLOn() };
				if (currKeys[0] != initKeys[0])
				{
#if (DEBUG)
					Console.Clear();
					Console.Write("Caps changed to {0}. Reverting to {1}.", currKeys[0], initKeys[0]);
#endif
					KeyIn(Keys.CapsLock);
				}
				if (currKeys[1] != initKeys[1])
				{
#if (DEBUG)
					Console.Clear();
					Console.Write("Num changed to {0}. Reverting to {1}.", currKeys[1], initKeys[1]);
#endif
					KeyIn(Keys.NumLock);
				}
				if (currKeys[2] != initKeys[2])
				{
#if (DEBUG)
					Console.Clear();
					Console.Write("Scroll changed to {0}. Reverting to {1}.", currKeys[2], initKeys[2]);
#endif
					KeyIn(Keys.Scroll);
				}

				// Sleeping/pausing every 50 milliseconds significantly lowers CPU usage.
				Thread.Sleep(50);
			}
			Environment.Exit(0);
		}

		#endregion

		#region Memory Optimization Functions

		private static void CallGC()
		{
			GC.Collect(GC.MaxGeneration);
			GC.WaitForPendingFinalizers();
			SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (UIntPtr)0xFFFFFFFF, (UIntPtr)0xFFFFFFFF);
		}

		[DllImport("kernel32.dll")]
		private static extern bool SetProcessWorkingSetSize(IntPtr process, UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);

		#endregion

		#region Console Window Functions

		[DllImport("user32.dll")]
		private static extern int DeleteMenu(IntPtr hMenu, uint nPosition = 61440, uint wFlags = 0);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert = false);

		private static void NoResize()
		{
			IntPtr handle = GetConsoleWindow();
			IntPtr sysMenu = GetSystemMenu(handle);
			if (handle != IntPtr.Zero)
			{
				DeleteMenu(sysMenu);
				DeleteMenu(sysMenu, 61488);
			}
		}

		private static void ResizeConsole()
		{
#if (DEBUG)
			Console.SetWindowSize(44, 1);
#else
			Console.SetWindowSize(42, 1);
#endif
			Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
		}

		#endregion

		#region Keyboard Input Functions

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan = 69, uint dwFlags = 1, uint dwExtraInfo = 0);

		private static void KeyIn(Keys k)
		{
			keybd_event((byte)k);
			keybd_event((byte)k, 69, 3);
		}

		#endregion

		#region Key State Check Functions

		private static bool CLOn()
		{
			return Control.IsKeyLocked(Keys.CapsLock);
		}

		private static bool NLOn()
		{
			return Control.IsKeyLocked(Keys.NumLock);
		}

		private static bool SLOn()
		{
			return Control.IsKeyLocked(Keys.Scroll);
		}

		#endregion
	}
}