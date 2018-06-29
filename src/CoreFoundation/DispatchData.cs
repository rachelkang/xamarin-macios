//
// DispatchData.cs: Support for dispatch_data_t APIs
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2018 Microsoft, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Runtime.InteropServices;
using System.Threading;
using ObjCRuntime;
using Foundation;

namespace CoreFoundation {

	public class DispatchData : DispatchObject {
		static IntPtr lib, free;
		static DispatchData ()
		{
			lib = Dlfcn.dlopen (Constants.libcLibrary, 0);
			free = Dlfcn.dlsym (lib, "_dispatch_data_destructor_free");
		}
		
		public DispatchData (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		[DllImport (Constants.libcLibrary)]
		extern static IntPtr dispatch_data_create (IntPtr buffer, ulong size, IntPtr dispatchQueue, IntPtr destructor);

		//
		// This constructor will do it for now, but we should support a constructor
		// that allows custom releasing of the buffer
		//
		public static DispatchData FromByteBuffer (byte [] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			var b = Marshal.AllocHGlobal (buffer.Length);
			Marshal.Copy (buffer, 0, b, buffer.Length);
			var dd = dispatch_data_create (b, (ulong) buffer.Length, IntPtr.Zero, destructor: free);
			return new DispatchData (dd, owns: true);
		}

		//
		// This will create a DispatchData by making a copy of the provided buffer
		// 
		public static DispatchData FromBuffer (IntPtr buffer, ulong size)
		{
			if (buffer == null)
				throw new ArgumentNullException (nameof (buffer));
			var dd = dispatch_data_create (buffer, size, IntPtr.Zero, destructor: IntPtr.Zero);
			return new DispatchData (dd, owns: true);
		}
		
		[DllImport (Constants.libcLibrary)]
		extern static IntPtr dispatch_data_get_size (IntPtr handle);

		public long Size => (long) dispatch_data_get_size (handle);

		[DllImport (Constants.libcLibrary)]
		extern static IntPtr dispatch_data_create_map (IntPtr handle, out IntPtr bufferPtr, out ulong size);

		public DispatchData CreateMap (out IntPtr bufferPtr, out ulong size)
		{
			var nh = dispatch_data_create_map (handle, out bufferPtr, out size);
			return new DispatchData (nh, owns: true);
		}

		[DllImport (Constants.libcLibrary)]
		extern static IntPtr dispatch_data_create_concat (IntPtr h1, IntPtr h2);

		public static DispatchData Concat (DispatchData data1, DispatchData data2)
		{
			if (data1 == null)
				throw new ArgumentNullException (nameof (data1));
			if (data2 == null)
				throw new ArgumentNullException (nameof (data2));
			
			return new DispatchData (dispatch_data_create_concat (data1.handle, data2.handle), owns: true);
		}

		[DllImport (Constants.libcLibrary)]
		extern static IntPtr dispatch_data_create_subrange (IntPtr handle, ulong offset, ulong size);

		public DispatchData CreateSubrange (ulong offset, ulong size)
		{
			return new DispatchData (dispatch_data_create_subrange (handle, offset, size), owns: true);
		}
	}
}
