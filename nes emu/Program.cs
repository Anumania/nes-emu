using System;


namespace nes_emu
{
    class Program { 

        static void Main(string[] args)
        {
            
        }

        static void Parse()
        {
            
        }
    }

    class Instructions
    {
        void x00() //BRK
        {
            Console.WriteLine("brk"); //(bork)
        }
        
        void x01() //ORA Indirect, X
        {

        }

    }

    static class Registers
    {
        static byte accumulator;
        static byte indexX;
        static byte indexY;
        static bool[] statusFlags = new bool[7];
        static byte stackPTR;
        static ushort PC;
    }
}

