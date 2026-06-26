using System.Text;
using Projektor.Core.Models;

namespace Projektor.Infrastructure.Config;

/// <summary>
/// Serializes <see cref="ProjektorConfig"/> to git-diffable, hand-editable TOML using
/// array-of-table header blocks (<c>[[action_templates]]</c>, <c>[[projects]]</c>, …).
/// Tomlyn 2.3.2's high-level serializer only emits inline tables, which collapse onto a
/// single growing line — so writing is done here while loading stays on Tomlyn's parser.
/// </summary>
internal static class TomlConfigWriter
{
    public static string Write(ProjektorConfig config)
    {
        var sb = new StringBuilder();

        sb.AppendLine("[settings]");
        KeyValue(sb, "hotkey", config.Settings.Hotkey);
        KeyValue(sb, "theme", config.Settings.Theme);

        foreach (var template in config.ActionTemplates)
        {
            sb.AppendLine();
            sb.AppendLine("[[action_templates]]");
            KeyValue(sb, "id", template.Id);
            KeyValue(sb, "name", template.Name);
            if (template.CommandWindows is not null)
                KeyValue(sb, "command_windows", template.CommandWindows);
            if (template.CommandLinux is not null)
                KeyValue(sb, "command_linux", template.CommandLinux);
            if (template.Icon is not null)
                KeyValue(sb, "icon", template.Icon);
            if (template.Terminal)
                BoolValue(sb, "terminal", template.Terminal);
        }

        foreach (var project in config.Projects)
        {
            sb.AppendLine();
            sb.AppendLine("[[projects]]");
            KeyValue(sb, "name", project.Name);
            KeyValue(sb, "path", project.Path);
            KeyValue(sb, "source", project.Source.ToString().ToLowerInvariant());
            StringArray(sb, "disabled_templates", project.DisabledTemplateIds);

            foreach (var action in project.CustomActions)
            {
                sb.AppendLine();
                sb.AppendLine("[[projects.actions]]");
                KeyValue(sb, "id", action.Id);
                KeyValue(sb, "name", action.Name);
                KeyValue(sb, "command", action.Command);
                if (action.Terminal)
                    BoolValue(sb, "terminal", action.Terminal);
            }
        }

        foreach (var root in config.ScanRoots)
        {
            sb.AppendLine();
            sb.AppendLine("[[scan_roots]]");
            KeyValue(sb, "path", root.Path);
        }

        return sb.ToString();
    }

    private static void KeyValue(StringBuilder sb, string key, string value) =>
        sb.Append(key).Append(" = ").AppendLine(Quote(value));

    private static void BoolValue(StringBuilder sb, string key, bool value) =>
        sb.Append(key).Append(" = ").AppendLine(value ? "true" : "false");

    private static void StringArray(StringBuilder sb, string key, IReadOnlyList<string> values)
    {
        sb.Append(key).Append(" = [");
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(Quote(values[i]));
        }
        sb.AppendLine("]");
    }

    private static string Quote(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
        return $"\"{escaped}\"";
    }
}
