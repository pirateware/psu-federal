using Obfuscator.Bytecode.IR;
using System.Collections.Generic;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpSuperOperator : VOpCode
    {
        public List<Instruction> Instructions = new List<Instruction>();
        public List<VOpCode> Virtuals = new List<VOpCode>();

        public bool IsWritten = false;

        public override bool IsInstruction(Instruction Instruction) => (false);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext)
        {
            string Obfuscated = "";
            bool canSelfcall = obf_dotnet.Obfuscator.FedsLexerModule._rnd.Next(0, 4) == 1;
            int InstructionPoint = 0;

            foreach (Instruction Instruction in Instructions)
            {
                var VirtualOpcode = Virtuals[InstructionPoint];

                Instruction.UpdateRegisters();

                if (!Instruction.CustomInstructionData.Mutated)
                    VirtualOpcode?.Mutate(Instruction);

                Instruction.CustomInstructionData.Mutated = true;
                string opcode = VirtualOpcode.GetObfuscated(ObfuscationContext) + " Instruction = Instruction[OP_E]; ";

                if (ObfuscationContext.OpcodeRandomization && obf_dotnet.OpCodes.OpcodeRandomizer.CanRandomize(opcode) && obf_dotnet.Obfuscator.FedsLexerModule._rnd.Next(1,5) == 2) // dont use opcode randomizer in super operators --> it seriously fucks it up 
                  // update 22.05: Well this should be enabled.
                {
                   // System.Console.WriteLine("is able to mutate rn");
                    string newObfuscated = obf_dotnet.OpCodes.OpcodeRandomizer.RandomizeOpcode(opcode);

                    opcode = newObfuscated;
                } else
                {
                    if (opcode.Contains("--ORIGINAL:"))
                    {
                        opcode = opcode.Split("--ORIGINAL:")[1]; // gets the unmodified opcode
                    }
                }


                if (canSelfcall && obf_dotnet.Obfuscator.FedsLexerModule._rnd.Next(0,5) == 1 && !opcode.Contains("return") && !opcode.Contains("..."))
                {
                    opcode = $"local antibeautifybs = (_{ obf_dotnet.Obfuscator.FedsLexerModule._rnd.Next(0, 200)});(function() {opcode} end){{}};";
                   // System.Console.WriteLine("Generated 1 anti beautify expression");
                }

                Obfuscated += opcode;

                Instruction.WrittenVIndex = 0;

                InstructionPoint++;
            };

            return (Obfuscated + " Instruction = Instruction[OP_E]; \n--IS_SUPER_OPERATOR\n");
        }
    };
}