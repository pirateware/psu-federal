using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpJump : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpJump);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) => 
            @"local INSTRC = Instruction; -- ELIGIBLE
local OPNUM_B = OP_B;
local INSTPTR = INSTRC[OPNUM_B];
InstructionPoint = INSTPTR;
--ORIGINAL:
InstructionPoint = Instruction[OP_B];";

        public override void Mutate(Instruction Instruction)
        {
            Instruction.B = Instruction.Chunk.InstructionMap[Instruction.References[0]];
        }
    };
};