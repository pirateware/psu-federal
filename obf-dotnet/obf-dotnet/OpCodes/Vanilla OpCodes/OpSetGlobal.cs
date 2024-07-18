using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpSetGlobal : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpSetGlobal);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) =>
            @"local INSTR = Instruction; --ELIGIBLE
local STKVAR = Stack;
local OPNUM_A = OP_A;
local ENVIDX = Constants[Instruction[OP_B]];
local TOMOVE = STKVAR[INSTR[OPNUM_A]];
Environment[ENVIDX] = TOMOVE;
--ORIGINAL:
Environment[Constants[Instruction[OP_B]]] = Stack[Instruction[OP_A]];";

        public override void Mutate(Instruction Instruction)
        {
            Instruction.ConstantType |= InstructionConstantType.RB;
        }
    };
};