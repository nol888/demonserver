/*
+---------------------------------------------------------------------------+
|	Demon - dAmn Emulator													|
|===========================================================================|
|	Copyright © 2008 Nol888													|
|===========================================================================|
|	This file is part of Demon.												|
|																			|
|	Demon is free software: you can redistribute it and/or modify			|
|	it under the terms of the GNU Affero General Public License as			|
|	published by the Free Software Foundation, either version 3 of the		|
|	License, or (at your option) any later version.							|
|																			|
|	This program is distributed in the hope that it will be useful,			|
|	but WITHOUT ANY WARRANTY; without even the implied warranty of			|
|	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the			|
|	GNU Affero General Public License for more details.						|
|																			|
|	You should have received a copy of the GNU Affero General Public License|
|	along with this program.  If not, see <http://www.gnu.org/licenses/>.	|
|																			|
|===========================================================================|
|	> $Date$
|	> $Revision$
|	> $Author$
+---------------------------------------------------------------------------+
*/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace DemonServer
{
	public static class Console
	{
		public delegate void ControlEventHandler(ConsoleEvent consoleEvent);
		public static event ControlEventHandler ControlEvent;
		private static ControlEventHandler eventHandler;

		public enum ConsoleEvent
		{
			CtrlC = 0,CtrlBreak = 1,CtrlClose = 2,CtrlLogoff = 5,CtrlShutdown = 6
		}

		static Console() {
			Console.eventHandler = new ControlEventHandler(Handler);
			Console.SetConsoleCtrlHandler(eventHandler, true);

			GC.SuppressFinalize(Console.eventHandler);
		}

		#region Public Static Properties
		public static string TimestampFormat = "";
		#endregion

		#region Public Methods
		public static void ShowInfo(string Text, string LineTerminator)
		{
			StringBuilder Formatted = new StringBuilder();
			if (TimestampFormat != "") Formatted.Append(DateTime.Now.ToString(TimestampFormat));
			Formatted.Append("[ \x1B[37mINFO\x1B[0m ]    ");
			Formatted.Append(Text);
			Formatted.Append(LineTerminator);
			WriteParseANSI(Formatted.ToString());
		}
		public static void ShowInfo(string Text) { ShowInfo(Text, "\n"); }

		public static void ShowStatus(string Text, string LineTerminator)
		{
			StringBuilder Formatted = new StringBuilder();
			if (TimestampFormat != "") Formatted.Append(DateTime.Now.ToString(TimestampFormat));
			Formatted.Append("[ \x1B[32mSTATUS\x1B[0m ]  ");
			Formatted.Append(Text);
			Formatted.Append(LineTerminator);
			WriteParseANSI(Formatted.ToString());
		}
		public static void ShowStatus(string Text) { ShowStatus(Text, "\n"); }

		public static void ShowWarning(string Text, string LineTerminator)
		{
			StringBuilder Formatted = new StringBuilder();
			if (TimestampFormat != "") Formatted.Append(DateTime.Now.ToString(TimestampFormat));
			Formatted.Append("[ \x1B[33mWARNING\x1B[0m ] ");
			Formatted.Append(Text);
			Formatted.Append(LineTerminator);
			WriteParseANSI(Formatted.ToString());
		}
		public static void ShowWarning(string Text) { ShowWarning(Text, "\n"); }

		public static void ShowError(string Text, string LineTerminator)
		{
			StringBuilder Formatted = new StringBuilder();
			if (TimestampFormat != "") Formatted.Append(DateTime.Now.ToString(TimestampFormat));
			Formatted.Append("[ \x1B[31mERROR\x1B[0m ]   ");
			Formatted.Append(Text);
			Formatted.Append(LineTerminator);
			WriteParseANSI(Formatted.ToString());
		}
		public static void ShowError(string Text) { ShowError(Text, "\n"); }

#if DEBUG
		public static void ShowDebug(string Text, string LineTerminator)
		{
			StringBuilder Formatted = new StringBuilder();
			if (TimestampFormat != "") Formatted.Append(DateTime.Now.ToString(TimestampFormat));
			Formatted.Append("[ \x1B[36mDEBUG\x1B[0m ]   ");
			Formatted.Append(Text);
			Formatted.Append(LineTerminator);
			WriteParseANSI(Formatted.ToString());
		}
		public static void ShowDebug(string Text) { ShowDebug(Text, "\n"); }
#else
		public static void ShowDebug(string Text, string LineTerminator) { }
		public static void ShowDebug(string Text) { }
#endif

		public static void WriteParseANSI(string Text)
		{
			lock (System.Console.Out)
			{
				char[] Characters = Text.ToCharArray();
				string ToWrite = "";
				string Sequence = "";
				for (int i = 0; i < Characters.Length; i++)
				{
					if (Characters[i] == "\x1B".ToCharArray()[0])
					{
						System.Console.Write(ToWrite);
						for (Sequence = ToWrite = ""; Characters[i] != "m".ToCharArray()[0]; i++)
						{
							Sequence += Characters[i];
						}
						switch (Sequence.Trim().ToLower())
						{
							case "\x1B[0": System.Console.ResetColor(); break;
							case "\x1B[30": System.Console.ForegroundColor = ConsoleColor.Black; break;
							case "\x1B[31": System.Console.ForegroundColor = ConsoleColor.Red; break;
							case "\x1B[32": System.Console.ForegroundColor = ConsoleColor.Green; break;
							case "\x1B[33": System.Console.ForegroundColor = ConsoleColor.Yellow; break;
							case "\x1B[34": System.Console.ForegroundColor = ConsoleColor.Blue; break;
							case "\x1B[35": System.Console.ForegroundColor = ConsoleColor.Magenta; break;
							case "\x1B[36": System.Console.ForegroundColor = ConsoleColor.Cyan; break;
							case "\x1B[37": System.Console.ForegroundColor = ConsoleColor.White; break;
							case "\x1B[39": System.Console.ForegroundColor = ConsoleColor.Gray; break;
						}
						++i;
					}
					ToWrite += Characters[i];
				}
				System.Console.Write(ToWrite);
			}
		}

		public static void ClearLine()
		{
			int Width = System.Console.WindowWidth - 1;
			string Spaces;
			for (Spaces = " "; Spaces.Length < Width; Spaces += " ") { continue; }
			Console.WriteParseANSI("\r" + Spaces + "\r");
		}

		public static void Pause()
		{
			lock (System.Console.In)
			{
				Console.ShowInfo("Press any key to continue...");
				System.Console.ReadKey(true);
			}
		}

		#endregion

		#region Private Methods
		private static void Handler(ConsoleEvent consoleEvent)
		{
			if (ControlEvent != null)
				ControlEvent(consoleEvent);
		}
		#endregion

		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);
	}
}
