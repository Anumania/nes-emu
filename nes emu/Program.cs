using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace nes_emu
{
    class Program {
        public static byte accumulator;
        public static byte X;
        public static byte Y;
        public static bool[] statusFlags = new bool[7]; //NV?BDIZC
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
                MethodInfo meth = typeof(Executor).GetMethod("x" + PRG[PC].ToString("X"), BindingFlags.NonPublic);
                if (meth == null)
                {
                    throw new Exception("instruction 0x" + PRG[PC].ToString("X") + " is either invalid or not implemented");
                }
                else
                {
                    meth.Invoke(null,null);
                    //Disassembler disassembler = new(PRG);

                    PC++;
                }
            }
        }
        static class Executor //nested class that handles all the operations
        {
            static byte Pop() //not really pop
            {
                PC++;
                return PRG[PC];
            }
            static void ZeroCheck(byte check)
            {
                if (check == 0x00)
                {
                    statusFlags[5] = true;
                }
                else
                {
                    statusFlags[5] = false;
                }
            
            static void x00() //BRK
            {
                Console.WriteLine("break");
            }
            static void x01() //ORA indirect, x
            {
                
            }

        }

    }
    
    static class oldInstructions //test run trying to figure things out.
    {
        static byte Pop() //not really pop
        {
            Program.PC++;
            return Program.PRG[Program.PC];
        }

        static void ZeroCheck(byte check)
        {
            if (check == 0x00)
            {
                    statusFlags[5] = true;
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
            Program.stackPTR = Program.X;
        }

        static void x8A() //TXA
        {
            Program.accumulator = Program.X;
        }
        static void x8D() //STA absolute
        {
            int addr = Pop() * 0x100;
            addr += Pop();
            Program.RAM[addr] = Program.accumulator;
        }
        static void x8E() //STX absolute
        {
            int addr = Pop() * 0x100;
            addr += Pop();
            Program.RAM[addr] = Program.X;
        }

        static void x95() //STA zero page, x
        {
            
            Program.RAM[Pop() + Program.X] = Program.accumulator;
            
        }

        static void x9D() //STA Absolute,X
        {
            int addr = Pop() * 0x100;
            addr += Pop();
            addr += Program.X;
            Program.RAM[addr] = Program.accumulator;
        }
        static void xA2() //LDX immediete
        {
            Program.X = Pop();
        }
        static void xA9()//LDA immediete
        { 
            Program.accumulator = Pop();
        }
        static void xD0() //BNE
        {
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
            Program.X++;
            ZeroCheck(Program.X);
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
  
    class Disassembler
    {
        ushort PC = 0;
        static string[] cc00 = new string[] { null, "BIT", "JMP", "JMPABS", "STY", "LDY", "CPY", "CPX" };
        static string[] cc01 = new string[] { "ORA", "AND", "EOR", "ADC", "STA", "LDA", "CMP", "SBC" };
        static string[] cc10 = new string[] { "ASL", "ROL", "LSR", "ROR", "STX", "LDX", "DEC", "INC" };
        byte[] PRG;
        public Disassembler(byte[] _PRG)
        {
            PRG = _PRG;
            List<string> disassembly = new();
            while(PC < PRG.Length)
            {
                disassembly.Add(Disassemble(ref PC, PRG));
            }
            Console.WriteLine("Done");
        }
        
        public static string Disassemble(ref ushort PC, byte[] PRG)
        {
            byte instruction = PRG[PC];
            instruction = (byte)(instruction - instruction % (byte)0b00100000);
            instruction = (byte)(instruction >> 5);
            string result = PC.ToString("X")+"(" + PRG[PC].ToString("X") +"):  ";
            switch (instruction % (byte)0b00000100)
            {
                case 0b00:
                    string thing = "";
                    
                    switch((instruction / 0b100) % 0b001_000){
                        case 0b000:
                            thing += "($" + PRG[PC].ToString("X") + ",X)";
                            break;
                        case 0b001:
                            thing += "$" + PRG[PC].ToString("X");
                            break;
                        case 0b010:
                            thing += "#$" + PRG[PC].ToString("X");
                            break;
                        case 0b011:
                            thing += "$" + PRG[PC + 1].ToString("X") + PRG[PC].ToString("X");
                            PC++;
                            break;
                        case 0b100:
                            thing += "$(" + PRG[PC].ToString("X") + "), Y";
                            break;
                        case 0b101:
                            thing += "$" + PRG[PC].ToString("X") + ",X";
                            break;
                        case 0b110:
                            thing += "$" + PRG[PC + 1].ToString("X") + PRG[PC].ToString("X") + ",X";
                            PC++;
                            break;
                        case 0b111:
                            thing += "$" + PRG[PC + 1].ToString("X") + PRG[PC].ToString("X") + ",Y";
                            PC++;
                            break;
                        default: //should never happen
                            break;
                    }
                    result += cc00[instruction] + " " + thing;
                    break;
                case 0b01:
                    result += cc01[instruction];
                    break;
                case 0b10:
                    result += cc10[instruction];
                    break;
                default:
                    break;
            }
            PC++;
            return result;
        }
    }
}

