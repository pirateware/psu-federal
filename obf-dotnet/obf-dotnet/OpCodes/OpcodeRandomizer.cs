using System;
using System.Collections.Generic;
using System.Text;

namespace obf_dotnet.OpCodes
{
    // written by federal 16.05.2021
    // c=====================================3 << my coc
    public static class OpcodeRandomizer
    {
        public static bool CanRandomize(string op)
        {
            if (op.Contains("ELIGIBLE") && !op.Contains("IS_SUPER_OPERATOR"))
            {
                return true;
            } else
            {
                return false;
            }
            // ignore:
            if (op.Contains("for") || op.Contains("function") || op.Contains("repeat") || op.Contains("while") || op.Contains("end"))
            {
                return false;
            }
            return true;
        }
        public static string RandomizeOpcode(string op)
        {
            string org = op;
            
            if (op.Contains("--ORIGINAL:"))
            {
                org = op.Split("--ORIGINAL:")[1]; // original opcode
                op = op.Split("--ORIGINAL:")[0]; // modified opcode (for randomization)
               
            }

            if (obf_dotnet.Obfuscator.FedsLexerModule._rnd.Next(1, 3) == 2)
            {
                return org;
            }

            string ret = "";

            string[] x = obf_dotnet.Obfuscator.FedsLexerModule.StripLocals(op);

            string locals = x[0];
            
            op = x[1];

            string[] l = op.Split("\n");
            List<string> lines = new List<string>();
            for (int i = 0; i < l.Length; i++)
            {
                string a = l[i];
                if (a.Replace(" ", "").Length > 2)
                {
                    lines.Add(a);
                }
            }

            if (lines.Count > 8) //big
            {
                Console.WriteLine("Skipped 1 random opcode");
                return org;
            }

            List<string> fakelines = new List<string>();



            fakelines.Add("Stack[Instruction[OP_A]] = Stack[Instruction[OP_B]] + Instruction[OP_C];");
            fakelines.Add("if(Stack[Instruction[OP_A]] == Instruction[OP_C])then InstructionPoint=InstructionPoint+1;else InstructionPoint=Instruction[OP_B];end;");
            fakelines.Add("Stack[Instruction[OP_A]]=Environment[Instruction[OP_B]];");
            fakelines.Add("InstructionPoint=Instruction[OP_B];");
            fakelines.Add("Stack[Instruction[OP_A]]=Stack[Instruction[OP_B]];");
            fakelines.Add("Stack[Instruction[OP_A]] = Instruction[OP_B]- Instruction[OP_C];");
            fakelines.Add("if(Instruction[OP_A] ~= Instruction[OP_C])then InstructionPoint=InstructionPoint+1;else InstructionPoint=Instruction[OP_B];end;");
            fakelines.Add("local B = Instruction[OP_B]; local Result = Stack[B]; for Index = B + 1, Instruction[OP_C] do Result = Result .. Stack[Index]; end; Stack[Instruction[OP_A]] = Result;");
            fakelines.Add("Stack[Instruction[OP_A]] = #Stack[Instruction[OP_B]];");


            string[] fakes = Obfuscator.Generation.AdvancedCFGenerator.Shuffle<string>(fakelines.ToArray());

            ret = locals + "\n" + Obfuscator.Generation.AdvancedCFGenerator.ForeskinConfusion.ConfuseChunks(lines.ToArray(), fakes);


            Console.WriteLine("Used 1 random opcode");

            return ret;
        }

    }
}
