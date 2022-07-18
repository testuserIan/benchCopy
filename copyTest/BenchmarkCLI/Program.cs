using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace TestPerf
{
    class Program
    {

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false), SuppressUnmanagedCodeSecurity]
        public static unsafe extern void* CopyMemory(void* dest, void* src, ulong count);

        [StructLayout(LayoutKind.Sequential)]
        struct CopyBlock
        {
            unsafe fixed long field1[4];
        }

        static unsafe void CustomCopy(void * dest, void* src, int count)
        {
            int block;

            block = count >> 3;

            long* pDest = (long*)dest;
            long* pSrc = (long*)src;

            for (int i = 0; i < block; i++)
            {
                *pDest = *pSrc; pDest++; pSrc++;
            }
            dest = pDest;
            src = pSrc;
            count = count - (block << 3);

            if (count > 0)
            {
                byte* pDestB = (byte*) dest;
                byte* pSrcB = (byte*) src;
                for (int i = 0; i < count; i++)
                {
                    *pDestB = *pSrcB; pDestB++; pSrcB++;
                }
            }
        }


        static unsafe void Main(string[] args)
        {
            int blockSize = 4;

            Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "BlockSize", "cpblk", "memcpy", "Array.Copy","CustomCopy","Marshal.Copy");

            for (int j = 0; j < 20; j++, blockSize *= 2)
            {
                byte[] dest = new byte[blockSize];

                byte[] src = new byte[blockSize];
                for (int i = 0; i < blockSize; i++)
                    src[i] = (byte) i;


                fixed (void* pDest = &dest[0])
                fixed (void* pSrc = &src[0])
                {

                    IntPtr pDestPtr = (IntPtr) pDest;
                    IntPtr pSrcPtr = (IntPtr) pSrc;


                    int count = (1 << 26)/blockSize;

                    Stopwatch watch = new Stopwatch();

                    watch.Reset();
                    watch.Start();
                    for (int i = 0; i < count; i++)
                    {
                        SharpDX.Interop.memcpy(pDest, pSrc, blockSize);
                        SharpDX.Interop.memcpy(pSrc, pDest, blockSize);
                        SharpDX.Interop.memcpy(pDest, pSrc, blockSize);
                        SharpDX.Interop.memcpy(pSrc, pDest, blockSize);
                        SharpDX.Interop.memcpy(pDest, pSrc, blockSize);
                        SharpDX.Interop.memcpy(pSrc, pDest, blockSize);
                        SharpDX.Interop.memcpy(pDest, pSrc, blockSize);
                        SharpDX.Interop.memcpy(pSrc, pDest, blockSize);
                        SharpDX.Interop.memcpy(pDest, pSrc, blockSize);
                        SharpDX.Interop.memcpy(pSrc, pDest, blockSize);
                    }
                    watch.Stop();
                    long memCopyTime = watch.ElapsedMilliseconds;

                    watch.Reset();
                    watch.Start();
                    for (int i = 0; i < count; i++)
                    {
                        CopyMemory(pDest, pSrc, (ulong) blockSize);
                        CopyMemory(pSrc, pDest, (ulong) blockSize);
                        CopyMemory(pDest, pSrc, (ulong) blockSize);
                        CopyMemory(pSrc, pDest, (ulong) blockSize);
                        CopyMemory(pDest, pSrc, (ulong) blockSize);
                        CopyMemory(pSrc, pDest, (ulong) blockSize);
                        CopyMemory(pDest, pSrc, (ulong) blockSize);
                        CopyMemory(pSrc, pDest, (ulong) blockSize);
                        CopyMemory(pDest, pSrc, (ulong) blockSize);
                        CopyMemory(pSrc, pDest, (ulong) blockSize);
                    }
                    watch.Stop();
                    long copyMemoryTime = watch.ElapsedMilliseconds;


                    watch.Reset();
                    watch.Start();
                    for (int i = 0; i < count; i++)
                    {
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                        Array.Copy(dest, src, blockSize);
                    }
                    watch.Stop();
                    long arrayCopyTime = watch.ElapsedMilliseconds;

                    watch.Reset();
                    watch.Start();
                    for (int i = 0; i < count; i++)
                    {
                        CustomCopy(pDest, pSrc, blockSize);
                        CustomCopy(pSrc, pDest, blockSize);
                        CustomCopy(pDest, pSrc, blockSize);
                        CustomCopy(pSrc, pDest, blockSize);
                        CustomCopy(pDest, pSrc, blockSize);
                        CustomCopy(pSrc, pDest, blockSize);
                        CustomCopy(pDest, pSrc, blockSize);
                        CustomCopy(pSrc, pDest, blockSize);
                        CustomCopy(pDest, pSrc, blockSize);
                        CustomCopy(pSrc, pDest, blockSize);
                    }
                    watch.Stop();
                    long customCopyTime = watch.ElapsedMilliseconds;

                    watch.Reset();
                    watch.Start();
                    for (int i = 0; i < count; i++)
                    {
                        Marshal.Copy(src, 0, pDestPtr, blockSize);
                        Marshal.Copy(dest, 0, pSrcPtr, blockSize);
                        Marshal.Copy(src, 0, pDestPtr, blockSize);
                        Marshal.Copy(dest, 0, pSrcPtr, blockSize);
                        Marshal.Copy(src, 0, pDestPtr, blockSize);
                        Marshal.Copy(dest, 0, pSrcPtr, blockSize);
                        Marshal.Copy(src, 0, pDestPtr, blockSize);
                        Marshal.Copy(dest, 0, pSrcPtr, blockSize);
                        Marshal.Copy(src, 0, pDestPtr, blockSize);
                        Marshal.Copy(dest, 0, pSrcPtr, blockSize);
                    }
                    watch.Stop();
                    long marshalCopyTime = watch.ElapsedMilliseconds;


                    double memFactor = count*10.0*blockSize/0.001/(1024*1024);

                    double memCpyOut = memFactor/memCopyTime;
                    double copyMemoryOut = memFactor/copyMemoryTime;
                    double arrayOut = memFactor/arrayCopyTime;
                    double customOut = memFactor/customCopyTime;
                    double marshalOut = memFactor / marshalCopyTime;

                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", blockSize, (long) memCpyOut, (long) copyMemoryOut,
                                      (long)arrayOut, (long)customOut, (long)marshalOut);
                }
            }


            //var bytecode = ShaderBytecode.FromFile("ergon.fxo");

            //var effect = new EffectBinaryFormat();

            //effect.LoadEffect(bytecode);



        }
    }
}
