using SharpDX;

namespace GenerateInterop
{
    class Program
    {
        static void Main(string[] args)
        {
            SharpDX.DynamicInterop.Generate(new DynamicInterop.CalliSignature[0], true);
        }
    }
}
