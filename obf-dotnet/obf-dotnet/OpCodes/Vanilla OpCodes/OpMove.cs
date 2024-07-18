using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpMove : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpMove);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) =>
            @"local INSTRTABLE = Instruction; --ELIGIBLE
local OPNUM_B = OP_B;
local OPNUM_A = OP_A;
local STKVAR = Stack;
local TOMOVE = STKVAR[INSTRTABLE[OPNUM_B]];
local STKIDX = INSTRTABLE[OPNUM_A];
if HASDONE ~= 1 then Stack[STKIDX] = TOMOVE; HASDONE = 1; end;
--ORIGINAL:
Stack[Instruction[OP_A]] = Stack[Instruction[OP_B]];";
    };
};