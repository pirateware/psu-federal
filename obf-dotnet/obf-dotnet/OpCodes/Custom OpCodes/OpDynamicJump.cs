using Obfuscator.Bytecode.IR;

namespace Obfuscator.Obfuscation.OpCodes
{
    public class OpDynamicJump : VOpCode
    {
        public override bool IsInstruction(Instruction Instruction) => (Instruction.OpCode == OpCode.OpDynamicJump);

        public override string GetObfuscated(ObfuscationContext ObfuscationContext) => 
            @"local STKVAR = Stack; -- ELIGIBLE
local OPNUM_A = OP_A;
local INSTRC = Instruction;
local TOMOVE = INSTRC[OPNUM_A];
InstructionPoint = STKVAR[TOMOVE];
--ORIGINAL:
InstructionPoint = Stack[Instruction[OP_A]];";
    };
};