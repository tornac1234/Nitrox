using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;
using NitroxTest.Patcher;

namespace NitroxPatcher.Patches.Dynamic;

[TestClass]
public class PDAScanner_Scan_PatchTest
{
    [TestMethod]
    public void Sanity()
    {
        Validate.NotNull(PDAScanner_Scan_Patch.INJECTION_OPERAND);
        Validate.NotNull(PDAScanner_Scan_Patch.INJECTION_OPERAND_2);

        List<CodeInstruction> instructions = PatchTestHelper.GenerateDummyInstructions(100);
        instructions.Add(new CodeInstruction(PDAScanner_Scan_Patch.INJECTION_OPCODE, PDAScanner_Scan_Patch.INJECTION_OPERAND));
        instructions.Add(new CodeInstruction(PDAScanner_Scan_Patch.INJECTION_OPCODE_2, PDAScanner_Scan_Patch.INJECTION_OPERAND_2));

        IEnumerable<CodeInstruction> result = PDAScanner_Scan_Patch.Transpiler(PDAScanner_Scan_Patch.TARGET_METHOD, instructions);

        Assert.AreEqual(instructions.Count + 4, result.Count());
    }

    [TestMethod]
    public void InjectionSanity()
    {
        IEnumerable<CodeInstruction> beforeInstructions = PatchTestHelper.GetInstructionsFromMethod(PDAScanner_Scan_Patch.TARGET_METHOD);
        IEnumerable<CodeInstruction> result = PDAScanner_Scan_Patch.Transpiler(PDAScanner_Scan_Patch.TARGET_METHOD, beforeInstructions);

        Assert.AreEqual(beforeInstructions.Count() + 4, result.Count());
    }
}
