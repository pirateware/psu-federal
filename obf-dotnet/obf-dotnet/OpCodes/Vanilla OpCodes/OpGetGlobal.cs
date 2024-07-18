using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpGetGlobal : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpGetGlobal);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) =>
            @"local ENVR = Environment; --ELIGIBLE
local OPNUM_A = OP_A;
local ENVRIDX = Constants[Instruction[OP_B]];
local TOMOVE = ENVR[ENVRIDX];
local STKIDX = Instruction[OPNUM_A];
Stack[STKIDX] = TOMOVE;
--ORIGINAL:
Stack[Instruction[OP_A]] = Environment[Constants[Instruction[OP_B]]];";

        public override void Mutate(Instruction Instruction)
        {
            Instruction.ConstantType |= InstructionConstantType.RB;
        }
    };
};