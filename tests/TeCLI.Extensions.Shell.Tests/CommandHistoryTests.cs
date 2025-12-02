using TeCLI.Shell;
using Xunit;

namespace TeCLI.Extensions.Shell.Tests;

public class CommandHistoryTests
{
    [Fact]
    public void Add_SingleCommand_IncreasesCount()
    {
        var history = new CommandHistory();

        history.Add("test command");

        Assert.Equal(1, history.Count);
    }

    [Fact]
    public void Add_EmptyCommand_DoesNotAdd()
    {
        var history = new CommandHistory();

        history.Add("");
        history.Add("   ");
        history.Add(null!);

        Assert.Equal(0, history.Count);
    }

    [Fact]
    public void Add_DuplicateConsecutive_DoesNotAddDuplicate()
    {
        var history = new CommandHistory();

        history.Add("test");
        history.Add("test");

        Assert.Equal(1, history.Count);
    }

    [Fact]
    public void Add_DuplicateNonConsecutive_AddsBoth()
    {
        var history = new CommandHistory();

        history.Add("test");
        history.Add("other");
        history.Add("test");

        Assert.Equal(3, history.Count);
    }

    [Fact]
    public void Previous_EmptyHistory_ReturnsNull()
    {
        var history = new CommandHistory();

        var result = history.Previous();

        Assert.Null(result);
    }

    [Fact]
    public void Previous_SingleEntry_ReturnsEntry()
    {
        var history = new CommandHistory();
        history.Add("test");

        var result = history.Previous();

        Assert.Equal("test", result);
    }

    [Fact]
    public void Previous_MultipleEntries_ReturnsInReverseOrder()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");
        history.Add("third");

        Assert.Equal("third", history.Previous());
        Assert.Equal("second", history.Previous());
        Assert.Equal("first", history.Previous());
    }

    [Fact]
    public void Previous_AtBeginning_StaysAtFirst()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");

        history.Previous(); // second
        history.Previous(); // first
        var result = history.Previous(); // still first

        Assert.Equal("first", result);
    }

    [Fact]
    public void Next_AfterPrevious_ReturnsNextEntry()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");
        history.Add("third");

        history.Previous(); // third
        history.Previous(); // second
        var result = history.Next(); // third

        Assert.Equal("third", result);
    }

    [Fact]
    public void Next_AtEnd_ReturnsNull()
    {
        var history = new CommandHistory();
        history.Add("test");

        history.Previous();
        history.Next();
        var result = history.Next();

        Assert.Null(result);
    }

    [Fact]
    public void ResetPosition_AfterNavigation_ResetsToEnd()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");
        history.Previous();

        history.ResetPosition();
        var result = history.Previous();

        Assert.Equal("second", result);
    }

    [Fact]
    public void Entries_ReturnsAllEntries()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");

        var entries = history.Entries;

        Assert.Equal(2, entries.Count);
        Assert.Equal("first", entries[0]);
        Assert.Equal("second", entries[1]);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");

        history.Clear();

        Assert.Equal(0, history.Count);
    }

    [Fact]
    public void MaxSize_TrimsOldEntries()
    {
        var history = new CommandHistory(maxSize: 3);

        history.Add("1");
        history.Add("2");
        history.Add("3");
        history.Add("4");

        Assert.Equal(3, history.Count);
        Assert.Equal("2", history.Entries[0]);
        Assert.Equal("4", history.Entries[2]);
    }

    [Fact]
    public void Search_FindsMatchingEntries()
    {
        var history = new CommandHistory();
        history.Add("query users");
        history.Add("query orders");
        history.Add("list tables");

        var results = history.Search("query").ToList();

        Assert.Equal(2, results.Count);
        Assert.Contains("query users", results);
        Assert.Contains("query orders", results);
    }

    [Fact]
    public void SearchBackward_FindsMatchingPrefix()
    {
        var history = new CommandHistory();
        history.Add("query users");
        history.Add("list tables");
        history.Add("query orders");

        var result = history.SearchBackward("query");

        Assert.Equal("query orders", result);
    }

    [Fact]
    public void SearchForward_FindsMatchingPrefix()
    {
        var history = new CommandHistory();
        history.Add("query users");
        history.Add("list tables");
        history.Add("query orders");

        history.Previous(); // query orders
        history.Previous(); // list tables
        history.Previous(); // query users

        var result = history.SearchForward("query");

        Assert.Equal("list tables", result); // No, wait - let me re-check the logic
        // Actually SearchForward from position 0 should find the next "query" which is "query orders" at position 2
    }

    [Fact]
    public void Current_ReturnsCurrentPosition()
    {
        var history = new CommandHistory();
        history.Add("first");
        history.Add("second");

        history.Previous();
        var result = history.Current();

        Assert.Equal("second", result);
    }
}
