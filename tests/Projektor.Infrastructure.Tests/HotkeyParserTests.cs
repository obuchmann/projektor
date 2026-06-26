using Projektor.Infrastructure.Hotkey;
using SharpHook.Data;

namespace Projektor.Infrastructure.Tests;

public sealed class HotkeyParserTests
{
    [Fact]
    public void TryParse_AltSpace_ParsesModifierAndKey()
    {
        var ok = HotkeyParser.TryParse("Alt+Space", out var modifiers, out var key);

        Assert.True(ok);
        Assert.Equal(EventMask.Alt, modifiers);
        Assert.Equal(KeyCode.VcSpace, key);
    }

    [Fact]
    public void TryParse_CtrlShiftLetter_ParsesAllModifiers()
    {
        var ok = HotkeyParser.TryParse("Ctrl+Shift+P", out var modifiers, out var key);

        Assert.True(ok);
        Assert.True((modifiers & EventMask.Ctrl) != EventMask.None);
        Assert.True((modifiers & EventMask.Shift) != EventMask.None);
        Assert.Equal(KeyCode.VcP, key);
    }

    [Fact]
    public void TryParse_KeyOnly_NoModifiers()
    {
        var ok = HotkeyParser.TryParse("Enter", out var modifiers, out var key);

        Assert.True(ok);
        Assert.Equal(EventMask.None, modifiers);
        Assert.Equal(KeyCode.VcEnter, key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Alt+NotAKey")]
    [InlineData("Bogus+Space")]
    public void TryParse_Invalid_ReturnsFalse(string input)
    {
        var ok = HotkeyParser.TryParse(input, out _, out _);

        Assert.False(ok);
    }

    [Fact]
    public void Matches_ExactModifiers_True()
    {
        Assert.True(HotkeyParser.Matches(EventMask.Alt, EventMask.Alt | EventMask.LeftAlt));
    }

    [Fact]
    public void Matches_ExtraModifier_False()
    {
        // Hotkey requires only Alt, but Ctrl is also held → no match.
        Assert.False(HotkeyParser.Matches(EventMask.Alt, EventMask.Alt | EventMask.Ctrl));
    }

    [Fact]
    public void Matches_MissingModifier_False()
    {
        Assert.False(HotkeyParser.Matches(EventMask.Alt, EventMask.None));
    }
}
