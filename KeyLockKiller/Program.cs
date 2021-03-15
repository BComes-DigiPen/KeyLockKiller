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

// Now, with that legal software licensing mumbo jumbo out of the way, here's the program's source,
// in all of its unoptimized glory.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeyLockKiller
{
	internal class Program
	{
		private static void Init()
		{
#if (DEBUG)
			Console.Title = "[DEBUG] KeyLockKiller";
			Console.ResetColor();
#else
			Console.Title = "KeyLockKiller (Press ESC to exit)";
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.Black;
#endif
			Console.CursorVisible = false;
		}

		[STAThread]
		private static void Main()
		{
			Init();

			// Capture state of lock keys when program starts.
			bool initCaps = CapsOn();
			bool initNum = NumOn();
			bool initScroll = ScrollOn();

			// Loop until ESC is pressed while the window is in focus.
			while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
			{
				ResizeConsole();

				if (CapsOn() != initCaps)
				{
#if (DEBUG)
					Console.Clear();
					Console.Write("Caps Lock state changed to {0} reverting to {1}.", CapsOn(), initCaps);
#endif
					PushKey(Keys.CapsLock);
				}
				else if (NumOn() != initNum)
				{
#if (DEBUG)
					Console.Clear();
					Console.Write("Num Lock state changed to {0}, reverting to {1}.", NumOn(), initNum);
#endif
					PushKey(Keys.NumLock);
				}
				else if (ScrollOn() != initScroll)
				{
#if (DEBUG)
					Console.Clear();
					Console.Write("Scroll Lock state changed to {0}, reverting to {1}.", ScrollOn(), initScroll);
#endif
					PushKey(Keys.Scroll);
				}

				// Sleeping/pausing every 5 milliseconds significantly lowers CPU usage.
				Thread.Sleep(5);

				// Minimize memory usage with garbage collection, slightly increases CPU usage
				MinimizeRAMUse();
			}
			Environment.Exit(0);
		}

		private static void ResizeConsole()
		{
#if (DEBUG)
			Console.SetWindowSize(55, 1);
#else
			Console.SetWindowSize(42, 1);
#endif
			try
			{
				Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
			}
			catch { }
		}

		#region Helper Functions For Key State Checking

		private static bool CapsOn()
		{
			return Control.IsKeyLocked(Keys.CapsLock);
		}

		private static bool NumOn()
		{
			return Control.IsKeyLocked(Keys.NumLock);
		}

		private static bool ScrollOn()
		{
			return Control.IsKeyLocked(Keys.Scroll);
		}

		#endregion

		#region Keyboard Input Stuff (From some StackOverflow/Exchange thing, I forget where.)

		[DllImport("user32.dll")]
		private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo = 0);

		private static void PushKey(Keys keyCode)
		{
			keybd_event((byte)keyCode, 0x45, 0x1);
			keybd_event((byte)keyCode, 0x45, 0x1 | 0x2);
		}

		#endregion

		#region Memory Optimization Stuff (From another StackOverflow/Exchange thing, I also forget where.)

		private static void MinimizeRAMUse()
		{
			GC.Collect(GC.MaxGeneration);
			GC.WaitForPendingFinalizers();
			SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,
				(UIntPtr)0xFFFFFFFF, (UIntPtr)0xFFFFFFFF);
		}

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetProcessWorkingSetSize(IntPtr process,
			UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);

		#endregion
	}
}