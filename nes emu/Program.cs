using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace nes_emu
{
    class Program {
        public static byte accumulator;
        public static byte indexX;
        public static byte indexY;
        public static bool[] statusFlags = new bool[7]; //NV-BDIZC
        public static byte stackPTR;
        public static ushort PC = 0;
        public static byte[] RAM = new byte[0xffff];
        public static byte[] PRG;
        public static byte[] CHR;
        static void Main(string[] args)
        {
            byte[] rom = File.ReadAllBytes("./rom.nes");
            Parser.Parse(rom, ref PRG, ref CHR);
            //PRG = 
            for(; ;)  //main loop
            {
                MethodInfo meth = typeof(Instructions).GetMethod("x" + PRG[PC].ToString("X"), BindingFlags.NonPublic|BindingFlags.Static);
                if (meth == null)
                {
                    throw new Exception("instruction 0x" + PRG[PC].ToString("X") + " is either invalid or not implemented");
                }
                else
                {
                    meth.Invoke(null,null);
                    PC++;
                }
            }
        }

    }

    static class Instructions
    {
        static byte Pop() //not really pop
        {
            return Program.PRG[Program.PC];
        }

        static void ZeroCheck(byte check)
        {
            if (check == 0x00)
            {
                Program.statusFlags[5] = true;
            }
            else
            {
                Program.statusFlags[5] = false;
            }
        }
        static void x00() //BRK
        {
            Console.WriteLine("brk"); //(bork)
        }
        
        static void x01() //ORA Indirect, X
        {

        }

        
        static void x78() //SEI set interrupt
        {
            Console.WriteLine("your balls are sussy");
        }

        static void x9A() //TXS
        {
            Program.stackPTR = Program.indexX;
        }

        static void x8A() //TXA
        {
            Program.accumulator = Program.indexX;
        }
        static void x8D() //STA absolute
        {
            Program.PC++;
            int addr = Pop() * 0x100;
            Program.PC++;
            addr += Pop();
            Program.RAM[addr] = Program.accumulator;
        }
        static void x8E() //STX absolute
        {
            Program.PC++;
            int addr = Pop() * 0x100;
            Program.PC++;
            addr += Pop();
            Program.RAM[addr] = Program.indexX;
        }

        static void x95() //STA zero page, x
        {
            
            Program.PC++;
            Program.RAM[Pop() + Program.indexX] = Program.accumulator;
            
        }

        static void x9D() //STA Absolute,X
        {
            Program.PC++;
            int addr = Pop() * 0x100;
            Program.PC++;
            addr += Pop();
            addr += Program.indexX;
            Program.RAM[addr] = Program.accumulator;
        }
        static void xA2() //LDX immediete
        {
            Program.PC++;
            Program.indexX = Pop();
        }
        static void xA9()//LDA immediete
        {
            Program.PC++;
            Program.accumulator = Pop();
        }
        static void xD0() //BNE
        {
            Program.PC++;
            sbyte dest = (sbyte)Pop();
            if (Program.statusFlags[5])
            {
                Program.PC = (ushort)(Program.PC + (short)dest);
            }
        }
        static void xEA() //nop
        {
            
            return;
        }
        
        static void xE8() //inx
        {
            Program.indexX++;
            ZeroCheck(Program.indexX);
        }

    }

    class Parser
    {
        public static void Parse(byte[] rom, ref byte[] PRG, ref byte[] CHR)
        {
            CheckMagic(rom);
            int prgSize = 0x4000 * rom[4];
            int chrSize = 0x2000 * rom[5];
            PRG = rom[16 .. prgSize];
            //CHR = rom[16 + prgSize..24]
        }

        static void CheckMagic(byte[] rom)
        {
            if (Enumerable.SequenceEqual(rom[0..4], new byte[] { 0x4E, 0x45, 0x53, 0x1a }))
            {
                return;
            }
            else
            {
                throw new Exception("header incorrect");
            }
        }

    }
}

