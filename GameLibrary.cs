using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodexFairy {
public sealed class GameLibraryItem {
    public int chapter;
    public string hint, detail, source;
}

public sealed class GameLibrary {
    public string game;
    public List<GameLibraryItem> entries;

    public static GameLibrary Load(string path) {
        if (new FileInfo(path).Length > 512 * 1024) throw new IOException("资料库不能超过 512 KB。");
        var library = Json.Parse<GameLibrary>(File.ReadAllText(path));
        if (library == null || GameGuides.Title(library.game) == "" || library.entries == null ||
            library.entries.Count == 0 || library.entries.Count > 100 ||
            library.entries.Any(item => item == null || item.chapter < 1 || item.chapter > 99 ||
                string.IsNullOrWhiteSpace(item.hint) || item.hint.Length > 300 ||
                (item.detail ?? "").Length > 600 || (item.source ?? "").Length > 2048))
            throw new IOException("资料库需要有效的游戏名、章节与简短提示。");
        return library;
    }

    public GameGuideEntry Find(string title, int chapter, bool detailed) {
        if (!string.Equals(game, title, StringComparison.OrdinalIgnoreCase)) return null;
        var item = entries.FirstOrDefault(entry => entry.chapter == chapter);
        if (item == null) return null;
        var sources = new SourceList();
        sources.Add("本地攻略来源", item.source);
        return new GameGuideEntry {
            Key = "本地资料库 · " + title + " · 第" + chapter + "章",
            Summary = detailed && !string.IsNullOrWhiteSpace(item.detail) ? item.hint + "\n" + item.detail : item.hint,
            Sources = sources.Items,
            Expires = DateTime.MaxValue
        };
    }
}
}
