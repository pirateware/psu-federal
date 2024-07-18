using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpLoadK : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpLoadK);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) =>
            @"local OPNUM_A = OP_A; --ELIGIBLE
local TOMOVE = Constants[Instruction[OP_B]];
local INST_FIRST = Instruction[OPNUM_A];
Stack[INST_FIRST] = TOMOVE;
--ORIGINAL:
Stack[Instruction[OP_A]] = Constants[Instruction[OP_B]];";

        public override void Mutate(Instruction Instruction)
        {
            Instruction.ConstantType |= InstructionConstantType.RB;
        }
    };
};