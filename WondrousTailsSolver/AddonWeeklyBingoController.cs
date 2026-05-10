using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace WondrousTailsSolver;

public unsafe class AddonWeeklyBingoController : IDisposable {
    private const string AddonName = "WeeklyBingo";
    private const string InstructionOriginalSegment = "\u7A7A\u767D\u5904\u8D34\u4E0A\u5370\u82B1";
    private const string InstructionReplacementSegment = "\u7A7A\u767D\u5904\u8D34\u4E0A\u5370\u82B111111111111111111";
    private const string StoryLineSegment = "\u6545\u4E8B\u7EBF";
    private const string StoryLineInstructionSegment = "\u5370\u82B1\u8D34\u51FA";
    private const string RewardNpcSegment = "\u5E2D\u6D1B\u00B7\u963F\u91CC\u4E9A\u73C0";
    private const string RemainingSpaceSegment = "\u5C1A\u6709\u53EF\u8D34\u7A7A\u95F4";
    private const string AllStickersSegment = "\u8D34\u5B8C9\u4E2A\u5370\u82B1";
    private const string ProbabilityPrefix = "\u6982\u7387\uFF1A";
    private const string AveragePrefix = "\u91CD\u6392\uFF1A";
    private const string LegacyProbabilityPrefix = "\u8FDE\u7EBF\u6982\u7387\uFF1A";
    private const string LegacyAveragePrefix = "\u91CD\u6392\u5E73\u5747\uFF1A";

    private uint instructionTextNodeId;
    private string? instructionOriginalText;
    private ushort instructionOriginalHeight;
    private TextFlags instructionOriginalFlags;
    private bool hasInstructionLayout;
    private bool disposed;

    public AddonWeeklyBingoController(IDalamudPluginInterface pluginInterface) {
        DalamudServices.Initialize(pluginInterface);

        DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, AddonName, OnAddonEvent);
        DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, AddonName, OnAddonEvent);
        DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, AddonName, OnAddonEvent);
        DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, AddonName, OnAddonEvent);
        DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, AddonName, OnAddonEvent);

        var currentAddon = GetOpenAddon();
        if (currentAddon is not null) {
            AddonRefresh(currentAddon);
        }
    }

    public void Dispose() {
        if (disposed) {
            return;
        }

        DalamudServices.AddonLifecycle.UnregisterListener(OnAddonEvent);
        ClearInstructionState();
        disposed = true;
    }

    private void OnAddonEvent(AddonEvent type, AddonArgs args) {
        var addon = (AddonWeeklyBingo*)args.Addon.Address;

        switch (type) {
            case AddonEvent.PostSetup:
                AddonRefresh(addon);
                return;

            case AddonEvent.PreFinalize:
                // Finalize callbacks are too late to safely mutate addon text nodes.
                ClearInstructionState();
                return;

            case AddonEvent.PostRefresh or AddonEvent.PostRequestedUpdate or AddonEvent.PostUpdate:
                AddonRefresh(addon);
                return;
        }
    }

    private void AddonRefresh(AddonWeeklyBingo* addon) {
        foreach (var index in Enumerable.Range(0, 16)) {
            System.PerfectTails.GameState[index] = PlayerState.Instance()->IsWeeklyBingoStickerPlaced(index);
        }

        UpdateInstructionText(addon);
    }

    private void UpdateInstructionText(AddonWeeklyBingo* addon) {
        var instructionNode = GetInstructionTextNode(addon);
        if (instructionNode is null) {
            return;
        }

        var currentText = SeString.Parse(instructionNode->NodeText).TextValue;
        if (string.IsNullOrEmpty(currentText)) {
            return;
        }

        var baseText = NormalizeInstructionText(currentText);
        if (!LooksLikeInstructionText(baseText)) {
            return;
        }

        CaptureInstructionState(instructionNode, currentText, baseText);

        instructionNode->TextFlags |= TextFlags.MultiLine;

        var (probabilityLine, averageLine) = System.PerfectTails.GetInlineDisplayLines();
        var replacedText = BuildInstructionDisplayText(baseText, probabilityLine, averageLine);
        var desiredHeight = GetDesiredInstructionHeight(baseText, replacedText, instructionNode->LineSpacing);
        if (desiredHeight > 0 && instructionNode->GetHeight() != desiredHeight) {
            instructionNode->SetHeight(desiredHeight);
        }

        if (!string.Equals(replacedText, currentText, StringComparison.Ordinal)) {
            instructionNode->SetText(BuildInstructionDisplayBytes(baseText));
        }
    }

    private void RestoreInstructionText(AddonWeeklyBingo* addon) {
        if (instructionTextNodeId == 0 || string.IsNullOrEmpty(instructionOriginalText)) {
            return;
        }

        var instructionNode = addon->GetTextNodeById(instructionTextNodeId);
        if (instructionNode is null) {
            return;
        }

        instructionNode->SetText(instructionOriginalText);

        if (instructionOriginalHeight > 0) {
            instructionNode->SetHeight(instructionOriginalHeight);
        }

        if (instructionOriginalFlags != 0) {
            instructionNode->TextFlags = instructionOriginalFlags;
        }
    }

    private void ClearInstructionState() {
        instructionTextNodeId = 0;
        instructionOriginalText = null;
        instructionOriginalHeight = 0;
        instructionOriginalFlags = 0;
        hasInstructionLayout = false;
    }

    private AtkTextNode* GetInstructionTextNode(AddonWeeklyBingo* addon) {
        if (instructionTextNodeId != 0) {
            var cachedNode = addon->GetTextNodeById(instructionTextNodeId);
            if (cachedNode is not null) {
                return cachedNode;
            }
        }

        foreach (var node in addon->UldManager.Nodes) {
            if (node.Value is null || node.Value->Type is not NodeType.Text) {
                continue;
            }

            var candidate = (AtkTextNode*)node.Value;
            if (!IsInstructionNode(candidate)) {
                continue;
            }

            instructionTextNodeId = candidate->AtkResNode.NodeId;
            return candidate;
        }

        return null;
    }

    private static string NormalizeInstructionText(string text) {
        var normalized = text.Replace(InstructionReplacementSegment, InstructionOriginalSegment, StringComparison.Ordinal);
        var lines = normalized
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.StartsWith(ProbabilityPrefix, StringComparison.Ordinal)
                        && !line.StartsWith(AveragePrefix, StringComparison.Ordinal)
                        && !line.StartsWith(LegacyProbabilityPrefix, StringComparison.Ordinal)
                        && !line.StartsWith(LegacyAveragePrefix, StringComparison.Ordinal))
            .ToArray();

        return string.Join("\r", lines);
    }

    private static string BuildInstructionDisplayText(string baseText, string probabilityLine, string averageLine) {
        var originalLines = baseText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();

        if (originalLines.Length == 0) {
            lines.Add(probabilityLine);
            lines.Add(averageLine);
            return string.Join("\r", lines);
        }

        foreach (var line in originalLines) {
            lines.Add(line);
        }

        lines.Add(probabilityLine);
        lines.Add(averageLine);

        return string.Join("\r", lines);
    }

    private static byte[] BuildInstructionDisplayBytes(string baseText) {
        var originalLines = baseText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var builder = new SeStringBuilder();

        for (var index = 0; index < originalLines.Length; index++) {
            if (index > 0) {
                builder.AddText("\r");
            }

            builder.AddText(originalLines[index]);
        }

        if (originalLines.Length > 0) {
            builder.AddText("\r");
        }

        builder.Append(System.PerfectTails.GetInlineDisplaySeString());
        return builder.Encode();
    }

    private void CaptureInstructionState(AtkTextNode* instructionNode, string currentText, string baseText) {
        if (ContainsInjectedLines(currentText)
            && hasInstructionLayout
            && string.Equals(instructionOriginalText, baseText, StringComparison.Ordinal)) {
            return;
        }

        instructionOriginalText = baseText;
        instructionOriginalHeight = instructionNode->GetHeight();
        instructionOriginalFlags = (TextFlags)instructionNode->TextFlags;
        hasInstructionLayout = true;
    }

    private ushort GetDesiredInstructionHeight(string baseText, string displayText, byte lineSpacing) {
        if (!hasInstructionLayout || instructionOriginalHeight == 0) {
            return 0;
        }

        var baseLineCount = CountDisplayLines(baseText);
        var displayLineCount = CountDisplayLines(displayText);
        var extraLineCount = Math.Max(0, displayLineCount - baseLineCount);
        var effectiveLineSpacing = lineSpacing > 0 ? lineSpacing : (byte)16;
        return (ushort)(instructionOriginalHeight + (effectiveLineSpacing * extraLineCount));
    }

    private static int CountDisplayLines(string text)
        => Math.Max(1, text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length);

    private static bool ContainsInjectedLines(string text)
        => text.Contains(ProbabilityPrefix, StringComparison.Ordinal)
           || text.Contains(AveragePrefix, StringComparison.Ordinal)
           || text.Contains(LegacyProbabilityPrefix, StringComparison.Ordinal)
           || text.Contains(LegacyAveragePrefix, StringComparison.Ordinal);

    private static bool LooksLikeInstructionText(string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            return false;
        }

        var normalized = text.Replace(InstructionReplacementSegment, InstructionOriginalSegment, StringComparison.Ordinal);
        return normalized.Contains(InstructionOriginalSegment, StringComparison.Ordinal)
               || normalized.Contains(RewardNpcSegment, StringComparison.Ordinal)
               || normalized.Contains(RemainingSpaceSegment, StringComparison.Ordinal)
               || normalized.Contains(AllStickersSegment, StringComparison.Ordinal)
               || (normalized.Contains(StoryLineInstructionSegment, StringComparison.Ordinal)
                   && normalized.Contains(StoryLineSegment, StringComparison.Ordinal));
    }

    private static bool IsInstructionNode(AtkTextNode* node) {
        if (node is null || node->NodeText.AsSpan().Length == 0) {
            return false;
        }

        var text = SeString.Parse(node->NodeText).TextValue;
        return LooksLikeInstructionText(text)
               || ContainsInjectedLines(text);
    }

    private static AddonWeeklyBingo* GetOpenAddon() {
        var address = DalamudServices.GameGui.GetAddonByName(AddonName).Address;
        return address == nint.Zero ? null : (AddonWeeklyBingo*)address;
    }
}
