/*-------------------------------------------------------------------------------------------------------
 File Name: Rs232.cs
 Function : Win32 API RS232C Serial Comunication Class Definition
---------------------------------------------------------------------------------------------------------
 Author: HAL Tokyo Advanced Robotics Dep.
---------------------------------------------------------------------------------------------------------
 Copyleft(c) 2015 HAL Tokyo Advanced Robotics Dep.
-------------------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace _HandMotionAirVR
{
	[StructLayout(LayoutKind.Sequential)]
	public struct COMMTIMEOUTS
	{
		public uint ReadIntervalTimeout;
		public uint ReadTotalTimeoutMultiplier;
		public uint ReadTotalTimeoutConstant;
		public uint WriteTotalTimeoutMultiplier;
		public uint WriteTotalTimeoutConstant;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DCB
	{
		public uint DCBlength;
		public uint BaudRate;
		public uint fBinary;
		public uint fParity;
		public uint fOutxCtsFlow;
		public uint fOutxDsrFlow;
		public uint fDtrControl;
		public uint fDsrSensitivit;
		public uint fTXContinueOnXoff;
		public uint fOutX;
		public uint fInX;
		public uint fErrorChar;
		public uint fNull;
		public uint fRtsControl;
		public uint fAbortOnError;
		public uint fDummy2;
		public ushort wReserved;
		public ushort XonLim;
		public ushort XoffLim;
		public byte ByteSize;
		public byte Parity;
		public byte StopBits;
		public char XonChar;
		public char XoffChar;
		public char ErrorChar;
		public char EofChar;
		public char EvtChar;
		public ushort wReserved1;
	};
	public class Rs232
	{
		#region Native Methos and Declaration
		const uint GENERIC_READ = 0x80000000;
		const uint GENERIC_WRITE = 0x40000000;
		const uint GENERIC_EXECUTE = 0x20000000;
		const uint GENERIC_ALL = 0x10000000;
		const uint CREATE_NEW = 1;
		const uint CREATE_ALWAYS = 2;
		const uint OPEN_EXISTING = 3;
		const uint OPEN_ALWAYS = 4;
		const uint TRUNCATE_EXISTING = 5;

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe IntPtr CreateFile(
			string FileName, // file name
			uint DesiredAccess, // access mode
			uint ShareMode, // share mode
			uint SecurityAttributes, // Security Attributes
			uint CreationDisposition, // how to create
			uint FlagsAndAttributes, // file attributes
			int hTemplateFile // handle to template file
		);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool ReadFile(
			IntPtr hFile, // handle to file
			void* pBuffer, // data buffer
			int NumberOfBytesToRead, // number of bytes to read
			int* pNumberOfBytesRead, // number of bytes read
			int Overlapped // overlapped buffer
		);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool WriteFile(
		IntPtr hFile, // handle to file
		void* pBuffer, // data buffer
		int nNumberOfBytesToWrite, // number of bytes to be written to the file
		int* lpNumberOfBytesWritten, // number of bytes written
		int Overlapped // overlapped buffer
	);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool CloseHandle(
			IntPtr hObject // handle to object
		);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool GetCommState(
		IntPtr hFile,
		ref DCB lpDCB);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool SetCommState(
		IntPtr hFile,
		ref DCB lpDCB);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool SetCommTimeouts(
			IntPtr hFile,
			ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool GetCommTimeouts(
			IntPtr hFile,
			ref COMMTIMEOUTS lpCommTimeouts);
		#endregion

		IntPtr handle;
		public Rs232()
		{

		}

		public bool Open(string comPort)
		{
			handle = CreateFile(
			comPort,
			GENERIC_READ | GENERIC_WRITE,
			0,
			0,
			OPEN_EXISTING,
			0,
			0);

			//				if (handle != IntPtr.Zero)
			if (handle.ToInt32() != -1)
				return true;
			else
				return false;
		}

		public unsafe int Read(byte[] buffer, int index, int count)
		{

			if (handle == IntPtr.Zero)
				return 0;

			int n = 0;
			fixed (byte* p = buffer)
			{

				if (!ReadFile(handle, p + index, count, &n, 0))
					return 0;
			}

			return n;
		}

		public unsafe int Write(byte[] buffer, int index, int count)
		{
			if (handle == IntPtr.Zero)
				return 0;

			int n = 0;
			fixed (byte* p = buffer)
			{
				if (!WriteFile(handle, p + index, count, &n, 0))
					return 0;
			}
			return n;
		}

		public unsafe bool Init(uint BaudRate, byte ByteSize, byte Parity, byte StopBits)
		{
			if (handle.ToInt32() == -1)
				return false;

			// Init the com state
			DCB dcb = new DCB();
			if (!GetCommState(handle, ref dcb))
				return false;

			dcb.BaudRate = BaudRate;
			dcb.ByteSize = ByteSize;
			dcb.Parity = Parity;
			dcb.StopBits = StopBits;

			if (!SetCommState(handle, ref dcb))
				return false;

			// Init the com timeouts
			COMMTIMEOUTS Commtimeouts = new COMMTIMEOUTS();
			if (!GetCommTimeouts(handle, ref Commtimeouts))
				return false;

			Commtimeouts.ReadIntervalTimeout = 600;
			if (!SetCommTimeouts(handle, ref Commtimeouts))
				return false;

			return true;
		}

		public bool Close()
		{
			// close file handle
			return CloseHandle(handle);
		}
	}

}
