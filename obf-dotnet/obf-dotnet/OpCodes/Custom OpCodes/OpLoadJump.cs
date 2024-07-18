using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpLoadJump : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpLoadJump);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) => 
            @"local INSTVAR = Instruction; --ELIGIBLE
local OPNUM_A = OP_A;
local OPNUM_B = OP_B;
local STKIDX = INSTVAR[OPNUM_A];
local TOMOVE = INSTVAR[OPNUM_B];
Stack[STKIDX] = TOMOVE;
--ORIGINAL:
Stack[Instruction[OP_A]] = Instruction[OP_B];";

        public override void Mutate(Instruction Instruction)
        {
            Instruction.B = Instruction.Chunk.InstructionMap[Instruction.References[0]]; Instruction.InstructionType = InstructionType.AsBx;
        }
    };
};